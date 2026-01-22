using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorScheduleViewRow
    {
        public int ScheduleAutoId { get; set; }
        public string? ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public string? FullName { get; set; }
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }
        public string? DayOfWeek { get; set; }          // e.g., "Monday"
        public bool Notify { get; set; }
        public bool ScheduleIsActive { get; set; }
        public bool DoctorIsActive { get; set; }
        public string? WeekNumbers { get; set; }        // e.g., "1,3,4"
    }
}
