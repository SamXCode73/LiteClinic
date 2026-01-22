using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class ScheduledDoctor
    {
        public int ScheduleAutoId { get; set; }
        public string? ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string? DayOfWeek { get; set; } // e.g., "Monday", "Thursday"
        public bool Notify { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? WeekNumbers { get; set; } = string.Empty;// e.g., "1,3,5" for odd weeks
    }
}
