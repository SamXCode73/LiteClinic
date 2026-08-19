using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;

namespace LiteClinic.ViewModels
{
    public partial class AppointmentsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AppointmentModel> AddedAppointments { get; set; } = [];
        public ObservableCollection<ScheduledAppointmentDisplay> DisplayedScheduledAppointmentsList { get; set; } = new();
        public ObservableCollection<PatientsModel> PatientsList { get; set; } = new();        
        public ObservableCollection<DoctorScheduleViewRow> DoctorList { get; set; } = new();
        public ObservableCollection<DoctorScheduleViewRow> DoctorListForSearch { get; set; } = new();
        public ObservableCollection<TimeSpan> TimeSlots { get; set; } = new();
        public ObservableCollection<AppointmentTypes> AppointmentTypes { get; } =
            new ObservableCollection<AppointmentTypes>(
                (AppointmentTypes[])Enum.GetValues(typeof(AppointmentTypes)));

        private readonly AppointmentsRepository _appointmentsRepository = new();
        private readonly DoctorsRepository _doctorsRepository = new();
        private readonly PatientsRepository _patientsRepository = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly ResourceLoader _loader = new();

        public ICommand? Btn_SaveCommand { get; }
        public ICommand? Btn_UpdateCommand { get; }
        public ICommand? Btn_DeactivateCommand { get; }
        public ICommand? Btn_ClearCommand { get; }

        public bool CanAddAppoitment => PermissionHelper.CanManageRecords; // Add New Record
        public bool CanEditAppoitment => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateApppoitment => PermissionHelper.CanEditRecords; // Deactivate Record


        public AppointmentsViewModel()
        {

            Btn_SaveCommand = new RelayCommand(SaveAppointment);
            Btn_UpdateCommand = new RelayCommand(UpdateAppointment);
            Btn_DeactivateCommand = new RelayCommand(DeactivateAppointment);
            Btn_ClearCommand = new RelayCommand(ClearAppointmentFields);

        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        private DoctorScheduleViewRow? _doctorListforAppoitnmetn;
        private DoctorScheduleViewRow? _doctorListforSearch;
        private List<PatientsModel> _allPatients = new();
        private List<ScheduledAppointmentDisplay> _allAppointment = new();

        private PatientsModel? _selectedPatient;
        private AppointmentModel? _selectedAppointment;
        private ScheduledAppointmentDisplay? _selectedScheduledAppointmentDisplay;
        private bool _isBusy;
        private string? _statusMessage;
        private int _scheduleId;
        private string? _appointmentID;
        private int _patientAutoId;
        private string? _patientName;
        private string? _patientMotherName;
        private string? _dateOfBirth;
        private bool _isAttending;

        private int _doctorId;
        private string? _doctorName;
        private string? _specialty;

        private DateTimeOffset _appointmentDate = DateTime.Now;
        private TimeSpan _appointmentTime;
        private string? _appointmentType;
        private string? _notes;
        private bool _isActive = true;
        private string? _createdBy;
        private string? _updatedBy;
        private DateTimeOffset _updatedAt;
        private string? _searchQueryPatient;
        private string? _searchQueryAppointment;
        private bool _isTodayFilterEnabled; // Filter to check today appointment
        private bool _isMissed;
        private AppointmentTypes _selectedType;

        private string? _hijriDate;
        private string? _romanDate;
        private bool _showGregorianDate = App.GlobalState.ShowGregorianDate;
        private bool _showHijriDate = App.GlobalState.ShowHijriDate;

        private AppointmentModel ConvertDisplayToAppointment(ScheduledAppointmentDisplay display)
        {
            return new AppointmentModel
            {
                ScheduleId = display.ScheduleId,
                PatientAutoId = PatientAutoId,
                DoctorId = DoctorId,
                AppointmentDate = AppointmentDate,
                AppointmentTime = AppointmentTime,
                AppointmentType = SelectedType,
                Notes = Notes ?? "",
                IsActive = IsActive,
                UpdatedBy = UpdatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}",
                UpdatedAt = DateTime.Now,
                IsMissed = IsMIssed
            };
        }

