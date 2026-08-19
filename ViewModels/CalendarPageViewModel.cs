using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;


namespace LiteClinic.ViewModels
{
    public partial class CalendarPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public CalendarPageViewModel()
        {
            // Subscribe to GlobalState changes
            App.GlobalState.PropertyChanged += (s, e) =>
            {
                //Status messge adn Status color
                if (e.PropertyName == nameof(AppState.StatusMessage))
                    StatusMessage = App.GlobalState.StatusMessage;

                if (e.PropertyName == nameof(AppState.StatusColor))
                    StatusColor = App.GlobalState.StatusColor;

                // Show.Hide Date
                if (e.PropertyName == nameof(AppState.ShowGregorianDate))
                    OnPropertyChanged(nameof(ShowGregorianDate));
                if (e.PropertyName == nameof(AppState.ShowHijriDate))
                    OnPropertyChanged(nameof(ShowHijriDate));
            };
        }

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

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

        // -------------------------
        // Show/Hide Date
        // -------------------------
        private bool _showGregorianDate = App.GlobalState.ShowGregorianDate;
        public bool ShowGregorianDate
        {
            get => _showGregorianDate;
            set
            {
                if (_showGregorianDate != value)
                {
                    _showGregorianDate = value;
                    OnPropertyChanged(nameof(ShowGregorianDate));

                    // Call async update method
                    _ = UpdateDateSettingsAsync(_showGregorianDate, _showHijriDate);
                }
            }
        }

        private bool _showHijriDate = App.GlobalState.ShowHijriDate;
        public bool ShowHijriDate
        {
            get => _showHijriDate;
            set
            {
                if (_showHijriDate != value)
                {
                    _showHijriDate = value;
                    OnPropertyChanged(nameof(ShowHijriDate));

                    // Call async update method
                    _ = UpdateDateSettingsAsync(_showGregorianDate, _showHijriDate);

                }
            }
        }

        public async Task UpdateDateSettingsAsync(bool showGregorianDate, bool showHijriDate)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
            UPDATE AppName SET
                ShowGregorianDate = @ShowGregorianDate,
                ShowHijriDate = @ShowHijriDate
            WHERE AutoAppName = 1;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ShowGregorianDate", showGregorianDate ? 1 : 0);
                cmd.Parameters.AddWithValue("@ShowHijriDate", showHijriDate ? 1 : 0);

                await cmd.ExecuteNonQueryAsync();
                transaction.Commit();

                // Update UI status
                App.GlobalState.ShowGregorianDate = showGregorianDate;
                App.GlobalState.ShowHijriDate = showHijriDate;
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Teal);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageDateSettingsUpdated");
                Logger.LogInfo($"Updated date settings: ShowGregorianDate={showGregorianDate}, ShowHijriDate={showHijriDate}");

                // Update local settings
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["ShowGregorianDate"] = showGregorianDate;
                localSettings.Values["ShowHijriDate"] = showHijriDate;

                await Task.Delay(2000, _cts.Token);

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                Logger.LogInfo("UpdateDateSettingsAsync was canceled by user.");
                return;
            }
            catch (Exception ex)
            {
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageErrorOccurredCheckLog");
                Logger.LogError(ex, "Error updating date settings.");
                await Task.Delay(2000, _cts.Token);
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
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
