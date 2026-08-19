using CommunityToolkit.Mvvm.Input;
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

namespace LiteClinic.ViewModels
{
    public partial class ClinicNameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public ICommand ApplyClinicNameCommand { get; }

        public ClinicNameViewModel()
        {
            // Initialize properties from global state
            App.GlobalState.PropertyChanged += (s, e) =>
            {

                //Status messge adn Status color
                if (e.PropertyName == nameof(AppState.StatusMessage))
                    StatusMessage = App.GlobalState.StatusMessage;

                if (e.PropertyName == nameof(AppState.StatusColor))
                    StatusColor = App.GlobalState.StatusColor;


                //Wire all properties used in SettingsPage
                if (e.PropertyName == nameof(AppState.CanManageSettingsMenu))
                    OnPropertyChanged(nameof(CanManageSettingsMenu));
                if (e.PropertyName == nameof(AppState.CanViewSettingsMenu))
                    OnPropertyChanged(nameof(CanViewSettingsMenu));
            };

            ApplyClinicNameCommand = new RelayCommand(ApplyClinicName);
        }

        public bool CanManageSettingsMenu => App.GlobalState.CanManageSettingsMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;


        private string _clinicName = App.GlobalState.ClinicName;
        public string ClinicName
        {
            get => _clinicName;
            set
            {
                if (_clinicName != value)
                {
                    _clinicName = value;
                    App.GlobalState.ClinicName = value;
                    OnPropertyChanged(nameof(ClinicName));
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

        private async void ApplyClinicName()
        {
            // TODO: Save ClinicName to AppState or persistence
            string loggedUsername = App.GlobalState.LoggedUserName;
            string windowsloggedUser = Environment.UserName;
            string dateNow = DateTime.Now.ToString("F");
            string updateCliniName = $"Logged User: {loggedUsername} - Logged Windows User: {windowsloggedUser} -  Update at: {dateNow}";


            if (App.GlobalState.LoggedUserRoleId > 1)
            {
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageClinicNameAdminOnly");
                return;
            }

            if (string.IsNullOrEmpty(ClinicName))
            {
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageClinicNameEmpty");
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
                    UPDATE AppName SET
                        AppName = @AppName,
                        AppNameEpoch = @AppNameEpoch
                    WHERE AutoAppName = 1;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@AppName", ClinicName);
                cmd.Parameters.AddWithValue("@AppNameEpoch", updateCliniName);

                cmd.ExecuteNonQuery();
                transaction.Commit();

                 App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Teal);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageClinicNameUpdated");
                Logger.LogInfo("Update clinic name: => ", updateCliniName);

                //Add clinic name to storage
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["ClinicName"] = ClinicName;

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
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageErrorLogCheckGeneral");
                Logger.LogError(ex, "Error updating ClinicName.");
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
