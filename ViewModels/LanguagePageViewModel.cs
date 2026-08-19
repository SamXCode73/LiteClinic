using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiteClinic.ViewModels
{
    public partial class LanguagePageViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();

        // Add this property
        public ObservableCollection<LanguageOption> AvailableLanguages { get; } =
            new ObservableCollection<LanguageOption>
            {
                new LanguageOption { Code = "en-US", Name = "English", FlagPath = "/Assets/images/uk_flag.png" },
                new() { Code = "ar", Name = "العربية", FlagPath = "/Assets/images/ksa_flag.png" },
                new() { Code = "fr-FR", Name = "Français", FlagPath = "/Assets/images/france_flag.png" }
            };

        

        private readonly ResourceLoader _loader = new();

        public ICommand ApplyUILanguage { get; }

        public LanguagePageViewModel()
        {
            // Subscribe to GlobalState changes
            App.GlobalState.PropertyChanged += (s, e) =>
            {
                // Set Lnaguage
                if (e.PropertyName == nameof(AppState.CurrentLanguage))
                {
                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            };
            ApplyUILanguage = new AsyncRelayCommand(async () => await UpdateLanguageSettingsAsync(SelectedLanguage!));

        }


        // Setting Language
        private string? _selectedLanguage = App.GlobalState.CurrentLanguage ?? LanguageManager.CurrentLanguage;

        public string? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = string.IsNullOrEmpty(value)
                        ? LanguageManager.CurrentLanguage
                        : value;

                    OnPropertyChanged(nameof(SelectedLanguage));

                    _ = UpdateLanguageSettingsAsync(_selectedLanguage);

                }
            }
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);
        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }
        public async Task UpdateLanguageSettingsAsync(string language)
        {
            try
            {

                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
            UPDATE AppSettings SET
                Language = @Language
            WHERE Id = 1;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Language", language);

                await cmd.ExecuteNonQueryAsync();
                transaction.Commit();

                // Update UI status
                App.GlobalState.CurrentLanguage = language;

                StatusColor = new SolidColorBrush(Colors.Teal);
                // Update UI status
                App.GlobalState.CurrentLanguage = language;

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Teal);

                App.GlobalState.StatusMessage =StatusMessage = _loader.GetString("Stp_StatusMessageLanguageUpdated");

                Logger.LogInfo($"Updated language setting: Language={language}");

                // Update local settings
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["SelectedLanguage"] = language;
                App.GlobalState.CurrentLanguage = language;


                await Task.Delay(5000, _cts.Token);

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                Logger.LogInfo("UpdateLanguageSettingsAsync was canceled by user.");
                return;
            }
            catch (Exception ex)
            {
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageErrorOccurredCheckLog");
                Logger.LogError(ex, "Error updating language setting.");
                await Task.Delay(3000, _cts.Token);
                StatusColor = new SolidColorBrush(Colors.Black);
                StatusMessage = string.Empty;
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use
            }
        }

        public void ClearSettingsMemory()
        {
            // Reset transient UI state
            App.GlobalState.StatusMessage = StatusMessage = string.Empty;
            App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);


            // Cancel CTS
            _cts.Cancel();
        }

    }


}
