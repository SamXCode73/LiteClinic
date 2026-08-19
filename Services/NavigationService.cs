using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public static class NavigationService
    {
        public static Frame? RootFrame =>
            App.MainAppWindow?.Content as Frame;

        public static bool NavigateTo<TPage>() where TPage : class
        {
            var frame = RootFrame;
            if (frame == null) return false;
            frame.BackStack.Clear(); // keep history clean
            return frame.Navigate(typeof(TPage));
        }

        public static void ResetContent<TPage>() where TPage : class
        {
            var frame = RootFrame;
            if (frame == null) return;
            frame.BackStack.Clear();
            frame.Navigate(typeof(TPage));
        }
    }
}
