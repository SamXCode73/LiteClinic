using LiteClinic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public static class PermissionHelper
    {
        // Get the current role object from global state
        private static RoleManager? CurrentRole => App.GlobalState.CurrentRole; 

        // Individual permission flags
        public static bool CanManageDoctors => CurrentRole?.CanManageDoctors ?? false;
        public static bool CanManageUsers => CurrentRole?.CanManageUsers ?? false;
        public static bool CanAccessDashboard => CurrentRole?.CanAccessDashboard ?? false;
        public static bool CanViewAppointments => CurrentRole?.CanViewAppointments ?? false;
        public static bool CanManageRecords => CurrentRole?.CanManageRecords ?? false;
        public static bool CanEditRecords => CurrentRole?.CanEditRecords ?? false;
        public static bool CanManageReports => CurrentRole?.CanManageReports ?? false;
        public static bool CanViewReports => CurrentRole?.CanViewReports ?? false;
        public static bool CanManageSettings => CurrentRole?.CanManageSettings ?? false;
        public static bool CanViewSettings => CurrentRole?.CanViewSettings ?? false;

    }
}
