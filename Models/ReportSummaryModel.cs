using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class ReportSummaryModel
    {
        // Time Filter
        public DateTime ReferenceDate { get; set; }
        public ReportPeriod Period { get; set; } // Enum: Day, Week, Month, Year

        // Summary Metrics
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalMissedAppointments { get; set; }

        // Breakdown by period
        public int DoctorsToday { get; set; }
        public int DoctorsThisWeek { get; set; }
        public int DoctorsThisMonth { get; set; }
        public int DoctorsThisYear { get; set; }

        // Patient breakdown
        public int PatientsToday { get; set; }
        public int PatientsThisWeek { get; set; }
        public int PatientsThisMonth { get; set; }
        public int PatientsThisYear { get; set; }

        // Missed breakdown
        public int MissedToday { get; set; }
        public int MissedThisWeek { get; set; }
        public int MissedThisMonth { get; set; }
        public int MissedThisYear { get; set; }

        // New explicit role count properties
        public int AdministratorCount { get; set; }
        public int HRCount { get; set; }
        public int ModeratorCount { get; set; }
        public int DoctorCount { get; set; }
        public int PharmacistCount { get; set; }
        public int NurseCount { get; set; }
        public int ReceptionistCount { get; set; }
        public int GuestCount { get; set; }
        public string MostCommonRole { get; set; } = string.Empty;

        public List<DoctorStatsModel> TopDoctors { get; set; } = [];

        // 👥 Role-Based User Counts (Dynamic)
        public Dictionary<string, int> RoleCounts { get; set; } = [];
        public int TotalUserRoleCount => RoleCounts.Count;

        // 🆕 Add Top Specializations
        public List<SpecializationStatsModel> TopSpecializations { get; set; } = [];

    }
}
