using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorWeeklySummary
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTimeOffset AppointmentDate { get; set; }
        public DateTime AppointmentTime { get; set; }
        public int PatientCount { get; set; }

        public string AppointmentDateFormatted => AppointmentDate.ToString("MMMM dd");

    }
}
