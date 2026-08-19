using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.ApplicationModel.Resources;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Windows.Media.Protection.PlayReady;

namespace LiteClinic.ViewModels
{
    public partial class LoginPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UserWithRole> UserList { get; set; } = [];
        private readonly UserRepository _userRepository = new();
        private readonly RoleRepository _roleRepository = new();
        private PatientNotificationService? _patientService;
        private DoctorNotificationService? _doctorService;

        private DateTime _lastGreetingDate = DateTime.MinValue;
        private string _lastGreetingPeriod = string.Empty; // "morning" or "afternoon"
        private readonly BackgroundService _backgroundService = new();
        private static readonly CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();

        public TelegramBotClient? BotClient { get; private set; }
        public ICommand? Btn_LoginCommand { get; }

        public LoginPageViewModel()
        {

            _backgroundService = App.GlobalState.BackgroundService;
            Btn_LoginCommand = new RelayCommand(async () => await CheckLoginUserAsync());

        }


        private string _username = string.Empty;
        private string _password = string.Empty;
        private int _roleId = 0;
        private string _passwordHash = string.Empty;
        private string _statusMessage = string.Empty;
        private SolidColorBrush _statusColor = new(Colors.Black);
        private List<UserWithRole> _usersList = [];
        private RoleManager _currentRole = new();

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string PasswordHash
        {
            get => _passwordHash;
            set => SetProperty(ref _passwordHash, value);
        }

        public int RoleId
        {
            get => _roleId;
            set => SetProperty(ref _roleId, value);
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async Task CheckLoginUserAsync()
        {
            try
            {
                _usersList = await _userRepository.GetAllUsersForAuthenticationAsync();

                if (_usersList != null && _usersList.Count > 0)
                {
                    var hashedPassword = HashPassword(Password);

                    var matchedUser = _usersList.FirstOrDefault(u =>
                        u.User != null &&
                        u.User.Username != null &&
                        u.User.Username.Equals(Username, StringComparison.OrdinalIgnoreCase) &&
                        u.User.PasswordHash == hashedPassword);

                    if (matchedUser != null)
                    {
                        StatusColor = new SolidColorBrush(Colors.Green);
                        StatusMessage = string.Format(_loader.GetString("LgnP_LoginSuccessMessage"),matchedUser.User!.Username);

                        App.GlobalState.LoggedUserName = matchedUser.User.Username!;
                        App.GlobalState.LoggedUserRoleId = matchedUser.User.RoleId;
                        App.GlobalState.LoggedUserId = matchedUser.User.UserAutoId.ToString();

                        Logger.LogUserEvent($"Logged in User - Role: {App.GlobalState.LoggedUserRoleId} | " +
                            $"ID: {App.GlobalState.LoggedUserId} | User: {App.GlobalState.LoggedUserName} | " +
                            $"Windows User: {Environment.UserName}, Action: User Logged In", "Login Page");

                        _currentRole = _roleRepository.GetRoleById(matchedUser.User.RoleId)!;
                        App.GlobalState.CurrentRole = _currentRole;

                        await Task.Delay(500); // Optional delay for UX
                        await NavigateToMainPageAsync(); //Navigate to main Page (Dashboard)
                        await Task.Delay(2000);
                        // Face One
                        await SendGreetingAsync(); // Send Greeting to Admin
                        // Face Tow
                        await InitializeNotificationsAsync(); // Stat Notification from Lelegram

                    }
                    else
                    {
                        StatusColor = new SolidColorBrush(Colors.Red);
                        StatusMessage = _loader.GetString("LgnP_InvalidCredentialsMessage");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (DbException dbEx)
            {
                Logger.LogError(dbEx, "Database error during login.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during login.");
            }
            finally
            {
                // Always clear sensitive data
                Password = string.Empty;
            }
        }


        public async Task NavigateToMainPageAsync()
        {
            try
            {
                var rootFrame = App.MainAppWindow?.Content as Frame;

                if (rootFrame != null)
                {

                    await Task.Delay(200); // Optional delay for better UX
                    rootFrame.Navigate(typeof(MainPage));
                    //rootFrame.Content = new MainPage();


                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                    var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                    var appWindow = AppWindow.GetFromWindowId(windowId);

                    var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                    var screenWidth = displayArea.WorkArea.Width;
                    var screenHeight = displayArea.WorkArea.Height;

                    var newSize = GetWindowSize(screenWidth, screenHeight);




                    appWindow.Resize(newSize);

                    // Calculate centered position
                    int x = (screenWidth - newSize.Width) / 2;
                    int y = (screenHeight - newSize.Height) / 2;

                    // -----------------------------
                    // Get the Pupop size 
                    //------------------------------
                    App.GlobalState.PopupActualWidth = (screenWidth * 0.18);
                    App.GlobalState.PopupActualHeight = (screenHeight * 0.45);

                    appWindow.Move(new Windows.Graphics.PointInt32(x, y));

                    // Show title bar buttons
                    appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                    appWindow.TitleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);
                    appWindow.TitleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);
                    appWindow.TitleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 232, 17, 35);
                    appWindow.TitleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);


                }
                else
                {
                    StatusColor = new SolidColorBrush(Colors.Red);
                    StatusMessage = _loader.GetString("LoginStatrusMessageError");

                }

            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = string.Format(_loader.GetString("LoginErrorWithDetails"),ex.Message);
                Logger.LogError(ex, "Error loading appointments | AppointmentsViewModel");
            }
            finally {
                // Clear password after attempt
                await Task.Delay(1000);
                StatusMessage = string.Empty;
                _cts.TryReset(); // Reset the cancellation token source for future use
            }
        }