        private string GetLocalizedStatus(AttendStatus status)
        {
            var loader = ResourceLoader.GetForViewIndependentUse();

            return status switch
            {
                AttendStatus.Missed => loader.GetString("Status_Missed"),
                AttendStatus.Attended => loader.GetString("Status_Attended"),
                AttendStatus.CurrentlyAttending => loader.GetString("Status_CurrentlyAttending"),
                _ => loader.GetString("Status_None")
            };
        }
        private AttendStatus _selectedStatus;
        public AttendStatus SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (_selectedStatus != value)
                {
                    _selectedStatus = value;
                    OnPropertyChanged(nameof(SelectedStatus));
                }
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

        public DoctorScheduleViewRow? SelectedDoctorListforAppoitnment
        {
            get => _doctorListforAppoitnmetn;
            set
            {
                _doctorListforAppoitnmetn = value;
                OnPropertyChanged(nameof(SelectedDoctorListforAppoitnment));
                OnPropertyChanged(nameof(Specialty));
            }
        }

        public DoctorScheduleViewRow? SelectedDoctorListforSearch
        {
            get => _doctorListforSearch;
            set
            {
                _doctorListforSearch = value;
                OnPropertyChanged(nameof(SelectedDoctorListforSearch));
                OnPropertyChanged(nameof(Specialty));
                ApplySearchFilterforAppointment();

            }
        }

