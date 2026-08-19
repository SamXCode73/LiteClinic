using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    internal static class InfoBarMessages
    {
        

        internal static readonly Dictionary<string, string> Messages = new()
        {
            { "Load_Success", "System status: All services running smoothly" },
            { "Load_SuccessAR", "حالة النظام: لا توجد مشاكل، جميع الخدمات تعمل بشكل طبيعي" },
            { "SaveSuccess", "Data saved successfully." },
            { "DeleteConfirm", "Are you sure you want to delete this item?" },
            { "ErrorOccurred", "An unexpected error occurred. Please try again." },
            { "ValidationFailed", "Please check the input fields and try again." },
            { "LoginSuccess", "Welcome back!" },
            { "ThemeApplied_Light", "Light theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Dark", "Dark theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Pink", "Pink theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_RoyalBlue", "RoyalBlue theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Teal", "Teal theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Violet", "Violet theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_MintGreen", "MintGreen theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Coral", "Coral theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Lavender", "Lavender theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Sandstone", "Sandstone theme applied successfully. It will take effect the next time you open the app." },
            { "ThemeApplied_Monochrome", "Monochrome theme applied successfully. It will take effect the next time you open the app." },
            { "English", "Language applied successfully. It will take effect the next time you open the app." },
            { "العربية", "تم تطبيق اللغة بنجاح. سيتم تفعيلها في المرة القادمة التي تفتح فيها التطبيق." },
            { "NoInternet", "No internet connection. Please check your network settings." },
            { "UpdateAvailable", "A new update is available. Please update to the latest version." },
            { "ProfileUpdated", "Profile updated successfully." },
            { "PasswordChanged", "Password changed successfully." },
            { "LogoutSuccess", "You have been logged out successfully." },
            { "SessionExpired", "Your session has expired. Please log in again." },
            { "ItemAdded", "Item added successfully." },
            { "ItemRemoved", "Item removed successfully." },
            { "SettingsSaved", "Settings saved successfully." },
            { "PermissionDenied", "You do not have permission to perform this action." },
            { "WarningRoleNameEN", "Role name is required." },
            { "WarningRoleNameAR", "اسم الدور مطلوب." },
            { "WarningRoleExists", "Role name already exists." },
            { "WarningSelectRole", "Please select a role to edit." },
            { "WarningSelectRoleDelete", "Please select a role to delete." },
            { "WarningCannotDeleteAdmin", "Cannot delete the Admin role." },
            { "WarningCannotDeleteCurrentRole", "Cannot delete the role currently assigned to you." },
            { "RoleAdded", "Role added successfully." },
            { "RoleUpdated", "Role updated successfully." },
            { "RoleDeactivated", "Role Deactivated successfully."},
            { "UserAdded", "User added successfully." },
            { "UserAddedError", "User added Error, Check log file (\"User Added Error\")." },
            { "UserUpdated", "User updated successfully." },
            { "UserUpdatedError", "User updated Error. Check log file (\"User Updated Error\")" },
            { "UserDeactivated", "User Deactivated successfully."},
            { "UserDeactivatedError", "User Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "DoctorAdded", "Doctor added successfully. Please remember to set the doctor’s schedule in the Open Scheduled Page." },
            { "DoctorAddedError", "Doctor added Error, Check log file (\"User Added Error\")." },
            { "DoctorUpdated", "Doctor updated successfully." },
            { "DoctorUpdatedError", "Doctor updated Error. Check log file (\"User Updated Error\")" },
            { "DoctorDeactivated", "Doctor Deactivated successfully."},
            { "DoctorDeactivatedError", "Doctor Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "PatientAdded", "Patient added successfully." },
            { "PatientddedError", "Patient added Error, Check log file (\"User Added Error\")." },
            { "PatientUpdated", "Patient updated successfully." },
            { "PatientUpdatedError", "Patient updated Error. Check log file (\"User Updated Error\")" },
            { "PatientDeactivated", "Patient Deactivated successfully."},
            { "PatientDeactivatedError", "Patient Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "ErrorDate", "Patient Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "ScheduleAdded", "Schedule added successfully." },
            { "ScheduleAddedError", "Schedule added Error, Check log file (\"Scheduled Added Error\")." },
            { "ScheduleUpdated", "Scheduled updated successfully." },
            { "ScheduledUpdatedError", "Scheduled updated Error. Check log file (\"Scheduled Updated Error\")" },
            { "ScheduleDeactivated", "Scheduled Deactivated successfully."},
            { "ScheduledDeactivatedError", "Scheduled Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "AppointmentAdded", "Appointment added successfully." },
            { "AppointmentAddedError", "Appointment added Error, Check log file (\"Scheduled Added Error\")." },
            { "AppointmentUpdated", "Appointment updated successfully." },
            { "AppointmentUpdatedError", "Appointment updated Error. Check log file (\"Scheduled Updated Error\")" },
            { "AppointmentDeactivated", "Appointment Deactivated successfully."},
            { "AppointmentDeactivatedError", "Appointment Deactivated Error. Check log file (\"User Deactivated Error\")"},
            { "LoginStatrusMessageError", "Invalid username or password"}

        };

        internal static async Task ShowStatusAsync(TextBlock statusTextBlock, string key, SolidColorBrush color)
        {
            var cts = new CancellationTokenSource();

            try
            {
                if (statusTextBlock == null) return;

                if (Messages.TryGetValue(key, out var message))
                {
                    color ??= new SolidColorBrush(ColorHelper.FromArgb(255, 0, 128, 128));
                    statusTextBlock.Foreground = color;
                    statusTextBlock.Text = message;

                    await Task.Delay(2000, cts.Token);
                    statusTextBlock.Text = string.Empty;
                    statusTextBlock.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 0));
                }
                
            }
            catch (TaskCanceledException ex) 
            {
                Logger.LogError(ex, "Error Time Delay");
            }
            finally
            {
                 
                cts.Cancel();
            }

        }

        internal static string ShowCustomStatus(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            return Messages.TryGetValue(key, out var message) ? message : string.Empty;
        }

    }
}
