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
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPageViewModel ViewModel { get; set; }

        public LoginPage()
        {
            InitializeComponent();

            // Force modern TLS before any network calls
            System.Net.ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            Logger.LogInfo($"TLS protocols enabled: {ServicePointManager.SecurityProtocol} | {this.GetType().Name}");

            ViewModel = new LoginPageViewModel();
            this.DataContext = ViewModel;
            ViewModel.ResetFields(); // clear username/password
            Loaded += async (s, e) =>
            {
                try
                {
                    if (App.MainAppWindow != null)
                        await WindowService.ConfigureMainWindowAsync(App.MainAppWindow);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "LoginPage window configuration failed");
                }
            };

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is LoginPageViewModel vm)
            {
                vm.ClearMemoryLoging();
            }
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginPageViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }

#pragma warning disable CA1822 // Mark members as static
        public async Task ConfigureMainWindowAsync(Window _window)
#pragma warning restore CA1822 // Mark members as static
        {
            // Get HWND and WindowId
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

            // Calculate centered position
            int screenWidth = displayArea.WorkArea.Width;
            int screenHeight = displayArea.WorkArea.Height;
            int windowWidth = 1020;
            int windowHeight = 620;
            int x = (screenWidth - windowWidth) / 2;
            int y = (screenHeight - windowHeight) / 2;

            // Get AppWindow and apply size/position
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));
            appWindow.Move(new Windows.Graphics.PointInt32(x, y));

            // Title Bar customization
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
            appWindow.Title = string.Empty;

            // Since all calls are synchronous, just complete the Task
            await Task.CompletedTask;
        }


    }
}
