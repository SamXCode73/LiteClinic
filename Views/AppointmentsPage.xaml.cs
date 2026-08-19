using LiteClinic.Models;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
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
    public sealed partial class AppointmentsPage : Page
    {
        public AppointmentsViewModel ViewModel { get; set; }

        public AppointmentsPage()
        {
            InitializeComponent();

            ViewModel = new AppointmentsViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            try
            {
                if (DataContext is AppointmentsViewModel vm)
                {
                    vm.OnNavigatedFrom();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ERROR_WHEN_NAVIGATE_FROM_APPOINTMENTS");
            }
        }

        private void ListView_DoubleTappedPatient(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is AppointmentsViewModel vm && vm.SelectedPatient != null)
            {
                // Populate fields from SelectedDoctor

                vm.PatientAutoId = vm.SelectedPatient.PatientAutoId;
                vm.PatientName = vm.SelectedPatient.PatientFullName;
                vm.DateOfBirth = vm.SelectedPatient.DateOfBirth;
                DoctorListAppointment.Focus(FocusState.Programmatic);
                DoctorListAppointment.Background = new SolidColorBrush(Colors.LightGreen);
                //DoctorListAppointment.IsDropDownOpen = true;                
            }
            //if (DataContext is AppointmentsViewModel Viewmode)
            //{
            //    CalendarDatePickerAppointment.Date = Viewmode.AppointmentDate;
            //}
        }

        private void ListView_DoubleTappedAppointment(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is AppointmentsViewModel vm && vm.SelectedScheduledAppointmentDisplay != null)
            {
                // Populate fields from SelectedDoctor
                vm.ScheduleId = vm.SelectedScheduledAppointmentDisplay.ScheduleId;
                vm.AppointmentID = vm.SelectedScheduledAppointmentDisplay.AppointmentID;
                vm.PatientAutoId = vm.SelectedScheduledAppointmentDisplay.PatientId;
                vm.PatientName = vm.SelectedScheduledAppointmentDisplay.PatientName;
                vm.DateOfBirth = vm.SelectedScheduledAppointmentDisplay.PatientDOBFormatted;
                vm.AppointmentDate = vm.SelectedScheduledAppointmentDisplay.AppointmentDate;
                vm.AppointmentTime = vm.SelectedScheduledAppointmentDisplay.AppointmentTime;
                vm.AppointmentType = vm.SelectedScheduledAppointmentDisplay.AppointmentType;
                vm.Notes = vm.SelectedScheduledAppointmentDisplay.Notes;
                vm.IsActive = vm.SelectedScheduledAppointmentDisplay.IsActive;
                vm.IsMIssed = vm.SelectedScheduledAppointmentDisplay.IsMissed;

                // Find the matching doctor object
                var selectedDoctor = vm.DoctorList.FirstOrDefault(d => d.DoctorId == vm.SelectedScheduledAppointmentDisplay.DoctorId);
                vm.SelectedDoctorListforAppoitnment = selectedDoctor;
            }
        }

        private void AppointmetnPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_SaveCommand!.CanExecute(null))
                        ViewModel.Btn_SaveCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.Btn_UpdateCommand!.CanExecute(null))
                        ViewModel.Btn_UpdateCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivateCommand!.CanExecute(null))
                        ViewModel.Btn_DeactivateCommand.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearCommand!.CanExecute(null))
                        ViewModel.Btn_ClearCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }

        private void DoctorListAppointment_LostFocus(object sender, RoutedEventArgs e)
        {
            DoctorListAppointment.ClearValue(Control.BackgroundProperty);
        }

        private void ComboBox_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            TimeStampAppointment.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TimeStampAppointment.ClearValue(Control.BackgroundProperty);
        }

        private void CalendarDatePickerAppointment_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            CalendarDatePickerAppointment.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void CalendarDatePickerAppointment_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            CalendarDatePickerAppointment.ClearValue(Control.BackgroundProperty);
        }

        private void NotesAppointment_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            NotesAppointment.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void NotesAppointment_LostFocus(object sender, RoutedEventArgs e)
        {
            NotesAppointment.ClearValue(Control.BackgroundProperty);
        }

        //private void ApplyBlackoutDates(List<DateTime> validDates, int year, int month)
        //{
        //    // Clear any previous blackout ranges
            

        //    // Build a hash set for quick lookup
        //    var allowed = new HashSet<DateTime>(validDates);

        //    // Get the start and end of the month
        //    var monthStart = new DateTime(year, month, 1);
        //    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        //    // Loop through each day in the month
        //    for (var date = monthStart; date <= monthEnd; date = date.AddDays(1))
        //    {
        //        // If the date is NOT in the allowed list, blackout it
        //        if (!allowed.Contains(date))
        //        {
        //            //CalendarDatePickerAppointment.BlackoutDates.Add(new CalendarDateRange(date));
        //        }
        //    }
        //}
    }
}
