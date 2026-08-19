using LiteClinic.Services;
using Microsoft.UI.Xaml.Controls;
using System;

namespace YourApp.Services
{
    public class FrameNavigationService : INavigationService
    {
        private readonly Frame _frame;

        public FrameNavigationService(Frame frame)
        {
            _frame = frame;
        }

        public void Navigate(Type pageType)
        {
            _frame.Navigate(pageType);
        }
    }
}