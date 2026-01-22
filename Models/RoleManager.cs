using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class RoleManager
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public bool CanManageDoctors { get; set; }
        public bool CanManageUsers { get; set; } = false;
        public bool CanAccessDashboard { get; set; } = true;
        public bool CanViewAppointments { get; set; } = true;
        public bool CanManageRecords { get; set; } = false;
        public bool CanEditRecords { get; set; } = false;
        public bool IsDeactivated { get; set; } = false;
        public bool CanManageReports { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public bool CanManageSettings { get; set; } = false;
        public bool CanViewSettings { get; set; } = false;


        public string PermissionSummary
        {
            get
            {
                var permissions = new List<string>();
                if (CanManageDoctors) permissions.Add("Manage Doctors");
                if (CanManageUsers) permissions.Add("Manage Users");
                if (CanAccessDashboard) permissions.Add("Dashboard Access");
                if (CanViewAppointments) permissions.Add("View Appointments");
                if (CanManageRecords) permissions.Add("Manage Records");
                if (CanEditRecords) permissions.Add("Edit Records");
                if (IsDeactivated) permissions.Add("Deactivated");
                if (CanManageReports) permissions.Add("Manage Reports");
                if (CanViewReports) permissions.Add("View Reports");
                if (CanManageSettings) permissions.Add("Manage Settings");
                if (CanViewSettings) permissions.Add("View Settings");

                return permissions.Count > 0 ? string.Join(", ", permissions) : "No Permissions";
            }
        }
        // Add more permission flags as needed

        public override string ToString()
        {
            return @$"{RoleName} - 
Doctors: {CanManageDoctors},
Users: {CanManageUsers}, 
Dashboard: {CanAccessDashboard}, 
Appointments: {CanViewAppointments},
Records: {CanManageRecords},
Edit: {CanEditRecords}, 
Deactivated: {IsDeactivated},
ManageReports: {CanManageReports},
ViewReports:{CanViewReports},
ManageSetting: {CanManageSettings},
ViewSettings: {CanViewSettings}";
        }
    }
}
