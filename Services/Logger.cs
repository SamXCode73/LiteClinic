using Windows.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace LiteClinic.Services
{
    internal static class Logger
    {
        // Lazy path resolution ensures LocalFolder is ready
        private static string LogFolder =>
            Path.Combine(ApplicationData.Current.LocalFolder.Path, "AppLogs");

        private static string ErrorLogPath => Path.Combine(LogFolder, "error.log");
        private static string InfoLogPath => Path.Combine(LogFolder, "info.log");
        private static string UserLogPath => Path.Combine(LogFolder, "User.log");
        private static string TelegramLogPath => Path.Combine(LogFolder, "TelegramLog.log");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // ✅ Public API stays the same
        public static void LogError(Exception ex, string context = "")
        {
            var errorEntry = new
            {
                Timestamp = DateTime.Now.ToString("F"),
                Level = "ERROR",
                Context = context,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };

            string json = JsonSerializer.Serialize(errorEntry, JsonOptions);
            WriteToFile(ErrorLogPath, json);
        }

        public static void LogInfo(string message, string context = "")
        {
            var infoEntry = new
            {
                Timestamp = DateTime.Now.ToString("F"),
                Level = "INFO",
                Context = context,
                Message = message,
                StackTrace = new StackTrace().ToString()
            };

            string json = JsonSerializer.Serialize(infoEntry, JsonOptions);
            WriteToFile(InfoLogPath, json);
        }

        public static void LogUserEvent(string userId, string action, string context = "")
        {
            var entry = new
            {
                Timestamp = DateTime.Now.ToString("F"),
                UserName = userId,
                Action = action,
                Context = context
            };

            string json = JsonSerializer.Serialize(entry, JsonOptions);
            WriteToFile(UserLogPath, json);
        }

        public static void LogTelegramEvent(string chatId, string message, string context = "")
        {
            var entry = new
            {
                Timestamp = DateTime.Now.ToString("F"),
                Level = "Telegram Action",
                ChatId = chatId,
                Message = message,
                Context = context
            };

            string json = JsonSerializer.Serialize(entry, JsonOptions);
            WriteToFile(TelegramLogPath, json);
        }

        // ✅ Safe file writer
        private static readonly object _lock = new();

        private static void WriteToFile(string path, string content)
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                    Directory.CreateDirectory(LogFolder);

                lock (_lock)
                {
                    File.AppendAllText(path, content + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Avoid recursion: just dump to Debug
                Trace.WriteLine($"Logger failed: {ex.Message}");
            }
        }
    }
}