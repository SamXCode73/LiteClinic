using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using LiteClinic.Views;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;


namespace LiteClinic.ViewModels
{
    public partial class SplashScreenViewModel : INotifyPropertyChanged
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly ResourceLoader _loader = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SplashScreenViewModel()
        {
            // Initialize any necessary properties or commands here
        }


        private string? _copyRightText;
        private string? _statusMessage;
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.DimGray);
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public string? CopyRightText
        {
            get => _copyRightText;
            set
            {
                if (_copyRightText != value)
                {
                    _copyRightText = value;
                    OnPropertyChanged(nameof(CopyRightText));
                }
            }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }
        public async Task InitializeAsync()
        {
            try
            {
                var loader = new ResourceLoader();

                await GetCopyRight();

                await Task.Delay(700, _cts.Token); // Simulate delay for copyright display

                // TOD add await methods
                //await EnsureDatabaseCopiedAsync();
                await InitializeDatabaseAsync();

                StatusMessage = loader.GetString("Status_LoadingDB"); // Key From Resources.resw
                await Task.Delay(700, _cts.Token); // Simulate delay for DB loading

                await LoadingNotificationSettingAsync();

                StatusMessage = loader.GetString("Status_LoadingSettings"); // Key From Resources.resw
                await Task.Delay(700, _cts.Token); // Simulate delay for settings loading

                await LoadingClinicNameAsync();

                StatusMessage = loader.GetString("Status_LoadingClinic"); // Key From Resources.resw
                await Task.Delay(700, _cts.Token); // Simulate delay for clinic name loading

                await LoadThemeAndLanguageAsync();

                StatusMessage = loader.GetString("Status_ApplyingTheme"); // Key From Resources.resw
                await Task.Delay(700, _cts.Token); // Simulate delay for theme/language application            

                await Task.Delay(500, _cts.Token); // Short delay before navigation
                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessage = loader.GetString("Status_Complete"); // Key From Resources.resw

                await NavigateToLoginPageAsync();
            }
            catch (TaskCanceledException)
            {
                // Initialization was canceled, likely because the app is closing
                Logger.LogInfo("Initialization was canceled.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Splash Screen] Error during initialization.");
            }

        }

        public async Task LoadingLanguageAsync()
        {
            string language = LanguageManager.CurrentLanguage; // fallback default

            var localSettings = Windows.Storage.ApplicationData.Current!.LocalSettings;

            // Try to load from LocalSettings first
            if (localSettings.Values.TryGetValue("Language", out object? value))
            {
                language = value as string ?? language;
            }
            else
            {
                try
                {
                    using var conn = await DatabaseHelper.GetConnectionAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                SELECT Language
                FROM AppSettings
                WHERE Id = 1;";

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        language = reader.GetString(0);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error loading language from DB.");
                }
                finally
                {
                    await DatabaseHelper.CloseConnectionAsync();
                }

                // Save back to LocalSettings for next time
                localSettings.Values["Language"] = language;
            }

            // Update GlobalState
            App.GlobalState.CurrentLanguage = language;
            Debug.WriteLine($"Loaded Language: {language}");

            StatusMessage = App.GlobalState.CurrentLanguage switch
            {
                "en-US" => "Loading language settings...",
                "ar" => "جاري تحميل إعدادات اللغة...",
                "fr-FR" => "Chargement des paramètres de langue...",
                _ => "Loading language settings...",
            };
        }
        public async Task LoadingNotificationSettingAsync()
        {
            bool sendViaTelegram = false, notify24h = false, notify2h = false, notifyDoctor = false;
            bool monday = false, tuesday = false, wednesday = false, thursday = false,
                 friday = false, saturday = false, sunday = false;

            var settings = ApplicationData.Current?.LocalSettings?.Values;
            bool hasLocalSettings = false;

            if (settings != null)
            {
                if (settings.TryGetValue("SendViaTelegram", out object? value1)) { sendViaTelegram = value1 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyPatient24h", out object? value)) { notify24h = value is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyPatient2h", out object? value2)) { notify2h = value2 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyDoctor", out object? value3)) { notifyDoctor = value3 is bool b && b; hasLocalSettings = true; }

                // Weekdays
                if (settings.TryGetValue("NotifyOnMonday", out object? value4)) { monday = value4 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnTuesday", out object? value5)) { tuesday = value5 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnWednesday", out object? value6)) { wednesday = value6 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnThursday", out object? value7)) { thursday = value7 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnFriday", out object? value8)) { friday = value8 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnSaturday", out object? value9)) { saturday = value9 is bool b && b; hasLocalSettings = true; }
                if (settings.TryGetValue("NotifyOnSunday", out object? value10)) { sunday = value10 is bool b && b; hasLocalSettings = true; }
            }

            if (!hasLocalSettings)
            {
                try
                {
                    using var conn = await DatabaseHelper.GetConnectionAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                SELECT SendViaProvider, NotifyPatient24h, NotifyPatient2h, NotifyDoctor,
                       NotifyOnMonday, NotifyOnTuesday, NotifyOnWednesday, NotifyOnThursday,
                       NotifyOnFriday, NotifyOnSaturday, NotifyOnSunday
                FROM NotificationSettings
                WHERE ProviderType=@ProviderType;";
                    cmd.Parameters.AddWithValue("@ProviderType", (int)ProviderType.Telegram);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        sendViaTelegram = reader.GetInt32(0) == 1;
                        notify24h = reader.GetInt32(1) == 1;
                        notify2h = reader.GetInt32(2) == 1;
                        notifyDoctor = reader.GetInt32(3) == 1;

                        monday = reader.GetInt32(4) == 1;
                        tuesday = reader.GetInt32(5) == 1;
                        wednesday = reader.GetInt32(6) == 1;
                        thursday = reader.GetInt32(7) == 1;
                        friday = reader.GetInt32(8) == 1;
                        saturday = reader.GetInt32(9) == 1;
                        sunday = reader.GetInt32(10) == 1;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error loading DB settings.");
                }
                finally
                {
                    await DatabaseHelper.CloseConnectionAsync();
                }
            }

            // Update GlobalState
            App.GlobalState.NotifyPatient2h = notify2h;
            App.GlobalState.NotifyPatient24h = notify24h;
            App.GlobalState.NotifyDoctor = notifyDoctor;
            App.GlobalState.SendViaTelegram = sendViaTelegram;

            App.GlobalState.NotifyOnMonday = monday;
            App.GlobalState.NotifyOnTuesday = tuesday;
            App.GlobalState.NotifyOnWednesday = wednesday;
            App.GlobalState.NotifyOnThursday = thursday;
            App.GlobalState.NotifyOnFriday = friday;
            App.GlobalState.NotifyOnSaturday = saturday;
            App.GlobalState.NotifyOnSunday = sunday;
        }



        public async Task LoadingClinicNameAsync()
        {
            string clinicName = "Default LiteClinic"; // fallback default
            bool showGregorianDate = true; // sensible default
            bool showHijriDate = false;    // sensible default

            var localSettings = Windows.Storage.ApplicationData.Current!.LocalSettings;

            // Try to load from LocalSettings first
            if (localSettings.Values.TryGetValue("ClinicName", out object? value))
            {
                clinicName = value as string ?? clinicName;
                showGregorianDate = localSettings.Values["ShowGregorianDate"] as bool? ?? showGregorianDate;
                showHijriDate = localSettings.Values["ShowHijriDate"] as bool? ?? showHijriDate;
            }
            else
            {
                try
                {
                    using var conn = await DatabaseHelper.GetConnectionAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                SELECT AppName, ShowGregorianDate, ShowHijriDate, AutoAppName
                FROM AppName
                WHERE AutoAppName = 1;";

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        clinicName = reader.GetString(0);
                        showGregorianDate = reader.GetInt32(1) == 1; // assuming DB stores 0/1
                        showHijriDate = reader.GetInt32(2) == 1;

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error loading DB settings.");
                }
                finally
                {
                    await DatabaseHelper.CloseConnectionAsync();
                }

                // Save back to LocalSettings for next time
                localSettings.Values["ClinicName"] = clinicName;
                localSettings.Values["ShowGregorianDate"] = showGregorianDate;
                localSettings.Values["ShowHijriDate"] = showHijriDate;
            }

            // Update GlobalState
            App.GlobalState.ClinicName = clinicName;
            App.GlobalState.ShowGregorianDate = showGregorianDate;
            App.GlobalState.ShowHijriDate = showHijriDate;
        }

        public async Task LoadThemeAndLanguageAsync()
        {
            var settings = ApplicationData.Current?.LocalSettings?.Values;
            if (settings == null) return;

            // --- Theme ---
            var themeName = settings["SelectedTheme"] as string ?? "Light";
            ThemeManager.CurrentTheme = themeName;
            App.GlobalState.SelectedTheme = Enum.TryParse(themeName, out ThemeType parsedTheme) ? parsedTheme : ThemeType.Light;
            Logger.LogInfo($"Selected Theme: {themeName}");

            // --- Language ---
            var lang = settings["SelectedLanguage"] as string ?? "en-US";
            LanguageManager.CurrentLanguage = lang;
            Logger.LogInfo($"Selected Language: {lang}");

            // Apply system override so resources reload
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();

            // --- Ensure keys exist ---
            settings["SelectedTheme"] = themeName;
            settings["SelectedLanguage"] = lang;

            await Task.CompletedTask; // placeholder if you later add async DB fallback
        }

        public async Task NavigateToLoginPageAsync()
        {
            try
            {
                // Delay for 3 seconds to show splash screen
                await Task.Delay(2000, _cts.Token);

                var rootFrame = App.MainAppWindow?.Content as Frame;
                if (rootFrame != null)
                {
                    rootFrame.Navigate(typeof(LoginPage));
                }
            }
            catch (TaskCanceledException)
            {
                // Navigation was canceled, likely because the app is closing
                Logger.LogInfo("Navigation to LoginPage was canceled.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error navigating to LoginPage.");
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            string dbName = "LiteClinic.db";
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // Step 1: Check if DB exists
            if (await localFolder.TryGetItemAsync(dbName) == null)
            {
                // Copy packaged DB (already has new columns)
                StorageFolder installFolder = Package.Current.InstalledLocation;
                StorageFile sourceFile = await installFolder.GetFileAsync(@"Assets\Data\LiteClinic.db");
                await sourceFile.CopyAsync(localFolder, dbName, NameCollisionOption.FailIfExists);
            }
            else
            {
                // Step 2: DB exists → check schema
                string dbPath = Path.Combine(localFolder.Path, dbName);
                using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
                await conn.OpenAsync();

                // --- Check AppSettings table ---
                var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info(AppSettings);";
                using var reader = await cmd.ExecuteReaderAsync();

                var columns = new HashSet<string>();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(1));
                }

                if (!columns.Contains("ShowGregorianDate"))
                {
                    var alter1 = conn.CreateCommand();
                    alter1.CommandText = "ALTER TABLE AppSettings ADD COLUMN ShowGregorianDate INTEGER DEFAULT 1;";
                    await alter1.ExecuteNonQueryAsync();
                }

                if (!columns.Contains("ShowHijriDate"))
                {
                    var alter2 = conn.CreateCommand();
                    alter2.CommandText = "ALTER TABLE AppSettings ADD COLUMN ShowHijriDate INTEGER DEFAULT 0;";
                    await alter2.ExecuteNonQueryAsync();
                }

                // --- Check PatientServiceIds table ---
                var cmd2 = conn.CreateCommand();
                cmd2.CommandText = "PRAGMA table_info(PatientServiceIds);";
                using var reader2 = await cmd2.ExecuteReaderAsync();

                var columns2 = new HashSet<string>();
                while (await reader2.ReadAsync())
                {
                    columns2.Add(reader2.GetString(1));
                }

                if (!columns2.Contains("NotifyFr"))
                {
                    var alter3 = conn.CreateCommand();
                    alter3.CommandText = "ALTER TABLE PatientServiceIds ADD COLUMN NotifyFr INTEGER DEFAULT 0;";
                    await alter3.ExecuteNonQueryAsync();
                }

                // --- Check DoctorServiceIds table ---
                var cmd3 = conn.CreateCommand();
                cmd3.CommandText = "PRAGMA table_info(DoctorServiceIds);";
                using var reader3 = await cmd3.ExecuteReaderAsync();

                var columns3 = new HashSet<string>();
                while (await reader3.ReadAsync())
                {
                    columns3.Add(reader3.GetString(1));
                }

                if (!columns3.Contains("NotifyFr"))
                {
                    var alter4 = conn.CreateCommand();
                    alter4.CommandText = "ALTER TABLE DoctorServiceIds ADD COLUMN NotifyFr INTEGER DEFAULT 0;";
                    await alter4.ExecuteNonQueryAsync();
                }

                // --- Check NotificationSettings table ---
                var cmd4 = conn.CreateCommand();
                cmd4.CommandText = "PRAGMA table_info(NotificationSettings);";
                using var reader4 = await cmd4.ExecuteReaderAsync();

                var columns4 = new HashSet<string>();
                while (await reader4.ReadAsync())
                {
                    columns4.Add(reader4.GetString(1));
                }

                if (!columns4.Contains("UpdatedAt"))
                {
                    var alter5 = conn.CreateCommand();
                    alter5.CommandText = "ALTER TABLE NotificationSettings ADD COLUMN UpdatedAt TEXT;";
                    await alter5.ExecuteNonQueryAsync();
                }

                // --- Check DoctorSchedule table ---
                var cmd5 = conn.CreateCommand();
                cmd5.CommandText = "PRAGMA table_info(DoctorSchedule);";
                using var reader5 = await cmd5.ExecuteReaderAsync();

                var columns5 = new HashSet<string>();
                while (await reader5.ReadAsync())
                {
                    columns5.Add(reader5.GetString(1));
                }

                if (!columns5.Contains("TimeFromTo"))
                {
                    var alter6 = conn.CreateCommand();
                    alter6.CommandText = "ALTER TABLE DoctorSchedule ADD COLUMN TimeFromTo TEXT NOT NULL DEFAULT 'Not Set';";
                    await alter6.ExecuteNonQueryAsync();
                }
                
            // --- Ensure Patient view ---
            await EnsureView(conn,
                    "ViewPatientWithServices",
                    @"CREATE VIEW ViewPatientWithServices AS
                            SELECT  
                                p.PatientAutoId,
                                p.PatientId,
                                psi.PatientServiceId,
                                psi.ServiceName,
                                psi.ServiceId,
                                psi.IsActive,
                                psi.NotifyEn,
                                psi.NotifyAr,
                                psi.NotifyFr,   -- new column added here
                                psi.AddedByUser,
                                psi.AddedAt,
                                psi.UpdatedByUser,
                                psi.UpdatedAt
                            FROM PatientTable p
                            JOIN PatientServiceIds psi ON p.PatientAutoId = psi.PatientId
                            WHERE psi.IsActive = 1
                            ORDER BY p.PatientId, psi.ServiceName;", "NotifyFr");

                // --- Ensure Doctor view ---
                await EnsureView(conn,
                    "ViewDoctorWithServices",
                    @"CREATE VIEW ViewDoctorWithServices AS
                            SELECT
                                d.DoctorId,
                                d.DoctorCode,
                                d.FullName       AS DoctorName,
                                d.Specialization AS Specialty,
                                dsi.DoctorServiceId,
                                dsi.ServiceName,
                                dsi.ServiceId,
                                dsi.IsActive,
                                dsi.NotifyEn,
                                dsi.NotifyFr,
                                dsi.notifyAr,
                                dsi.AddedByUser,
                                dsi.AddedAt,
                                dsi.UpdatedByUser,
                                dsi.UpdatedAt
                            FROM Doctors d
                            JOIN DoctorServiceIds dsi ON d.DoctorId = dsi.DoctorId
                            WHERE d.IsActive = 1
                            ORDER BY d.FullName, dsi.ServiceName;", "NotifyFr");

                // --- Ensure View Scheduled Appointments With Services ---
                await EnsureView(conn,
                    "ViewScheduledAppointmentsWithServices",
                    @"CREATE VIEW ViewScheduledAppointmentsWithServices AS
                        SELECT
                            sa.ScheduleId,
                            sa.AppointmentID,
                            sa.AppointmentDate,
                            sa.AppointmentTime,
                            sa.AppointmentType,
                            sa.Notes,
                            sa.PatientId, -- PatientAutoId
                            p.PatientAutoId AS PatientAutoId,
                            p.PatientId AS PatientExternalId,
                            sa.IsActive,
                            sa.IsMissed,
                            sa.IsAttending,
                            sa.AttendStatus,
                            p.FirstName,
                            p.MiddleName,
                            p.LastName,
                            (p.FirstName || ' ' || p.MiddleName || ' ' || p.LastName) AS PatientFullName,
                            p.Email,
                            p.PhoneNumber,
                            p.FullMotherName AS PatientMotherName,
                            p.DateOfBirth AS PatientDOB,
                            sa.DoctorId,
                            d.FullName AS DoctorName,
                            d.Specialization AS Specialty,
                            psi.PatientServiceId,
                            psi.PatientId AS ServicePatientId,
                            psi.PatientIdText,
                            psi.ServiceName,
                            psi.ServiceId,
                            psi.IsActive AS ServiceIsActive,
                            psi.NotifyEn,
                            psi.NotifyFr,
                            psi.NotifyAr,
                            psi.AddedByUser,
                            psi.AddedAt,
                            psi.UpdatedByUser,
                            psi.UpdatedAt
                        FROM ScheduledAppointments sa
                        JOIN PatientTable p ON sa.PatientId = p.PatientAutoId
                        JOIN Doctors d ON sa.DoctorId = d.DoctorId
                        LEFT JOIN PatientServiceIds psi ON sa.PatientId = psi.PatientId
                        WHERE d.IsActive = 1
                        ORDER BY sa.AppointmentDate ASC, sa.AppointmentTime ASC;", "NotifyFr");

                // --- Ensure Doctor Schedule With Services ---
                await EnsureView(conn,
                    "DoctorScheduleWithServices",
                    @"CREATE VIEW DoctorScheduleWithServices AS
                            SELECT
                                ds.ScheduleAutoId,
                                ds.ScheduleId,
                                ds.DoctorId,
                                d.DoctorCode,
                                d.FullName AS DoctorFullName,
                                d.Specialization,
                                d.PhoneNumber,
                                d.LandLineNumber,
                                ds.DayOfWeek,
                                ds.Notify,
                                ds.IsActive AS ScheduleIsActive,
                                d.IsActive AS DoctorIsActive,
                                ds.WeekNumbers,
                                -- Service info
                                dsi.ServiceName,
                                dsi.ServiceId,
                                dsi.IsActive AS ServiceIsActive,
                                dsi.NotifyEn,
                                dsi.NotifyFr,
                                dsi.NotifyAr,
                                dsi.AddedByUser,
                                dsi.AddedAt,
                                dsi.UpdatedByUser,
                                dsi.UpdatedAt
                            FROM DoctorSchedule ds
                            JOIN Doctors d ON ds.DoctorId = d.DoctorId
                            LEFT JOIN DoctorServiceIds dsi ON ds.DoctorId = dsi.DoctorId
                            WHERE ds.IsActive = 1 AND d.IsActive = 1
                            ORDER BY ds.DayOfWeek ASC, d.FullName ASC;", "NotifyFr");


                await EnsureDoctorNotificationHistory(conn);
                await CreateIndexes(conn);

                await EnsureView(conn,
                    "DoctorScheduleWithServices",
                    @"CREATE VIEW DoctorScheduleWithServices AS
                            SELECT
                                ds.ScheduleAutoId,
                                ds.ScheduleId,
                                ds.DoctorId,
                                d.DoctorCode,
                                d.FullName AS DoctorFullName,
                                d.Specialization,
                                d.PhoneNumber,
                                d.LandLineNumber,
                                ds.DayOfWeek,
                                ds.Notify,
                                ds.IsActive AS ScheduleIsActive,
                                d.IsActive AS DoctorIsActive,
                                ds.WeekNumbers,
                                ds.TimeFromTo,
                                -- Service info
                                dsi.ServiceName,
                                dsi.ServiceId,
                                dsi.IsActive AS ServiceIsActive,
                                dsi.NotifyEn,
                                dsi.NotifyFr,
                                dsi.NotifyAr,
                                dsi.AddedByUser,
                                dsi.AddedAt,
                                dsi.UpdatedByUser,
                                dsi.UpdatedAt
                            FROM DoctorSchedule ds
                            JOIN Doctors d ON ds.DoctorId = d.DoctorId
                            LEFT JOIN DoctorServiceIds dsi ON ds.DoctorId = dsi.DoctorId
                            WHERE ds.IsActive = 1 AND d.IsActive = 1
                            ORDER BY ds.DayOfWeek ASC, d.FullName ASC;", "TimeFromTo");

                // Add new culumns to Doctores Table for Profile
                await EnsureColumnExistsAsync(conn, "Doctors", "Gender", "TEXT");
                await EnsureColumnExistsAsync(conn, "Doctors", "ProfilePicturePath", "TEXT");
                // I forget to add default image for SQLite limitation ( or maybe i did not knwo how to use correctly)
                await EnsureColumnDefaultAsync(conn, "Doctors", "ProfilePicturePath", "");

                // --- Ensure Doctor Schedule With day and time ---
                await EnsureView(conn,
                    "DoctorScheduleView",
                    @"CREATE VIEW DoctorScheduleView AS
                        SELECT 
                            ds.ScheduleAutoId,
                            ds.ScheduleId,
                            ds.DoctorId,
                            d.DoctorCode,
                            d.FullName,
                            d.Specialization,
                            d.PhoneNumber,
                            d.LandLineNumber,
                            d.Gender,
                            d.ProfilePicturePath,
                            ds.DayOfWeek,
                            ds.Notify,
                            ds.IsActive AS ScheduleIsActive,
                            d.IsActive AS DoctorIsActive,
                            ds.WeekNumbers AS WeekNumbers,
                            ds.TimeFromTo AS TimeFromTo,
                            s.ServiceId,
                            s.IsActive AS ServiceIsActive
                        FROM 
                            DoctorSchedule ds
                        JOIN 
                            Doctors d ON ds.DoctorId = d.DoctorId
                        LEFT JOIN 
                            DoctorServiceIds s ON ds.DoctorId = s.DoctorId
                        WHERE 
                            ds.IsActive = 1 
                            AND d.IsActive = 1;", "Gender", "ProfilePicturePath, ServiceId, ServiceIsActive");

            }

        }


        private async Task EnsureView(SqliteConnection conn, string viewName, string createSql, params string[] requiredColumns)
        {
            try
            {
                var checkView = conn.CreateCommand();
                checkView.CommandText = $"SELECT sql FROM sqlite_master WHERE type='view' AND name='{viewName}';";

                if (checkView.ExecuteScalar() is not string currentSql ||
                    requiredColumns.Any(col => !currentSql.Contains(col, StringComparison.OrdinalIgnoreCase)))
                {
                    var dropView = conn.CreateCommand();
                    dropView.CommandText = $"DROP VIEW IF EXISTS {viewName};";
                    dropView.ExecuteNonQuery();

                    var createView = conn.CreateCommand();
                    createView.CommandText = createSql;
                    await createView.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                string key = "ERROR_CREATE_NEW_VIEW";
                Logger.LogError(ex, $"{key} | Error ensuring view {viewName} | {this.GetType().Name}.");
            }
        }

        private async Task EnsureDoctorNotificationHistory(SqliteConnection conn)
        {
            try
            {
                // Step 1: Check if table exists
                using var checkTableCmd = conn.CreateCommand();
                checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='DoctorNotificationHistory';";
                var tableExists = checkTableCmd.ExecuteScalar() != null;

                if (!tableExists)
                {
                    using var createTableCmd = conn.CreateCommand();
                    createTableCmd.CommandText = @"
                        CREATE TABLE DoctorNotificationHistory (
                            NotificationId INTEGER PRIMARY KEY AUTOINCREMENT,
                            ScheduleId INTEGER NOT NULL,
                            DoctorId INTEGER,                -- nullable if NotificationType = 2 (Doctor) 
                            ProviderType TEXT NOT NULL,      -- e.g. 'Telegram', 'Email'
                            NotifyFlag INTEGER NOT NULL,     -- 1 = Patient, 2 = Doctor
                            Notify2h INTEGER DEFAULT 0,      -- 0 = not sent, 1 = sent
                            Notify24h INTEGER DEFAULT 0,     -- 0 = not sent, 1 = sent
	                        NotifyDoctor INTEGER DEFAULT 0,  -- 0 = not sent, 1 = sent
                            SentAt TEXT,                     -- timestamp of last successful send
                            LogDate TEXT,                    -- record creation timestamp
                            LoggedInUser TEXT,               -- username or staff ID
                            FOREIGN KEY(DoctorId) REFERENCES Doctors(DoctorId));";

                    await createTableCmd.ExecuteNonQueryAsync();

                }                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to ensure DoctorNotificationHistory table/index | {this.GetType().Name}");
            }
        }

        //Helper method for indexes
        private static async Task CreateIndexes(SqliteConnection conn)
        {
            var indexCommands = new[]
            {
        "CREATE INDEX IF NOT EXISTS idx_drnotification_type ON DoctorNotificationHistory(NotifyFlag);",
        "CREATE INDEX IF NOT EXISTS idx_drnotification_schedule ON DoctorNotificationHistory(ScheduleId);",
        "CREATE INDEX IF NOT EXISTS idx_drnotification_sentat ON DoctorNotificationHistory(SentAt);"
    };

            foreach (var cmdText in indexCommands)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task EnsureColumnExistsAsync(SqliteConnection conn, string tableName, string columnName, string columnType)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = await cmd.ExecuteReaderAsync();

            var columns = new HashSet<string>();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(1)); // column name
            }

            if (!columns.Contains(columnName))
            {
                var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType};";
                await alter.ExecuteNonQueryAsync();
            }
        }

        private static async Task EnsureColumnDefaultAsync(SqliteConnection conn, string tableName, string columnName, string defaultValue)
        {
            // Update all rows where the column is NULL
            var update = conn.CreateCommand();
            update.CommandText = $"UPDATE {tableName} SET {columnName} = @defaultValue WHERE {columnName} IS NULL;";
            update.Parameters.AddWithValue("@defaultValue", defaultValue);
            await update.ExecuteNonQueryAsync();
        }

        private async Task GetCopyRight()
        {
            string newdate = DateTime.Now.Year.ToString();
            var v = Package.Current.Id.Version;
            string fullVersion = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

            // Fetch localized template from resources
            string template = _loader.GetString("CopyRightMessage");

            // Replace placeholders with dynamic values
            CopyRightText = string.Format(template, newdate, fullVersion);

            await Task.CompletedTask;
        }

    }
}
