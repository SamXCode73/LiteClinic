using CommunityToolkit.Mvvm.Input;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;

namespace LiteClinic.ViewModels
{
    public partial class LogsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public ICommand OpenLogFolderCommand { get; }

        public LogsPageViewModel()
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

            
            OpenLogFolderCommand = new AsyncRelayCommand(OpenLogFolderAsync);


        }

        public bool CanManageSettingsMenu => App.GlobalState.CanManageSettingsMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;

        private string? _backupLocation = string.Empty;
        public string? BackupLocation
        {
            get => _backupLocation;
            set { _backupLocation = value; OnPropertyChanged(nameof(BackupLocation)); }
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

        private async Task OpenLogFolderAsync()
        {
            string logFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "AppLogs");

            try
            {
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder); // Ensure it exists

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(logFolder);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception ex)
            {
                string key = "ERROR_OPEN_LOG_FOLDER";
                Logger.LogError(ex, $"{key}: Failed to open log folder");
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageLogFolderOpenFailed");
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
