using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;

namespace LiteClinic.ViewModels
{
    public partial class UsersViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UserModel> Users { get; set; } = new ObservableCollection<UserModel>();
        public ObservableCollection<RoleManager> Roles { get; set; } = new ObservableCollection<RoleManager>();
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly UserRepository _userRepository = new();
        private readonly RoleRepository _roleRepository = new();


        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        private readonly ResourceLoader _loader = new();
        public bool CanAddUsers => PermissionHelper.CanManageUsers; // Add New Record
        public bool CanEditUsers => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateUsers => PermissionHelper.CanEditRecords; // Deactivate Record


        public ICommand? Btn_SaveUserCommand { get; }
        public ICommand? Btn_UpdateUserCommand { get; }
        public ICommand? Btn_ClearCommand { get; }
        public ICommand? Btn_DeactivateRoleCommadn { get; }

        public UsersViewModel()
        {

            Btn_SaveUserCommand = new RelayCommand(SaveUser);
            Btn_UpdateUserCommand = new RelayCommand(UpdateUser);
            Btn_ClearCommand = new RelayCommand(ClearData);
            Btn_DeactivateRoleCommadn = new RelayCommand(DeactivateUser);
        }

        public Action? ClearPasswordBoxesAction { get; set; }

        private UserModel? _selectedUser;
        private string? _statusMessage;
        private bool _isBusy;
        private string? _password;
        private string? _confirmPassword;
        private int _usserAutoId;
        private string? _userId;
        private string? _username;
        private string? _fullName;
        private int _roleId;
        private string? _roleName;
        private string? _email;
        private string? _phoneNumber;
        private string? _landLineNumber;
        private string _language = "en-US";
        private bool _isActive = true;

        private DateTime? _createdAt;
        private DateTime? _updatedAt;
        private string? _updatedBy;

