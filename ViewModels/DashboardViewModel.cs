using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Types;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Resources;
using Windows.UI.Composition;

namespace LiteClinic.ViewModels
{
    public partial class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundService? _backgroundService;

        private readonly ResourceLoader _loader = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IAsyncRelayCommand Btn_refresh { get; }

        //public bool CanAddAppoitment => PermissionHelper.CanManageUsers; // Add New Record
        //public bool CanEditAppoitment => PermissionHelper.CanEditRecords; // Edit existing Record
        //public bool CanDeactivateApppoitment => PermissionHelper.CanEditRecords; // Deactivate Record

        public DashboardViewModel() 
        {
            _backgroundService = App.GlobalState.BackgroundService;
            Btn_refresh = new AsyncRelayCommand(RefreshDashboardAsync);
            _backgroundService.GreetingStarted += OnGreetingStarted;
            //_backgroundService.GreetingFinished += OnGreetingFinished;
        }

        private readonly AppointmentsRepository _appointmentsRepository = new();
        private readonly DoctorsRepository _doctorsRepository = new();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private SolidColorBrush _statusColor = new(Colors.Black);

        private AppointmentModel? _selectedAppointment;
        private DoctorsModel? _selectedDoctor;
        private List<AppointmentModel> _fullAppointments = [];
        private List<ScheduledAppointmentDisplay> _scheduledAppointments = [];
        private List<ScheduledAppointmentDisplay> _scheduledAppointmentsForPatientFilters = []; // add this new list for filtered patients

        private int _scheduleId;
        private string? _doctorName;
        private string? _specialty;
        private string? _selectedDoctorName;
        private string? _selectedDoctorSpecialty;
        private string? _patientFullName;
        private int _patientAutoId;
        private DateTime? _appointmentDate;
        private int _totalDoctors;
        private int _totalPatients;
        private int _missedAppointments;
        private string? _statusMessage;
        private bool _isBusy;
        private bool _isMissed;
        private bool _isAttending;
        private ScheduledAppointmentDisplay? _selectedDisplayPatient;
        private int _attendingNow;
        private int _attended;
        private string? _hijriDate;
        private string? _romanDate;

        public int SelectedDoctorId { get; set; }
        public DateTimeOffset SelectedDoctorDate { get; set; }
        public SolidColorBrush? CircleColor { get; set; }


        public ObservableCollection<AppointmentModel> Appointments { get; set; } = [];
        public ObservableCollection<ScheduledAppointmentDisplay> ScheduledAppointmentDisplay { get; set; } = [];
        public ObservableCollection<ScheduledAppointmentDisplay> ScheduledAppointmentDisplayForPatients { get; set; } = [];
        public ObservableCollection<AppointmentModel> SelectedDoctorAppointments { get; set; } = [];
        public ObservableCollection<DoctorsModel> Doctors { get; set; } = [];
        public ObservableCollection<DoctorWeeklySummary> WeeklyDoctorSummaries { get; set; } = [];
        public ObservableCollection<DoctorGroup> GroupedSummaries { get; set; } = [];
        public ObservableCollection<AttendDisplayViewModel> AttendDisplayViewModel { get; set; } = [];


        public ICommand? UpateStatusCommand { get; }

        private AppointmentModel ConvertDisplayToAppointment(ScheduledAppointmentDisplay display)
        {
            return new AppointmentModel
            {
                ScheduleId = display.ScheduleId,
                UpdatedBy = display.UpdatedBy,
                UpdatedAt = display.UpdatedAt ?? DateTimeOffset.Now,
                IsAttending = display.IsAttending,
                IsMissed = display.IsMissed,
                AttendStatus = display.AttendStatus // ✅ Add this line
            };
        }
        public ScheduledAppointmentDisplay? SelectedDisplayPatient
        { 
            get => _selectedDisplayPatient;
            set {
                _selectedDisplayPatient = value;
                OnPropertyChanged(nameof(SelectedDisplayPatient)); 
            }
        }

        public int ScheduleId
        {
            get => _scheduleId;
            set { _scheduleId = value; OnPropertyChanged(nameof(ScheduleId)); }
        }

        public int PatientAutoId
        {
            get => _patientAutoId;
            set { _patientAutoId = value; OnPropertyChanged(nameof(PatientAutoId)); }
        }

        public string? DoctorName
        {
            get => _doctorName;
            set { _doctorName = value; OnPropertyChanged(nameof(DoctorName)); }
        }
        //_specialization

