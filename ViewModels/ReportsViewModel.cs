using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.ViewModels
{
    public partial class ReportsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // 1. Scheduled Appointments
        public ObservableCollection<ScheduledAppointmentDisplay>? ScheduledAppointments { get; set; }

        // 2. Doctor Appointments
        public ObservableCollection<ScheduledDoctorDisplayModel>? DoctorAppointments { get; set; }

        // 3. Users with Roles (from view V)
        public ObservableCollection<UsersWithRolesDisplay>? UsersWithRoles { get; set; }

        private readonly AppointmentsRepository _viewappointmentsRepository = new();
        private readonly ScheduledDoctorRepository _viewdoctorsRepository = new();
        private readonly UserRepository _viewuserRepository = new();

        private List<ScheduledAppointmentDisplay>? _parentSchedulView = [];
        private List<ScheduledDoctorDisplayModel>? _parentDoctorView = [];
        private List<UsersWithRolesDisplay>? _parentUserView = [];
        public List<SpecializationStatsModel> TopSpecializations { get; set; } = [];

        private readonly ResourceLoader _loader = new();

        public ReportsViewModel()
        {
            // Set default titles (can be localized)
            _topDoctorTitle = string.Format(_loader.GetString("Rpg_TopDoctorTitle/Text"), DateTime.Now.Year);
            _topSpecializationsTitle = string.Format(_loader.GetString("Rpg_TopSpecializationsTitle/Text"), DateTime.Now.Year);
        }

        // Load Doctor Appointments
        public async Task LoadDoctorAppointments()
        {
            _parentDoctorView = await Task.Run(() => _viewdoctorsRepository.GetDoctorSchedules());
            DoctorAppointments = new ObservableCollection<ScheduledDoctorDisplayModel>(_parentDoctorView);
            OnPropertyChanged(nameof(DoctorAppointments));
        }

        // Load Scheduled Appointments
        public async Task LoadScheduledAppointments()
        {
            _parentSchedulView = await Task.Run(() => _viewappointmentsRepository.GetDisplayedAppointments());
            ScheduledAppointments = new ObservableCollection<ScheduledAppointmentDisplay>(_parentSchedulView);
            OnPropertyChanged(nameof(ScheduledAppointments));
        }

        // Load Users with Roles
        public async Task LoadUsersWithRoles()
        {
            _parentUserView = await Task.Run(() => _viewuserRepository.GetUsersWithRoles());
            UsersWithRoles = new ObservableCollection<UsersWithRolesDisplay>(_parentUserView);
            OnPropertyChanged(nameof(UsersWithRoles));
        }

        // Optional: Load everything at once
        public async Task InitializeAsync()
        {
            await LoadScheduledAppointments();
            await LoadDoctorAppointments();
            await LoadUsersWithRoles();
            await LoadReportSummary();
            await LoadTopSpecializations();
            await GetHiriRomanDate();
        }

        // NEW: Summary model
        private ReportSummaryModel _reportSummary = new();
        public ReportSummaryModel ReportSummary
        {
            get => _reportSummary;
            set
            {
                _reportSummary = value;
                OnPropertyChanged(nameof(ReportSummary));
            }
        }

        // Example: set the period filter
        public ReportPeriod CurrentPeriod { get; set; } = ReportPeriod.Year;

        private int _totalPatientsCount;
        public int TotalPatientsCount
        {
            get => _totalPatientsCount;
            set
            {
                _totalPatientsCount = value;
                OnPropertyChanged(nameof(TotalPatientsCount));
            }
        }

        private int _TopSpecializationsCount;
        public int TopSpecializationsCount
        {
            get => _TopSpecializationsCount;
            set
            {
                _TopSpecializationsCount = value;
                OnPropertyChanged(nameof(TopSpecializationsCount));
            }
        }

        private string? _topDoctorTitle;
        public string? TopDoctorTitle
        {
            get => _topDoctorTitle;
            set
            {
                _topDoctorTitle = value;
                OnPropertyChanged(nameof(TopDoctorTitle));
            }
        }

        private string? _topSpecializationsTitle;
        public string? TopSpecializationsTitle
        {
            get => _topSpecializationsTitle;
            set
            {
                _topSpecializationsTitle = value;
                OnPropertyChanged(nameof(TopSpecializationsTitle));
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

        public async Task LoadReportSummary()
        {
            try
            {
                var now = DateTime.Now;

                // Defensive null checks
                var appointments = ScheduledAppointments ?? new ObservableCollection<ScheduledAppointmentDisplay>();
                var users = UsersWithRoles ?? new ObservableCollection<UsersWithRolesDisplay>();

                // Filter attended appointments by selected period
                var attended = await Task.Run(() =>
                    appointments
                        .Where(a => a?.AttendStatus == AttendStatus.Attended)
                        .Where(a =>
                        {
                            if (a == null) return false;
                            return CurrentPeriod switch
                            {
                                ReportPeriod.Day => a.AppointmentDate.Date == now.Date,
                                ReportPeriod.Week => a.AppointmentDate >= now.AddDays(-7),
                                ReportPeriod.Month => a.AppointmentDate.Month == now.Month && a.AppointmentDate.Year == now.Year,
                                ReportPeriod.Year => a.AppointmentDate.Year == now.Year,
                                _ => true
                            };
                        })
                        .ToList()
                );

                var missed = await Task.Run(() =>
                    appointments
                        .Where(a => a?.AttendStatus == AttendStatus.Missed)
                        .Where(a =>
                        {
                            if (a == null) return false;
                            return CurrentPeriod switch
                            {
                                ReportPeriod.Day => a.AppointmentDate.Date == now.Date,
                                ReportPeriod.Week => a.AppointmentDate >= now.AddDays(-7),
                                ReportPeriod.Month => a.AppointmentDate.Month == now.Month && a.AppointmentDate.Year == now.Year,
                                ReportPeriod.Year => a.AppointmentDate.Year == now.Year,
                                _ => true
                            };
                        })
                        .ToList()
                );

                // Build summary safely
                ReportSummary = new ReportSummaryModel
                {
                    PatientsToday = attended.Count(a => a.AppointmentDate.Date == now.Date),
                    PatientsThisWeek = attended.Count(a => a.AppointmentDate >= now.AddDays(-7)),
                    PatientsThisMonth = attended.Count(a => a.AppointmentDate.Month == now.Month && a.AppointmentDate.Year == now.Year),
                    PatientsThisYear = attended.Count(a => a.AppointmentDate.Year == now.Year),

                    MissedToday = missed.Count(a => a.AppointmentDate.Date == now.Date),
                    MissedThisWeek = missed.Count(a => a.AppointmentDate >= now.AddDays(-7)),
                    MissedThisMonth = missed.Count(a => a.AppointmentDate.Month == now.Month && a.AppointmentDate.Year == now.Year),
                    MissedThisYear = missed.Count(a => a.AppointmentDate.Year == now.Year),

                    ReferenceDate = now,
                    Period = CurrentPeriod,

                    TotalDoctors = attended.Select(a => a.DoctorId).Distinct().Count(),
                    DoctorsToday = attended.Where(a => a.AppointmentDate.Date == now.Date).Select(a => a.DoctorId).Distinct().Count(),
                    DoctorsThisWeek = attended.Where(a => a.AppointmentDate >= now.AddDays(-7)).Select(a => a.DoctorId).Distinct().Count(),
                    DoctorsThisMonth = attended.Where(a => a.AppointmentDate.Month == now.Month && a.AppointmentDate.Year == now.Year).Select(a => a.DoctorId).Distinct().Count(),
                    DoctorsThisYear = attended.Where(a => a.AppointmentDate.Year == now.Year).Select(a => a.DoctorId).Distinct().Count(),

                    TotalPatients = attended.Count,
                    TotalMissedAppointments = appointments.Count(a => a?.AttendStatus == AttendStatus.Missed),

                    TopDoctors = attended
                        .GroupBy(a => a.DoctorName)
                        .Select(g => new DoctorStatsModel
                        {
                            DoctorName = g.Key ?? "Unknown",
                            PatientCount = g.Count()
                        })
                        .OrderByDescending(x => x.PatientCount)
                        .Take(3)
                        .ToList(),

                    RoleCounts = users
                        .Where(u => !string.IsNullOrWhiteSpace(u.RoleName))
                        .GroupBy(u => u.RoleName!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // Populate explicit role counts safely
                ReportSummary.AdministratorCount = ReportSummary.RoleCounts.TryGetValue("Administrator", out var admin) ? admin : 0;
                ReportSummary.HRCount = ReportSummary.RoleCounts.TryGetValue("HR", out var hr) ? hr : 0;
                ReportSummary.ModeratorCount = ReportSummary.RoleCounts.TryGetValue("Moderator", out var mod) ? mod : 0;
                ReportSummary.DoctorCount = ReportSummary.RoleCounts.TryGetValue("Doctor", out var doc) ? doc : 0;
                ReportSummary.PharmacistCount = ReportSummary.RoleCounts.TryGetValue("Pharmacist", out var pharm) ? pharm : 0;
                ReportSummary.NurseCount = ReportSummary.RoleCounts.TryGetValue("Nurse", out var nurse) ? nurse : 0;
                ReportSummary.ReceptionistCount = ReportSummary.RoleCounts.TryGetValue("Receptionist", out var rec) ? rec : 0;
                ReportSummary.GuestCount = ReportSummary.RoleCounts.TryGetValue("Guest", out var guest) ? guest : 0;

                // Determine most common role
                ReportSummary.MostCommonRole = ReportSummary.RoleCounts.Count > 0
                    ? $"{ReportSummary.RoleCounts.OrderByDescending(r => r.Value).First().Key} ({ReportSummary.RoleCounts.OrderByDescending(r => r.Value).First().Value})"
                    : "No roles available";

                // Safe Max for TopDoctors
                TotalPatientsCount = ReportSummary.TopDoctors.Count != 0
                    ? ReportSummary.TopDoctors.Max(d => d.PatientCount) + 5
                    : 0;
            }
            catch (Exception ex)
            {
                // Log error and set safe defaults
                Logger.LogError(ex, "Error in ReportsViewModel - LoadReportSummary");
                ReportSummary = new ReportSummaryModel
                {
                    ReferenceDate = DateTime.Now,
                    Period = CurrentPeriod,
                    MostCommonRole = "Error loading summary"
                };
                TotalPatientsCount = 0;
            }
        }

        public async Task LoadTopSpecializations()
        {
            if (ScheduledAppointments == null || ScheduledAppointments.Count == 0)
            {
                ReportSummary.TopSpecializations = [];
                TopSpecializationsCount = 0;
                return;
            }

            // Run filtering and grouping asynchronously
            var specializationStats = await Task.Run(() =>
            {
                var attended = ScheduledAppointments
                    .Where(a => (a.AttendStatus == AttendStatus.Attended) &&
                                !string.IsNullOrWhiteSpace(a.Specialty))
                    .ToList();

                return attended
                    .GroupBy(a => a.Specialty)
                    .Select(g => new SpecializationStatsModel
                    {
                        Specialization = g.Key,
                        PatientCount = g.Count()
                    })
                    .OrderByDescending(x => x.PatientCount)
                    .Take(3)
                    .ToList();
            });

            ReportSummary.TopSpecializations = specializationStats;
            OnPropertyChanged(nameof(ReportSummary));

            TopSpecializationsCount = ReportSummary.TopSpecializations.Any()
                ? ReportSummary.TopSpecializations.Max(d => d.PatientCount) + 5
                : 0;
        }


        public async Task  GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearReportsMemory()
        {
            ScheduledAppointments?.Clear();
            DoctorAppointments?.Clear();
            UsersWithRoles?.Clear();

            _parentSchedulView?.Clear();
            _parentDoctorView?.Clear();
            _parentUserView?.Clear();
            TopSpecializations.Clear();

            ReportSummary = new ReportSummaryModel();
            TotalPatientsCount = 0;
            TopSpecializationsCount = 0;
            TopDoctorTitle = string.Empty;
            TopSpecializationsTitle = string.Empty;

            HijriDate = string.Empty;
            RomanDate = string.Empty;
        }
    }
}

