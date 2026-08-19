using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.ViewModels;
using Microsoft.VisualBasic;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Windows.ApplicationModel;
using Windows.Media.AppBroadcasting;

namespace LiteClinic.Services
{
    public class DoctorNotificationService(DoctorsRepository doctorRepo, LoginPageViewModel loginPageViewModel)
    {
        
        private readonly LoginPageViewModel? _loginPageViewModel = loginPageViewModel;
        private readonly DoctorsRepository _doctorRepo = doctorRepo;
        private readonly AppState? _appState = App.GlobalState;

        private bool NotifyDoctor { get; set; }
         private DateTime DueDate { get; set; }
        string DueDateString => DueDate.ToString("d", CultureInfo.CurrentCulture);


        private async Task<bool> CheckInternetAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("https://api.telegram.org");
                
                Logger.LogInfo($"{this.GetType().Name} - Internet connectivity check successful in Doctor Notification.", $"Status Code: {{{response.StatusCode}}}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowNotification("Connectivity Error", "Internet connectivity check failed. Please check your connection.");
                Logger.LogError(ex, $"Internet connectivity check failed in Doctor Notification. {this.GetType().Name}");
                return false;
            }
        }

        public async Task RunNotifications()
        {
            if (!App.GlobalState.NotifyDoctor)
            {
                NotificationHelper.ShowNotification(
                    "Doctors Notifications Stopped",
                    "Doctors notifications have been stopped. You can restart them anytime from the Settings page."
                );
                Logger.LogInfo($"{this.GetType().Name} - Doctor notifications are disabled in settings. Service will not run.");
                return;
            }

            int counter = 0;
            DateTime lastRestDate = DateTime.Now;

            while (true)
            {
                // Reset flags at midnight
                var now = DateTime.Now;
                if (now.Date > lastRestDate)
                {
                    NotifyDoctor = false;
                    lastRestDate = now.Date;
                    Logger.LogInfo($"{this.GetType().Name} cycle counter reset, midnight reached.");
                }

                counter++;
                NotificationHelper.ShowNotification("Doctor Notification Service", $"Doctor notification service is running.");

                bool online = await CheckInternetAsync();
                if (online)
                {
                    var schedules = await _doctorRepo.GetSchedulesWithServices();

                    // Group schedules by Doctor + DayOfWeek + WeekNumbers
                    var groupedSchedules = schedules
                        .GroupBy(s => new { s.DoctorId, s.DayOfWeek, s.WeekNumbers });

                    foreach (var group in groupedSchedules)
                    {
                        // Combine all intervals for this doctor/day/week
                        string allIntervals = string.Join(Environment.NewLine,
                            group.Select(g =>
                            {
                                var parts = (g.TimeFromTo ?? string.Empty).Split('-');
                                if (parts.Length >= 2)
                                    return $"From {parts[0].Trim()} To {parts[1].Trim()}";
                                return g.TimeFromTo;
                            })
                            .Where(t => !string.IsNullOrWhiteSpace(t)));

                        // Use one schedule as representative, overwrite TimeFromTo with combined string
                        var representative = group.First();
                        representative.TimeFromTo = allIntervals;

                        if (IsNotificationDue(representative))
                        {
                            try
                            {
                                // 1. Send Telegram message once per doctor/day/week
                                await SendNotificationAsync(representative);

                                // 2. Open database connection to log history
                                using var conn = DatabaseHelper.GetConnection();
                                conn.Open();

                                // Step 1: Check if record already exists for today
                                using var checkCmd = conn.CreateCommand();
                                checkCmd.CommandText = @"
                            SELECT COUNT(*) 
                            FROM DoctorNotificationHistory
                            WHERE ScheduleId = @ScheduleId
                              AND DoctorId = @DoctorId
                              AND date(LogDate) = date('now')
                              AND NotifyFlag = 2;";
                                checkCmd.Parameters.AddWithValue("@ScheduleId", representative.ScheduleAutoId);
                                checkCmd.Parameters.AddWithValue("@DoctorId", representative.DoctorId);

                                var scalarResult = checkCmd.ExecuteScalar();
                                long existsCount = (scalarResult == null || scalarResult == DBNull.Value) ? 0 : Convert.ToInt64(scalarResult);
                                bool exists = existsCount > 0;

                                if (exists)
                                {
                                    try
                                    {
                                        // Step 2: Update existing record
                                        using var updateCmd = conn.CreateCommand();
                                        updateCmd.CommandText = @"
                                    UPDATE DoctorNotificationHistory
                                    SET ProviderType = @ProviderType,
                                        NotifyFlag = @NotifyFlag,
                                        NotifyDoctor = @NotifyDoctor,
                                        SentAt = @SentAt,
                                        LoggedInUser = @LoggedInUser
                                    WHERE ScheduleId = @ScheduleId
                                      AND DoctorId = @DoctorId
                                      AND date(LogDate) = date('now');";

                                        updateCmd.Parameters.AddWithValue("@ScheduleId", representative.ScheduleAutoId);
                                        updateCmd.Parameters.AddWithValue("@DoctorId", representative.DoctorId);
                                        updateCmd.Parameters.AddWithValue("@ProviderType", ProviderType.Telegram.ToString());
                                        updateCmd.Parameters.AddWithValue("@NotifyFlag", (int)NotifyFlag.SentForDoctor);
                                        updateCmd.Parameters.AddWithValue("@NotifyDoctor", NotifyDoctor ? 1 : 0);
                                        updateCmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow.ToString("o"));
                                        updateCmd.Parameters.AddWithValue("@LoggedInUser", $"{_appState?.LoggedUserName} - {Environment.UserName}" ?? Environment.UserName);

                                        updateCmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        string key = "FAILED_TO_UPDATE_DOCTOR_NOTIFICATION_SETTING_TABLE";
                                        Logger.LogError(ex, $" {key} | Failed to update logging for Dr. {representative.DoctorFullName} | {this.GetType().Name}");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        // Step 3: Insert new record
                                        using var insertCmd = conn.CreateCommand();
                                        insertCmd.CommandText = @"
                                    INSERT INTO DoctorNotificationHistory
                                        (ScheduleId, DoctorId, ProviderType, NotifyFlag, NotifyDoctor, SentAt, LogDate, LoggedInUser)
                                    VALUES
                                        (@ScheduleId, @DoctorId, @ProviderType, @NotifyFlag, @NotifyDoctor, @SentAt, @LogDate, @LoggedInUser);";

                                        insertCmd.Parameters.AddWithValue("@ScheduleId", representative.ScheduleAutoId);
                                        insertCmd.Parameters.AddWithValue("@DoctorId", representative.DoctorId);
                                        insertCmd.Parameters.AddWithValue("@ProviderType", ProviderType.Telegram.ToString());
                                        insertCmd.Parameters.AddWithValue("@NotifyFlag", (int)NotifyFlag.SentForDoctor);
                                        insertCmd.Parameters.AddWithValue("@NotifyDoctor", NotifyDoctor ? 1 : 0);
                                        insertCmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow.ToString("o"));
                                        insertCmd.Parameters.AddWithValue("@LogDate", DateTime.UtcNow.ToString("o"));
                                        insertCmd.Parameters.AddWithValue("@LoggedInUser", $"{_appState?.LoggedUserName} - {Environment.UserName}" ?? Environment.UserName);

                                        insertCmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        string key = "FAILED_TO_INSERT_DOCTOR_NOTIFICATION_SETTING_TABLE";
                                        Logger.LogError(ex, $" {key} | Failed to insert logging for Dr. {representative.DoctorFullName} | {this.GetType().Name}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, $"Failed to send notification for Dr. {representative.DoctorFullName} | {this.GetType().Name}");
                            }
                            finally
                            {
                                NotifyDoctor = false;
                                DatabaseHelper.CloseConnection();
                                await Task.Delay(800);
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogInfo($"{this.GetType().Name} - Internet offline, skipping notification cycle. {counter}");
                }

                Logger.LogInfo($"{this.GetType().Name} cycle {counter} complete. Sleeping for 15 minutes (Doctors).");
                await Task.Delay(TimeSpan.FromMinutes(12));
            }
        }


        private bool IsNotificationDue(NotificationDataDoctor schedule)
        {
            var now = DateTime.Now;
            var targetDate = now.Date.AddDays(1); // notify for tomorrow
            DueDate = targetDate;

            bool alreadyNotifiedToday = false;

            // Check if we already sent a "tomorrow" reminder today
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
SELECT NotifyDoctor
FROM DoctorNotificationHistory
WHERE ScheduleId = @ScheduleId
  AND NotifyFlag = 2;";
                cmd.Parameters.AddWithValue("@ScheduleId", schedule.ScheduleAutoId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int notifyDoctorFlag = reader.GetInt32(0);
                    alreadyNotifiedToday = notifyDoctorFlag == 1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in IsNotificationDue DB check (tomorrow reminder). {this.GetType().Name}");
            }

            if (alreadyNotifiedToday)
            {
                Logger.LogInfo("Tomorrow reminder already sent today", "Skipping further notification.");
                return false;
            }

            // Time window (today, e.g., 8 AM – 11 AM) to send reminder for tomorrow's schedule
            // change time to test at any time if you want
            var startWindow = new TimeSpan(8, 0, 0);
            var endWindow = new TimeSpan(23, 0, 0);
            bool inWindow = now.TimeOfDay >= startWindow && now.TimeOfDay <= endWindow;

            // Respect doctor toggle
            bool notifyToggle = _appState!.NotifyDoctor && inWindow;

            // Day-of-week match (for tomorrow)
            bool matchesDay = string.Equals(schedule.DayOfWeek, targetDate.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase);

            // Working day validation from settings ---
            bool isWorkingDay = false;
            switch (targetDate.DayOfWeek)
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

            // Week-of-month match (supports Week 1–5)
            bool matchesWeek = false;
            try
            {
                if (!string.IsNullOrEmpty(schedule.WeekNumbers))
                {
                    var weeks = schedule.WeekNumbers.Split(',')
                                                    .Select(w => int.Parse(w.Trim()));

                    var firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
                    int offset = (int)targetDate.DayOfWeek - (int)firstDayOfMonth.DayOfWeek;
                    if (offset < 0) offset += 7;

                    int weekOfMonth = ((targetDate.Day + offset) / 7) + 1;

                    matchesWeek = weeks.Contains(weekOfMonth);
                }
                else
                {
                    matchesWeek = true; // no week restriction
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error parsing WeekNumbers in IsNotificationDue (tomorrow).");
            }

            // Final decision
            if (notifyToggle && matchesDay && matchesWeek && isWorkingDay)
            {
                NotifyDoctor = true;
                return true;
            }

            NotifyDoctor = false;
            return false;
        }

        private async Task SendNotificationAsync(NotificationDataDoctor schedule)
        {
            try
            {
                var v = Package.Current.Id.Version;
                string fullVersion = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                var vm = new MessagingIntegrationPageViewModel();

                // Step 1: Decrypt token from DB
                var result = vm.DecryptFromBase64(ProviderType.Telegram);
                string token = result.Token;
                string chatId = schedule.ServiceId!; // doctor’s chatId from DoctorServiceIds

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
                {
                    Logger.LogInfo("Token or ChatId missing, cannot send doctor notification.");
                    return;
                }

                // Step 2: Initialize bot client
                //var botClient = new TelegramBotClient(token);


                // Step 3: Build reminder texts based on NotifyEn / NotifyAr flags
                var messages = new List<string>();

                // English Notifiction
                if (schedule.NotifyEn)
                {
                    string reminderEn = @$"{_appState?.ClinicName} - Doctor Reminder
Hello Dr. {schedule.DoctorFullName},
You have a scheduled appointment:
• Day: {schedule.DayOfWeek}
• Date: {DueDateString}
Clinic hours for you today:
{schedule.TimeFromTo}

LiteClinic V{fullVersion}
---
LiteClinic is free — download it today from the Microsoft Store!";
                    messages.Add(reminderEn);
                }

                //Arabic Notification
                if (schedule.NotifyAr)
                {
                    string reminderAr = @$"{_appState?.ClinicName} - تذكير الطبيب
مرحباً د. {schedule.DoctorFullName},
لديك موعد مجدول:
• اليوم: {schedule.DayOfWeek}
• التاريخ: {DueDateString}
ساعات دوامك اليوم:
{schedule.TimeFromTo}

لايت كلينك نسخة رقم {fullVersion}
---
لايت كلينك نسخة مجانية — حمّلها الآن من متجر Microsoft!";
                    messages.Add(reminderAr);
                }

                // French Notifcation
                if (schedule.NotifyFr)
                {
                    string reminderFr = @$"{_appState?.ClinicName} - Rappel du médecin
Bonjour Dr. {schedule.DoctorFullName},
Vous avez un rendez-vous prévu :
• Jour : {schedule.DayOfWeek}
• Date : {DueDateString}
Vos horaires de présence aujourd'hui :
{schedule.TimeFromTo}

LiteClinic V{fullVersion}
---
LiteClinic est une version gratuite — téléchargez-la dès aujourd'hui depuis le Microsoft Store!";
                    messages.Add(reminderFr);
                }

                if (messages.Count == 0)
                {
                    // You can dedbug here to see if this is expected or if there's a configuration issue
                    // For Example:"[SendNotificationAsync] No messages prepared (NotifyEn/NotifyAr both false). Skipping send.")
                    return;
                }

                // Step 4: Send one or both messages
                foreach (var msg in messages)
                {
                    try
                    {
                        // You add Logger infor if you need
                        // Logger.LogInfo($"Sending Telegram message to Dr. {schedule.DoctorFullName} (ChatId: {chatId})", $"Message: {msg}");
                        if (_loginPageViewModel!.BotClient != null)
                            await _loginPageViewModel.BotClient.SendMessage(
                            chatId: chatId,
                            text: msg
                        );
                        // You can add LoggerInfor here to check if message sent successfully                        
                        Logger.LogInfo($"Telegram message sent to Dr. {schedule.DoctorFullName} (ChatId: {chatId}).");
                    }
                    catch (Exception exInner)
                    {
                        string key = "TELEGRAM_ERROR_EX";
                        Logger.LogError(exInner, $"{key}: Error sending Telegram message to Dr. {schedule.DoctorFullName}");
                    }
                }

                Logger.LogInfo($"Telegram notification(s) sent to Doctor {schedule.DoctorFullName} for {schedule.DayOfWeek}, weeks {schedule.WeekNumbers}");
            }
            catch (Exception ex)
            {
                string key = "TELEGRAM_ERROR_EX";
                Logger.LogError(ex, $"{key}: Error sending Telegram notification to Doctor {schedule.DoctorFullName}.");
                // Optional: fallback to SMS/email here
            }
        }
    }
}