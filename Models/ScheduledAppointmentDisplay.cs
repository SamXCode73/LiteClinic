using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class ScheduledAppointmentDisplay
    // This model is used for UI display only.

    {
        public int ScheduleId { get; set; }
        public int PatientAutoId { get; set; }
        public string? AppointmentID { get; set; }

        public int PatientId { get; set; }
        public string? PatientCode { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientMotherName { get; set; } = string.Empty;
        public DateTime PatientDOB { get; set; }
        public string PatientDOBFormatted => PatientDOB.ToString("dd/MM/yyyy") ?? "";

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public string AppointmentDateFormatted => AppointmentDate.ToString("dd/MM/yyyy");
        public TimeSpan AppointmentTime { get; set; }
        public string AppointmentTimeFormatted => DateTime.Today.Add(AppointmentTime).ToString("hh:mm tt");

        public string? AppointmentType { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public bool IsMissed { get; set; }
        public bool IsAttending { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? PatientFullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullMotherName { get; set; }

        public AttendStatus AttendStatus { get; set; }
        public SolidColorBrush? CircleColor { get; set; }
        public AttendDisplayViewModel? Visuals { get; set; }

        public bool CanAddAppoitment => PermissionHelper.CanManageUsers; // Add New Record
        public bool CanEditAppoitment => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateApppoitment => PermissionHelper.CanEditRecords; // Deactivate Record
    }
}
