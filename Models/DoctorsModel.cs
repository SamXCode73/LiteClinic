using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models;

    public class DoctorsModel
    {
        public int DoctorId { get; set; } // Auto-incremented primary key
        public string? DoctorCode { get; set; } // Custom ID like DR0001
        public string? FullName { get; set; }
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }
        public bool IsActive { get; set; } = true; // 1 = true, 0 = false
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } // Stored as TEXT in SQLite
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } // Stored as TEXT in SQLite
        public string FullNameWithSpecialty => $"{FullName} - {Specialization}";
}

