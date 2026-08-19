using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Windows.ApplicationModel;

namespace LiteClinic.Services
{
    public class PatientNotificationService(PatientsRepository patientRepo, LoginPageViewModel loginPageViewModel)
    {
        private readonly LoginPageViewModel? _loginPageViewModel = loginPageViewModel;
        private readonly PatientsRepository _patientRepo = patientRepo;
        private readonly AppState? _appState = App.GlobalState; // inject or pass in your AppState
        private bool Due2h { get; set; }
        private bool Due24h { get; set; }

        private async Task<bool> CheckInternetAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("https://api.telegram.org");
                Logger.LogInfo($"{this.GetType().Name} - Internet connectivity check successful in Patients Notification.", $"Status Code: {{{response.StatusCode}}}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowNotification("Connectivity Error", "Internet connectivity check failed. Please check your connection.");
                Logger.LogError(ex, $"Internet connectivity check failed in Patients Notification. {this.GetType().Name}");
                return false;
            }
        }

        public async Task RunNotifications()
        {
            // Validation logic for patient notifications:
            // We check both 24h and 2h flags separately instead of a single combined flag.
            // Reason: Users may want to disable only one type of reminder (e.g., 2h) while keeping the other active.
            // - If both are disabled, we stop the service completely.
            // - If only one is disabled, we continue running but inform the user which reminder type is inactive.
            // This ensures flexibility and clear communication to the user.

            if (!App.GlobalState.NotifyPatient24h && !App.GlobalState.NotifyPatient2h)
            {
                NotificationHelper.ShowNotification(
                    "Patient Notifications Stopped",
                    "All patient notifications have been stopped. You can restart them anytime from the Settings page."
                );
                Logger.LogInfo($"{this.GetType().Name} - All patient notifications stopped by user settings.");
                return;
            }
            else if (!App.GlobalState.NotifyPatient24h)
            {
                NotificationHelper.ShowNotification(
                    "24h Notifications Disabled",
                    "24-hour patient reminders have been disabled. Only 2-hour reminders will be sent."
                );
                Logger.LogInfo($"{this.GetType().Name} - 24h patient notifications disabled by user settings.");
                return;
            }
            else if (!App.GlobalState.NotifyPatient2h)
            {
                NotificationHelper.ShowNotification(
                    "2h Notifications Disabled",
                    "2-hour patient reminders have been disabled. Only 24-hour reminders will be sent."
                );
                Logger.LogInfo($"{this.GetType().Name} - 2h patient notifications disabled by user settings.");
                return;
            }

            // Sater the Notifiation service loop
            int counter = 0;
            DateTime lastRestDate = DateTime.Now;

            while (true)
            {
                // Reset flags at midnight
                var now = DateTime.Now;
                if (now.Date > lastRestDate)
                {
                    Due2h = false;
                    Due24h = false;
                    lastRestDate = now.Date;
                    Logger.LogInfo($"{this.GetType().Name} cycle counter reset, midnight reached.", "Run Patient Notifications");
                }

                counter++;

                NotificationHelper.ShowNotification("Patient Notifications", "Patient notification service is running.");
                bool online = await CheckInternetAsync();

                if (online)
                {
                    var appointments = await _patientRepo.GetAppointmentsWithServices();

                    try
                    {
                        foreach (var appt in appointments)
                        {
                            if (IsNotificationDue(appt))
                            {
                                try
                                {
                                    await SendNotificationAsync(appt);

                                    using var conn = DatabaseHelper.GetConnection();
                                    conn.Open();

                                    // Step 1: Check if record already exists
                                    using var checkCmd = conn.CreateCommand();
                                    checkCmd.CommandText = @"
                                SELECT COUNT(*) 
                                FROM NotificationHistory
                                WHERE ScheduleId = @ScheduleId 
                                  AND PatientId = @PatientId
                                  AND date(LogDate) = date('now')
                                  AND NotifyFlag = 1";
                                    checkCmd.Parameters.AddWithValue("@ScheduleId", appt.ScheduleId);
                                    checkCmd.Parameters.AddWithValue("@PatientId", appt.PatientAutoId);

                                    int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                                    // Step 2: Update if exists, otherwise insert
                                    using var cmd = conn.CreateCommand();
                                    if (existingCount > 0)
                                    {
                                        cmd.CommandText = @"
                                    UPDATE NotificationHistory
                                    SET Notify2h = @Notify2h,
                                        Notify24h = @Notify24h,
                                        SentAt = @SentAt,
                                        LogDate = @LogDate,
                                        LoggedInUser = @LoggedInUser
                                    WHERE ScheduleId = @ScheduleId 
                                      AND PatientId = @PatientId 
                                      AND NotifyFlag = 1";
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"
                                    INSERT INTO NotificationHistory 
                                        (ScheduleId, PatientId, ProviderType, NotifyFlag, Notify2h, Notify24h, SentAt, LogDate, LoggedInUser)
                                    VALUES 
                                        (@ScheduleId, @PatientId, @ProviderType, @NotifyFlag, @Notify2h, @Notify24h, @SentAt, @LogDate, @LoggedInUser)";
                                        cmd.Parameters.AddWithValue("@ProviderType", ProviderType.Telegram.ToString()); // store as text
                                        cmd.Parameters.AddWithValue("@NotifyFlag", (int)NotifyFlag.SentForPatient); // 1 = Patient, 2 = Doctor
                                    }

                                    cmd.Parameters.AddWithValue("@ScheduleId", appt.ScheduleId);
                                    cmd.Parameters.AddWithValue("@PatientId", appt.PatientAutoId);
                                    cmd.Parameters.AddWithValue("@Notify2h", Due2h ? 1 : 0);
                                    cmd.Parameters.AddWithValue("@Notify24h", Due24h ? 1 : 0);
                                    cmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow.ToString("o"));
                                    cmd.Parameters.AddWithValue("@LogDate", DateTime.UtcNow.ToString("o"));
                                    cmd.Parameters.AddWithValue("@LoggedInUser", $"{_appState?.LoggedUserName} - {Environment.UserName}" ?? Environment.UserName);

                                    cmd.ExecuteNonQuery();

                                    Logger.LogInfo($"Notification history saved/updated for {appt.PatientFullName}");

                                    await Task.Delay(800);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, $"Failed to insert/update notification history for {appt.PatientFullName}");
                                }
                                finally
                                {
                                    // Reset due flags for next appointment
                                    Due2h = false;
                                    Due24h = false;
                                    DatabaseHelper.CloseConnection();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Failed to send patient notifications, {this.GetType().Name}");
                    }
                }
                else
                {
                    Logger.LogInfo($"Internet offline, will retry later. Counter = {counter}, {this.GetType().Name}");
                }

                Logger.LogInfo($"{this.GetType().Name} cycle {counter} complete. Sleeping for 10 minutes {this.GetType().Name}.");
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        private bool IsNotificationDue(NotificationDataPatient appt)
        {
            var now = DateTime.Now;

            try
            {
                bool alreadySent2h = false;
                bool alreadySent24h = false;

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            SELECT Notify2h, Notify24h
            FROM NotificationHistory
            WHERE ScheduleId = @ScheduleId AND PatientId = @PatientId AND NotifyFlag = 1";
                cmd.Parameters.AddWithValue("@ScheduleId", appt.ScheduleId);
                cmd.Parameters.AddWithValue("@PatientId", appt.PatientAutoId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    alreadySent2h = reader.GetInt32(0) == 1;
                    alreadySent24h = reader.GetInt32(1) == 1;
                }

                // Combine date + time
                var appointmentDateTime = new DateTime(
                    appt.AppointmentDate.Year,
                    appt.AppointmentDate.Month,
                    appt.AppointmentDate.Day,
                    appt.AppointmentTime.Hour,
                    appt.AppointmentTime.Minute,
                    0
                );

                // --- Inline working day check ---
                bool isWorkingDay = false;
                switch (appointmentDateTime.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        isWorkingDay = _appState?.NotifyOnMonday ?? false;
                        break;
                    case DayOfWeek.Tuesday:
                        isWorkingDay = _appState?.NotifyOnTuesday ?? false;
                        break;
                    case DayOfWeek.Wednesday:
                        isWorkingDay = _appState?.NotifyOnWednesday ?? false;
                        break;
                    case DayOfWeek.Thursday:
                        isWorkingDay = _appState?.NotifyOnThursday ?? false;
                        break;
                    case DayOfWeek.Friday:
                        isWorkingDay = _appState?.NotifyOnFriday ?? false;
                        break;
                    case DayOfWeek.Saturday:
                        isWorkingDay = _appState?.NotifyOnSaturday ?? false;
                        break;
                    case DayOfWeek.Sunday:
                        isWorkingDay = _appState?.NotifyOnSunday ?? false;
                        break;
                }

                // --- 2h reminder: only for today with grace period of 30 min ---
                bool due2h = _appState!.NotifyPatient2h
                             && isWorkingDay
                             && appointmentDateTime.Date == now.Date
                             && now >= appointmentDateTime.AddHours(-2) // neglect the appoitmetn if they passed more than 2h before the appointment time
                             && now <= appointmentDateTime.AddMinutes(30) // grace period to send the notification even if it's a bit late (e.g., user opens the app after 2h mark but before appointment time)
                             && !alreadySent2h;

                // --- 24h reminder: only for tomorrow ---
                bool due24h = _appState.NotifyPatient24h
                              && isWorkingDay
                              && appointmentDateTime.Date == now.Date.AddDays(1)
                              && !alreadySent24h;

                Due2h = due2h;
                Due24h = due24h;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in IsNotificationDue method. {this.GetType().Name}");
            }

            return Due2h || Due24h;
        }

        private async Task SendNotificationAsync(NotificationDataPatient appt)
        {
            try
            {
                var v = Package.Current.Id.Version;
                string fullVersion = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                var vm = new MessagingIntegrationPageViewModel();

                // Step 1: Get token + chatId from DB
                var result = vm.DecryptFromBase64(ProviderType.Telegram);
                string token = result.Token;
                string chatId = appt.ServiceId!; // patient’s chatId stored in DB
                

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
                {
                    Logger.LogInfo($"Token or ChatId missing, cannot send patient notification. {this.GetType().Name}");
                    return;
                }

                // Step 2: Initialize bot client
                //var botClient = new TelegramBotClient(token);

                // Step 3: Build reminder texts based on NotifyEn / NotifyAr flags
                var messages = new List<string>();

                if (appt.NotifyEn) // English notification
                {
                    string reminderEn = @$"{_appState?.ClinicName} - Appointment Reminder
Hello {appt.PatientFullName},

You have an appointment scheduled:
• Date: {appt.AppointmentDate:dddd, dd/MM/yyyy}
• Time: {appt.AppointmentTime:hh:mm tt}
• Doctor: {appt.DoctorName} ({appt.Specialty})

We look forward to seeing you.
---
LiteClinic V{fullVersion}";
                    messages.Add(reminderEn);
                }

                if (appt.NotifyAr) // Arabic notification Unicode for RTL \u200F
                {
                    string reminderAr = @$"{_appState?.ClinicName} - تذكير بالموعد
مرحباً {appt.PatientFullName}, 
لديك موعد مجدول:
• التاريخ: {appt.AppointmentDate:dddd, dd/MM/yyyy}
• الوقت: {appt.AppointmentTime:hh:mm tt}
• الطبيب: {appt.DoctorName} ({appt.Specialty})
يرجى الحضور في الوقت المحدد.
---
لايت كلينك، نسخة رقم {fullVersion}";
                    messages.Add(reminderAr);
                }

                if (appt.NotifyFr) // French notification
                {
                    string reminderFr = @$"{_appState?.ClinicName} - Rappel de Rendez-vous  
Bonjour {appt.PatientFullName}, 
Vous avez un rendez-vous prévu :
• Date : {appt.AppointmentDate:dddd, dd/MM/yyyy}
• Heure : {appt.AppointmentTime:hh:mm tt}
• Médecin : {appt.DoctorName} ({appt.Specialty})
Veuillez vous assurer d'être présent à l'heure.
---
LiteClinic V{fullVersion}";
                    messages.Add(reminderFr);
                }

                // Step 4: Send one or both messages
                foreach (var msg in messages)
                {
                    if (_loginPageViewModel!.BotClient != null)
                        await _loginPageViewModel.BotClient.SendMessage(
                        chatId: chatId,
                        text: msg
                    );
                }

                Logger.LogInfo($"Telegram notification(s) sent to patient {appt.PatientFullName} for appointment at {appt.AppointmentDate}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error sending Telegram notification to patient. {this.GetType().Name}");
                // Optional: fallback to email/SMS here
            }
        }

        //private async Task IsNotificationDue2H()
        //{ }

        //private async Task IsNotificationDue24H()
        //{ }


    }
}
