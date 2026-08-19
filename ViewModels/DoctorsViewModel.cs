using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.System;
using WinRT.Interop;

namespace LiteClinic.ViewModels
{
    public partial class DoctorsViewModel : INotifyPropertyChanged
    {       
        public ObservableCollection<DoctorsModel> DoctorList { get; set; } = new ObservableCollection<DoctorsModel>();
        public ObservableCollection<DoctorDisplayModel> DisplayDoctorList { get; set; } = [];

        private readonly DoctorsRepository _doctorsRepository = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IAsyncRelayCommand? Btn_SaveDcotorCommand { get; }
        public IAsyncRelayCommand? Btn_UpdateDcotorCommand { get; }
        public IAsyncRelayCommand? Btn_ClearCommand { get; }
        public IAsyncRelayCommand? Btn_DeactivateDoctorCommadn { get; }
        public IAsyncRelayCommand? Btn_GotoServicePage { get; }
        public IAsyncRelayCommand? Btn_OpenScheduledPage { get; }

        public IAsyncRelayCommand? Btn_BrowsPicture { get; }

        public bool CanAddDoctor => PermissionHelper.CanManageDoctors; // Add New Record
        public bool CanEditDoctor => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateDoctor => PermissionHelper.CanEditRecords; // Deactivate Record


        public DoctorsViewModel()
        {

            Btn_SaveDcotorCommand = new AsyncRelayCommand(SaveDoctor);
            Btn_UpdateDcotorCommand = new AsyncRelayCommand(UpdateDoctor);
            Btn_ClearCommand = new AsyncRelayCommand(ClearDoctorFields);
            Btn_DeactivateDoctorCommadn = new AsyncRelayCommand(DeactivateDoctor);
            Btn_GotoServicePage = new AsyncRelayCommand(NavigateToServicePageAsync);
            Btn_OpenScheduledPage = new AsyncRelayCommand(OpenScheduledPageAsync);
            Btn_BrowsPicture = new AsyncRelayCommand(ChangePictureAsync);
        }

        private readonly ResourceLoader _loader = new();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        private List<DoctorsModel>? _allDoctorsList = [];
        private List<DoctorDisplayModel> _allDcotorDisplayLsit = [];

        private DoctorsModel? _selectedDoctor;
        private DoctorDisplayModel? _selectedDislayDoctor;
        private int _doctorId;
        private string? _doctorCode;
        private string? _fullName;
        private string? _specialization;
        private string? _phoneNumber;
        private string? _landLinrNumber;
        private bool _isActive = true;
        private string? _createdBy;
        private DateTime? _createdAt;
        private string? _updatedBy;
        private DateTime? _updatedAt;
        private bool _isBusy;
        private string? _statusMessage;
        private string? _gender;
        private string? _profilePicturePath;
        private string? _initials;
        private string? _doctorFilter;
        private string? _hijriDate;
        private string? _romanDate;

        private DoctorsModel ConvertDispalyDoctorToDoctor(DoctorDisplayModel dispaly)
        {
            return new DoctorsModel
            {
                DoctorId = dispaly.DoctorId,
                DoctorCode = DoctorCode,
                FullName = FullName,
                Specialization = Specialization,
                Gender = Gender,
                PhoneNumber = PhoneNumber,
                LandLineNumber = LandLineNumber,
                IsActive = IsActive,
                UpdatedAt = DateTime.Now,
                UpdatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}",
                ProfilePicturePath = ProfilePicturePath ?? "ms-appx:///Assets/Profiles/Defaults/male_avatar.png" // If no image is used,
            };

        }


        //Filtered Properties
        public string? DoctorFilter
        {
            get => _doctorFilter;
            set
            {
                if (_doctorFilter != value)
                {
                    _doctorFilter = value;
                    OnPropertyChanged(nameof(DoctorFilter));
                    ApplyDoctorFilter();
                }
            }
        }

        public DoctorsModel? SelectedDoctor 
        { get => _selectedDoctor;
            set 
            {
                _selectedDoctor = value;
                OnPropertyChanged(nameof(SelectedDoctor));
            } 
        }

