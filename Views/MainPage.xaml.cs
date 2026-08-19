using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;
using Windows.Storage;
using Windows.UI.Text;
using YourApp.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; set; }
        private bool isSidebarOpen = false;
        public LoginPageViewModel? LoginViewModel { get; }

        public MainPage()
        {
            InitializeComponent();

            // Create navigation service using your ContentFrame
            var navigationService = new FrameNavigationService(ContentFrame);

            // Pass it into the ViewModel
            ViewModel = new MainPageViewModel();
            this.DataContext = ViewModel;


            ChangeFontSizeForArbic();
            ChangeLayoutDirection();

            // Set initial page to Dashboard
            ContentFrame.Navigate(typeof(MainDbPage));
            InitializeThemedImages();
            //ChangeSubTitle();
            ContentFrame.Navigated += ContentFrame_Navigated;

            this.Unloaded += MainPage_Unloaded;

        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.LogUserEvent($"User session ended by closing window. " +
                    $"Role: {App.GlobalState.LoggedUserRoleId} | " +
                    $"ID: {App.GlobalState.LoggedUserId} | User: {App.GlobalState.LoggedUserName} | " +
                    $"Windows User: {Environment.UserName}, " +
                    $"Action: Main Window Closed", "LOG OUT AND EXIT");

                // Clear Botlient when closing for safty.
                LoginViewModel?.DisposeBotClient();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while closing the main window. | {GetType().Name}");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set the spinner image source
            LoadingSpinner.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("spinner.svg")));

            // Create and start the rotation animation
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, LoadingSpinnerRotateTransform);
            Storyboard.SetTargetProperty(animation, "Angle");
            storyboard.Begin();
        }

        internal static Frame? GetContentFrame()
        {
            var rootFrame = App.MainAppWindow!.Content as Frame;
            var mainPage = rootFrame?.Content as MainPage;
            return mainPage?.ContentFrame;
        }

        private void HamburgerMenu_Tapped(object sender, RoutedEventArgs e)
        {
            var langoption = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string ?? "en-US";
            ;

            //isSidebarOpen = !isSidebarOpen;
            EnglishSidebarColumn.Width = isSidebarOpen ? new GridLength(180) : new GridLength(60);

            if (langoption == "ar")
            {
                HamburgerMenu.Source = isSidebarOpen
                    ? new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("lists_96r.svg")))
                    : new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("more_vert_96r.svg")));

            }
            else
            {
                HamburgerMenu.Source = isSidebarOpen
                    ? new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("lists_96.svg")))
                    : new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("more_vert_96.svg")));
            }



            HomeLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            DoctorsLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            PatientsLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            AppointmentsLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            UsersLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            ReportsLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            SettingsLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            AboutLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            LogoutLabel.Visibility = isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;

            isSidebarOpen = !isSidebarOpen;

        }

        //Dashboard
        private async void DashboardMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                // 2. Check logic from ViewModel
                if (ContentFrame.Content is MainDbPage)
                {
                    await ShakeMenu(DashboardShakeTransform);
                }
                else
                {
                    LoadingSpinner.Visibility = Visibility.Visible;
                    ChangeFontSizeOnClick(HomeLabel, DashboardMenu);

                    await Task.Delay(300);
                    ContentFrame.Navigate(typeof(MainDbPage));
                    App.GlobalState.UpdateSubtitle("Dashboard/Text");

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ERROR_WHEN_NAVIGATE_FROM_DASHBOARD");
            }
        }

        // Doctors
        private async void DoctorsMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on DoctorsPage, just shake and stop
            if (ContentFrame.Content is DoctorsPage)
            {
                await ShakeMenu(DoctorsShakeTransform);
                return; // Stop here so we don't navigate or reload data
            }

            // 2. Start Visual Feedback
            LoadingSpinner.Visibility = Visibility.Visible;
            ChangeFontSizeOnClick(DoctorsLabel, DoctorsMenu);

            // 3. Small delay for visual feedback
            await Task.Delay(300);

            // 4. Perform the Navigation
            ContentFrame.Navigate(typeof(DoctorsPage));

            // 5. Update Global Title (MainPageViewModel is listening for this)
            // Make sure "Doctors" is a key in your en-US and ar-SA resource files
            App.GlobalState.UpdateSubtitle("DoctorsMenu/Text");

            // 6. Hide the spinner
            //LoadingSpinner.Visibility = Visibility.Collapsed;
        }

        // Patients
        private async void PatientsMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on PatientsPage, just shake and stop
            if (ContentFrame.Content is PatientsPage)
            {
                await ShakeMenu(PatientsShakeTransform);
            }
            else
            {
                // 2. Start Visual Feedback
                LoadingSpinner.Visibility = Visibility.Visible;
                ChangeFontSizeOnClick(PatientsLabel, PatientsMenu);

                // 3. Small delay for visual feedback
                await Task.Delay(300);

                // 4. Perform the Navigation
                ContentFrame.Navigate(typeof(PatientsPage));

                // 5. Update Global Title (MainPageViewModel is listening for this)
                App.GlobalState.UpdateSubtitle("Patients/Text");

                // 6. Hide the spinner
                //LoadingSpinner.Visibility = Visibility.Collapsed;
            }
        }

        // Appointments
        private async void AppointmentsMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                // 1. Safety check: If already on AppointmentsPage, just shake and stop
                if (ContentFrame.Content is AppointmentsPage)
                {
                    await ShakeMenu(AppointmentsShakeTransform);
                }
                else
                {
                    // 2. Start Visual Feedback
                    LoadingSpinner.Visibility = Visibility.Visible;
                    ChangeFontSizeOnClick(AppointmentsLabel, AppointmentsMenu);

                    // 3. Small delay for visual feedback
                    await Task.Delay(300);

                    // 4. Perform the Navigation
                    ContentFrame.Navigate(typeof(AppointmentsPage));

                    // 5. Update Global Title (MainPageViewModel is listening for this)
                    App.GlobalState.UpdateSubtitle("Appointments/Text");

                    // 6. Hide the spinner
                    LoadingSpinner.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ERROR_FROM_NAVIGATING_FROM_APP");
            }
        }

        // Users
        private async void UsersMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on UsersPage, just shake and stop
            if (ContentFrame.Content is UsersPage)
            {
                await ShakeMenu(UsersShakeTransform);
            }
            else
            {
                // 2. Start Visual Feedback
                LoadingSpinner.Visibility = Visibility.Visible;
                ChangeFontSizeOnClick(UsersLabel, UsersMenu);

                // 3. Small delay for visual feedback
                await Task.Delay(300);

                // 4. Perform the Navigation
                ContentFrame.Navigate(typeof(UsersPage));

                // 5. Update Global Title (MainPageViewModel is listening for this)
                App.GlobalState.UpdateSubtitle("Users/Text");

                // 6. Hide the spinner
                //LoadingSpinner.Visibility = Visibility.Collapsed;
            }
        }

        // Reports
        private async void ReportsMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on ReportsPage, just shake and stop
            if (ContentFrame.Content is ReportsPage)
            {
                await ShakeMenu(ReportsShakeTransform);
            }
            else
            {
                // 2. Start Visual Feedback
                LoadingSpinner.Visibility = Visibility.Visible;
                ChangeFontSizeOnClick(ReportsLabel, ReportsMenu);

                // 3. Small delay for visual feedback
                await Task.Delay(300);

                // 4. Perform the Navigation
                ContentFrame.Navigate(typeof(ReportsPage));

                // 5. Update Global Title (MainPageViewModel is listening for this)
                App.GlobalState.UpdateSubtitle("Reports/Text");

                // 6. Hide the spinner
                //LoadingSpinner.Visibility = Visibility.Collapsed;
            }
        }

        // Settings        
        private async void SettingsMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on SettingsPage, just shake and stop
            if (ContentFrame.Content is SettingsPage)
            {
                await ShakeMenu(SettingsShakeTransform);
            }
            else
            {
                // 2. Start Visual Feedback
                LoadingSpinner.Visibility = Visibility.Visible;
                ChangeFontSizeOnClick(SettingsLabel, SettingsMenu);

                // 3. Small delay for visual feedback
                await Task.Delay(300);

                // 4. Perform the Navigation
                ContentFrame.Navigate(typeof(SettingsPage));

                // 5. Update Global Title (MainPageViewModel is listening for this)
                App.GlobalState.UpdateSubtitle("Settings/Text");

                // 6. Hide the spinner
                LoadingSpinner.Visibility = Visibility.Collapsed;
            }
        }

        // About Page
        private async void AboutMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Safety check: If already on AboutPage, just shake and stop
            if (ContentFrame.Content is AboutPage)
            {
                await ShakeMenu(AboutShakeTransform);
            }
            else
            {
                // 2. Start Visual Feedback
                LoadingSpinner.Visibility = Visibility.Visible;
                ChangeFontSizeOnClick(AboutLabel, AboutMenu);

                // 3. Small delay for visual feedback
                await Task.Delay(300);

                // 4. Perform the Navigation
                ContentFrame.Navigate(typeof(AboutPage));

                // 5. Update Global Title (MainPageViewModel is listening for this)
                App.GlobalState.UpdateSubtitle("About/Text");

                // 6. Hide the spinner
                LoadingSpinner.Visibility = Visibility.Collapsed;
            }
        }

        // Logout Page

        private async void Tapped_LogOut(object sender, TappedRoutedEventArgs e)
        {
            //SubTitile.Text = LanguageManager.CurrentLanguage == "en-US" ? "Logout" : " ”ÃÌ· «·Œ—ÊÃ";

            LoadingSpinner.Visibility = Visibility.Visible;
            ChangeFontSizeOnClick(LogoutLabel, LogoutMenu);

            Logger.LogUserEvent($"Loged in User - Role: {App.GlobalState.LoggedUserRoleId} | " +
                $"ID: {App.GlobalState.LoggedUserId} | User: {App.GlobalState.LoggedUserName} | " +
                $"Windows User: {Environment.UserName}, " +
                $"Action: User Logged Out", "Login Page");


            await Task.Delay(300);

            // Replace the root content with LoginPage (outside the frame)
            var rootFrame = App.MainAppWindow!.Content as Frame;
            if (rootFrame != null)
            {
                rootFrame.Navigate(typeof(LoginPage));

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                var newSize = new Windows.Graphics.SizeInt32(1020, 620);
                appWindow.Resize(newSize);

                // Get screen size
                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                var screenWidth = displayArea.WorkArea.Width;
                var screenHeight = displayArea.WorkArea.Height;

                // Calculate centered position
                int x = (screenWidth - newSize.Width) / 2;
                int y = (screenHeight - newSize.Height) / 2;

                appWindow.Move(new Windows.Graphics.PointInt32(x, y));

                // Show title bar buttons
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                appWindow.TitleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);
                appWindow.TitleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);
                appWindow.TitleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 232, 17, 35);
                appWindow.TitleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);

                
                App.GlobalState.LoggedUserName = string.Empty;
                App.GlobalState.LoggedUserId = string.Empty;
                App.GlobalState.LoggedUserRoleId = 0;
                App.GlobalState.CurrentRole = null;
            }

        }

        private static async Task ShakeMenu(TranslateTransform transform)
        {
            double[] offsets = { -5, 5, -3, 3, 0 };
            foreach (var offset in offsets)
            {
                transform.X = offset;
                await Task.Delay(50);
            }
        }

        private void ChangeFontSizeForArbic()
        {
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(savedLang))
            {
                if (savedLang == "ar")
                {
                    // Change font size for Arabic
                    HomeLabel.FontSize = 16; // Example size for Arabic
                    DoctorsLabel.FontSize = 16;
                    PatientsLabel.FontSize = 16;
                    AppointmentsLabel.FontSize = 16;
                    UsersLabel.FontSize = 16;
                    ReportsLabel.FontSize = 16;
                    SettingsLabel.FontSize = 16;
                    AboutLabel.FontSize = 16;
                    LogoutLabel.FontSize = 16;

                }
            }
        }
        private void ChangeLayoutDirection()
        {
            var selectedlang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(selectedlang))
            {
                if (selectedlang == "ar")
                {
                    // Change layout to Right-to-Left for Arabic
                    this.FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    // Change layout to Left-to-Right for other languages
                    this.FlowDirection = FlowDirection.LeftToRight;
                }
            }
        }

        private void InitializeThemedImages()
        {
            var langoption = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string ?? "en-US";

            if (langoption == "ar")
            {
                // Change layout to Right-to-Left for Arabic
                HamburgerMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("lists_96r.svg")));

            }
            else
            {
                HamburgerMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("lists_96.svg")));
            }

            DashboardMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("dashboard_96.svg")));
            DoctorsMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("stethoscope_check_96.svg")));
            PatientsMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("patient_list_96.svg")));
            AppointmentsMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("calendar_clock_96.svg")));
            ReportsMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("assignment_96.svg"))); // fixed typo
            UsersMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("supervisor_account_96.svg")));
            SettingsMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("settings_96.svg")));
            AboutMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("help_96.svg")));
            LogoutMenu.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("logout_96.svg"))); // changed to logout icon
            LoadingSpinner.Source = new SvgImageSource(new Uri(ThemeManager.GetThemedImagePath("spinner.svg"))); // Spinner Image

        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.Child is StackPanel panel)
            {
                var tb = panel.Children.OfType<TextBlock>().FirstOrDefault();
                var img = panel.Children.OfType<Image>().FirstOrDefault();

                if (tb != null && img != null && border != null)
                {
                    ChangeFontOnHoverEnter(tb, img);
                    AddBorderToMenuItemOnEnter(border);

                }
                else
                {
                    return;
                }
            }
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.Child is StackPanel panel)
            {
                var tb = panel.Children.OfType<TextBlock>().FirstOrDefault();
                var img = panel.Children.OfType<Image>().FirstOrDefault();

                if (tb != null && img != null && border != null)
                {

                    ChangeFontOnHoverLeave(tb, img);
                    RemoveBorderToMenuItemOnExitr(border);
                }
                else
                {
                    return;
                }
            }
        }

        private void ChangeFontOnHoverEnter(TextBlock tb, Image img)
        {
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(savedLang))
            {
                
                if (savedLang == "ar")
                {
                    tb.FontSize = 18;
                    img.Width = 26; img.Height = 26;
                }
                else 
                {
                    tb.FontSize = 16;
                    img.Width = 26; img.Height = 26;

                }
            }
        }

        private void AddBorderToMenuItemOnEnter(Border border)
        {
            border.BorderThickness = new Thickness(1);
            //border.BorderBrush = new SolidColorBrush(Colors.RoyalBlue); // Example color
        }

        private void RemoveBorderToMenuItemOnExitr(Border border)
        {
            border.BorderThickness = new Thickness(0);
        }


        private void ChangeFontOnHoverLeave(TextBlock tb, Image img)
        {
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(savedLang))
            {
                if (savedLang == "ar")
                {
                    tb.FontSize = 16;
                    img.Width = 24; img.Height = 24;
                }
                else
                {
                    tb.FontSize = 14;
                    img.Width = 24; img.Height = 24;

                }
            }
        }

        private void ChangeFontSizeOnClick(TextBlock tb, Image img)
        {
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(savedLang))
            {
                if (savedLang == "ar")
                {
                    tb.FontSize = 12;
                    img.Width = 16; img.Height = 16;
                    tb.FontSize = 14;
                    img.Width = 24; img.Height = 24;
                }
                else
                {
                    tb.FontSize = 12;
                    img.Width = 16; img.Height = 16;
                    tb.FontSize = 14;
                    img.Width = 24; img.Height = 24;

                }
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {            
            LoadingSpinner.Visibility = Visibility.Collapsed;
        }

    }

}