        public PatientsModel? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged(nameof(SelectedPatient));
            }
        }

        public AppointmentModel? SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;
                OnPropertyChanged(nameof(SelectedAppointment));
            }
        }

        public ScheduledAppointmentDisplay? SelectedScheduledAppointmentDisplay
        {
            get => _selectedScheduledAppointmentDisplay;
            set
            {
                _selectedScheduledAppointmentDisplay = value;
                OnPropertyChanged(nameof(SelectedScheduledAppointmentDisplay));
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

        public int ScheduleId
        {
            get => _scheduleId;
            set
            {
                _scheduleId = value;
                OnPropertyChanged(nameof(ScheduleId));
            }
        }

        public string? AppointmentID
        {
            get => _appointmentID;
            set
            {
                _appointmentID = value;
                OnPropertyChanged(nameof(AppointmentID));
            }
        }

        public int PatientAutoId
        {
            get => _patientAutoId;
            set
            {
                _patientAutoId = value;
                OnPropertyChanged(nameof(_patientAutoId));
            }
        }

        public string? PatientName
        {
            get => _patientName;
            set
            {
                _patientName = value;
                OnPropertyChanged(nameof(PatientName));
            }
        }

        public string? PatientMotherName
        {
            get => _patientMotherName;
            set
            {
                _patientMotherName = value;
                OnPropertyChanged(nameof(PatientMotherName));
            }
        }

        public string? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                _dateOfBirth = value;
                OnPropertyChanged(nameof(DateOfBirth));
            }
        }

        public int DoctorId
        {
            get => _doctorId;
            set
            {
                _doctorId = value;
                OnPropertyChanged(nameof(DoctorId));
            }
        }

        public string? DoctorName
        {
            get => _doctorName;
            set
            {
                _doctorName = value;
                OnPropertyChanged(nameof(DoctorName));
            }
        }

        public string? Specialty
        {
            get => _specialty;
            set
            {
                _specialty = value;

                OnPropertyChanged(nameof(Specialty));
            }
        }        

        public DateTimeOffset AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                if(_appointmentDate.Date != value.Date)
                {
                    _appointmentDate = value;
                    OnPropertyChanged(nameof(AppointmentDate));
                    //Load doctors based on the selected date
                    LoadDoctors();
                }
            }
        }
        public string AppointmentDateFormatted => AppointmentDate.ToString("dd/MM/yyyy") ?? "";

        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set
            {
                _appointmentTime = value;
                OnPropertyChanged(nameof(AppointmentTime));
            }
        }

        public string AppointmentTimeFormatted =>
            AppointmentTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);

        public string? AppointmentType
        {
            get => _appointmentType;
            set
            {
                _appointmentType = value;
                OnPropertyChanged(nameof(AppointmentType));
            }
        }

        public string? Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
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

        public string? CreatedBy
        {
            get => _createdBy;
            set
            {
                _createdBy = value;
                OnPropertyChanged(nameof(CreatedBy));
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

        public DateTimeOffset UpdatedAt
        {
            get => _updatedAt;
            set
            {
                _updatedAt = value;
                OnPropertyChanged(nameof(UpdatedAt));
            }
        }

        public string? SearchQueryPatient
        {
            get => _searchQueryPatient;
            set
            {
                if (_searchQueryPatient != value)
                {
                    _searchQueryPatient = value;
                    OnPropertyChanged(nameof(SearchQueryPatient));
                    ApplySearchFilterforPatient(); // Trigger filtering automatically
                }
            }
        }

        public string? SearchQueryAppointment
        {
            get => _searchQueryAppointment;
            set
            {
                if (_searchQueryAppointment != value)
                {
                    _searchQueryAppointment = value;
                    OnPropertyChanged(nameof(SearchQueryAppointment));
                    ApplySearchFilterforAppointment(); // Trigger filtering automatically
                }
            }
        }

        public bool IsTodayFilterEnabled
        {
            get => _isTodayFilterEnabled;
            set
            {
                if (_isTodayFilterEnabled != value)
                {
                    _isTodayFilterEnabled = value;
                    OnPropertyChanged(nameof(IsTodayFilterEnabled));

                    // Apply today's date as search query or clear it
                    SearchQueryAppointment = value
                        ? DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                        : string.Empty;

                    ApplySearchFilterforAppointment();
                }
            }
        }

        public bool IsMIssed
        {
            get => _isMissed;
            set
            {
                _isMissed = value;
                OnPropertyChanged(nameof(IsMIssed));
            }
        }

        public bool IsAttending
        {
            get => _isAttending;
            set
            {
                _isAttending = value;
                OnPropertyChanged(nameof(IsAttending));
            }
        }

        public AppointmentTypes SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(nameof(SelectedType));
                }
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

        public void LoadTimeSlots()
        {
            TimeSlots.Clear();
            for (int hour = 8; hour <= 17; hour++) // Clinic hours: 8 AM to 5 PM
            {
                TimeSlots.Add(new TimeSpan(hour, 0, 0));
                TimeSlots.Add(new TimeSpan(hour, 15, 0));
                TimeSlots.Add(new TimeSpan(hour, 30, 0));
                TimeSlots.Add(new TimeSpan(hour, 45, 0));
            }
        }

        public void LoadDoctorsForSearch()
        {
            IsBusy = true;
            DoctorListForSearch.Clear();

            var loadedDoctors = _doctorsRepository.GetDoctorScheduleViewRows();
            foreach (var user in loadedDoctors)
            {
                DoctorListForSearch.Add(user);
            }
        }

        public async Task LoadDoctorsForSearchAsync()
        {
            IsBusy = true;
            DoctorListForSearch.Clear();

            var loadedDoctors = await Task.Run(() => _doctorsRepository.GetDoctorScheduleViewRows());
            foreach (var user in loadedDoctors)
            {
                DoctorListForSearch.Add(user);
            }
            IsBusy = false;

        }

        public void LoadDoctors()
        {
            IsBusy = true;
            DoctorList.Clear();

            var allSchedules = _doctorsRepository.GetDoctorScheduleViewRows();

            // Use the selected AppointmentDate instead of DateTime.Today
            var selectedDate = AppointmentDate.Date;
            string dayName = selectedDate.DayOfWeek.ToString();
            int weekNumber = (selectedDate.Day - 1) / 7 + 1;

            var filteredDoctors = allSchedules
                .Where(ds =>
                    !string.IsNullOrEmpty(ds.DayOfWeek) &&
                    ds.DayOfWeek.Equals(dayName, StringComparison.OrdinalIgnoreCase) &&
                    (ds.WeekNumbers?.Split(',').Contains(weekNumber.ToString()) ?? false)
                )
                .GroupBy(ds => ds.DoctorId)
                .Select(g => g.First())
                .ToList();

            if (filteredDoctors.Count == 0)
            {
                filteredDoctors = allSchedules
                    .GroupBy(ds => ds.DoctorId)
                    .Select(g => g.First())
                    .ToList();
            }

            foreach (var doctor in filteredDoctors)
            {
                DoctorList.Add(doctor);
            }

            IsBusy = false;
        }

        public async Task LoadDoctorsAsync()
        {
            IsBusy = true;
            DoctorList.Clear();

            var allSchedules = await Task.Run(() => _doctorsRepository.GetDoctorScheduleViewRows());

            // Use the selected AppointmentDate instead of DateTime.Today
            var selectedDate = AppointmentDate.Date;
            string dayName = selectedDate.DayOfWeek.ToString();
            int weekNumber = (selectedDate.Day - 1) / 7 + 1;

            var filteredDoctors = allSchedules
                .Where(ds =>
                    !string.IsNullOrEmpty(ds.DayOfWeek) &&
                    ds.DayOfWeek.Equals(dayName, StringComparison.OrdinalIgnoreCase) &&
                    (ds.WeekNumbers?.Split(',').Contains(weekNumber.ToString()) ?? false)
                )
                .GroupBy(ds => ds.DoctorId)
                .Select(g => g.First())
                .ToList();

            if (filteredDoctors.Count == 0)
            {
                filteredDoctors = allSchedules
                    .GroupBy(ds => ds.DoctorId)
                    .Select(g => g.First())
                    .ToList();
            }

            foreach (var doctor in filteredDoctors)
            {
                DoctorList.Add(doctor);
            }

            IsBusy = false;
        }
        public void LoadPatients()
        {
            IsBusy = true;
            PatientsList.Clear();

            _allPatients = _patientsRepository.GetAllActivePatients();

            foreach (var patient in _allPatients)
            {
                PatientsList.Add(patient);

            }
            IsBusy = false;
        }

        public async Task LoadPatientsAsync()
        {
            IsBusy = true;
            PatientsList.Clear();


            _allPatients = await Task.Run(() => _patientsRepository.GetAllActivePatients());
            foreach (var patient in _allPatients)
            {
                PatientsList.Add(patient);
            }
            IsBusy = false;

        }

        public void LoadAppointments()
        {
            IsBusy = true;
            AddedAppointments.Clear();

            try
            {

                var addedAppoint = _appointmentsRepository.GetAllAppointments();

                foreach (var appointment in addedAppoint)
                {
                    AddedAppointments.Add(appointment);

                }

                StatusColor = new SolidColorBrush(Colors.Black);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_LoadedPatients"), PatientsList.Count);
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_ErrorLoadingPatients"), ex.Message);
                Logger.LogError(ex, "Error loading patients | AppointmentsViewModel");
            }
            finally
            {
                IsBusy = false;

            }
        }

        public void LoadDisplayAppointments()
        {
            IsBusy = true;
            DisplayedScheduledAppointmentsList.Clear();

            try
            {

                _allAppointment = _appointmentsRepository.GetDisplayedAppointments();

                foreach (var appointment in _allAppointment)
                {
                    DisplayedScheduledAppointmentsList.Add(appointment);

                }

                StatusColor = new SolidColorBrush(Colors.Black);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_LoadedAppointments"), DisplayedScheduledAppointmentsList.Count);
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_ErrorLoadingAppointments"), ex.Message);
                Logger.LogError(ex, "Error loading appointments | AppointmentsViewModel");
            }
            finally
            {
                IsBusy = false;

            }
            OnPropertyChanged(nameof(StatusMessage));
        }

        public async Task LoadDisplayAppointmentsAsync()
        {
            IsBusy = true;
            DisplayedScheduledAppointmentsList.Clear();
            try
            {
                _allAppointment = await Task.Run(() => _appointmentsRepository.GetDisplayedAppointments());

                foreach (var appointmentDisplay in _allAppointment)
                {
                    DisplayedScheduledAppointmentsList.Add(appointmentDisplay);
                }

                StatusColor = new SolidColorBrush(Colors.Black);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_LoadedAppointments"), DisplayedScheduledAppointmentsList.Count);
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("AppV_Status_ErrorLoadingAppointments"), ex.Message);
                Logger.LogError(ex, "Error loading appointments | AppointmentsViewModel");
            }
            finally
            {
                IsBusy = false;

            }
            OnPropertyChanged(nameof(StatusMessage));
        }



        public async Task InitializeAsync()
        {
            LoadTimeSlots();
            await LoadDoctorsAsync();
            await LoadPatientsAsync();
            await LoadDisplayAppointmentsAsync();
            await LoadDoctorsForSearchAsync();
            await GetHiriRomanDate();

        }

        private void ClearAppointmentFields()
        {
            //_patientAutoId = 0;
            PatientName = string.Empty;
            PatientMotherName = string.Empty;
            DateOfBirth = string.Empty;
            DoctorId = 0;
            DoctorName = string.Empty;
            //Specialty = string.Empty;
            AppointmentDate = DateTime.Now;
            //AppointmentTime = new TimeSpan(8, 0, 0);
            AppointmentType = string.Empty;
            Notes = string.Empty;
            IsActive = true;
            DisplayedScheduledAppointmentsList.Clear();
            DoctorList.Clear();
            PatientsList.Clear();
            IsMIssed = false;
            IsTodayFilterEnabled = false;
            SearchQueryAppointment = string.Empty;


            LoadTimeSlots();
            LoadDisplayAppointments();
            LoadDoctors();
            LoadPatients();
            LoadDoctorsForSearch();
        }


        //public string GenerateNextAppointmentId()
        //{
        //    var appointment = _appointmentsRepository.GetLastppointment();

        //    int lastId = appointment.ScheduleId;
        //    int nextId = lastId + 1;
        //    return $"Aptn{nextId:D9}"; // D9 for zero-padded to 4 digits like LC0001
        //}

        public string GenerateNextAppointmentId()
        {
            var appointment = _appointmentsRepository.GetLastppointment();

            int lastId = appointment.ScheduleId;
            int nextId = lastId + 1;
            return $"Aptn{nextId:D9}"; // D9 for zero-padded to 4 digits like LC0000000001
        }

        public void OnNavigatedFrom()
        {
            ClearAppointmentMemory();

        }

        public async void SaveAppointment()
        {
            if (SelectedDoctorListforAppoitnment != null)
                this.DoctorId = SelectedDoctorListforAppoitnment.DoctorId;

            if (PatientAutoId == 0 || DoctorId == 0)
            {
                
                StatusMessage = _loader.GetString("AppV_Status_PatientDoctorRequired");
                StatusColor = new SolidColorBrush(Colors.Red);
                return;
            }

            // Rule 1: Prevent same patient booking same doctor twice in one day
            var existingAppointments = _appointmentsRepository.GetAllAppointments();
            bool duplicateExists = existingAppointments.Any(a =>
                a.DoctorId == DoctorId &&
                a.PatientAutoId == PatientAutoId &&
                a.AppointmentDate.Date == AppointmentDate.Date);

            if (duplicateExists)
            {
                StatusColor = new SolidColorBrush(Colors.DarkOrange);
                StatusMessage = _loader.GetString("AppV_Status_DuplicateAppointment");
                return;
            }

            // Rule 2: Prevent doctor being double-booked at same time (different patient)
            bool doctorConflict = existingAppointments.Any(a =>
                a.DoctorId == DoctorId &&
                a.AppointmentDate.Date == AppointmentDate.Date &&
                a.AppointmentTime == AppointmentTime);

            if (doctorConflict)
            {
                StatusColor = new SolidColorBrush(Colors.DarkOrange);
                StatusMessage = string.Format(
                    _loader.GetString("AppV_Status_DoctorAlreadyBooked"),
                    PatientAutoId,
                    AppointmentTime);
                return;
            }

            var appointmentModel = new AppointmentModel
            {
                AppointmentID = GenerateNextAppointmentId(),
                PatientAutoId = PatientAutoId,
                DoctorId = DoctorId,
                AppointmentDate = AppointmentDate,
                AppointmentTime = AppointmentTime,
                AppointmentType = SelectedType,
                Notes = Notes ?? "",
                IsActive = true,
                CreatedBy = $"Created By: Window Username: {Environment.UserName} | Logged Username: {App.GlobalState.LoggedUserName} | ID: {App.GlobalState.LoggedUserId}",
                CreatedAt = DateTime.Now
            };

            var success = _appointmentsRepository.SaveAppointment(appointmentModel);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentAdded");
            }
            else
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentAddedError");
            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                ClearAppointmentFields();
            }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in AppointmentsViewModel - SaveAppointment");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_UnexpectedError");
            }
            finally
            {
                _cts.TryReset();
            }
        }

        public async void UpdateAppointment()
        {
            if (SelectedScheduledAppointmentDisplay == null)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_NoAppointmentSelectedToUpdate");
                return;
            }

            var appointmentToUpdate = ConvertDisplayToAppointment(SelectedScheduledAppointmentDisplay);

            // Role 1 Validation: check for duplicate appointment (excluding itself)
            var existingAppointments = _appointmentsRepository.GetAllAppointments();
            bool duplicateExists = existingAppointments.Any(a =>
                a.ScheduleId != appointmentToUpdate.ScheduleId && // exclude current
                a.DoctorId == appointmentToUpdate.DoctorId &&
                a.PatientAutoId == appointmentToUpdate.PatientAutoId &&
                a.AppointmentDate.Date == appointmentToUpdate.AppointmentDate.Date);
            

            if (duplicateExists)
            {
                StatusColor = new SolidColorBrush(Colors.DarkOrange);
                StatusMessage = _loader.GetString("AppV_Status_DuplicateAppointmentAlt");
                return;
            }

            // Rule 2: Prevent doctor being double-booked at same time (different patient)
            bool doctorConflict = existingAppointments.Any(a =>
                a.ScheduleId != appointmentToUpdate.ScheduleId && // exclude current
                a.DoctorId == appointmentToUpdate.DoctorId &&
                a.AppointmentDate.Date == appointmentToUpdate.AppointmentDate.Date &&
                a.AppointmentTime == appointmentToUpdate.AppointmentTime);

            if (doctorConflict)
            {
                StatusColor = new SolidColorBrush(Colors.DarkOrange);
                StatusMessage = string.Format(
                    _loader.GetString("AppV_Status_DoctorAlreadyBooked"),
                    PatientAutoId,
                    AppointmentTime);
                return;
            }

            // Rule 3: Prevent duplicate appointments for the same patient with the same doctor
            // on the same date and time. This ensures a patient cannot be double-booked
            // with one doctor in a single timeslot, while still allowing rescheduling
            // to a different doctor or a different day/time.
            bool duplicateExistsSameClient = existingAppointments.Any(a =>
                a.ScheduleId != appointmentToUpdate.ScheduleId && // exclude current
                a.PatientAutoId == appointmentToUpdate.PatientAutoId &&
                a.AppointmentDate.Date == appointmentToUpdate.AppointmentDate.Date);

            if (duplicateExistsSameClient)
            {
                StatusColor = new SolidColorBrush(Colors.DarkOrange);
                StatusMessage = _loader.GetString("AppV_Status_DuplicateAppointmentAlt");
                return;
            }

            // overwrite with new values from ViewModel (only the fields that can be edited)

            if (SelectedDoctorListforAppoitnment != null)
                this.DoctorId = SelectedDoctorListforAppoitnment.DoctorId;

            appointmentToUpdate.ScheduleId = this.ScheduleId;
            appointmentToUpdate.DoctorId = this.DoctorId;
            appointmentToUpdate.AppointmentDate = this.AppointmentDate;
            appointmentToUpdate.AppointmentTime = this.AppointmentTime;
            appointmentToUpdate.AppointmentType = this.SelectedType;
            appointmentToUpdate.Notes = this.Notes ?? "";
            appointmentToUpdate.UpdatedBy = $"Updated By: Window Username: {Environment.UserName} | Logged Username: {App.GlobalState.LoggedUserName} | ID: {App.GlobalState.LoggedUserId}";
            appointmentToUpdate.UpdatedAt = this.UpdatedAt = DateTimeOffset.Now;
            appointmentToUpdate.IsActive = this.IsActive;

            var success = _appointmentsRepository.UpdateAppointment(appointmentToUpdate);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentUpdated");
            }
            else
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentUpdatedError");
            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                ClearAppointmentFields();
            }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in AppointmentsViewModel - UpdateAppointment");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_UnexpectedError");
            }
            finally
            {
                _cts.TryReset();
            }
        }
        public async void DeactivateAppointment()
        {
            if (SelectedScheduledAppointmentDisplay == null ||
                string.IsNullOrEmpty(SelectedScheduledAppointmentDisplay.PatientName) ||
                string.IsNullOrEmpty(SelectedScheduledAppointmentDisplay.DoctorName))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_NoAppointmentSelectedToDeactivate");
                return;
            }

            var appointmentToUpdate = ConvertDisplayToAppointment(SelectedScheduledAppointmentDisplay);

            appointmentToUpdate.IsActive = false;
            appointmentToUpdate.UpdatedBy = Environment.UserName;
            appointmentToUpdate.UpdatedAt = DateTime.Now;

            var success = _appointmentsRepository.DeactivateAppointment(appointmentToUpdate);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentDeactivated");
            }
            else
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_AppointmentDeactivatedError");
            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                ClearAppointmentFields();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in AppointmentsViewModel - DeactivateAppointment");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("AppV_Status_UnexpectedError");
            }
            finally
            {
                _cts.TryReset();
            }

            OnPropertyChanged(nameof(StatusMessage));

        }

        public void ApplySearchFilterforPatient()
        {
            if (_allPatients == null || _allPatients.Count == 0) return;

            PatientsList.Clear();

            var query = SearchQueryPatient?.Trim().ToLower();

            foreach (var patient in _allPatients)
            {
                // Fix for CS8602: Check for null before calling ToLower
                // Fix for CS0019: Remove '?? false' after Contains, as Contains returns bool and PatientId may be null
                bool matchesQuery =
                    string.IsNullOrWhiteSpace(query) ||
                    (patient.FirstName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.MiddleName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.LastName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.PatientFullName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.Email?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.PhoneNumber?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (patient.PatientAutoId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (patient.FullMotherName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (!string.IsNullOrEmpty(patient.DateOfBirth) && patient.DateOfBirth.ToLower(CultureInfo.CurrentCulture).Contains(query!, StringComparison.CurrentCultureIgnoreCase) ||
                    (patient.PatientAge.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(patient.PatientId) && patient.PatientId.ToLower(CultureInfo.CurrentCulture).Contains(query!, StringComparison.CurrentCultureIgnoreCase)));

                if (matchesQuery)
                {
                    PatientsList.Add(patient);
                }
            }
        }

        public void ApplySearchFilterforAppointment()
        {
            if (_allAppointment == null || _allAppointment.Count == 0)
                return;

            DisplayedScheduledAppointmentsList.Clear();

            var query = SearchQueryAppointment?.Trim().ToLower();
            var selectedDoctorId = SelectedDoctorListforSearch?.DoctorId;
            var selectedDoctorNameOnly = SelectedDoctorListforSearch?.FullNameWithSpecialty?.Split('-')[0]?.Trim();
            bool isDateFilterEnabled = IsTodayFilterEnabled;

            int doctorAppointmentCount = 0;

            foreach (var schedule in _allAppointment)
            {
                bool matchesQuery =
                    string.IsNullOrWhiteSpace(query) ||
                    schedule.ScheduleId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    (schedule.AppointmentID?.Contains(query) ?? false) ||
                    (schedule.PatientName?.ToLower().Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (schedule.PatientMotherName?.ToLower().Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    schedule.PatientId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    schedule.PatientDOB.ToString("dd/MM/yyyy").Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    schedule.DoctorName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    schedule.AppointmentDateFormatted.Contains(query, StringComparison.CurrentCultureIgnoreCase);

                bool matchesDoctor = string.IsNullOrWhiteSpace(selectedDoctorNameOnly) ||
                                     schedule.DoctorName.Equals(selectedDoctorNameOnly, StringComparison.CurrentCultureIgnoreCase);

                bool matchesDate = !isDateFilterEnabled || schedule.AppointmentDate.Date == AppointmentDate.Date;

                if (matchesQuery && matchesDoctor && matchesDate)
                {
                    DisplayedScheduledAppointmentsList.Add(schedule);

                    if (!string.IsNullOrWhiteSpace(selectedDoctorNameOnly))
                    {
                        doctorAppointmentCount++;
                    }
                }
            }

            // Set status message based on filters
            if (!string.IsNullOrWhiteSpace(selectedDoctorNameOnly))
            {
                if (isDateFilterEnabled)
                {
                    StatusMessage = string.Format(
                        _loader.GetString("AppV_Status_DoctorAppointmentsOnDate"),
                        selectedDoctorNameOnly,
                        doctorAppointmentCount,
                        AppointmentDate.ToString("dd/MM/yyyy")
);
                }
                else
                {
                    StatusMessage = string.Format(
                        _loader.GetString("AppV_Status_DoctorTotalAppointments"),
                        selectedDoctorNameOnly,
                        doctorAppointmentCount);
                }
            }
            else
            {
                StatusMessage = string.Format(
                    _loader.GetString("AppV_Status_ShowingAppointments"),
                    DisplayedScheduledAppointmentsList.Count);
            }
        }

        public void ClearAppointmentMemory()
        {
            PatientName = string.Empty;
            PatientMotherName = string.Empty;
            DateOfBirth = string.Empty;
            DoctorId = 0;
            DoctorName = string.Empty;
            AppointmentDate = DateTime.Now;
            AppointmentType = string.Empty;
            Notes = string.Empty;
            IsActive = true;
            DisplayedScheduledAppointmentsList.Clear();
            DoctorList.Clear();
            PatientsList.Clear();
            AddedAppointments.Clear();
            StatusMessage = string.Empty;
            IsMIssed = false;
            IsAttending = false;
            IsTodayFilterEnabled = false;
            SearchQueryAppointment = string.Empty;
            SearchQueryPatient = string.Empty;
            SelectedAppointment = null;
            SelectedScheduledAppointmentDisplay = null;
            SelectedPatient = null;
            SelectedDoctorListforAppoitnment = null;
            SelectedDoctorListforSearch = null;
            _cts.Cancel();
        }


        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }


    }
}