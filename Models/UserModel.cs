<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// Contains identity-related information such as username, email, and role assignment.
    /// </summary>
    public class UserModel
    {
        public int UserAutoId { get; set; }              // Auto-increment primary key
        public string? UserId { get; set; }              // Optional external/system user ID
        public string? Username { get; set; }            // Unique username for login
        public string? PasswordHash { get; set; }        // Hashed password for authentication
        public string? FullName { get; set; }            // User's full name
        public string? Email { get; set; }               // Email address
        public string? PhoneNumber { get; set; }         // Mobile phone number
        public string? LandLineNumber { get; set; }      // Landline number (optional)
        public string Language { get; set; } = "en-US";  // Preferred language, defaults to English
        public int RoleId { get; set; }                  // Foreign key linking to RoleModel
        public string? RoleName { get; set; }            // Convenience field for role name
        public bool IsActive { get; set; }               // Indicates if the user account is active
        public DateTime? CreatedAt { get; set; }         // Timestamp when user was created
        public DateTime? UpdatedAt { get; set; }         // Timestamp when user was last updated
        public string? UpdatedBy { get; set; }           // Username of the person who last updated
    }

    /// <summary>
    /// Represents a role definition in the system.
    /// Contains permission flags that determine what actions a user can perform.
    /// </summary>
    public class RoleModel
    {
        public int RoleId { get; set; }                  // Primary key for the role
        public string? RoleName { get; set; }            // Human-readable role name

        // Permission flags (true = allowed, false = denied)
        public bool CanManageUsers { get; set; }
        public bool CanAccessDashboard { get; set; }
        public bool CanViewAppointments { get; set; }
        public bool CanEditRecords { get; set; }
        public bool IsDeactivated { get; set; }
        public bool CanManageReports { get; set; }
        public bool CanViewReports { get; set; }
        public bool CanManageSettings { get; set; }
        public bool CanViewSettings { get; set; }
        public bool CanManageRecords { get; set; }
        public bool CanManageDoctors { get; set; }
    }

    /// <summary>
    /// Composite model that links a UserModel with its associated RoleModel.
    /// Useful for authentication and authorization scenarios where both identity and permissions are needed.
    /// </summary>
    public class UserWithRole
    {
        public UserModel? User { get; set; }             // The user identity information
        public RoleModel? Role { get; set; }             // The role and permissions assigned to the user
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class UserModel
    {
        public int UserAutoId { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandLineNumber { get; set; }
        public string Language { get; set; } = "en-US"; // Default to 'en'
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
>>>>>>> 9bd97308ed79d11fb3a9601f83e76357c193962c
