using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorCardModel
    {
        // Doctor ID
        public int DoctorId { get; set; }

        // Doctor Name
        public string? FullName { get; set; } = string.Empty;

        // Specialty
        public string Specialization { get; set; } = string.Empty;

        // Distinct attending days (for quick display or filters)
        public string AttendingDays { get; set; } = string.Empty;
        public string TimeSlots { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }

        public string? ProfilePicturePath { get; set; }

        public string? ServiceId { get; set; }

        public string? Initials { get; set; }

        public Brush DayBackground { get; set; } = new SolidColorBrush(Colors.Transparent);  // #D0F0F2

        //public Brush DayBackground { get; set; } = new SolidColorBrush(ColorHelper.FromArgb(255, 208, 240, 242));  // #D0F0F2
        // Collection of day → time slots
        public ObservableCollection<DaySlotModel> DaySlots { get; set; } = new();
        //public Brush SpecializationForeground { get; set; }
        //    = new SolidColorBrush(ColorHelper.FromArgb(255, 91, 28, 149)); //  dark Violet
        public Brush SpecializationForeground { get; set; }
            = new SolidColorBrush(ColorHelper.FromArgb(255, 106, 13, 173)); //  dark Violet

    }

}
