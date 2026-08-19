using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiteClinic.ViewModels
{
    public partial class UserRoleViewModel : INotifyPropertyChanged
    {
        private readonly RoleRepository _roleRepository = new();
        public ObservableCollection<RoleManager> Roles { get; set; } = new ObservableCollection<RoleManager>();


        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand? SaveRoleCommand { get; }
        public ICommand? UpdateRoleCommand { get; }
        public ICommand? Btn_DeactivateRoleCommadn { get; }
        public ICommand? Button_ClearRoleData { get; }


        public bool CanAddUserRole => PermissionHelper.CanManageUsers; // Add New Record
        public bool CanEditUserRole => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateUserRole => PermissionHelper.CanEditRecords; // Deactivate Record

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly ResourceLoader _loader = new();
        public UserRoleViewModel()
        {

            SaveRoleCommand = new RelayCommand(SaveRole);
            UpdateRoleCommand = new RelayCommand(UpdateRole);
            Button_ClearRoleData = new RelayCommand(ClearData);
            Btn_DeactivateRoleCommadn = new RelayCommand(ToggleDeactivation);
        }


        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string? _roleName;
        private bool _canManageDoctors;
        private bool _canManageUsers;
        private bool _canAccessDashboard;
        private bool _canViewAppointments;
        private bool _canManageRecords;
        private bool _canEditRecords;
        private bool _isDeactivated;
        private bool _CanManageReports;
        private bool _CanViewReports;
        private bool _CanManageSettings;
        private bool _CanViewSettings;

        private string? _statusMessage;

        private RoleManager? _selectedRole;

        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        public string RoleName
        {
            get => _roleName!;
            set => SetProperty(ref _roleName, value);
        }

        public bool CanManageDoctors
        {
            get => _canManageDoctors;
            set => SetProperty(ref _canManageDoctors, value);
        }

        public bool CanManageUsers
        {
            get => _canManageUsers;
            set => SetProperty(ref _canManageUsers, value);
        }

        public bool CanAccessDashboard
        {
            get => _canAccessDashboard;
            set => SetProperty(ref _canAccessDashboard, value);
        }

        public bool CanViewAppointments
        {
            get => _canViewAppointments;
            set => SetProperty(ref _canViewAppointments, value);
        }

        public bool CanManageRecords
        {
            get => _canManageRecords;
            set => SetProperty(ref _canManageRecords, value);
        }

        public bool CanEditRecords
        {
            get => _canEditRecords;
            set => SetProperty(ref _canEditRecords, value);
        }

        public bool IsDeactivated
        {
            get => _isDeactivated;
            set => SetProperty(ref _isDeactivated, value);
        }

        public bool CanManageReports
        {
            get => _CanManageReports;
            set => SetProperty(ref _CanManageReports, value);
        }

        public bool CanViewReports
        {
            get => _CanViewReports;
            set => SetProperty(ref _CanViewReports, value);
        }

        public bool CanManageSettings
        {
            get => _CanManageSettings;
            set => SetProperty(ref _CanManageSettings, value);
        }

        public bool CanViewSettings
        {
            get => _CanViewSettings;
            set => SetProperty(ref _CanViewSettings, value);
        }

        public RoleManager? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);

        }

        public string StatusMessage
        {
            get => _statusMessage!;
            set => SetProperty(ref _statusMessage, value);
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        private string? _hijriDate;
        public string? HijriDate
        {
            get => _hijriDate;
            set => SetProperty(ref _hijriDate, value);

        }

        private string? _romanDate;
        public string? RomanDate
        {
            get => _romanDate;
            set => SetProperty(ref _romanDate, value);

        }

        // -------------------------
        // Show/Hide Date
        // -------------------------
        private bool _showGregorianDate = App.GlobalState.ShowGregorianDate;
        public bool ShowGregorianDate
        {
            get => _showGregorianDate;
            set => SetProperty(ref _showGregorianDate, value);

        }

        private bool _showHijriDate = App.GlobalState.ShowHijriDate;
        public bool ShowHijriDate
        {
            get => _showHijriDate;
            set => SetProperty(ref _showHijriDate, value);
        }

        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

        private async Task LoadRolesAsync()
        {
            Roles.Clear();
            var roles = await _roleRepository.GetAllRolesAsync();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(_loader.GetString("UrLp_StatusMessageFormat"), Roles.Count);

            //DebugRoles();

        }

        public async Task InitializeUserRoleAsync()
        {
            await LoadRolesAsync();
            await GetHiriRomanDate();
        }
        private async void SaveRole()
        {


            bool allPermissionsChecked =
                CanManageDoctors &&
                CanManageUsers &&
                CanAccessDashboard &&
                CanViewAppointments &&
                CanManageRecords &&
                CanEditRecords &&
                CanManageReports &&
                CanViewReports &&
                CanManageSettings &&
                CanViewSettings;

            bool managedUsers = CanManageUsers;

            // Rule: Only Admin (RoleId == 1) can create a role with full permissions
            if (App.GlobalState.LoggedUserRoleId > 1 && allPermissionsChecked)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_NoPrivilegedRoleMessage");
                return;
            }

            // Rule: Only Admin (RoleId == 1) can edit who can managed Users
            if (App.GlobalState.LoggedUserRoleId > 1 && managedUsers)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_AdminManageUsersMessage");
                return;
            }

            if (string.IsNullOrWhiteSpace(RoleName))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_RoleNameRequiredMessage");
                return;
            }

            var newRole = new RoleManager
            {
                RoleName = RoleName,
                CanManageDoctors = CanManageDoctors,
                CanManageUsers = CanManageUsers,
                CanAccessDashboard = CanAccessDashboard,
                CanViewAppointments = CanViewAppointments,
                CanManageRecords = CanManageRecords,
                CanEditRecords = CanEditRecords,
                IsDeactivated = IsDeactivated,
                CanManageReports = CanManageReports,
                CanViewReports = CanViewReports,
                CanManageSettings = CanManageSettings,
                CanViewSettings = CanViewSettings
            };

            if (_roleRepository.SaveRole(newRole))
            {
                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessage = _loader.GetString("UrLp_RoleAddedMessage");
                //LoadRoles();


                try
                {
                    await Task.Delay(2000, _cts.Token); // Await the delay with the new token
                    ClearData();
                }
                catch (TaskCanceledException)
                {
                    // Ignore if the task was canceled
                    return;
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions that may occur
                    Logger.LogError(ex, $"An error occurred while saving the role:");
                }
                finally
                {
                    _cts.TryReset(); // Reset the CancellationTokenSource for future use    
                }
            }
        }

        private async void UpdateRole()
        {

            bool allPermissionsChecked =
                CanManageUsers &&
                CanAccessDashboard &&
                CanViewAppointments &&
                CanManageRecords &&
                CanEditRecords &&
                CanManageReports &&
                CanViewReports &&
                CanManageSettings &&
                CanViewSettings;

            bool managedUsers = CanManageUsers;
            // Rule: Only Admin (RoleId == 1) can create a role with full permissions
            if (App.GlobalState.LoggedUserRoleId > 1 && allPermissionsChecked)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_NoPrivilegedRoleMessage");
                return;
            }

            // Rule: Only Admin (RoleId == 1) can edit who can managed Users
            if (App.GlobalState.LoggedUserRoleId > 1 && managedUsers)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_AdminManageUsersMessage");
                return;
            }

            if (SelectedRole == null)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_NoRoleSelectedtoUpdateMessage");
                return;
            }

            SelectedRole.RoleName = RoleName;
            SelectedRole.CanManageDoctors = CanManageDoctors;
            SelectedRole.CanManageUsers = CanManageUsers;
            SelectedRole.CanAccessDashboard = CanAccessDashboard;
            SelectedRole.CanViewAppointments = CanViewAppointments;
            SelectedRole.CanManageRecords = CanManageRecords;
            SelectedRole.CanEditRecords = CanEditRecords;
            SelectedRole.IsDeactivated = IsDeactivated;
            SelectedRole.CanManageReports = CanManageReports;
            SelectedRole.CanViewReports = CanViewReports;
            SelectedRole.CanManageSettings = CanManageSettings;
            SelectedRole.CanViewSettings = CanViewSettings;

            if (_roleRepository.UpdateRole(SelectedRole))
            {
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessage = _loader.GetString("UrLp_RoleUpdatedMessage");
                //LoadRoles();


                try
                {
                    await Task.Delay(2000, _cts.Token); // Await the delay with the new token
                    ClearData();
                }
                catch (TaskCanceledException)
                {
                    // Ignore if the task was canceled
                    return;
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions that may occur
                    Logger.LogError(ex, $"An error occurred while updating the role:");
                }
                finally
                {
                    _cts.TryReset(); // Reset the CancellationTokenSource for future use    
                }
            }
        }


        private async void ToggleDeactivation()
        {
            bool IsNotDeactivate = IsDeactivated == false;

            // Rule: Only Admin (RoleId == 1) can create a role with full permissions
            if (App.GlobalState.LoggedUserRoleId > 1 && IsNotDeactivate)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_AdminDeactivateRoleMessage");
                return;
            }

            if (SelectedRole == null)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_NoRoleSelectedMessage");
                return;
            }

            bool currentStatus = SelectedRole.IsDeactivated;
            if (_roleRepository.ToggleDeactivation(SelectedRole.RoleId, currentStatus))
            {
                StatusColor = new SolidColorBrush(Colors.IndianRed);
                StatusMessage = currentStatus
                    ? _loader.GetString("UrLp_RoleActivatedMessage")
                    : _loader.GetString("UrLp_RoleDeactivatedMessage");

                try
                {
                    await Task.Delay(2000, _cts.Token); // Await the delay with the new token
                    await LoadRolesAsync();
                }
                catch (TaskCanceledException)
                {
                    // Ignore if the task was canceled
                    return;
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions that may occur
                    Logger.LogError(ex, $"An error occurred while toggling role deactivation:");
                }
                finally
                {
                    _cts.TryReset(); // Reset the CancellationTokenSource for future use    
                }
            }
        }

        public async void LoadRoleForEditing(RoleManager role)
        {
            if (role == null) return;
            if (role.RoleId == 1 && App.GlobalState.LoggedUserRoleId > 1)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UrLp_AdminEditRoleMessage");
                return;
            }
            RoleName = role.RoleName!;
            CanManageDoctors = role.CanManageDoctors;
            CanManageUsers = role.CanManageUsers;
            CanAccessDashboard = role.CanAccessDashboard;
            CanViewAppointments = role.CanViewAppointments;
            CanManageRecords = role.CanManageRecords;
            CanEditRecords = role.CanEditRecords;
            IsDeactivated = role.IsDeactivated;
            CanManageReports = role.CanManageReports;
            CanViewReports = role.CanViewReports;
            CanManageSettings = role.CanManageSettings;
            CanViewSettings = role.CanViewSettings;

            try
            {
                await Task.Delay(200, _cts.Token); // Await the delay with the new token
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that may occur
                Logger.LogError(ex, $"An error occurred while loading role for editing:");
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use    
            }
            StatusMessage = string.Empty;


        }


        private void ClearData()
        {
            RoleName = string.Empty;
            CanManageDoctors = false;
            CanManageUsers = false;
            CanAccessDashboard = false;
            CanViewAppointments = false;
            CanManageRecords = false;
            CanEditRecords = false;
            IsDeactivated = false;
            CanManageReports = false;
            CanViewReports = false;
            CanManageSettings = false;
            CanViewSettings = false;
            StatusMessage = string.Empty;
            SelectedRole = new RoleManager();
            _ = LoadRolesAsync();
        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearMemoryRoles()
        {
            RoleName = string.Empty;
            CanManageDoctors = false;
            CanManageUsers = false;
            CanAccessDashboard = false;
            CanViewAppointments = false;
            CanManageRecords = false;
            CanEditRecords = false;
            IsDeactivated = false;
            CanManageReports = false;
            CanViewReports = false;
            CanManageSettings = false;
            CanViewSettings = false;

            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            SelectedRole = null;
            Roles.Clear();

            _cts.Cancel();   // stop any pending delays

        }
    }
}