        public UserModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }
                
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
                
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
                
        public string? Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
                
        public string? ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }
        
        public int UserAutoId
        {
            get => _usserAutoId;
            set
            {
                _usserAutoId = value;
                OnPropertyChanged(nameof(UserAutoId));
            }
        }
                
        public string? UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }
                
        public string? Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        
        public string? FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }
        
        public int RoleId
        {
            get => _roleId;
            set
            {
                _roleId = value;
                OnPropertyChanged(nameof(RoleId));
            }
        }
        
        public string? RoleName
        {
            get => _roleName;
            set
            {
                _roleName = value;
                OnPropertyChanged(nameof(RoleName));
            }
        }
        
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
        
        public string? PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string? LandLineNumber
        {
            get => _landLineNumber;
            set
            {
                _landLineNumber = value;
                OnPropertyChanged(nameof(LandLineNumber));
            }
        }
        
        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged(nameof(Language));
            }
        }
        
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }
               
        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set
            {
                _updatedAt = value;
                OnPropertyChanged(nameof(UpdatedAt));
            }
        }
        

        public string? UpdatedBy
        {
            get => _updatedBy;
            set
            {
                _updatedBy = value;
                OnPropertyChanged(nameof(UpdatedBy));
            }
        }

        public SolidColorBrush StatusColor

            {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        private string? _hijriDate;
        public string? HijriDate
        {
            get => _hijriDate;
            set
            {
                _hijriDate = value;
                OnPropertyChanged(nameof(HijriDate));
            }
        }

        private string? _romanDate;
        public string? RomanDate
        {
            get => _romanDate;
            set
            {
                _romanDate = value;
                OnPropertyChanged(nameof(RomanDate));
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

                }
            }
        }

        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;
        private void LoadUsers()
        {
            IsBusy = true;
            Users.Clear();

            var loadedUsers = _userRepository.GetAllUsersWithRoles();
            foreach (var user in loadedUsers)
            {
                Users.Add(user);
            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(
                _loader.GetString("LoadedUsersMessage"),
                Users.Count);
            IsBusy = false;

           OnPropertyChanged(nameof(StatusMessage));

        }

        public async Task LoadUsersAsync()
        {
            IsBusy = true;
            Users.Clear();

            var loadedUsers = await Task.Run(() => _userRepository.GetAllUsersWithRoles());
            foreach (var user in loadedUsers)
            {
                Users.Add(user);
            }

            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(
                _loader.GetString("LoadedUsersMessage"),
                Users.Count);
            IsBusy = false;

            OnPropertyChanged(nameof(StatusMessage));
        }


        public async Task LoadRolesAsync()
        {
            IsBusy = true;
            Roles.Clear();

            var loadedRoles = await _roleRepository.GetAllRolesAsync();
            foreach (var role in loadedRoles)
            {
                Roles.Add(role);
            }

            IsBusy = false;
            OnPropertyChanged(nameof(StatusMessage));
            DatabaseHelper.CloseConnection();
        }

        public async void SaveUser()
        {



            if (string.IsNullOrWhiteSpace(Username))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UsernameRequiredMessage");

                return;
            }
            
            if(!ValidateAndHashPassword()) return;

            var SelectedroleName = Roles.FirstOrDefault(r => r.RoleId == RoleId)?.RoleName;
            var matchedRole = Roles.FirstOrDefault(r => r.RoleId == RoleId);
            int currentAddedRolId = matchedRole != null ? matchedRole.RoleId : 0;


            if (App.GlobalState.LoggedUserRoleId > 1 && currentAddedRolId == 1)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("OnlyAdminCanAddAdminMessage");
                return;
            }

                if (matchedRole == null)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("InvalidRoleSelectionMessage");
                return;
            }

            var userModel = new UserModel
            {

                UserId = GenerateNextUserId(),
                Username = Username,
                PasswordHash = Password,
                FullName = FullName ?? "",
                RoleId = RoleId,
                RoleName = RoleName,
                Email = Email ?? "",
                PhoneNumber = PhoneNumber ?? "",
                LandLineNumber = LandLineNumber ?? "",
                Language = Language ?? "en-US",
                IsActive = IsActive,
                CreatedAt = DateTime.Now,
                UpdatedBy = Environment.UserName

            };
         
            
            var success = _userRepository.SaveUser(userModel);
            if (success)
            {
                Users.Add(userModel);
                StatusColor = new SolidColorBrush(Colors.Teal); // ✅ success color
                StatusMessage = _loader.GetString("UserAdded");

            }

            try
            {
                await Task.Delay(2000, _cts.Token);
                //LoadUsers();
                ClearData();

            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro in UserViewModel- User Added Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(
                    _loader.GetString("UserAddedErrorWithException"),
                    ex.Message);

            }
            finally             
            {
                _cts.TryReset();


            }

            OnPropertyChanged(nameof(StatusMessage));
            StatusMessage = string.Empty;
        }

        public async void UpdateUser()
        {
            if (SelectedUser == null) return;

            if (string.IsNullOrWhiteSpace(SelectedUser.Username))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UsernameRequiredMessage");
                return;
            }

            var SelectedroleName = Roles.FirstOrDefault(r => r.RoleId == RoleId)?.RoleName;
            var matchedRole = Roles.FirstOrDefault(r => r.RoleId == RoleId);
            int currentAddedRolId = matchedRole != null ? matchedRole.RoleId : 0;


            if (App.GlobalState.LoggedUserRoleId > 1 && currentAddedRolId == 1)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("OnlyAdminCanEditAdminMessage");

                return;
            }


            if (Password != ConfirmPassword)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("PasswordsDoNotMatchMessage");
                return;
            }
            else if (!string.IsNullOrWhiteSpace(Password) && Password == ConfirmPassword)
            {
                SelectedUser.PasswordHash = HashPassword(Password!);
            }

            SelectedUser.Username = Username;

            SelectedUser.FullName = FullName ?? "";
            SelectedUser.RoleId = RoleId;
            SelectedUser.RoleName = Roles.FirstOrDefault(r => r.RoleId == RoleId)?.RoleName ?? "";
            SelectedUser.Email = Email ?? "";
            SelectedUser.PhoneNumber = PhoneNumber ?? "";
            SelectedUser.LandLineNumber = LandLineNumber ?? "";
            SelectedUser.Language = Language ?? "en-US";
            SelectedUser.IsActive = IsActive;
            SelectedUser.UpdatedAt = DateTime.Now;
            SelectedUser.UpdatedBy = Environment.UserName;

            var success = _userRepository.UpdateUser(SelectedUser);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessage  = _loader.GetString("UserUpdated");

            }
            try
            {
                await Task.Delay(2000, _cts.Token);
                
                ClearData();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in UserViewModel - User Updated Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(
                    _loader.GetString("UserUpdatedErrorWithException"),
                    ex.Message);
            }
            finally
            {

                _cts.TryReset(); // Reset the token source for future use
            }
            OnPropertyChanged(nameof(StatusMessage));
        }

        public async void DeactivateUser()
        {
            if (SelectedUser == null || string.IsNullOrEmpty(SelectedUser.UserId)) return;

            SelectedUser.IsActive = false;
            SelectedUser.UpdatedAt = DateTime.Now;
            SelectedUser.UpdatedBy = Environment.UserName;
            
            var success = _userRepository.DeactivateUser(SelectedUser);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("UserDeactivated");



            }
            try
            {
                await Task.Delay(2000, _cts.Token); // Await the delay with the new token
                ClearData();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error UserViewModel- User Deactivated Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(
                    _loader.GetString("UserDeactivatedErrorWithException"),
                    ex.Message);

            }
            finally
            {
                _cts.TryReset(); // Reset the token source for future use
            }



            OnPropertyChanged(nameof(StatusMessage));
            StatusMessage = string.Empty;
        }

        private void ClearData()
        {
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            RoleId = 0;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            LandLineNumber = string.Empty;
            Language = string.Empty;
            IsActive = true;
            CreatedAt = null;
            UpdatedAt = null;
            UpdatedBy = string.Empty;
            SelectedUser = new UserModel();
            StatusMessage = string.Empty;
            ClearPasswordBoxesAction?.Invoke();
            SelectedUser = new UserModel();
            LoadUsers();
            _ = LoadRolesAsync();
        }


        private bool ValidateAndHashPassword()
        {
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("PasswordFieldsEmptyMessage");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("PasswordsDoNotMatchMessage");
                return false;
            }
            else
            {

                Password = HashPassword(Password);
                return true;
            }
            
        }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
            await LoadRolesAsync();
            await GetHiriRomanDate();
        }

        public static string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public string GenerateNextUserId()
        {
            var users = _userRepository.GetAllUsers(); // Or query only IDs
            var lastId = users
                .Select(u => u.UserId)
                .Where(id => id!.StartsWith("LC"))
                .Select(id => int.Parse(id![2..]))
                .DefaultIfEmpty(0)
                .Max();

            int nextId = lastId + 1;
            return $"LC{nextId:D4}"; // D4 for zero-padded to 4 digits like LC0001
        }

        private void OnNavigatedFrom()
        {
            _cts.Cancel(); // Stop any pending delays
        }

        public void ClearMemoryUser()
        {
            // Reset user fields
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            LandLineNumber = string.Empty;
            Language = "en-US"; // reset to default
            IsActive = true;
            CreatedAt = null;
            UpdatedAt = null;
            UpdatedBy = string.Empty;

            // Reset selections
            SelectedUser = null;

            // Reset status
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            // Clear collections
            Users.Clear();
            Roles.Clear();            

            // Clear password boxes in UI
            ClearPasswordBoxesAction?.Invoke();
            OnNavigatedFrom();


        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

    }
}