        private static string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public void ResetFields()
        {
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = string.Empty;
            _currentRole = new();
        }

        //private static Windows.Graphics.SizeInt32 GetWindowSize(int screenWidth, int screenHeight)
        //{
        //    if (screenWidth <= 1600 && screenHeight <= 900)
        //        return new Windows.Graphics.SizeInt32(1400, 800);
        //    if (screenWidth <= 1200 && screenHeight <= 760)
        //        return new Windows.Graphics.SizeInt32(1100, 700);
        //    if (screenWidth <= 1366 && screenHeight <= 768)
        //        return new Windows.Graphics.SizeInt32(1275, 700);
        //    if (screenWidth <= 1920 && screenHeight <= 1080)
        //        return new Windows.Graphics.SizeInt32(1700, 900);

        //    return new Windows.Graphics.SizeInt32(800, 600); // default
        //}

        private static Windows.Graphics.SizeInt32 GetWindowSize(int screenWidth, int screenHeight)
        {
            // Use 80% of screen width and 80% of screen height
            int targetWidth = (int)(screenWidth * 0.9);
            int targetHeight = (int)(screenHeight * 0.9);

            // Optionally enforce minimums so it doesn’t get too small
            int minWidth = 800;
            int minHeight = 600;

            return new Windows.Graphics.SizeInt32(
                Math.Max(targetWidth, minWidth),
                Math.Max(targetHeight, minHeight)
                //Math.Max(minWidth, minWidth),
                //Math.Max(minHeight, minHeight)
            );
        }