        public string? Specialty
        {
            get => _specialty;
            set { _specialty  = value; OnPropertyChanged(nameof(Specialty)); }
        }

        public string? SelectedDoctorName
        {
            get => _selectedDoctorName;
            set { _selectedDoctorName = value; OnPropertyChanged(nameof(SelectedDoctorName)); }
        }


        public string? SelectedDoctorSpecialty
        {
            get => _selectedDoctorSpecialty;
            set { _selectedDoctorSpecialty = value; OnPropertyChanged(nameof(SelectedDoctorSpecialty)); }
        }

        public string? PatientName
        {
            get => _patientFullName;
            set { _patientFullName = value; OnPropertyChanged(nameof(PatientName)); }
        }

        public DateTime? AppointmentDate
        {
            get => _appointmentDate;
            set { _appointmentDate = value; OnPropertyChanged(nameof(AppointmentDate)); }
        }

        public int TotalDoctors
        {
            get => _totalDoctors;
            set { _totalDoctors = value; OnPropertyChanged(nameof(TotalDoctors)); }
        }

        public int TotalPatients
        {
            get => _totalPatients;
            set { _totalPatients = value; OnPropertyChanged(nameof(TotalPatients)); }
        }

        public int MissedAppointments
        {
            get => _missedAppointments;
            set { _missedAppointments = value; OnPropertyChanged(nameof(MissedAppointments)); }
        }

        public int AttendingNow
        {
            get => _attendingNow;
            set { _attendingNow = value; OnPropertyChanged(nameof(AttendingNow)); }
        }

        public int Attended
        {
            get => _attended;
            set { _attended = value; OnPropertyChanged(nameof(Attended)); }
        }

        public DoctorsModel? SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                if (_selectedDoctor != value)
                {
                    _selectedDoctor = value;
                    OnPropertyChanged(nameof(SelectedDoctor));
                    FilterAppointmentsByDoctor();
                }
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

        public bool IsMissed
        {
            get => _isMissed;
            set
            {
                _isMissed = value;
                OnPropertyChanged(nameof(IsMissed));


            }
        }

