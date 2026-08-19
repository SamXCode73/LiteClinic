using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Windows.Graphics;

namespace LiteClinic.Services
{
    public static class WindowService
    {
        public static async Task ConfigureMainWindowAsync(Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

            int screenWidth = displayArea.WorkArea.Width;
            int screenHeight = displayArea.WorkArea.Height;
            int windowWidth = 1020;
            int windowHeight = 620;
            int x = (screenWidth - windowWidth) / 2;
            int y = (screenHeight - windowHeight) / 2;

            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(windowWidth, windowHeight));
            appWindow.Move(new PointInt32(x, y));

            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
            appWindow.Title = string.Empty;

            await Task.CompletedTask;
        }
    }
}