        // Face Tow
        private async Task InitializeNotificationsAsync()
        {
            try
            {

                await Task.Delay(10000);
                await TestTelegramBotAsync();
                await Task.Delay(2000);
                //await StartListeningForUpdates();
                await BlockedUsersManager.LoadBlockedUsers();

                var patientsRepo = new PatientsRepository();
                var doctorsRepo = new DoctorsRepository();
                _patientService = new PatientNotificationService(patientsRepo, this);
                _doctorService = new DoctorNotificationService(doctorsRepo, this);

                await Task.Delay(TimeSpan.FromMinutes(1));

                var now = DateTime.Now;
                if (now.Hour >= 1 && now.Hour < 23)
                {
                    Logger.LogInfo($"Starting notification services at: {now:HH:mm tt}");
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(3));
                        if (_patientService != null) await _patientService.RunNotifications();
                    });
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(9));
                        if (_doctorService != null) await _doctorService.RunNotifications();
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during async initialization. | {GetType().Name}");
            }
        }

        // Face One
        public async Task SendGreetingAsync()
        {

            var vm = new MessagingIntegrationPageViewModel();

            try
            {
                await StartListeningForUpdates();
                // Get token + admin chatId from DB
                var result = vm.DecryptFromBase64(ProviderType.Telegram);
                string token = result.Token;
                string adminId = result.AdminId;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(adminId))
                {

                    NotificationHelper.ShowNotification("Bot Testing ", "Telegram bot token not configured. Please add a token in settings or contact admin.");
                    _backgroundService.StartGreeting("Telegram bot token not configured. Please add a token in settings or contact admin.");
                    await Task.Delay(5000); // Show notification for a while
                    Logger.LogInfo("Token or AdminId missing, cannot send greeting.");
                    //await Task.Delay(10000, _cts.Token); // Show notification for a while    
                    return;
                }

                // Check if today is a working day
                DayOfWeek today = DateTime.Now.DayOfWeek;
                bool notifyToday = today switch
                {
                    DayOfWeek.Monday => App.GlobalState.NotifyOnMonday,
                    DayOfWeek.Tuesday => App.GlobalState.NotifyOnTuesday,
                    DayOfWeek.Wednesday => App.GlobalState.NotifyOnWednesday,
                    DayOfWeek.Thursday => App.GlobalState.NotifyOnThursday,
                    DayOfWeek.Friday => App.GlobalState.NotifyOnFriday,
                    DayOfWeek.Saturday => App.GlobalState.NotifyOnSaturday,
                    DayOfWeek.Sunday => App.GlobalState.NotifyOnSunday,
                    _ => false
                };


                if (!notifyToday)
                {
                    Logger.LogInfo($"Greeting skipped: notifications disabled for {today}.");
                    return;
                }
                else if (!App.GlobalState.SendViaTelegram) // added this code to disabled greeting if (Send Via Telegram)
                {
                    Logger.LogInfo($"Greeting skipped: Send Via Telegram Is Disabled.");
                    return;

                }


                // Reset counter daily at 5 AM
                if (DateTime.Now.Hour >= 5 && _lastGreetingDate.Date < DateTime.Now.Date)
                {
                    _lastGreetingDate = DateTime.MinValue;
                    _lastGreetingPeriod = string.Empty;
                }

                string greetingText = string.Empty;
                string period = string.Empty;

                // Morning: 6–10 AM
                if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 12)
                {
                    var userName = App.GlobalState.LoggedUserName;
                    greetingText = $"Good morning {userName} 🌞, Bot is running at {DateTime.Now:HH:mm}.";
                    period = "morning";
                }
                // Afternoon: 12–17 (12 PM–5 PM)
                else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 18)
                {
                    var userName = App.GlobalState.LoggedUserName;
                    greetingText = $"Good afternoon {{{userName}}} 🌤️, Bot is running at {DateTime.Now:HH:mm}.";
                    period = "afternoon";
                }
                // Evening: 18–20 (6 PM–8 PM)
                else if (DateTime.Now.Hour >= 18 && DateTime.Now.Hour <= 23)
                {
                    var userName = App.GlobalState.LoggedUserName;
                    greetingText = $"Good evening {userName} 🌆, Bot is running at {DateTime.Now:HH:mm}.";
                    period = "evening";
                }

                // Only send if greeting not already sent for this period today
                if (!string.IsNullOrEmpty(greetingText) &&
                    (_lastGreetingDate.Date != DateTime.Now.Date || _lastGreetingPeriod != period) && BotClient != null)
                {
                    await BotClient.SendMessage(chatId: adminId, text: greetingText);
                    Logger.LogInfo($"{period} greeting sent to owner.");

                    NotificationHelper.ShowNotification("Bot Testing ", $"{greetingText}");

                    // Update counter
                    _lastGreetingDate = DateTime.Now;
                    _lastGreetingPeriod = period;
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error sending greeting. | {GetType().Name}");
            }
        }

        public async Task StartListeningForUpdates()
        {
            var vm = new MessagingIntegrationPageViewModel();

            try
            {
                if (string.IsNullOrEmpty(vm.DecryptFromBase64(ProviderType.Telegram).Token))
                {
                    Logger.LogInfo("Telegram token not found. Cannot start listening for updates.");
                    NotificationHelper.ShowNotification("Telegram token", "Telegram token not found. Cannot start listening for updates.");
                    return;
                }
                var result = vm.DecryptFromBase64(ProviderType.Telegram);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { } // receive all update types
                };

                string token = result.Token;

                

                BotClient = new TelegramBotClient(token);
                var updateHandler = new MyUpdateHandler();


                await Task.Delay(500,_cts.Token);


                BotClient?.StartReceiving(
                        updateHandler: updateHandler,
                        receiverOptions: receiverOptions,
                        cancellationToken: CancellationToken.None
                    );
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.IndianRed);
                StatusMessage = _loader.GetString("MiP_StatusMessage_TelegramBotTestFailed");
                Logger.LogError(ex, $"Error testing Telegram bot. {GetType().Name}");

                var inner = ex.InnerException?.ToString() ?? "No inner exception";
                // Log or show this to diagnose the exact TLS/cert/proxy issue
                Logger.LogError(ex,$"Inner: {inner}\n {GetType().Name}");
            }

        }

        private async Task TestTelegramBotAsync()
        {
            var vm = new MessagingIntegrationPageViewModel();
            var result = vm.DecryptFromBase64(ProviderType.Telegram);
            try
            {
                // Step 1: Get decrypted token

                string token = result.Token;

                if (!App.GlobalState.SendViaTelegram) // added this code to disabled greeting if (Send Via Telegram)
                {
                    NotificationHelper.ShowNotification(
                        "Initialize Notifications",
                        "Send via Telegram is disabled. All notifications have been stopped. Please enable (Send via Telegram) in Settings or contact the administrator."
                        );
                    Logger.LogInfo($"Greeting skipped: Send Via Telegram Is Disabled.");
                    return;

                }

                if (string.IsNullOrEmpty(token))
                {

                    NotificationHelper.ShowNotification(
                        "Initialize Notifications",
                        "Telegram bot token is not configured. All notifications have been stopped. Please add a token in Settings or contact the administrator."
                    );
                    Logger.LogInfo($"Telegram notifications disabled, skipping initialization. {this.GetType().Name}");

                    StatusColor = new SolidColorBrush(Colors.IndianRed);
                    StatusMessage = _loader.GetString("MiP_StatusMessage_NoTelegramToken");
                    await Task.Delay(5000, _cts.Token); // Show notification for a while                    
                    return;
                }                

                // Step 2: Initialize bot client
                //var botClient = new TelegramBotClient(token);

                // Step 3: Call GetMe to test
                await Task.Delay(500); // Small delay to simulate async operation
                if (BotClient != null)
                {
                    var me = await BotClient.GetMe();
                    // Step 4: Update status
                    StatusColor = new SolidColorBrush(Colors.Teal);
                    StatusMessage = string.Format(_loader.GetString("MiP_StatusMessage_TelegramBotValid"), me.Username, me.Id);
                    Logger.LogInfo($"Telegram bot test successful: {me.Username} ({me.Id})");
                }

                // send real messge 

                await Task.Delay(3000);
                StatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                StatusColor = new SolidColorBrush(Colors.IndianRed);
                StatusMessage = _loader.GetString("MiP_StatusMessage_TelegramBotTestFailed");
                Logger.LogError(ex, $"Error testing Telegram bot. | {GetType().Name}");
            }
        }

        // In LoginPageViewModel
        public void DisposeBotClient()
        {
            string client;
            try
            {
                _cts.Cancel(); // stop receiving
                BotClient = null; // release reference
                client = BotClient == null ? "Null" : BotClient.ToString()!;

                Logger.LogInfo($"BotClient disposed cleanly.| BotClinet: {client}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error disposing BotClient. | {GetType().Name}");
            }
        }

        public void ClearMemoryLoging()
        {
            // Cancel and reset CTS
            //_cts.Cancel();

            // Clear sensitive login data
            Username = string.Empty;
            Password = string.Empty;
            PasswordHash = string.Empty;

            // Reset role and user list
            _currentRole = new RoleManager();
            _usersList.Clear();
            UserList.Clear();

            // Reset UI state
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            // Reset greeting counters
            _lastGreetingDate = DateTime.MinValue;
            _lastGreetingPeriod = string.Empty;

            // Stop notification services
            _patientService = null;
            _doctorService = null;
        }
    }
}
