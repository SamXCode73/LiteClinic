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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.AppBroadcasting;

namespace LiteClinic.ViewModels
{
    public partial class ScheduledDoctorViewModel : INotifyPropertyChanged
    {
        //For Data Add adn Update 
        public ObservableCollection<ScheduledDoctor> ScheduledList { get; set; } = new ObservableCollection<ScheduledDoctor>();
        // For updating Combox Doctor List
        public ObservableCollection<DoctorsModel> DoctorsListforScheduled { get; set; } = new ObservableCollection<DoctorsModel>();
        // For Data Binding
        public ObservableCollection<ScheduledDoctorDisplayModel> ScheduledDisplayList { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ScheduledDoctorRepository _scheduledDoctorRepository = new();
        private readonly DoctorsRepository _doctorsRepository = new();

        private List<ScheduledDoctor> _allScheduled = new();
        private List<ScheduledDoctorDisplayModel> _allScheduledDisplay = new();

        private readonly ResourceLoader _loader = new();
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand? Btn_SaveDcotorScheduledCommand { get; }
        public ICommand? Btn_UpdateDcotorScheduledCommand { get; }
        public ICommand? Btn_ClearCommand { get; }
        public ICommand? Btn_DeactivateDoctorScheduledCommadn { get; }

        public bool CanAddDoctorScheduled => PermissionHelper.CanManageDoctors; // Add New Record
        public bool CanEditDoctorScheduled => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateDoctorScheduled => PermissionHelper.CanEditRecords; // Deactivate Record  

        public ScheduledDoctorViewModel()
        {

            Btn_SaveDcotorScheduledCommand = new RelayCommand(SaveScheduledDoctor);
            Btn_UpdateDcotorScheduledCommand = new RelayCommand(UpdateScheduledDoctor);
            Btn_ClearCommand = new RelayCommand(ClearScheduledFields);
            Btn_DeactivateDoctorScheduledCommadn = new RelayCommand(DeactivateScheduledDoctor);
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        private ScheduledDoctor? _selectedSchdel;
        private ScheduledDoctorDisplayModel? _selectedSchdelDisplay;

        private string? _scheduledStatusMessage;
        private bool _isBusy;
        private int _schdledAutoId;
        private string? _scheduleId;
        private int? _doctorId;
        private string? _doctorCode;
        private string? _fullName;
        private string? _specialization;
        private string? _phoneNumber;
        private string? _dayOfTheWeek;
        private bool _canNotify = true;
        private bool _isScheduleActiveDis = true;
        private bool _isWeek1;
        private bool _isWeek2;
        private bool _isWeek3;
        private bool _isWeek4;
        private bool _isWeek5;
        private string? _disTime;
        private TimeSpan? _disTimeFrom;
        private TimeSpan? _disTimeTo;

        private string BuildWeekNumbersCsv()
        {
            var selected = new List<string>();
            if (IsWeek1) selected.Add("1");
            if (IsWeek2) selected.Add("2");
            if (IsWeek3) selected.Add("3");
            if (IsWeek4) selected.Add("4");
            if (IsWeek5) selected.Add("5");


            return string.Join(",", selected);
        }

        private string? _scheduleFilter;
        public string? ScheduleFilter
        {
            get => _scheduleFilter;
            set
            {
                if (_scheduleFilter != value)
                {
                    _scheduleFilter = value;
                    OnPropertyChanged(nameof(ScheduleFilter));
                    ApplyScheduleFilter();
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public string? StatusMessageSch
        {
            get => _scheduledStatusMessage;
            set
            {
                _scheduledStatusMessage = value;
                OnPropertyChanged(nameof(StatusMessageSch));
            }
        }

        public ScheduledDoctor? SelectedScheduled
        {
            get => _selectedSchdel;
            set
            {
                _selectedSchdel = value;
                OnPropertyChanged(nameof(SelectedScheduled));
            }
        }

        // For display in Datagird

        public ScheduledDoctorDisplayModel? SelectedScheduledDisplay
        {
            get => _selectedSchdelDisplay;
            set
            {
                _selectedSchdelDisplay = value;
                OnPropertyChanged(nameof(SelectedScheduledDisplay));
            }
        }

        public int ScheduledAutoId
        {
            get => _schdledAutoId;
            set
            {
                _schdledAutoId = value;
                OnPropertyChanged(nameof(ScheduledAutoId));
            }
        }

        public string? ScheduleId
        {
            get => _scheduleId;
            set
            {
                _scheduleId = value;
                OnPropertyChanged(nameof(ScheduleId));
            }
        }

        public int? DoctorId
        {
            get => _doctorId;
            set
            {
                _doctorId = value;
                OnPropertyChanged(nameof(DoctorId));
            }
        }

        public string? DoctorCode
        {
            get => _doctorCode;
            set
            {
                _doctorCode = value;
                OnPropertyChanged(nameof(DoctorId));
            }
        }

        public string? Specialization
        {
            get => _specialization;
            set
            {
                _specialization = value;
                OnPropertyChanged(nameof(Specialization));
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

        public string? DoctorName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(DoctorName));
            }
        }

        public string? DayOfTheWeek
        {
            get => _dayOfTheWeek;
            set
            {
                _dayOfTheWeek = value;
                OnPropertyChanged(nameof(DayOfTheWeek));
            }
        }

        public bool CanNotify
        {
            get => _canNotify;
            set
            {
                _canNotify = value;
                OnPropertyChanged(nameof(CanNotify));
            }
        }

        public bool IsScheduleActiveDis
        {
            get => _isScheduleActiveDis;
            set
            {
                _isScheduleActiveDis = value;
                OnPropertyChanged(nameof(IsScheduleActiveDis));
            }
        }



        public bool IsWeek1
        {
            get => _isWeek1;
            set { _isWeek1 = value; OnPropertyChanged(nameof(IsWeek1)); }
        }
        public bool IsWeek2
        {
            get => _isWeek2;
            set { _isWeek2 = value; OnPropertyChanged(nameof(IsWeek2)); }
        }
        public bool IsWeek3
        {
            get => _isWeek3;
            set { _isWeek3 = value; OnPropertyChanged(nameof(IsWeek3)); }
        }
        public bool IsWeek4
        {
            get => _isWeek4;
            set { _isWeek4 = value; OnPropertyChanged(nameof(IsWeek4)); }
        }

        public bool IsWeek5
        {
            get => _isWeek5;
            set { _isWeek5 = value; OnPropertyChanged(nameof(IsWeek5)); }
        }

        public string? DisTime
        {
            get => _disTime;
            set
            { _disTime = value; OnPropertyChanged(nameof(DisTime)); }
        }

        public TimeSpan? DisTimeFrom
        {
            get => _disTimeFrom;
            set
            { _disTimeFrom = value; OnPropertyChanged(nameof(DisTimeFrom)); }
        }

        public TimeSpan? DisTimeTo
        {
            get => _disTimeTo;
            set
            { _disTimeTo = value; OnPropertyChanged(nameof(DisTimeTo)); }
        }

        private static ScheduledDoctor? MapToScheduledDoctor(ScheduledDoctorDisplayModel display)
        {
            if (display == null) return null;

            var timeFromTo = string.Empty;

            var weeks = new List<string>();
            if (display.IsWeek1) weeks.Add("1");
            if (display.IsWeek2) weeks.Add("2");
            if (display.IsWeek3) weeks.Add("3");
            if (display.IsWeek4) weeks.Add("4");
            if (display.IsWeek5) weeks.Add("5");
            if (!string.IsNullOrWhiteSpace(display.TimeDis)) timeFromTo = display.TimeDis;

            return new ScheduledDoctor
            {
                ScheduleAutoId = display.ScheduleAutoIdDis,
                ScheduleId = display.ScheduleIdDis,
                DoctorId = display.DoctorIdDis,
                DayOfWeek = display.DayOfWeekDis,
                Notify = display.NotifyDis,
                IsActive = display.IsScheduleActiveDis,
                WeekNumbers = string.Join(",", weeks),
                DisTime = timeFromTo

            };
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

        public void LoadDoctorScheduled()
        {
            IsBusy = true;
            ScheduledList.Clear();

            try
            {

                _allScheduled = _scheduledDoctorRepository.GetAllScheduledDoctors();

                foreach (var schedule in _allScheduled)
                {
                    ScheduledList.Add(schedule);

                }


            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSch"),ex.Message);

            }
            finally
            {
                IsBusy = false;

            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchLoaded"), ScheduledList.Count);
            OnPropertyChanged(nameof(StatusMessageSch));
        }

        public async Task LoadDoctorScheduledAsync()
        {
            IsBusy = true;
            ScheduledList.Clear();
            try
            {
                var loadedSchedudel = await Task.Run(() => _scheduledDoctorRepository.GetAllScheduledDoctors());

                foreach (var shcedule in loadedSchedudel)
                {
                    ScheduledList.Add(shcedule);
                }


            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSch"), ex.Message);
            }
            finally
            {
                IsBusy = false;
                
            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchLoaded"), ScheduledList.Count);
            OnPropertyChanged(nameof(StatusMessageSch));
        }

        //###############

        // For Only dispay in data binding

        public void LoadDoctorScheduledView()
        {
            IsBusy = true;
            ScheduledDisplayList.Clear();

            try
            {

                _allScheduledDisplay = _scheduledDoctorRepository.GetAllScheduledDoctorsView();

                foreach (var viewScheduleDisplay in _allScheduledDisplay)

                {
                    // Parse WeekNumbersDis string into booleans
                    var weeks = (viewScheduleDisplay.WeekNumbersDis ?? string.Empty)
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim());

                    viewScheduleDisplay.IsWeek1 = weeks.Contains("1");
                    viewScheduleDisplay.IsWeek2 = weeks.Contains("2");
                    viewScheduleDisplay.IsWeek3 = weeks.Contains("3");
                    viewScheduleDisplay.IsWeek4 = weeks.Contains("4");
                    viewScheduleDisplay.IsWeek5 = weeks.Contains("5");

                    // add list
                    ScheduledDisplayList.Add(viewScheduleDisplay);

                }

            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSch"),ex.Message);

            }
            finally
            {
                IsBusy = false;

            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchLoadedDisplay"),ScheduledDisplayList.Count);
            OnPropertyChanged(nameof(StatusMessageSch));
        }

        public async Task LoadDoctorScheduledViewAsync()
        {
            IsBusy = true;
            ScheduledDisplayList.Clear();
            try
            {
                _allScheduledDisplay = await Task.Run(() => _scheduledDoctorRepository.GetAllScheduledDoctorsView());

                foreach (var viewScheduleDisplay in _allScheduledDisplay)
                {
                    // Parse WeekNumbersDis string into booleans
                    var weeks = (viewScheduleDisplay.WeekNumbersDis ?? string.Empty)
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim());

                    viewScheduleDisplay.IsWeek1 = weeks.Contains("1");
                    viewScheduleDisplay.IsWeek2 = weeks.Contains("2");
                    viewScheduleDisplay.IsWeek3 = weeks.Contains("3");
                    viewScheduleDisplay.IsWeek4 = weeks.Contains("4");
                    viewScheduleDisplay.IsWeek5 = weeks.Contains("5"); // this line ws missing

                    // add list
                    ScheduledDisplayList.Add(viewScheduleDisplay);
                }

                
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSch"), ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchLoadedDisplay"), ScheduledDisplayList.Count);
            OnPropertyChanged(nameof(StatusMessageSch));
        }
        //###############

        public void LoadDoctorsList()
        {
            IsBusy = true;
            DoctorsListforScheduled.Clear();
            try
            {
                var doctors = _doctorsRepository.GetAllActiveDoctors();
                foreach (var doc in doctors)
                {
                    DoctorsListforScheduled.Add(doc);
                }
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchDoctorsError"),ex.Message);
                Logger.LogError(ex, "Error loading Doctors, Scheduled Doctor Box:");
                IsBusy = false;
                OnPropertyChanged(nameof(StatusMessageSch));
            }
        }

        public async Task LoadDoctorsListAsync()
        {
            IsBusy = true;
            DoctorsListforScheduled.Clear();
            try
            {
                var doctors = await Task.Run(() => _doctorsRepository.GetAllActiveDoctors());
                foreach (var doc in doctors)
                {
                    DoctorsListforScheduled.Add(doc);
                }
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchDoctorsError"), ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
            OnPropertyChanged(nameof(StatusMessageSch));
        }

        public async void SaveScheduledDoctor()
        {

            string textARNotSpecified = "غير محدد";
            string textENNotSpecified = "Not specified";
            string textFRNotSpecified = "Non spécifié";

            if (DoctorId <= 0 || string.IsNullOrWhiteSpace(DayOfTheWeek))
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchDoctorDayRequired");
                return;
            }

            var weekCsv = BuildWeekNumbersCsv();
            if (string.IsNullOrWhiteSpace(weekCsv))
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchDoctorDayRequired");
                return;
            }

            // Parse selected weeks
            var selectedWeeks = weekCsv
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .ToList();

            // Validation: Doctor + DayOfWeek + overlapping weeks with different time
            var existingSchedule = ScheduledDisplayList.FirstOrDefault(s =>
                s.DoctorIdDis == DoctorId &&
                s.TimeDis != DisTime &&
                !string.IsNullOrWhiteSpace(s.DayOfWeekDis) &&
                s.DayOfWeekDis.Equals(DayOfTheWeek, StringComparison.OrdinalIgnoreCase) &&
                (s.WeekNumbersDis ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .Intersect(selectedWeeks)
                    .Any()
            );

            if (existingSchedule != null)
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchOverlap"),existingSchedule.ScheduleAutoIdDis);
                return;
            }

            var SelecteDoctorName = DoctorsListforScheduled.FirstOrDefault(r => r.DoctorId == DoctorId)?.FullName;
            var matchedRole = DoctorsListforScheduled.FirstOrDefault(r => r.DoctorId == DoctorId);

            //  Check null first and Get the time for each schudel
            if (DisTimeFrom == null || DisTimeTo == null)
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                string fromText = DisTimeFrom.HasValue
                    ? TimeOnly.FromTimeSpan(DisTimeFrom.Value).ToString("hh:mm tt") 
                    : "Not specified";

                StatusMessageSch = string.Format(_loader.GetString("SchDp_TimeStampCannotBeNull"), $"From: {fromText}");
                return;
            }
            else if (DisTimeFrom >= DisTimeTo)
            {

                StatusColor = new SolidColorBrush(Colors.OrangeRed);

                string fromText = DisTimeFrom.HasValue
                    ? TimeOnly.FromTimeSpan(DisTimeFrom.Value).ToString("hh:mm tt")
                    : "Not specified";

                string toText = DisTimeTo.HasValue
                    ? TimeOnly.FromTimeSpan(DisTimeTo.Value).ToString("hh:mm tt")
                    : "Not specified";

                StatusMessageSch = string.Format(
                    _loader.GetString("SchDp_TimeStampIsSameOrInvalid"),
                    fromText,
                    toText
                );
                return;
            }

            var fromDate = DateTime.Today.Add(DisTimeFrom!.Value);
            var toDate = DateTime.Today.Add(DisTimeTo!.Value);
            DisTime = $"{fromDate:hh\\:mm tt}-{toDate:hh\\:mm tt}";


            var scheduleModel = new ScheduledDoctor
            {
                ScheduleId = GenerateNextSchedeledID(),
                DoctorId = (int)DoctorId!,
                DayOfWeek = DayOfTheWeek,
                Notify = CanNotify,
                IsActive = IsScheduleActiveDis,
                WeekNumbers = weekCsv,
                DisTime = DisTime
            };

            //Debug.WriteLine($"Time To Display: {DisTime}");

            var success = _scheduledDoctorRepository.SaveScheduledDoctor(scheduleModel);
            if (success)
            {
                ScheduledList.Add(scheduleModel);

                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleAdded");
            }

            try
            {
                await Task.Delay(3000, _cts.Token);
                ClearScheduledFields();
            }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving schedule.");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleAddedError");
            }
            finally
            {
                _cts.TryReset();
            }
        }

        public async void UpdateScheduledDoctor()
        {
            if (SelectedScheduledDisplay == null)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchNoScheduleForUpdate");
                return;
            }

            // Validate weeks
            var weekCsv = BuildWeekNumbersCsv();
            if (string.IsNullOrWhiteSpace(weekCsv))
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchWeekRequired");
                return;
            }

