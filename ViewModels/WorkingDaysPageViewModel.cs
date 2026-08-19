using LiteClinic.Models.Enums;
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
using System.Windows.Input;
using Windows.Storage;
using CommunityToolkit.Mvvm.Input;

namespace LiteClinic.ViewModels
{
    public partial class WorkingDaysPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();
        public ICommand ApplyWorkingDaysCommand { get; }

        public bool CanManageSettingsMenu => App.GlobalState.CanManageSettingsMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;

        public WorkingDaysPageViewModel()
        {

            App.GlobalState.PropertyChanged += (s, e) =>
            {

                //Status messge adn Status color
                if (e.PropertyName == nameof(AppState.StatusMessage))
                    StatusMessage = App.GlobalState.StatusMessage;

                if (e.PropertyName == nameof(AppState.StatusColor))
                    StatusColor = App.GlobalState.StatusColor;

                // Wire all properties used in SettingsPage
                if (e.PropertyName == nameof(AppState.CanManageSettingsMenu))
                    OnPropertyChanged(nameof(CanManageSettingsMenu));
                if (e.PropertyName == nameof(AppState.CanViewSettingsMenu))
                    OnPropertyChanged(nameof(CanViewSettingsMenu));

                // Working days
                if (e.PropertyName == nameof(AppState.NotifyOnMonday))
                    OnPropertyChanged(nameof(NotifyOnMonday));
                if (e.PropertyName == nameof(AppState.NotifyOnTuesday))
                    OnPropertyChanged(nameof(NotifyOnTuesday));
                if (e.PropertyName == nameof(AppState.NotifyOnWednesday))
                    OnPropertyChanged(nameof(NotifyOnWednesday));
                if (e.PropertyName == nameof(AppState.NotifyOnThursday))
                    OnPropertyChanged(nameof(NotifyOnThursday));
                if (e.PropertyName == nameof(AppState.NotifyOnFriday))
                    OnPropertyChanged(nameof(NotifyOnFriday));
                if (e.PropertyName == nameof(AppState.NotifyOnSaturday))
                    OnPropertyChanged(nameof(NotifyOnSaturday));
                if (e.PropertyName == nameof(AppState.NotifyOnSunday))
                    OnPropertyChanged(nameof(NotifyOnSunday));
            };

                ApplyWorkingDaysCommand = new RelayCommand(ApplyWorkingDaysAsync); ;
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

        private bool _notifyOnMonday = App.GlobalState.NotifyOnMonday;
        public bool NotifyOnMonday
        {
            get => _notifyOnMonday;
            set
            {
                if (_notifyOnMonday != value)
                {
                    _notifyOnMonday = value;
                    App.GlobalState.NotifyOnMonday = value;
                    OnPropertyChanged(nameof(NotifyOnMonday));
                }
            }
        }

        private bool _notifyOnTuesday = App.GlobalState.NotifyOnTuesday;
        public bool NotifyOnTuesday
        {
            get => _notifyOnTuesday;
            set
            {
                if (_notifyOnTuesday != value)
                {
                    _notifyOnTuesday = value;
                    App.GlobalState.NotifyOnTuesday = value;
                    OnPropertyChanged(nameof(NotifyOnTuesday));
                }
            }
        }

        private bool _notifyOnWednesday = App.GlobalState.NotifyOnWednesday;
        public bool NotifyOnWednesday
        {
            get => _notifyOnWednesday;
            set
            {
                if (_notifyOnWednesday != value)
                {
                    _notifyOnWednesday = value;
                    App.GlobalState.NotifyOnWednesday = value;
                    OnPropertyChanged(nameof(NotifyOnWednesday));
                }
            }
        }

        private bool _notifyOnThursday = App.GlobalState.NotifyOnThursday;
        public bool NotifyOnThursday
        {
            get => _notifyOnThursday;
            set
            {
                if (_notifyOnThursday != value)
                {
                    _notifyOnThursday = value;
                    App.GlobalState.NotifyOnThursday = value;
                    OnPropertyChanged(nameof(NotifyOnThursday));
                }
            }
        }

        private bool _notifyOnFriday = App.GlobalState.NotifyOnFriday;
        public bool NotifyOnFriday
        {
            get => _notifyOnFriday;
            set
            {
                if (_notifyOnFriday != value)
                {
                    _notifyOnFriday = value;
                    App.GlobalState.NotifyOnFriday = value;
                    OnPropertyChanged(nameof(NotifyOnFriday));
                }
            }
        }

        private bool _notifyOnSaturday = App.GlobalState.NotifyOnSaturday;
        public bool NotifyOnSaturday
        {
            get => _notifyOnSaturday;
            set
            {
                if (_notifyOnSaturday != value)
                {
                    _notifyOnSaturday = value;
                    App.GlobalState.NotifyOnSaturday = value;
                    OnPropertyChanged(nameof(NotifyOnSaturday));
                }
            }
        }

        private bool _notifyOnSunday = App.GlobalState.NotifyOnSunday;
        public bool NotifyOnSunday
        {
            get => _notifyOnSunday;
            set
            {
                if (_notifyOnSunday != value)
                {
                    _notifyOnSunday = value;
                    App.GlobalState.NotifyOnSunday = value;
                    OnPropertyChanged(nameof(NotifyOnSunday));
                }
            }
        }

        private async void ApplyWorkingDaysAsync()
        {
            // TODO: Commit working days to AppState or persistence
            bool applyChanges = false;

            try
            {
                // Show "please wait" status
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Orange);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageApplyingChanges");

                // Update local settings
                var localSettings = ApplicationData.Current.LocalSettings.Values;
                localSettings["NotifyOnMonday"] = NotifyOnMonday;
                localSettings["NotifyOnTuesday"] = NotifyOnTuesday;
                localSettings["NotifyOnWednesday"] = NotifyOnWednesday;
                localSettings["NotifyOnThursday"] = NotifyOnThursday;
                localSettings["NotifyOnFriday"] = NotifyOnFriday;
                localSettings["NotifyOnSaturday"] = NotifyOnSaturday;
                localSettings["NotifyOnSunday"] = NotifyOnSunday;

                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
            UPDATE NotificationSettings SET
                NotifyOnMonday = @monday,
                NotifyOnTuesday = @tuesday,
                NotifyOnWednesday = @wednesday,
                NotifyOnThursday = @thursday,
                NotifyOnFriday = @friday,
                NotifyOnSaturday = @saturday,
                NotifyOnSunday = @sunday
            WHERE ProviderType = @ProviderType;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@monday", NotifyOnMonday ? 1 : 0);
                cmd.Parameters.AddWithValue("@tuesday", NotifyOnTuesday ? 1 : 0);
                cmd.Parameters.AddWithValue("@wednesday", NotifyOnWednesday ? 1 : 0);
                cmd.Parameters.AddWithValue("@thursday", NotifyOnThursday ? 1 : 0);
                cmd.Parameters.AddWithValue("@friday", NotifyOnFriday ? 1 : 0);
                cmd.Parameters.AddWithValue("@saturday", NotifyOnSaturday ? 1 : 0);
                cmd.Parameters.AddWithValue("@sunday", NotifyOnSunday ? 1 : 0);
                cmd.Parameters.AddWithValue("@ProviderType", (int)ProviderType.Telegram);

                cmd.ExecuteNonQuery();
                transaction.Commit();

                applyChanges = true;

                await Task.Delay(2000);

                if (applyChanges)
                {
                    // Success message
                    App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Teal);
                    App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageChangesAppliedSuccessfully");
                }

                await Task.Delay(2000, _cts.Token);
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                const string errorTag = "[ERR_WEEKDAY_UPDATE]";
                Logger.LogError(ex, $"{errorTag} Error updating weekday notification settings. UserId={App.GlobalState.LoggedUserId}");

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Red);
                App.GlobalState.StatusMessage = StatusMessage = string.Format(
                    _loader.GetString("Stp_StatusMessageErrorLogCheck"),
                    errorTag
                );
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
