using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using System.Threading.Tasks;

namespace LiteClinic.ViewModels
{
    public partial class DoctorDirectoryPageViewModel : INotifyPropertyChanged
    {

        // add delay time to unfreez the UI
        private readonly DispatcherTimer? _debounceTimer;  // private field because it’s not meant to be exposed or bound.

        public IAsyncRelayCommand RefreshCommandAsync { get; }

        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

        private string? _hijriDate;
        private string? _romanDate;
        private string? _statusMessage;
        private SolidColorBrush _statusColor = new(Colors.Black);


        public IAsyncRelayCommand? Btn_OpenAppointmentPage { get; }
        public IAsyncRelayCommand? Btn_OpenProfilePage { get; }
        public IAsyncRelayCommand? Btn_CloseProfilePage { get; }

        public DoctorDirectoryPageViewModel()
        {
            RefreshCommandAsync = new AsyncRelayCommand(ResetAllFilters);

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // pause time
            };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                _ = ApplyFilter(); // run filter after pause
            };

            Btn_OpenAppointmentPage = new AsyncRelayCommand(OpenAppointmentPageAsync);
            Btn_OpenProfilePage = new AsyncRelayCommand<DoctorCardModel>(OpenProfilePageAsync);
            Btn_CloseProfilePage = new AsyncRelayCommand(CloseProfilePageAsync);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Keep a master list of all doctors
        private List<DoctorCardModel> _allDoctors = [];

        // Doctor cards collection
        private ObservableCollection<DoctorCardModel> _doctorCards = [];
        public ObservableCollection<DoctorCardModel> DoctorCards
        {
            get => _doctorCards;
            set
            {
                _doctorCards = value;
                OnPropertyChanged(nameof(DoctorCards));
            }
        }

        private ObservableCollection<string> _specializations = [];
        public ObservableCollection<string> Specializations
        {
            get => _specializations;
            set
            {
                _specializations = value;
                OnPropertyChanged(nameof(Specializations));
            }
        }