        public DoctorDisplayModel? SelectedDisplayDoctor
        {
            get => _selectedDislayDoctor;
            set
            {
                _selectedDislayDoctor = value;
                OnPropertyChanged(nameof(SelectedDisplayDoctor));
            }
        }


        public int DoctorId
        {
            get => _doctorId;
            set { _doctorId = value; OnPropertyChanged(nameof(DoctorId)); }
        }

        
        public string? DoctorCode
        {
            get => _doctorCode;
            set { _doctorCode = value; OnPropertyChanged(nameof(DoctorCode)); }
        }

        
        public string? FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
        }

        
        public string? Specialization
        {
            get => _specialization;
            set { _specialization = value; OnPropertyChanged(nameof(Specialization)); }
        }

        public string? Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(nameof(Gender)); }
        }
        public string? PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        
        public string? LandLineNumber
        {
            get => _landLinrNumber;
            set { _landLinrNumber = value; OnPropertyChanged(nameof(LandLineNumber)); }
        }

        
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }

        
        public string? CreatedBy
        {
            get => _createdBy;
            set { _createdBy = value; OnPropertyChanged(nameof(CreatedBy)); }
        }

        
        public DateTime? CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        
        public string? UpdatedBy
        {
            get => _updatedBy;
            set { _updatedBy = value; OnPropertyChanged(nameof(UpdatedBy)); }
        }

        
        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(nameof(UpdatedAt)); }
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
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

        public string? HijriDate
        {
            get => _hijriDate;
            set
            {
                _hijriDate = value;
                OnPropertyChanged(nameof(HijriDate));
            }
        }
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
        public string? ProfilePicturePath
        {
            get => _profilePicturePath;
            set
            {
                if (_profilePicturePath != value)
                {
                    _profilePicturePath = value;
                    OnPropertyChanged(nameof(ProfilePicturePath));
                }
            }
        }

        public string? Initials
        {
            get => _initials;
            set
            {
                _initials = value;
                OnPropertyChanged(nameof(Initials));
            }
        }


        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

        public async Task LoadDisplayDoctorsAsync()
        {
            IsBusy = true;
            DisplayDoctorList.Clear(); // public ObservableCollection<DoctorDisplayModel> DisplayDoctorList { get; set; } = [];

            var loadedDoctors = await Task.Run(() => _doctorsRepository.GetAllDisplayDoctorAsync());
            foreach (var user in loadedDoctors)
            {
                // Add raw DB entity
                DisplayDoctorList.Add(user);                
            }

            _allDcotorDisplayLsit = loadedDoctors; // private List<DoctorDisplayModel> _allDcotorDisplayLsit = [];

            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(_loader.GetString("DrP_Status_LoadedUsers"), DoctorList.Count);
            IsBusy = false;
            

            OnPropertyChanged(nameof(StatusMessage));

        }


        public async Task LoadDoctorsAsync()
        {
            IsBusy = true;
            DoctorList.Clear();

            var loadedDoctors = await Task.Run(() => _doctorsRepository.GetAllDoctors());
            foreach (var user in loadedDoctors)
            {
                // Add raw DB entity
                DoctorList.Add(user);

                // Map into display model
            }

            _allDoctorsList = loadedDoctors;

            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(_loader.GetString("DrP_Status_LoadedUsers"), DoctorList.Count);
            IsBusy = false;

            OnPropertyChanged(nameof(StatusMessage));
        }


        public async Task InitializeAsync()
        {
            await LoadDoctorsAsync();
            await LoadDisplayDoctorsAsync();
            await GetHiriRomanDate();
        }

        public async Task SaveDoctor()
        {

            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorNameRequired");
                return;
            }

            var doctorModel = new DoctorsModel
            {
                DoctorCode = GenerateNextDoctorId(),
                FullName = FullName ?? "",
                Specialization = Specialization ?? "",
                Gender = Gender,
                PhoneNumber = PhoneNumber ?? "",
                LandLineNumber = LandLineNumber ?? "",
                IsActive = IsActive,
                CreatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}",
                CreatedAt = DateTime.Now,
                ProfilePicturePath = ProfilePicturePath ?? "ms-appx:///Assets/Profiles/Defaults/male_avatar.png"
            };

            var success = _doctorsRepository.SaveDoctor(doctorModel);
            if (success)
            {
                DoctorList.Add(doctorModel);
                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessage = _loader.GetString("DrP_Status_DoctorAdded");

            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                //LoadDoctors();
                await ClearDoctorFields();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in DoctorsViewModel - Doctor Added Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorAddedError");
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use
            }
        }

        public async Task UpdateDoctor()
        {
            if (SelectedDisplayDoctor == null) return;

            if (string.IsNullOrWhiteSpace(SelectedDisplayDoctor.FullName))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorNameRequired");
                return;
            }


            var updatedDipalyDoctor =  ConvertDispalyDoctorToDoctor(SelectedDisplayDoctor);

            var success = _doctorsRepository.UpdateDoctor(updatedDipalyDoctor);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessage = _loader.GetString("DrP_Status_DoctorUpdated");

            }
            try
            {
                
                await Task.Delay(3000, _cts.Token); // taek some time to show message
                await ClearDoctorFields(); // update doctors again to show updated data

            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in DoctorsViewModel - Doctor Updated Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorUpdatedError");
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use
            }

            OnPropertyChanged(nameof(StatusMessage));
            StatusMessage = string.Empty;
        }

        public async Task DeactivateDoctor()
        {
            if (SelectedDoctor == null || string.IsNullOrEmpty(SelectedDoctor.DoctorCode)) return;

            SelectedDoctor.IsActive = false;
            SelectedDoctor.UpdatedAt = DateTime.Now;
            SelectedDoctor.UpdatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}";

            var success = _doctorsRepository.DeactivateDoctor(SelectedDoctor);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorDeactivated");
            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                //LoadDoctors();
                await ClearDoctorFields();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error DoctorsViewModel - Doctor Deactivated Error");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("DrP_Status_DoctorDeactivatedError");
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use    
            }

            OnPropertyChanged(nameof(StatusMessage));
            StatusMessage = string.Empty;
        }

        private async Task NavigateToServicePageAsync()
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(DrServiceIdsPage));
            App.GlobalState.UpdateSubtitle("DrServiceIdsPage/Text");
            await Task.CompletedTask; // optional, can be removed
        }

        private async Task OpenScheduledPageAsync()
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(ScheduledDoctorPage));
            App.GlobalState.UpdateSubtitle("DoctorsScheduled/Text");
            await Task.CompletedTask; // optional, can be removed
        }
        private async Task ClearDoctorFields()
        {
            _allDoctorsList = []; // List
            _allDcotorDisplayLsit = []; //List
            DoctorId = 0; // Property
            DoctorCode = string.Empty; // Property
            FullName = string.Empty; // Property
            Specialization = string.Empty; // Property
            PhoneNumber = string.Empty; // Property
            LandLineNumber = string.Empty; // Property
            IsActive = true; // Property
            CreatedBy = string.Empty; // Property
            CreatedAt = null; // Property
            UpdatedBy = string.Empty; // Property
            UpdatedAt = null; // Property
            StatusMessage = string.Empty; // Property
            StatusColor = new SolidColorBrush(Colors.Black); // Property
            SelectedDoctor = new DoctorsModel(); // Model
            SelectedDisplayDoctor = new DoctorDisplayModel(); // Model
            ProfilePicturePath = string.Empty;  // Property
            Gender = string.Empty; // Property
            DoctorList.Clear(); // Collection
            DisplayDoctorList.Clear(); // Collection
            await LoadDoctorsAsync(); // Loading New collectin if needed
            await LoadDisplayDoctorsAsync(); // Loading New collectin if needed
        }

        public void ClearDoctorsMemory()
        {
            _allDoctorsList = [];
            _allDcotorDisplayLsit = [];
            DoctorId = 0;
            DoctorCode = string.Empty;
            FullName = string.Empty;
            Specialization = string.Empty;
            PhoneNumber = string.Empty;
            LandLineNumber = string.Empty;
            IsActive = true;
            CreatedBy = string.Empty;
            CreatedAt = null;
            UpdatedBy = string.Empty;
            UpdatedAt = null;
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedDoctor = new DoctorsModel();
            SelectedDisplayDoctor = new DoctorDisplayModel();
            ProfilePicturePath = string.Empty;
            Gender = string.Empty;
            DoctorList.Clear();
            DisplayDoctorList.Clear();
            OnNavigatedFrom();

        }

        public string GenerateNextDoctorId()
        {
            var doctors = _doctorsRepository.GetAllDoctors();
            var lastId = doctors
                .Select(u => u.DoctorCode)
                .Where(id => id!.StartsWith("Dr"))
                .Select(id => int.Parse(id!.Substring(2)))
                .DefaultIfEmpty(0)
                .Max();

            int nextId = lastId + 1;
            return $"Dr{nextId:D4}"; // D4 for zero-padded to 4 digits like LC0001
        }

        public void OnNavigatedFrom()
        {
            _cts.Cancel();
        }

        // Filter Method


        private void ApplyDoctorFilter()
        {
            if (_allDcotorDisplayLsit == null) return;

            var query = DoctorFilter?.Trim().ToLower();
            DisplayDoctorList.Clear();

            IEnumerable<DoctorDisplayModel> filtered;

            if (string.IsNullOrEmpty(query))
            {
                filtered = _allDcotorDisplayLsit;
            }
            else
            {
                filtered = _allDcotorDisplayLsit.Where(d =>
                    d.DoctorId.ToString().Contains(query) ||
                    (!string.IsNullOrEmpty(d.DoctorCode) && d.DoctorCode.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.FullName) && d.FullName.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.Specialization) && d.Specialization.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.PhoneNumber) && d.PhoneNumber.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.LandLineNumber) && d.LandLineNumber.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                );
            }

            foreach (var doctor in filtered)
                DisplayDoctorList.Add(doctor);
        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        private async Task ChangePictureAsync()
        {
            try
            {
                string uniqueName = string.Empty;
                var picker = new Windows.Storage.Pickers.FileOpenPicker
                {
                    ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
                };
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

                var file = await picker.PickSingleFileAsync();
                if (file == null) return;

                // Generate folders in Local Folder
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var profilesFolder = await localFolder.CreateFolderAsync("Profiles", Windows.Storage.CreationCollisionOption.OpenIfExists);
                var doctorsFolder = await profilesFolder.CreateFolderAsync("Doctors", Windows.Storage.CreationCollisionOption.OpenIfExists);


                Windows.Storage.StorageFile copiedFile;

                // Adding images
                if (SelectedDisplayDoctor == null || string.IsNullOrWhiteSpace(SelectedDisplayDoctor.ProfilePicturePath))
                {
                    // New doctor OR doctor without image → generate new GUID filename
                    uniqueName = $"{Guid.NewGuid()}{file.FileType}";
                    copiedFile = await file.CopyAsync(doctorsFolder, uniqueName, Windows.Storage.NameCollisionOption.ReplaceExisting);
                }
                else
                {
                    // Existing doctor with image → reuse filename
                    uniqueName = System.IO.Path.GetFileName(SelectedDisplayDoctor.ProfilePicturePath);

                    // If old image was a default avatar, generate new GUID
                    if (uniqueName.Contains("avatar", StringComparison.OrdinalIgnoreCase))
                        uniqueName = $"{Guid.NewGuid()}{file.FileType}";

                    copiedFile = await file.CopyAsync(doctorsFolder, uniqueName, Windows.Storage.NameCollisionOption.ReplaceExisting);

                }

                // rebuild image pathe again from fressh update. Force UI
                ProfilePicturePath = string.Empty; 
                ProfilePicturePath = copiedFile.Path;
                OnPropertyChanged(nameof(ProfilePicturePath)); // Force UI refresh
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error DoctorsViewModel - Doctor Brows Picture Error");
            }
        }

    }
}

