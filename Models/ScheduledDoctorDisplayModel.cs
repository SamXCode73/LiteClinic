using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class ScheduledDoctorDisplayModel
    {
        // This model is used for UI display only.
        // It combines ScheduledDoctor data with related Doctor details (e.g., PatientFullName, PhoneNumber).
        // Not mapped to the database — used to populate the DataGrid with enriched information.

        public int ScheduleAutoIdDis { get; set; }
        public string? ScheduleIdDis { get; set; }
        public int DoctorIdDis { get; set; }
        public string? DoctorCodeDis { get; set; }
        public string? FullNameDis { get; set; }
        public string? SpecializationDis { get; set; }
        public string? PhoneNumberDis { get; set; }
        public string? LandLineNumber { get; set; }
        public string? DayOfWeekDis { get; set; }
        public bool NotifyDis { get; set; }
        public bool IsScheduleActiveDis { get; set; }
        public bool IsDoctorActiveDis { get; set; }
        public string? WeekNumbersDis { get; set; }
        public bool IsWeek1;
        public bool IsWeek2;
        public bool IsWeek3;
        public bool IsWeek4;
        public bool IsWeek5;
    }
}
