using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            ViewModel = new DashboardViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel.InitializeAsync();
            };

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is DashboardViewModel vm)
            {
                vm.OnNavigatedFrom();
            }
        }

        private async void ShowStatus()
        {
            // Initial status message
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string ?? "en-US";
            string keyEN = "Load_Success";
            string keyAR = "Load_SuccessAR";

            string key = savedLang == "en-US" ? keyEN : keyAR;
            SolidColorBrush color = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 0));
            await InfoBarMessages.ShowStatusAsync(StatusInfoTextDashboard, key, color);
        }

        private async void DoctorStackPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is DoctorWeeklySummary doctor)
            {

                await ChangePatientStatus(doctor);

            }
        }

        private async Task ChangePatientStatus(DoctorWeeklySummary doctor)
        {
            if (DataContext is DashboardViewModel vm)
            {


                vm.SelectedDoctorName = string.Empty;

                vm.SelectedDoctorSpecialty = string.Empty;
               
                vm.SelectedDoctorId = doctor.DoctorId;
                vm.SelectedDoctorDate = doctor.AppointmentDate;

                //vm.IsListVisible = false;
                //PatienFilterInDashBoard.Visibility = Visibility.Collapsed;
                await vm.FilterAppointmentsByDoctor(doctor.DoctorId, doctor.AppointmentDate);
                
                vm.SelectedDoctorName = doctor.DoctorName;

                //PatienFilterInDashBoard.Visibility = Visibility.Visible;
                vm.SelectedDoctorSpecialty = doctor.DoctorSpecialty;
                if (vm.IsListVisible)
                {
                    return;
                }
                else
                {
                    vm.IsListVisible = true;
                }

            }
        }

        private async void RadioButton_Tapped(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.DataContext is ScheduledAppointmentDisplay patient)
            {
                if (DataContext is DashboardViewModel vm)
                {

                    vm.SelectedDisplayPatient = patient;
                    await vm.SetAppointmentStatus();

                    // Refresh the patient list to reflect changes
                    //await vm.LoadDisplayAppointmentsAsync();

                    if (vm.IsListVisible) vm.IsListVisible = false;
                    await vm.FilterAppointmentsByDoctor(vm.SelectedDoctorId, vm.SelectedDoctorDate);
                    vm.IsListVisible = true;

                    //   TODO:Recalculate the Summery in the DashBoardPage


                }
            }
        }
    
    }
}
