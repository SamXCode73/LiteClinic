using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorDisplayModel
    {
        public int DoctorId { get; set; } // Auto-incremented primary key
        public string? DoctorCode { get; set; } // Custom ID like DR0001
        public string? FullName { get; set; }
        public string? Specialization { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }
        public bool IsActive { get; set; } = true; // 1 = true, 0 = false
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } // Stored as TEXT in SQLite
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } // Stored as TEXT in SQLite
        public string FullNameWithSpecialty => $"{FullName} - {Specialization}";
        private string? _profilePicturePath { get; set; }
        public string? ProfilePicturePath { get; set; }

        private string? _nitials => FullName;

        // Property to get initials (first + last only)
        public string? Initials { get; set; }        


        //// UI-only property
        //public string? DisplayProfilePicturePath
        //{
        //    get => string.IsNullOrEmpty(ProfilePicturePath)
        //        ? GetRandomDefaultAvatar()
        //        : ProfilePicturePath;
        //}

        //private string GetRandomDefaultAvatar()
        //{
        //    var random = new Random();
        //    string[] defaults =
        //    {
        //        "ms-appx:///Assets/Profiles/Defaults/male_avatar.png",
        //        "ms-appx:///Assets/Profiles/Defaults/female_avatar.png",
        //        "ms-appx:///Assets/Profiles/Defaults/female_avatar_hejab.png"
        //    };
        //    return defaults[random.Next(defaults.Length)];
        //}
    }
}
