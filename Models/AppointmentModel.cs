using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class AppointmentModel
    {
        public int ScheduleId { get; set; }

        public string? AppointmentID { get; set; }

        public int PatientAutoId { get; set; }

        public int DoctorId { get; set; }

        public DateTimeOffset AppointmentDate { get; set; }  // Stores full date

        public TimeSpan AppointmentTime { get; set; }  // Stores time only

        public string AppointmentTimeFormatted => 
            DateTime.Today.Add(AppointmentTime).ToString("hh:mm tt", CultureInfo.InvariantCulture);

        public AppointmentTypes AppointmentType { get; set; }

        public string? Notes { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Boolean IsActive { get; set; }
        public bool IsMissed { get; set; } 
        public bool IsAttending { get; set; }

        public string AppointmentDateFormatted => AppointmentDate.ToString("dd/MM/yyyy") ?? "";

        public AttendStatus AttendStatus { get; set; }
    }
}
