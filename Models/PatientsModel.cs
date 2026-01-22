using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models;

    public class PatientsModel
    {
    // Primary key (auto-incremented)
    public int PatientAutoId { get; set; }

    // Manual ID like 'PAT001'
    public string? PatientId { get; set; }

    // Personal Info
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FullMotherName { get; set; }
    public string? CivilRecord { get; set; }

    // Gender: 'Male', 'Female', 'Other'
    public string? Gender { get; set; }

    // Date of Birth
    //public DateTime? DateOfBirth { get; set; }
    //public string? StringToDate { get; set; }

    public string? DateOfBirth { get; set; }
    public string? StringDay { get; set; }
    public string? StringMonth { get; set; }
    public string? StringYear { get; set; }
    public int PatientAge { get; set; }
    public string? PatientDOB { get; set; }

    // Contact Info
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // Insurance Info
    public bool GotInsurance { get; set; }
    public string? InsuranceName { get; set; }
    public string? InsuranceNumber { get; set; }

    // NSN Info
    public bool GotNSN { get; set; }
    public string? NSNName { get; set; }
    public string? NSNNumber { get; set; }

    // Medical Info
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }

    // System Info
    public string Language { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Computed property
    public string PatientFullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    

}

