using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class NotificationDataPatient
    {
        public int ScheduleId { get; set; }
        public int AppointmentID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime AppointmentTime { get; set; }
        //public string? AppointmentDate { get; set; }
        //public string? AppointmentTime { get; set; }
        public string? AppointmentType { get; set; }
        public string? Notes { get; set; }
        public int PatientAutoId { get; set; }
        public string? PatientExternalId { get; set; }
        public string? PatientFullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PatientMotherName { get; set; }
        public DateTime PatientDOB { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? Specialty { get; set; }
        // Service info
        public int PatientServiceId { get; set; }
        public string? PatientIdText { get; set; }
        public int ServiceName { get; set; }
        public string? ServiceId { get; set; }
        public bool ServiceIsActive { get; set; }
        public bool NotifyEn { get; set; }
        public bool NotifyAr { get; set; }
        public string? AddedByUser { get; set; }
        public string? AddedAt { get; set; }
        public string? UpdatedByUser { get; set; }
        public string? UpdatedAt { get; set; }

        public string? AppointmentDateFormatted => AppointmentDate.ToString("yyyy-MM-dd");
        public string? AppointmentTimeFormatted => AppointmentTime.ToString("hh:mm");

    }
}
