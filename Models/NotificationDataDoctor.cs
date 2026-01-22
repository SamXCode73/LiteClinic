using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class NotificationDataDoctor
    {
        public int ScheduleAutoId { get; set; }
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public string? DoctorFullName { get; set; }
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }
        public string? DayOfWeek { get; set; }
        public bool  Notify { get; set; }
        public bool ScheduleIsActive { get; set; }
        public bool DoctorIsActive { get; set; }
        public string? WeekNumbers { get; set; }
        // Service info
        public int DoctorServiceId { get; set; }
        public int ServiceName { get; set; }
        public string? ServiceId { get; set; }
        public bool ServiceIsActive { get; set; }
        public bool NotifyEn { get; set; }
        public bool NotifyAr { get; set; }
        public string? AddedByUser { get; set; }
        public string? AddedAt { get; set; }
        public string? UpdatedByUser { get; set; }
        public string? UpdatedAt { get; set; }
    }
}
