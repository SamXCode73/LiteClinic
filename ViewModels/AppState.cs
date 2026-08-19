using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;

namespace LiteClinic.ViewModels
{
    public partial class AppState : INotifyPropertyChanged
    {
        internal BackgroundService BackgroundService { get; } = new BackgroundService();
        public bool CanAccesssAppMenu => PermissionHelper.CanViewAppointments;
        public bool CanAccessMenu => PermissionHelper.CanViewReports; // Add New Record
        public bool CanManageSettingsMenu => PermissionHelper.CanManageSettings;
        public bool CanViewSettingsMenu => PermissionHelper.CanViewSettings;


        private string? _ClinicName;
        public string ClinicName
        {
            get => _ClinicName!;
            set { _ClinicName = value; OnPropertyChanged(nameof(ClinicName)); }
        }

        private string? _loggedUserName;
        public string LoggedUserName
        {
            get => _loggedUserName!;
            set { _loggedUserName = value; OnPropertyChanged(nameof(LoggedUserName)); }
        }

        private string? _loggedUserId;
        public string? LoggedUserId
        {
            get => _loggedUserId;
            set { _loggedUserId = value; OnPropertyChanged(nameof(LoggedUserId)); }
        }

        private int _loggedUserRoleId;
        public int LoggedUserRoleId
        {
            get => _loggedUserRoleId;
            set { _loggedUserRoleId = value; OnPropertyChanged(nameof(LoggedUserRoleId)); }
        }

        private string _currentLanguage = LanguageManager.CurrentLanguage; // default to English 
        public string CurrentLanguage
        { 
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(nameof(CurrentLanguage));
                }
            }
        }

        private bool _isEnglishSelected;
        public bool IsEnglishSelected
        {
            get => _isEnglishSelected;
            set { _isEnglishSelected = value; OnPropertyChanged(nameof(IsEnglishSelected)); }
        }

        private bool _isArabicSelected;
        public bool IsArabicSelected
        {
            get => _isArabicSelected;
            set { _isArabicSelected = value; OnPropertyChanged(nameof(IsArabicSelected)); }
        }

        private bool _isFrenchSelected;
        public bool IsFrenchSelected
        {
            get => _isFrenchSelected;
            set { _isFrenchSelected = value; OnPropertyChanged(nameof(IsFrenchSelected)); }
        }

        private RoleManager? _currentRole;
        public RoleManager? CurrentRole
        {
            get => _currentRole;
            set { _currentRole = value; OnPropertyChanged(nameof(CurrentRole)); }
        }

        // Parent toggle for provider (Telegram in this case)
        private bool _sendViaTelegram;
        public bool SendViaTelegram
        {
            get => _sendViaTelegram;
            set { _sendViaTelegram = value; OnPropertyChanged(nameof(SendViaTelegram)); }
        }

        // Child toggles
        private bool _notifyPatient24h;
        public bool NotifyPatient24h
        {
            get => _notifyPatient24h;
            set { _notifyPatient24h = value; OnPropertyChanged(nameof(NotifyPatient24h)); }
        }

        private bool _notifyPatient2h;
        public bool NotifyPatient2h
        {
            get => _notifyPatient2h;
            set { _notifyPatient2h = value; OnPropertyChanged(nameof(NotifyPatient2h)); }
        }

        private bool _notifyDoctor;
        public bool NotifyDoctor
        {
            get => _notifyDoctor;
            set { _notifyDoctor = value; OnPropertyChanged(nameof(NotifyDoctor)); }
        }


        // === Private backing fields ===

        // These flags are used to control whether notifications
        // should be sent on specific weekdays. They map directly
        // to the NotifyOn<Day> columns in the NotificationSettings table.

        private bool _notifyOnMonday;
        public bool NotifyOnMonday
        {
            get => _notifyOnMonday;
            set
            {
                _notifyOnMonday = value;
                OnPropertyChanged(nameof(NotifyOnMonday));
            }
        }

        private bool _notifyOnTuesday;
        public bool NotifyOnTuesday
        {
            get => _notifyOnTuesday;
            set
            {
                _notifyOnTuesday = value;
                OnPropertyChanged(nameof(NotifyOnTuesday));
            }
        }

        private bool _notifyOnWednesday;
        public bool NotifyOnWednesday
        {
            get => _notifyOnWednesday;
            set
            {
                _notifyOnWednesday = value;
                OnPropertyChanged(nameof(NotifyOnWednesday));
            }
        }

        private bool _notifyOnThursday;
        public bool NotifyOnThursday
        {
            get => _notifyOnThursday;
            set
            {
                _notifyOnThursday = value;
                OnPropertyChanged(nameof(NotifyOnThursday));
            }
        }

        private bool _notifyOnFriday;
        public bool NotifyOnFriday
        {
            get => _notifyOnFriday;
            set
            {
                _notifyOnFriday = value;
                OnPropertyChanged(nameof(NotifyOnFriday));
            }
        }

        private bool _notifyOnSaturday;
        public bool NotifyOnSaturday
        {
            get => _notifyOnSaturday;
            set
            {
                _notifyOnSaturday = value;
                OnPropertyChanged(nameof(NotifyOnSaturday));
            }
        }

        private bool _notifyOnSunday;
        public bool NotifyOnSunday
        {
            get => _notifyOnSunday;
            set
            {
                _notifyOnSunday = value;
                OnPropertyChanged(nameof(NotifyOnSunday));
            }
        }

        // -------------------------
        // Theme Properties
        // -------------------------

        private ThemeType _selectedTheme = ThemeType.Light; // default
        public ThemeType SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged(nameof(SelectedTheme));

                    // Apply theme
                    ThemeManager.CurrentTheme = _selectedTheme.ToString();
                }
            }
        }

        // -------------------------
        // Show/Hide Date
        // -------------------------
        private bool _showGregorianDate;
         public bool ShowGregorianDate
        {
            get => _showGregorianDate;
            set
            {
                if (_showGregorianDate != value)
                {
                    _showGregorianDate = value;
                    OnPropertyChanged(nameof(ShowGregorianDate));
                }
            }
        }


        private bool _showHijriDate;
        public bool ShowHijriDate
        {
            get => _showHijriDate;
            set
            {
                if (_showHijriDate != value)
                {
                    _showHijriDate = value;
                    OnPropertyChanged(nameof(ShowHijriDate));
                }
            }
        }


        public Visibility GregorianDateVisibility => ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;



        // The Event that MainPageViewModel will listen to
        public event Action<string>? SubTitleChanged;

        // The method any page (Main, Dashboard, Invoice) can call
        public void  UpdateSubtitle(string resourceKey)
        {
            // This "invokes" the event and sends the resource key (e.g., "Dashboard")
            SubTitleChanged?.Invoke(resourceKey);
        }

        //____________________
        // Status Messge
        //--------------------
        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);
        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); }
        }

        //-----------------------
        // Popup WIth an Height
        //-----------------------
        private double _popupActualWidth;
        public double PopupActualWidth
        {
            get => _popupActualWidth;
            set
            {
                if (_popupActualWidth != value)
                {
                    _popupActualWidth = value;
                    OnPropertyChanged(nameof(PopupActualWidth));
                }
            }
        }

        private double _popupActualHeight;
        public double PopupActualHeight
        {
            get => _popupActualHeight;
            set
            {
                if (_popupActualHeight != value)
                {
                    _popupActualHeight = value;
                    OnPropertyChanged(nameof(PopupActualHeight));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
