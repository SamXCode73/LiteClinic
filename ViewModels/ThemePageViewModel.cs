using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace LiteClinic.ViewModels
{
    public partial class ThemePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public List<ThemeType> AvailableThemes { get; } =
            [.. Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>()];

        private ThemeType _selectedTheme = ThemeType.Light;
        private readonly bool _isInitializingTheme = false;

        public ThemeType SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged(nameof(SelectedTheme));

                    Services.ThemeManager.CurrentTheme = _selectedTheme.ToString();
                    ApplicationData.Current.LocalSettings.Values["SelectedTheme"] = _selectedTheme.ToString();

                    if (!_isInitializingTheme)
                    {
                        _ = SaveThemeAsync(_selectedTheme);
                    }
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

        private async Task SaveThemeAsync(ThemeType theme)
        {
            try
            {
                await SettingsRepository.UpdateThemeAsync(theme.ToString());
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageThemeChangeSaved");
                await Task.Delay(3000, _cts.Token);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.RoyalBlue);

            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception dbEx)
            {
                Logger.LogError(dbEx, "Database error while saving theme");
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageThemeSaveFailed");
                await Task.Delay(2000, _cts.Token);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                return;
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use
            }
        }

        private void SetThemeSilent(ThemeType theme)
        {
            _selectedTheme = theme;
            OnPropertyChanged(nameof(SelectedTheme));
            Services.ThemeManager.CurrentTheme = _selectedTheme.ToString();
            ApplicationData.Current.LocalSettings.Values["SelectedTheme"] = _selectedTheme.ToString();
        }


        public async Task InitializeSettingsAsync()
        {
            try
            {

                // Try to get theme from DB
                var themeName = await SettingsRepository.GetThemeAsync();

                // Fallback to LocalSettings if DB value is missing
                var settings = ApplicationData.Current?.LocalSettings?.Values;
                if (string.IsNullOrWhiteSpace(themeName))
                {
                    themeName = settings?["SelectedTheme"] as string ?? "Light";
                }

                // Apply theme immediately
                ThemeManager.CurrentTheme = themeName;

                // Convert string → enum safely
                if (Enum.TryParse(themeName, out ThemeType parsedTheme))
                {
                    App.GlobalState.SelectedTheme = parsedTheme;
                    SetThemeSilent(parsedTheme); // silent setter avoids SaveThemeAsync
                }
                else
                {
                    App.GlobalState.SelectedTheme = ThemeType.Light; // fallback
                    SetThemeSilent(ThemeType.Light);
                }
            }
            catch (Exception ex)
            {
                // If DB fails, fallback to LocalSettings only
                var settings = ApplicationData.Current?.LocalSettings?.Values;
                var themeName = settings?["SelectedTheme"] as string ?? "Light";

                ThemeManager.CurrentTheme = themeName;

                if (Enum.TryParse(themeName, out ThemeType parsedTheme))
                {
                    App.GlobalState.SelectedTheme = parsedTheme;
                    SetThemeSilent(parsedTheme);
                }
                else
                {
                    App.GlobalState.SelectedTheme = ThemeType.Light;
                    SetThemeSilent(ThemeType.Light);
                }

                Logger.LogError(ex, "Failed to initialize settings from database");
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