            // Parse selected weeks
            var selectedWeeks = weekCsv
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .ToList();

            // 1. Validation: Doctor + DayOfWeek + overlapping weeks (exclude current schedule)
            var existingSchedule = ScheduledDisplayList.FirstOrDefault(s =>
                s.DoctorIdDis == DoctorId &&
                !string.IsNullOrWhiteSpace(s.DayOfWeekDis) &&
                s.DayOfWeekDis.Equals(DayOfTheWeek, StringComparison.OrdinalIgnoreCase) &&
                s.ScheduleAutoIdDis != SelectedScheduledDisplay.ScheduleAutoIdDis && // exclude current
                (s.WeekNumbersDis ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .Intersect(selectedWeeks)
                    .Any()
            );

            if (existingSchedule != null)
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_StatusMessageSchOverlap"),existingSchedule.ScheduleAutoIdDis);
                return;
            }


            // 2. check if null first and update Get the time for each schudel
            if (DisTimeFrom == null || DisTimeTo == null) // in case this value is 'Not Set"
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_TimeStampCannotBeNull"), $"From {DisTimeFrom} - To {DisTimeTo}");
                return;

            }
            // 3. Check if the time is invalid
            if (DisTimeFrom >= DisTimeTo)
            {
                StatusColor = new SolidColorBrush(Colors.OrangeRed);
                StatusMessageSch = string.Format(_loader.GetString("SchDp_TimeStampIsSameOrInvalid"), $"From {DisTimeFrom} - To {DisTimeTo}");
                return;
            }

