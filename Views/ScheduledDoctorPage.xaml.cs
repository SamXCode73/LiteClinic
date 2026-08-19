using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduledDoctorPage : Page
    {
        public ScheduledDoctorViewModel ViewModel { get; set; }

        public ScheduledDoctorPage()
        {
            InitializeComponent();
            ViewModel = new ScheduledDoctorViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is ScheduledDoctorViewModel vm)
            {
                vm.OnNavigatedFrom();
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is ScheduledDoctorViewModel vm && vm.SelectedScheduledDisplay != null)
            {
                
                vm.ScheduledAutoId = vm.SelectedScheduledDisplay.ScheduleAutoIdDis;
                vm.DoctorId = vm.SelectedScheduledDisplay.DoctorIdDis;
                vm.DayOfTheWeek = vm.SelectedScheduledDisplay.DayOfWeekDis;
                vm.CanNotify = vm.SelectedScheduledDisplay.NotifyDis;
                vm.IsScheduleActiveDis = vm.SelectedScheduledDisplay.IsScheduleActiveDis;                
                vm.IsWeek1 = vm.SelectedScheduledDisplay.IsWeek1;
                vm.IsWeek2 = vm.SelectedScheduledDisplay.IsWeek2;
                vm.IsWeek3 = vm.SelectedScheduledDisplay.IsWeek3;
                vm.IsWeek4 = vm.SelectedScheduledDisplay.IsWeek4;
                vm.IsWeek5 = vm.SelectedScheduledDisplay.IsWeek5;
                //Parse the TimeSpan
                var timeSpanFromTo = vm.SelectedScheduledDisplay.TimeDis;

                if (!string.IsNullOrWhiteSpace(timeSpanFromTo) &&
                    !string.Equals(timeSpanFromTo, "Not Set", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = timeSpanFromTo.Split('-');
                    if (parts.Length == 2)
                    {
                        // Use ParseExact with explicit format + InvariantCulture
                        if (DateTime.TryParseExact(parts[0].Trim(), "hh\\:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                            vm.DisTimeFrom = fromDate.TimeOfDay;

                        if (DateTime.TryParseExact(parts[1].Trim(), "hh\\:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                            vm.DisTimeTo = toDate.TimeOfDay;
                    }
                }
                else
                {
                    vm.DisTimeFrom = null;
                    vm.DisTimeTo = null;
                }
            }
        }

        private void Button_Click_BackToDoctorPage(object sender, RoutedEventArgs e)
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(DoctorsPage));
            App.GlobalState.UpdateSubtitle("DoctorsMenu/Text");
        }

        private void ScheduledPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_SaveDcotorScheduledCommand!.CanExecute(null))
                        ViewModel.Btn_SaveDcotorScheduledCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.Btn_UpdateDcotorScheduledCommand!.CanExecute(null))
                        ViewModel.Btn_UpdateDcotorScheduledCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivateDoctorScheduledCommadn!.CanExecute(null))
                        ViewModel.Btn_DeactivateDoctorScheduledCommadn.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearCommand!.CanExecute(null))
                        ViewModel.Btn_ClearCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }
    }
}