        //Fil;er by ComboBox
        private string? _selectedSpecialization;
        public string? SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                _selectedSpecialization = value;
                OnPropertyChanged(nameof(SelectedSpecialization));
                if(!_isResettingFilter)
                    _ = ApplyFilter(); // trigger filtering logic
            }
        }

        // Search query (by name/specialty)
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                if (!_isResettingFilter)
                {
                    _debounceTimer?.Stop();
                    _debounceTimer?.Start();
                }
            }
        }

        private string _selectedDay = string.Empty;
        public string SelectedDay
        {
            get => _selectedDay;
            set
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
                // trigger filtering logic here
            }
        }

        private string _attendingDays = string.Empty;

        public string AttendingDays
        {
            get => _attendingDays;
            set
            {
                _attendingDays = value;
                OnPropertyChanged(nameof(AttendingDays));
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


        private string? _profilePicturePath;
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

        private string? _gender;
        public string? Gender
        {
            get => _gender;
            set
            {
                if (value != _gender)
                {
                    _gender = value;
                    OnPropertyChanged(nameof(Gender));
                }
            }
        }

        private string? _initials;

        public string? Initials
        {
            get => _initials;
            set
            {
                _initials = value;
                OnPropertyChanged(nameof(Initials));
            }
        }


        public string? StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); }
        }

        // reset filters
        private bool _isResettingFilter = false;

        // change the back ground if using filter for doctors

        private Brush _dayBackground = new SolidColorBrush(Colors.Transparent);  // #D0F0F2



        public Brush DayBackground
        {
            get => _dayBackground;
            set
            {
                _dayBackground = value;
                OnPropertyChanged(nameof(DayBackground));
            }
        }

        //Toggle Buttons
        private bool _isMorningSelected;
        public bool IsMorningSelected
        {
            get => _isMorningSelected;
            set
            {
                _isMorningSelected = value;
                OnPropertyChanged(nameof(IsMorningSelected));
                if (!_isResettingFilter)
                    _ = ApplyFilter();
            }
        }

        private bool _isAfternoonSelected;
        public bool IsAfternoonSelected
        {
            get => _isAfternoonSelected;
            set
            {
                _isAfternoonSelected = value;
                OnPropertyChanged(nameof(IsAfternoonSelected));
                if (!_isResettingFilter)
                    _ = ApplyFilter();
            }
        }

        private bool _isEveningSelected;
        public bool IsEveningSelected
        {
            get => _isEveningSelected;
            set
            {
                _isEveningSelected = value;
                OnPropertyChanged(nameof(IsEveningSelected));
                if (!_isResettingFilter)
                    _ = ApplyFilter();
            }
        }

        private bool _isTodaySelected;
        public bool IsTodaySelected
        {
            get => _isTodaySelected;
            set
            {
                _isTodaySelected = value;
                OnPropertyChanged(nameof(IsTodaySelected));
                if (!_isResettingFilter)
                    _ = ApplyFilter();
            }
        }

        private DoctorCardModel? _selectedDoctor;
        public DoctorCardModel? SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                if (_selectedDoctor != value)
                {
                    _selectedDoctor = value;
                    OnPropertyChanged(nameof(SelectedDoctor));
                }
            }
        }

        private bool _isDoctorProfilePopupOpen;
        public bool IsDoctorProfilePopupOpen
        {
            get => _isDoctorProfilePopupOpen;
            set
            {
                if (_isDoctorProfilePopupOpen != value)
                {
                    _isDoctorProfilePopupOpen = value;
                    OnPropertyChanged(nameof(IsDoctorProfilePopupOpen));
                }
            }
        }


        //----------------------------------
        // ## Calculate the Popup Keep them not static for ynamic changes
        //----------------------------------
        public double PopupActualWidth => App.GlobalState.PopupActualWidth;

        public double PopupActualHeight => App.GlobalState.PopupActualHeight;

        // Define standard time ranges for filtering
        private readonly (TimeSpan Start, TimeSpan End) _morningRange =
            (TimeSpan.FromHours(8), TimeSpan.FromHours(12));

        private readonly (TimeSpan Start, TimeSpan End) _afternoonRange =
            (TimeSpan.FromHours(12), TimeSpan.FromHours(17));

        private readonly (TimeSpan Start, TimeSpan End) _eveningRange =
            (TimeSpan.FromHours(17), TimeSpan.FromHours(22));


        public async Task InitializeAsync()
        {
            await LoadDoctorCardsAsync();
            await GetHiriRomanDate();
            //ResetAllFilters();
        }

        private async Task LoadDoctorCardsAsync()
        {
            try
            {
                var repo = new ScheduledDoctorRepository(); // non-static instance
                var doctorSchedules = await repo.GetAllDoctorCardsViewAsync();

                var groupedDoctors = doctorSchedules
                    .GroupBy(d => new { d.DoctorId, d.FullName, d.Specialization })
                    .Select(g => new DoctorCardModel
                    {
                        DoctorId = g.Key.DoctorId,
                        FullName = g.Key.FullName,
                        Specialization = g.Key.Specialization,

                        // Distinct days (for quick filter display)
                        AttendingDays = string.Join(", ", g.Select(x => x.AttendingDays).Distinct()),

                        // Flat string for filtering (combine all slots)
                        TimeSlots = string.Join(";", g.Select(x => x.TimeSlots)),

                        // Structured DaySlots for UI
                        DaySlots = new ObservableCollection<DaySlotModel>(
                            g.GroupBy(x => x.AttendingDays)
                             .Select(dayGroup => new DaySlotModel
                             {
                                 DaySelected = dayGroup.Key,
                                 SlotsSelected = new ObservableCollection<SlotModel>(
                                     dayGroup.Select(x => new SlotModel
                                     {
                                         TimeSelected = x.TimeSlots
                                     })
                                 )
                             })
                        ),

                        // 👇 Preserve doctor details from the first record in the group
                        ProfilePicturePath = g.First().ProfilePicturePath,
                        Gender = g.First().Gender,
                        PhoneNumber = g.First().PhoneNumber,
                        LandLineNumber = g.First().LandLineNumber,
                        Initials = g.First().Initials
                    })
                    .ToList();

                // Populate unique specializations from active doctors
                Specializations = new ObservableCollection<string>(
                    groupedDoctors
                        .Select(d => d.Specialization)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .OrderBy(s => s)
                );

                // Insert a reset option at the top
                Specializations.Insert(0, "All");

                DoctorCards = new ObservableCollection<DoctorCardModel>(groupedDoctors);
                _allDoctors = groupedDoctors; // Keep a master list for filtering

                await HighlightToday();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error retrieving doctor cards. {GetType().Name}");
                // Debug.WriteLine($"Error loading doctor cards: {ex.Message}");
            }
        }


        private async Task ApplyFilter()
        {
            try
            {
                var query = SearchQuery;                     // capture current text
                var specialization = SelectedSpecialization; // capture selected specialization

                var filtered = await Task.Run(() =>
                {
                    IEnumerable<DoctorCardModel> doctors = _allDoctors;

                    // Search filter
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        doctors = doctors.Where(d =>
                            (!string.IsNullOrEmpty(d.FullName) &&
                             d.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||

                            (!string.IsNullOrEmpty(d.Specialization) &&
                             d.Specialization.Contains(query, StringComparison.OrdinalIgnoreCase))
                        );
                    }

                    // Specialization filter (skip if "All" or null)
                    if (!string.IsNullOrWhiteSpace(specialization) && specialization != "All")
                    {
                        doctors = doctors.Where(d =>
                            !string.IsNullOrEmpty(d.Specialization) &&
                            d.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase)
                        );
                    }

                    if (IsMorningSelected || IsAfternoonSelected || IsEveningSelected || IsTodaySelected)
                    {
                        doctors = doctors.Where(d =>
                        {
                            bool result = false;

                            // Time-of-day filters
                            if (!string.IsNullOrEmpty(d.TimeSlots) &&
                                (IsMorningSelected || IsAfternoonSelected || IsEveningSelected))
                            {
                                result = MatchesTimeFilter(d.TimeSlots);
                            }

                            // Today filter (check DaySlots or AttendingDays)
                            if (IsTodaySelected)
                            {
                                var today = DateTime.Today.DayOfWeek.ToString(); // e.g. "Friday"

                                // Option 1: check AttendingDays string
                                if (!string.IsNullOrEmpty(d.AttendingDays) &&
                                    d.AttendingDays.Contains(today, StringComparison.OrdinalIgnoreCase))
                                {
                                    result = true;
                                }

                                // Option 2 (cleaner): check DaySlots collection
                                if (d.DaySlots.Any(ds => ds.DaySelected.Equals(today, StringComparison.OrdinalIgnoreCase)))
                                {
                                    result = true;
                                }
                            }
                            return result;
                        });                        
                    }


                    var finalList = doctors.ToList();
                    return finalList;
                });

                DoctorCards = new ObservableCollection<DoctorCardModel>(filtered);

                // Highlight logic
                if (!string.IsNullOrWhiteSpace(query) ||
                    (!string.IsNullOrWhiteSpace(specialization) && specialization != "All"))
                {
                    await HighlightFilteredDoctors();
                }
                else
                {
                    await HighlightToday();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error retrieving doctor cards. {GetType().Name}");
            }
        }

        private async Task HighlightToday()
        {
            var today = DateTime.Today.DayOfWeek.ToString();

            foreach (var doctor in DoctorCards)
            {
                foreach (var daySlot in doctor.DaySlots)
                {
                    // Reset all slots first
                    daySlot.DayForeground = new SolidColorBrush(Colors.Black);
                    daySlot.DayFontWeight = FontWeights.Normal;

                    foreach (var timeSlot in daySlot.SlotsSelected)
                    {
                        timeSlot.SlotForeground = new SolidColorBrush(Colors.Black);
                        timeSlot.SlotFontWeight = FontWeights.Normal;
                    }

                    // Highlight today's slot + its time slots
                    if (daySlot.DaySelected == today)
                    {
                        daySlot.DayForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 128, 128)); // Teal
                        daySlot.DayFontWeight = FontWeights.Bold;

                        foreach (var timeSlot in daySlot.SlotsSelected)
                        {
                            timeSlot.SlotForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 128, 128)); // Teal
                            timeSlot.SlotFontWeight = FontWeights.Bold;
                        }
                    }
                }
            }

            await Task.CompletedTask; // keeps async signature consistent
        }

        //Highlights doctors in the UI when any filter(search, specialization, or time toggles) is active
        private async Task HighlightFilteredDoctors()
        {
            var today = DateTime.Today.DayOfWeek.ToString();

            if (HasActiveFilter())
            {
                // Apply filter highlight
                foreach (var doctor in DoctorCards)
                {
                    // Reset all slots first
                    foreach (var daySlot in doctor.DaySlots)
                    {
                        daySlot.DayForeground = new SolidColorBrush(Colors.Black);
                        daySlot.DayFontWeight = FontWeights.Normal;

                        foreach (var timeSlot in daySlot.SlotsSelected)
                        {
                            timeSlot.SlotForeground = new SolidColorBrush(Colors.Black);
                            timeSlot.SlotFontWeight = FontWeights.Normal;
                        }
                    }

                    // Highlight only today's slot if doctor matches filter
                    if (doctor.FullName != null && doctor.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                        doctor.Specialization == SelectedSpecialization)
                    {
                        foreach (var daySlot in doctor.DaySlots)
                        {
                            if (daySlot.DaySelected == today)
                            {
                                daySlot.DayForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 65, 105, 225)); // Royal Blue
                                daySlot.DayFontWeight = FontWeights.Bold;

                                foreach (var timeSlot in daySlot.SlotsSelected)
                                {
                                    timeSlot.SlotForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 65, 105, 225)); // Royal Blue
                                    timeSlot.SlotFontWeight = FontWeights.Bold;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // No filter → just highlight today
                await HighlightToday();
            }

            await Task.CompletedTask;
        }


        // You trigger this method (e.g., from a "Clear Filters" Button click)
        public async Task ResetAllFilters()
        {
            _isResettingFilter = true; // Set the flag to prevent ApplyFilter from being called during reset
            SearchQuery = string.Empty;
            SelectedSpecialization = null; // ""All" or null if you prefer shoe place holder
            IsMorningSelected = false;
            IsAfternoonSelected = false;
            IsEveningSelected = false;
            IsTodaySelected = false;

            _isResettingFilter = false; // Reset the flag after clearing filters
            await ApplyFilter(); // Reapply filter to show all doctors
        }


        private static List<(TimeSpan Start, TimeSpan End)> ParseTimeRanges(string timeFromTo)
        {
            var ranges = new List<(TimeSpan, TimeSpan)>();

            var parts = timeFromTo.Split(
                ['\n', ';'],
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var times = part.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (times.Length == 2)
                {
                    if (DateTime.TryParse(times[0].Trim(), out var startTime) &&
                        DateTime.TryParse(times[1].Trim(), out var endTime))
                    {
                        ranges.Add((startTime.TimeOfDay, endTime.TimeOfDay));
                    }
                }
            }

            return ranges;
        }


        private bool MatchesTimeFilter(string timeFromTo)
        {
            //Debug.WriteLine($"MatchesTimeFilter called with: {timeFromTo}");

            var ranges = ParseTimeRanges(timeFromTo);

            foreach (var (start, end) in ranges)
            {
                //Debug.WriteLine($"Parsed range: {start} - {end}");

                if (IsMorningSelected && start < _morningRange.End && end > _morningRange.Start)
                {
                    //Debug.WriteLine("Matched Morning");
                    return true;
                }

                if (IsAfternoonSelected && start < _afternoonRange.End && end > _afternoonRange.Start)
                {
                    //Debug.WriteLine("Matched Afternoon");
                    return true;
                }

                if (IsEveningSelected && start < _eveningRange.End && end > _eveningRange.Start)
                {
                    //Debug.WriteLine("Matched Evening");
                    return true;
                }

                if (IsTodaySelected)
                {
                    //Debug.WriteLine("Matched Today (placeholder)");
                    return true;
                }
            }

            return false;
        }
        // Checks if any filter (search, specialization, or time toggles) is active
        private bool HasActiveFilter()
        {
            return !string.IsNullOrEmpty(SearchQuery)
                || !string.IsNullOrWhiteSpace(SelectedSpecialization)
                || IsMorningSelected
                || IsAfternoonSelected
                || IsEveningSelected
                || IsTodaySelected;
        }
        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearDoctorsMemory()
        {
            _allDoctors = [];
            _doctorCards = [];
            _specializations = [];
            _selectedSpecialization = string.Empty;
            _searchQuery = string.Empty;
            _selectedDay = string.Empty;
            _attendingDays = string.Empty;
            _profilePicturePath = string.Empty;
            _gender = string.Empty;
            _initials = string.Empty;
            _statusMessage = string.Empty;
            _isMorningSelected = false;
            _isAfternoonSelected = false;
            _isEveningSelected = false;
            _isTodaySelected = false;

        }

        private async Task OpenAppointmentPageAsync()
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(AppointmentsPage));
            App.GlobalState.UpdateSubtitle("Appointments/Text");
            await Task.CompletedTask; // optional, can be removed
        }

        private async Task OpenProfilePageAsync(DoctorCardModel? doctor)
        {
            // Set the selected doctor so the Popup binds correctly
            if (doctor is null) return;

            SelectedDoctor = doctor;

            // Open the Popup (you can access it via code-behind)
            IsDoctorProfilePopupOpen = true;

            //App.GlobalState.UpdateSubtitle("DoctorsScheduled/Text");
            await Task.CompletedTask; // optional
        }

        private async Task CloseProfilePageAsync()
        {
            // Reset the selected doctor so the Popup binds correctly
            SelectedDoctor = null;

            // Open the Popup (you can access it via code-behind)
            IsDoctorProfilePopupOpen = false;

            await Task.CompletedTask; // optional
        }

    }
}
