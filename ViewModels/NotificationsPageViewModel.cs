using CommunityToolkit.Mvvm.Input;
using LiteClinic.Repository;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;


namespace LiteClinic.ViewModels
{
    public partial class NotificationsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public ICommand NavigateToConfigPageCommand { get; }

        public NotificationsPageViewModel() 
        {
            App.GlobalState.PropertyChanged += (s, e) =>
            {

                //Status messge adn Status color
                if (e.PropertyName == nameof(AppState.StatusMessage))
                    StatusMessage = App.GlobalState.StatusMessage;

                if (e.PropertyName == nameof(AppState.StatusColor))
                    StatusColor = App.GlobalState.StatusColor;

                if (e.PropertyName == nameof(AppState.NotifyPatient24h))
                    OnPropertyChanged(nameof(NotifyPatient24h));
                if (e.PropertyName == nameof(AppState.NotifyPatient2h))
                    OnPropertyChanged(nameof(NotifyPatient2h));
                if (e.PropertyName == nameof(AppState.NotifyDoctor))
                    OnPropertyChanged(nameof(NotifyDoctor));
                if (e.PropertyName == nameof(AppState.SendViaTelegram))
                    OnPropertyChanged(nameof(SendViaTelegram));



                //Wire all properties used in SettingsPage
                if (e.PropertyName == nameof(AppState.CanManageSettingsMenu))
                    OnPropertyChanged(nameof(CanManageSettingsMenu));
                if (e.PropertyName == nameof(AppState.CanViewSettingsMenu))
                    OnPropertyChanged(nameof(CanViewSettingsMenu));
            };

            NavigateToConfigPageCommand = new RelayCommand(NavigateToConfigPage);

        }

        public bool CanManageSettingsMenu => App.GlobalState.CanManageSettingsMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;

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

        // Patient / Doctor / Telegram notifications
        private bool _notifyPatient24h = App.GlobalState.NotifyPatient24h;
        public bool NotifyPatient24h
        {
            get => _notifyPatient24h;
            set
            {
                if (_notifyPatient24h != value)
                {
                    // Only allow changes if parent is enabled
                    if (SendViaTelegram)
                    {
                        _notifyPatient24h = value;
                        App.GlobalState.NotifyPatient24h = value;
                        _ = UpdateNotificationSettingsAsync();
                        if (ApplicationData.Current != null && ApplicationData.Current.LocalSettings != null && ApplicationData.Current.LocalSettings.Values != null)
                        {
                            ApplicationData.Current.LocalSettings.Values["NotifyPatient24h"] = value;
                        }
                        OnPropertyChanged(nameof(NotifyPatient24h));
                    }
                    else
                    {
                        // If parent is off, force false
                        _notifyPatient24h = false;
                        App.GlobalState.NotifyPatient24h = false;
                        OnPropertyChanged(nameof(NotifyPatient24h));
                    }
                }
            }
        }

        private bool _notifyPatient2h = App.GlobalState.NotifyPatient2h;
        public bool NotifyPatient2h
        {
            get => _notifyPatient2h;
            set
            {
                if (_notifyPatient2h != value)
                {
                    // Only allow changes if parent is enabled
                    if (SendViaTelegram)
                    {
                        _notifyPatient2h = value;
                        App.GlobalState.NotifyPatient2h = value;
                        _ = UpdateNotificationSettingsAsync();
                        if (ApplicationData.Current != null && ApplicationData.Current.LocalSettings != null && ApplicationData.Current.LocalSettings.Values != null)
                        {
                            ApplicationData.Current.LocalSettings.Values["NotifyPatient2h"] = value;
                        }
                        OnPropertyChanged(nameof(NotifyPatient2h));
                    }
                    else
                    {
                        // If parent is off, force false
                        _notifyPatient2h = false;
                        App.GlobalState.NotifyPatient2h = false;
                        OnPropertyChanged(nameof(NotifyPatient2h));
                    }
                }
            }
        }

        private bool _notifyDoctor = App.GlobalState.NotifyDoctor;
        public bool NotifyDoctor
        {
            get => _notifyDoctor;
            set
            {
                if (_notifyDoctor != value)
                {
                    // Only allow changes if parent is enabled
                    if (SendViaTelegram)
                    {
                        _notifyDoctor = value;
                        App.GlobalState.NotifyDoctor = value;
                        _ = UpdateNotificationSettingsAsync();
                        if (ApplicationData.Current != null && ApplicationData.Current.LocalSettings != null && ApplicationData.Current.LocalSettings.Values != null)
                        {
                            ApplicationData.Current.LocalSettings.Values["NotifyDoctor"] = value;
                        }
                        OnPropertyChanged(nameof(NotifyDoctor));
                    }
                    else
                    {
                        // If parent is off, force false
                        _notifyDoctor = false;
                        App.GlobalState.NotifyDoctor = false;
                        OnPropertyChanged(nameof(NotifyDoctor));
                    }
                }
            }
        }

        private bool _sendViaTelegram = App.GlobalState.SendViaTelegram;
        public bool SendViaTelegram
        {
            get => _sendViaTelegram;
            set
            {
                if (_sendViaTelegram != value)
                {
                    _sendViaTelegram = value;
                    App.GlobalState.SendViaTelegram = value;
                    _ = UpdateNotificationSettingsAsync();
                    if (ApplicationData.Current != null && ApplicationData.Current.LocalSettings != null && ApplicationData.Current.LocalSettings.Values != null)
                    {
                        ApplicationData.Current.LocalSettings.Values["SendViaTelegram"] = value;
                    }
                    OnPropertyChanged(nameof(SendViaTelegram));

                    // Notify UI that children enabled state may change
                    OnPropertyChanged(nameof(AreNotificationsEnabled));

                    // If unchecked, reset children
                    if (!_sendViaTelegram)
                    {
                        NotifyPatient24h = false;
                        NotifyPatient2h = false;
                        NotifyDoctor = false;

                        // Reset the local Setting too
                        if (ApplicationData.Current?.LocalSettings?.Values != null)
                        {
                            ApplicationData.Current.LocalSettings.Values["NotifyPatient24h"] = false;
                            ApplicationData.Current.LocalSettings.Values["NotifyPatient2h"] = false;
                            ApplicationData.Current.LocalSettings.Values["NotifyDoctor"] = false;
                        }
                    }
                }
            }
        }

        public bool AreNotificationsEnabled => SendViaTelegram;
        private async Task UpdateNotificationSettingsAsync()
        {
            await Task.Delay(500); // slight delay to ensure AppState is updated and DB
            await SettingsRepository.UpdateNotificationSettingsAsync(
                SendViaTelegram,
                NotifyPatient24h,
                NotifyPatient2h,
                NotifyDoctor
            );

            App.GlobalState.NotifyDoctor = NotifyDoctor;
            App.GlobalState.NotifyPatient2h = NotifyPatient2h;
            App.GlobalState.NotifyPatient24h = NotifyPatient24h;
            App.GlobalState.SendViaTelegram = SendViaTelegram;
        }

        private void NavigateToConfigPage()
        {
            // TODO: Navigation logic to helper/config page
            MainPage.GetContentFrame()?.Navigate(typeof(MessagingIntegrationPage));
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