            var fromDate = DateTime.Today.Add(DisTimeFrom.Value);
            var toDate = DateTime.Today.Add(DisTimeTo.Value);
            DisTime = $"{fromDate:hh\\:mm tt}-{toDate:hh\\:mm tt}";

            // Map display → entity
            SelectedScheduled = MapToScheduledDoctor(SelectedScheduledDisplay);

            // Update with current ViewModel values
            SelectedScheduled!.DoctorId = (int)DoctorId!;
            SelectedScheduled.DayOfWeek = DayOfTheWeek;
            SelectedScheduled.Notify = CanNotify;
            SelectedScheduled.IsActive = IsScheduleActiveDis;
            SelectedScheduled.WeekNumbers = weekCsv;
            SelectedScheduled.DisTime = DisTime;

            // Update Value
            var success = _scheduledDoctorRepository.UpdateScheduledDoctor(SelectedScheduled);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleUpdated");
            }

            try
            {
                await Task.Delay(2000, _cts.Token);
                ClearScheduledFields();
            }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating schedule.");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleUpdatedError");
            }
            finally
            {
                _cts.TryReset();
            }

            OnPropertyChanged(nameof(StatusMessageSch));
        }

        public async void DeactivateScheduledDoctor()
        {
            if (SelectedScheduledDisplay == null || SelectedScheduledDisplay.ScheduleAutoIdDis <= 0)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchNoScheduleForDeactivation");
                return;
            }


            // Map display → entity
            SelectedScheduled = MapToScheduledDoctor(SelectedScheduledDisplay!);

            SelectedScheduled!.IsActive = false;

            var success = _scheduledDoctorRepository.DeactivateScheduledDoctor(SelectedScheduled);
            if (success)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleDeactivated");
            }

            try
            {
                await Task.Delay(2000, _cts.Token);
                ClearScheduledFields();
            }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deactivating schedule.");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessageSch = _loader.GetString("SchDp_StatusMessageSchScheduleDeactivatedError");
            }
            finally
            {
                _cts.TryReset();
            }

            OnPropertyChanged(nameof(StatusMessageSch));          
        }

        public async Task InitializeAsync()
        {
            //ClearScheduledFields();
            await LoadDoctorScheduledViewAsync();
            await LoadDoctorsListAsync();
            await GetHiriRomanDate();
        }

        public void ClearScheduledFields()
        {
            ScheduleId = string.Empty;
            DoctorId = 0;
            DayOfTheWeek = string.Empty;
            CanNotify = true;
            IsScheduleActiveDis = true;
            IsWeek1 = IsWeek2 = IsWeek3 = IsWeek4 = IsWeek5 = false;
            DisTimeFrom = null;
            DisTimeTo = null;
            StatusMessageSch = string.Empty;
            DoctorsListforScheduled.Clear();
            LoadDoctorScheduledView();
            LoadDoctorsList();
        }


        public string GenerateNextSchedeledID()
        {
            var Schedled = _scheduledDoctorRepository.GetAllScheduledDoctors();
            var lastId = Schedled
                .Select(u => u.ScheduleId)
                .Where(id => id!.StartsWith("SCHDL"))
                .Select(id => int.Parse(id!.Substring(5)))
                .DefaultIfEmpty(0)
                .Max();

            int nextId = lastId + 1;
            return $"SCHDL{nextId:D4}"; // D4 for zero-padded to 4 digits like LC0001
        }

        public void OnNavigatedFrom()
        {
            ClearScheduledMemory();
        }


        // Filter method
        private void ApplyScheduleFilter()
        {
            if (_allScheduledDisplay == null) return;

            var query = ScheduleFilter?.Trim().ToLower();
            ScheduledDisplayList.Clear();

            IEnumerable<ScheduledDoctorDisplayModel> filtered;

            if (string.IsNullOrEmpty(query))
            {
                filtered = _allScheduledDisplay;
            }
            else
            {
                filtered = _allScheduledDisplay.Where(s =>
                    s.ScheduleAutoIdDis.ToString().Contains(query) ||
                    (!string.IsNullOrEmpty(s.ScheduleIdDis) && s.ScheduleIdDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.DoctorCodeDis) && s.DoctorCodeDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.FullNameDis) && s.FullNameDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.SpecializationDis) && s.SpecializationDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.PhoneNumberDis) && s.PhoneNumberDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.DayOfWeekDis) && s.DayOfWeekDis.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (s.DoctorIdDis.ToString().Contains(query))
                );
            }

            foreach (var schedule in filtered)
                ScheduledDisplayList.Add(schedule);
        }

        /// <summary>
        /// Checks if the doctor already has a schedule for the same day
        /// and at least one overlapping week.
        /// </summary>
        private bool DoctorHasExistingSchedule(int doctorId, string dayOfWeek, IEnumerable<int> selectedWeeks)
        {
            if (_allScheduledDisplay == null || !_allScheduledDisplay.Any())
                return false;

            foreach (var existing in _allScheduledDisplay)
            {
                // Match doctor and day
                if (existing.DoctorIdDis == doctorId &&
                    !string.IsNullOrWhiteSpace(existing.DayOfWeekDis) &&
                    existing.DayOfWeekDis.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase))
                {
                    // Parse existing weeks
                    var existingWeeks = (existing.WeekNumbersDis ?? string.Empty)
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => int.TryParse(w, out var num) ? num : -1)
                        .Where(num => num > 0);

                    // Check if any overlap with selected weeks
                    if (existingWeeks.Intersect(selectedWeeks).Any())
                    {
                        return true; // Found conflict
                    }
                }
            }

            return false; // No conflict
        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearScheduledMemory()
        {
            ScheduleId = string.Empty;
            DoctorId = 0;
            DoctorCode = string.Empty;
            DoctorName = string.Empty;
            Specialization = string.Empty;
            PhoneNumber = string.Empty;
            DayOfTheWeek = string.Empty;

            CanNotify = true;
            IsScheduleActiveDis = true;
            IsWeek1 = IsWeek2 = IsWeek3 = IsWeek4 = IsWeek5 = false;
            DisTimeFrom = null;
            DisTimeTo = null;

            StatusMessageSch = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            SelectedScheduled = null;
            SelectedScheduledDisplay = null;

            ScheduledList.Clear();
            ScheduledDisplayList.Clear();
            DoctorsListforScheduled.Clear();
            _allScheduled.Clear();
            _allScheduledDisplay.Clear();

            _cts.Cancel();   // stop any pending delays
        }
    }
}
