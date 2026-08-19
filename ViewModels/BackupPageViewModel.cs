using CommunityToolkit.Mvvm.Input;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace LiteClinic.ViewModels
{
    public partial class BackupPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public ICommand BackupDatabaseCommand { get; }
        public ICommand BrowseBackupFolderCommand { get; }

        public BackupPageViewModel()
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

            BrowseBackupFolderCommand = new AsyncRelayCommand(BrowseBackupFolderAsync);
            BackupDatabaseCommand = new AsyncRelayCommand(BackupDatabaseAsync);

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue("BackupFolderPath", out object? value))
            {
                BackupLocation = value?.ToString();
            }
            else
            {
                BackupLocation = null; // No folder set yet
            }
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

        private async Task BrowseBackupFolderAsync()
        {
            // TODO: Open folder picker dialog and update BackupLocation
            // Check if a backup path was saved
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            // You need to initialize the folder picker with a window handle in WinUI 3
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder? selectedFolder = await folderPicker.PickSingleFolderAsync();
            if (selectedFolder != null)
            {
                BackupLocation = selectedFolder.Path;
                ApplicationData.Current.LocalSettings.Values["BackupFolderPath"] = selectedFolder.Path;
                await SettingsRepository.UpdateBackupPathAsync(BackupLocation);
            }
        }

        private async Task BackupDatabaseAsync()
        {
            DatabaseHelper.CloseConnection();

            try
            {
                // 1. Get database file path (LocalFolder)
                string dbPath = Path.Combine(
                    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                    "LiteClinic.db");

                if (!File.Exists(dbPath))
                {
                    App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                    App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageDatabaseFileNotFound");
                    return;
                }

                // 2. Get backup folder path from settings
                string? backupFolder = ApplicationData.Current.LocalSettings.Values["BackupFolderPath"] as string;
                if (string.IsNullOrEmpty(backupFolder) || !Directory.Exists(backupFolder))
                {
                    App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                    App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageBackupFolderInvalid");
                    return;
                }

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageBackupGenerating");

                // 3. Generate backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"LiteClinic_{timestamp}.db";
                string backupFilePath = Path.Combine(backupFolder, backupFileName);

                // 4. Use SQLite Backup API to copy database safely
                await Task.Run(() =>
                {
                    using (var source = new SqliteConnection($"Data Source={dbPath}"))
                    using (var destination = new SqliteConnection($"Data Source={backupFilePath}"))
                    {
                        source.Open();
                        destination.Open();
                        source.BackupDatabase(destination);
                    }
                });

                // 5. Make a temporary copy and zip it
                string tempCopyPath = Path.Combine(backupFolder, $"LiteClinic_{timestamp}_copy.db");
                File.Copy(backupFilePath, tempCopyPath, true);

                string backupZipPath = Path.Combine(backupFolder, $"LiteClinic_{timestamp}.zip");
                await Task.Run(() =>
                {
                    using var archive = System.IO.Compression.ZipFile.Open(backupZipPath, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(tempCopyPath, Path.GetFileName(tempCopyPath));
                });

                // Optionally delete the temp copy
                File.Delete(tempCopyPath);

                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Teal);
                App.GlobalState.StatusMessage = StatusMessage = string.Format(
                    _loader.GetString("Stp_StatusMessageBackupCreatedZipped"),
                    Path.GetFileName(backupZipPath)
                );

                await Task.Delay(3000, _cts.Token);
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.Black);
                App.GlobalState.StatusMessage = StatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                App.GlobalState.StatusColor = StatusColor = new SolidColorBrush(Colors.IndianRed);
                App.GlobalState.StatusMessage = StatusMessage = _loader.GetString("Stp_StatusMessageBackupFailed");
                Logger.LogError(ex, "Backup failed:");
                DatabaseHelper.CloseConnection();
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
