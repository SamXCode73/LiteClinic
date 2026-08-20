using LiteClinic.Models.Enums;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Repository
{
    internal class SettingsRepository
    {
        public static async Task UpdateThemeAsync(string themeName)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
            INSERT INTO AppSettings (Id, SelectedTheme, UpdatedAt)
            VALUES (1, $theme, CURRENT_TIMESTAMP)
            ON CONFLICT(Id) DO UPDATE SET
                SelectedTheme = excluded.SelectedTheme,
                UpdatedAt = CURRENT_TIMESTAMP;";

                cmd.Parameters.AddWithValue("$theme", themeName);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle error gracefully
                // Example: log to file, console, or your Logger service
                Console.WriteLine($"Error updating theme in DB: {ex.Message}");
                Logger.LogError(ex, "Failed to update theme");

            }
        }

        public static async Task UpdateBackupPathAsync(string backUpPath)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = @"
            UPDATE AppSettings
            SET BackupPath = @BackupPath,
                UpdatedAt = CURRENT_TIMESTAMP
            WHERE Id = 1;";

                cmd.Parameters.AddWithValue("@BackupPath", backUpPath);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Handle error gracefully
                // Example: log to file, console, or your Logger service
                Console.WriteLine($"Error updating Backup path in DB: {ex.Message}");
                Logger.LogError(ex, "Failed to update Backup path");
            }
        }

        public static async Task<string> GetThemeAsync()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT SelectedTheme FROM AppSettings WHERE Id = 1 LIMIT 1;";

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "Light"; // default fallback
            }
            catch (Exception ex)
            {
                // Handle error gracefully
                Console.WriteLine($"Error reading theme from DB: {ex.Message}");

                // If you have a Logger service:
                // Logger.LogError("Failed to read theme", ex);

                // Fallback to default theme
                return "Light";
            }
        }

        //public static async Task UpdateNotificationSettingsAsync(
        //    bool sendViaTelegram,
        //    bool notify24h,
        //    bool notify2h,
        //    bool notifyDoctor)
        //{
        //    string UpdatedAt = $"{Environment.UserName} - {App.GlobalState.LoggedUserName} - {DateTime.Now:F} | Update Telegram Notification";
        //    try
        //    {
        //        using var conn = DatabaseHelper.GetConnection();
        //        await conn.OpenAsync();

        //        using var transaction = conn.BeginTransaction();
        //        using var cmd = conn.CreateCommand();
        //        cmd.Transaction = transaction;

        //        cmd.CommandText = @"
        //    UPDATE NotificationSettings
        //    SET SendViaProvider = @SendViaProvider,
        //        NotifyPatient24h = @NotifyPatient24h,
        //        NotifyPatient2h = @NotifyPatient2h,
        //        NotifyDoctor = @NotifyDoctor,
        //        UpdatedAt = @UpdatedAt
        //    WHERE Id = 1;";

        //        cmd.Parameters.AddWithValue("@SendViaProvider", sendViaTelegram);
        //        cmd.Parameters.AddWithValue("@NotifyPatient24h", notify24h);
        //        cmd.Parameters.AddWithValue("@NotifyPatient2h", notify2h);
        //        cmd.Parameters.AddWithValue("@NotifyDoctor", notifyDoctor);
        //        cmd.Parameters.AddWithValue("@UpdatedAt", UpdatedAt);

        //        await cmd.ExecuteNonQueryAsync();
        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine($"Error updating notification settings in DB: {ex.Message}");
        //        Logger.LogError(ex, "Failed to update notification settings");
        //    }
        //}

        public static async Task UpdateNotificationSettingsAsync(
            bool sendViaTelegram,
            bool notify24h,
            bool notify2h,
            bool notifyDoctor,
            ProviderType providerType = ProviderType.Telegram)
        {
            string UpdatedAt = $"{Environment.UserName} - {App.GlobalState.LoggedUserName} - {DateTime.Now:F} | Update Telegram Notification";
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                // Check if row exists
                cmd.CommandText = "SELECT COUNT(*) FROM NotificationSettings WHERE Id = 1;";
                var result = await cmd.ExecuteScalarAsync();
                var count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt64(result);

                cmd.Parameters.Clear();

                if (count == 0)
                {
                    // Insert new row
                    cmd.CommandText = @"
                INSERT INTO NotificationSettings
                    (Id, ProviderType, SendViaProvider, NotifyPatient24h, NotifyPatient2h, NotifyDoctor, UpdatedAt)
                VALUES
                    (1, @ProviderType, @SendViaProvider, @NotifyPatient24h, @NotifyPatient2h, @NotifyDoctor, @UpdatedAt);";
                }
                else
                {
                    // Update existing row
                    cmd.CommandText = @"
                UPDATE NotificationSettings
                SET SendViaProvider = @SendViaProvider,
                    NotifyPatient24h = @NotifyPatient24h,
                    NotifyPatient2h = @NotifyPatient2h,
                    NotifyDoctor = @NotifyDoctor,
                    UpdatedAt = @UpdatedAt
                WHERE Id = 1;";
                }

                cmd.Parameters.AddWithValue("@ProviderType", (int)providerType);
                cmd.Parameters.AddWithValue("@SendViaProvider", sendViaTelegram ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyPatient24h", notify24h ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyPatient2h", notify2h ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyDoctor", notifyDoctor ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedAt", UpdatedAt);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                const string errorTag = "[ERR_NOTIFICATION_UPDATE]";
                Logger.LogError(ex, $"{errorTag} Failed to update notification settings. UserId={App.GlobalState.LoggedUserId}");
            }
        }

    }
}