        public AppointmentModel? SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; OnPropertyChanged(nameof(SelectedAppointment)); }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); }
        }

        
        private bool _isListVisible = false;
        public bool IsListVisible
        {
            get => _isListVisible;
            set
            {
                _isListVisible = value;
                OnPropertyChanged(nameof(IsListVisible));
            }
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
                    FilterAppointmentsByStatus();
                }
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

        public string? HijriDate
        {
            get => _hijriDate;
            set
            {
                _hijriDate = value;
                OnPropertyChanged(nameof(HijriDate));
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

        public async Task LoadDisplayAppointmentsAsync()
        {
            //IsBusy = true;
            ScheduledAppointmentDisplay.Clear();
            ScheduledAppointmentDisplayForPatients.Clear();

            var today = DateTime.Now;

            try
            {
                _scheduledAppointments = await Task.Run(() => _appointmentsRepository.GetAppointmentsForWeek(today));

                _scheduledAppointmentsForPatientFilters = [.. _scheduledAppointments]; // Store the full list for filtering

                foreach (var appointment in _scheduledAppointments)
                {
                    ScheduledAppointmentDisplay.Add(appointment);
                    
                }

                foreach (var apptPatientsFilter in _scheduledAppointmentsForPatientFilters)
                {
                    ScheduledAppointmentDisplayForPatients.Add(apptPatientsFilter);
                }

                GenerateWeeklyDoctorSummaries();
                await Task.Delay(1000); // Slight delay to ensure UI updates
                ClearAppointmentFields();

            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("Db_StatusErrorLoadingAppointments"),ex.Message);
                Logger.LogError(ex, "Error loading appointments | DashboardViewModel");
            }
            finally
            {
                _cts.TryReset(); // Reset the cancellation token source for future use
                OnPropertyChanged(nameof(StatusMessage));
                CalculateSummaryMetrics();
            }
        }

        public async Task LoadDisplayAppointmentsAsyncForPatients()
        {
            ScheduledAppointmentDisplayForPatients.Clear();

            var today = DateTime.Now;

            try
            {
                _scheduledAppointments = await Task.Run(() => _appointmentsRepository.GetAppointmentsForWeek(today));

                _scheduledAppointmentsForPatientFilters = [.. _scheduledAppointments]; // Store the full list for filtering


                foreach (var apptPatientsFilter in _scheduledAppointmentsForPatientFilters)
                {
                    ScheduledAppointmentDisplayForPatients.Add(apptPatientsFilter);
                }

                //GenerateWeeklyDoctorSummaries();
                await Task.Delay(1000); // Slight delay to ensure UI updates
                ClearAppointmentFields();

            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("Db_StatusErrorLoadingAppointments"), ex.Message);
                Logger.LogError(ex, "Error loading appointments | DashboardViewModel");
            }
            finally
            {
                _cts.TryReset(); // Reset the cancellation token source for future use
                OnPropertyChanged(nameof(StatusMessage));
                CalculateSummaryMetrics();
            }
        }


        public async Task LoadAppointmentsAsync()
        {
            IsBusy = true;
            Appointments.Clear();

            try
            {
                _fullAppointments = await Task.Run(() => _appointmentsRepository.GetAllAppointments());
                foreach (var appointment in _fullAppointments)
                    Appointments.Add(appointment);                
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("Db_StatusErrorLoadingFullAppointments"),ex.Message);
                //Logger.LogError(ex, "Error loading full appointments | DashboardViewModel");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public async Task LoadDoctorsAsync()
        {
            IsBusy = true;
            Doctors.Clear();

            try
            {
                var loadedDoctors = await Task.Run(() => _doctorsRepository.GetAllActiveDoctors());
                foreach (var doctor in loadedDoctors)
                    Doctors.Add(doctor);
            }
            
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("Db_StatusErrorLoadingDoctors"),ex.Message);
                Logger.LogError(ex, "Error loading doctors | DashboardViewModel");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private void FilterAppointmentsByDoctor()
        {
            SelectedDoctorAppointments.Clear();
            if (SelectedDoctor is null) return;

            var filtered = Appointments.Where(a => a.DoctorId == SelectedDoctor.DoctorId);
            foreach (var appt in filtered)
                SelectedDoctorAppointments.Add(appt);
        }


        private void GenerateWeeklyDoctorSummaries()
        {
            WeeklyDoctorSummaries.Clear();

            var grouped = ScheduledAppointmentDisplay
                .GroupBy(a =>new {a.DoctorId, a.DoctorName, a.AppointmentDate.Date});            

            foreach (var group in grouped)
            {
                WeeklyDoctorSummaries.Add(new DoctorWeeklySummary
                {
                    DoctorId = group.Key.DoctorId,
                    DoctorName = group.Key.DoctorName,
                    AppointmentDate = group.Key.Date,
                    PatientCount = group.Count(),
                    DoctorSpecialty = group.First()?.Specialty ?? "N/A",
                });

            }
            LoadGroupedSummaries();
        }


        private void CalculateSummaryMetrics()
        {
            var today = DateTime.Today;

            // Filter appointments for today
            var todaysAppointments = ScheduledAppointmentDisplay
                .Where(a => a.AppointmentDate.Date == today)
                .ToList();

            TotalDoctors = Doctors.Count;
            TotalPatients = todaysAppointments.Count;
            MissedAppointments = todaysAppointments.Count(a => a.AttendStatus == AttendStatus.Missed);
            AttendingNow = todaysAppointments.Count(a => a.AttendStatus == AttendStatus.CurrentlyAttending);
            Attended = todaysAppointments.Count(a => a.AttendStatus == AttendStatus.Attended);
        }

        // TODO: Calculating Patient Metrics after update the patient status


        public void LoadGroupedSummaries()
        {
            GroupedSummaries.Clear();

            var grouped = WeeklyDoctorSummaries
                .GroupBy(d => d.AppointmentDateFormatted)
                .Select(g => new DoctorGroup
                {
                    DateLabel = g.Key,
                    Doctors = new ObservableCollection<DoctorWeeklySummary>(g)
                });

            foreach (var group in grouped)
            {
                // Debug output to confirm which doctors are in each date group
                //Debug.WriteLine($"Date: {group.DateLabel!}, Doctors: {string.Join(",", group.Doctors!.Select(d => d.DoctorName))}");

                GroupedSummaries.Add(group);
            }
        }

        public async Task FilterAppointmentsByDoctor(int doctorId, DateTimeOffset date)
        {
            await LoadDisplayAppointmentsAsyncForPatients();

            //if the list is null or empty. That avoids unnecessary LINQ queries and object creation.
            if (_scheduledAppointmentsForPatientFilters == null || _scheduledAppointmentsForPatientFilters.Count == 0)
            {
                return;
            }

            var filtered = _scheduledAppointmentsForPatientFilters
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date.Date)
                .Select(a => new ScheduledAppointmentDisplay
                {
                    ScheduleId = a.ScheduleId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.DoctorName,
                    Specialty = a.Specialty,
                    PatientName = a.PatientName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    IsMissed = a.IsMissed,
                    IsAttending = a.IsAttending,
                    AttendStatus = a.AttendStatus,
                    Visuals = new AttendDisplayViewModel { Status = a.AttendStatus } // ✅ This is the key

                    // Add only fields needed for display

                })
                .ToList();

            ScheduledAppointmentDisplay.Clear();
            foreach (var item in filtered)
                ScheduledAppointmentDisplay.Add(item);
            await Task.Delay(200, _cts.Token); // Small delay to ensure UI updates
            foreach (var appointment in ScheduledAppointmentDisplay)
            {
                appointment.Visuals!.RefreshVisuals();
            }
        }


        public async Task RefreshDashboardAsync()
        {
            
            IsBusy = true;            
            IsListVisible = false; // Collapse before refresh
            await LoadAppointmentsAsync();
            await LoadDisplayAppointmentsAsync();
            await LoadDoctorsAsync();
            CalculateSummaryMetrics();
            SelectedDoctorName = string.Empty;
            SelectedDoctorSpecialty = string.Empty;
            IsBusy = false;
        }

        public async Task InitializeAsync()
        {
            App.GlobalState.UpdateSubtitle("Dashboard/Text");
            await RefreshDashboardAsync();
            await GetHiriRomanDate();
        }

        public void ClearSelection()
        {
            SelectedDoctor = null;
            SelectedAppointment = null;
            SelectedDoctorAppointments.Clear();
        }

        private void ClearAppointmentFields()
        {

            StatusMessage = string.Empty;

        }

        public async Task SetAppointmentStatus()
        {
            if (SelectedDisplayPatient == null)
            {
                StatusMessage = _loader.GetString("Db_StatusNoPatientSelected");
                return;
            }

            SelectedDisplayPatient.UpdatedBy = Environment.UserName;
            SelectedDisplayPatient.UpdatedAt = DateTime.Now;

            // Update the database
            var appointmentStatusToUpdate = ConvertDisplayToAppointment(SelectedDisplayPatient);
            bool success = _appointmentsRepository.UpdateAppointmentStatus(appointmentStatusToUpdate);

            if (success)
            {
                //await LoadDisplayAppointmentsAsync();
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessage = _loader.GetString("Db_StatusAppointmentUpdated");
            }
            else
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("Db_StatusAppointmentUpdatedError");
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
                Logger.LogError(ex, "Error in DashBoardViewModel - SetAppointmentStatusIsAttendingOn");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("Db_StatusUnexpectedError");
            }

            OnPropertyChanged(nameof(StatusMessage));            
        }

        private void FilterAppointmentsByStatus()
        {
            if (_scheduledAppointments == null || _scheduledAppointments.Count == 0)
                return;

            var filtered = _scheduledAppointments
                .Where(a => a.AttendStatus == SelectedStatus)
                .ToList();

            ScheduledAppointmentDisplay.Clear();
            foreach (var item in filtered)
                ScheduledAppointmentDisplay.Add(item);
        }


        public void OnNavigatedFrom()
        {
            ClearMemory();
        }

        public void ClearMemory()
        {
            StatusMessage = string.Empty;
            _cts.Cancel();
        }

        private async void OnGreetingStarted(GreetingEventArgs args)
        {

            try
            {
                StatusColor = new SolidColorBrush(Colors.IndianRed);
                StatusMessage = string.Format(_loader.GetString("Db_StatusGreetingMessage"),args.GreetingText);
                await Task.Delay(3000, _cts.Token); // Keep the message for 3 seconds
                StatusColor = new SolidColorBrush(Colors.Black);
                StatusMessage = string.Empty;
            }
            catch (TaskCanceledException) { 
                Logger.LogInfo("Greeting message display canceled", "DashboardViewModel - OnGreetingStarted");
                return; 
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Greeting message display canceled");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("Db_StatusUnexpectedError");
            }
        }
        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

    }
}