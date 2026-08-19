using LiteClinic.Models;
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
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PatientsPage : Page
    {
        public PatientsViewModel ViewModel { get; set; }

        public PatientsPage()
        {
            InitializeComponent();
            ViewModel = new PatientsViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is PatientsViewModel vm)
            {                
                vm.ClearPatientMemory();
            }
        }

        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is PatientsViewModel vm && vm.SelectedPatient != null)
            {
                // Populate fields from SelectedUser
                vm.FirstName = vm.SelectedPatient.FirstName;
                vm.MiddleName = vm.SelectedPatient.MiddleName;
                vm.LastName = vm.SelectedPatient.LastName;
                vm.FullMotherName = vm.SelectedPatient.FullMotherName;
                vm.CivilRecord = vm.SelectedPatient.CivilRecord;
                vm.Gender = vm.SelectedPatient.Gender;

                // add the Dateofbirth code here

                if (DateTime.TryParse(vm.SelectedPatient.DateOfBirth, out DateTime dob))
                {
                    vm.StringDay = dob.Day.ToString("00");     // e.g., "01"
                    vm.StringMonth = dob.Month.ToString("00"); // e.g., "12"
                    vm.StringYear = dob.Year.ToString();       // e.g., "1960"
                }

                //vm.StringDay = vm.SelectedPatient.StringDay;
                //vm.StringMonth = vm.SelectedPatient.StringMonth;
                //vm.StringYear = vm.SelectedPatient.StringYear;

                vm.PhoneNumber = vm.SelectedPatient.PhoneNumber;
                vm.Email = vm.SelectedPatient.Email;
                vm.Address = vm.SelectedPatient.Address;
                vm.City = vm.SelectedPatient.City;
                vm.Country = vm.SelectedPatient.Country;
                vm.GotInsurance = vm.SelectedPatient.GotInsurance;
                vm.InsuranceName = vm.SelectedPatient.InsuranceName;
                vm.InsuranceNumber = vm.SelectedPatient.InsuranceNumber;
                vm.GotNSN = vm.SelectedPatient.GotNSN;
                vm.NSNName = vm.SelectedPatient.NSNName;
                vm.NSNNumber = vm.SelectedPatient.NSNNumber;
                vm.BloodType = vm.SelectedPatient.BloodType;
                vm.Allergies = vm.SelectedPatient.Allergies;
                vm.MedicalHistory = vm.SelectedPatient.MedicalHistory;
                vm.Language = vm.SelectedPatient.Language switch
                    {
                        "en" => "English",
                        "fr" => "French",
                        "ar" => "Arabic",
                        _ => "English"
                    };
                vm.IsActive = vm.SelectedPatient.IsActive;
            }
        }
        

        private void UserPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_SavePatientCommand!.CanExecute(null))
                        ViewModel.Btn_SavePatientCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.Btn_UpdatePatientCommand!.CanExecute(null))
                        ViewModel.Btn_UpdatePatientCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivatePatientCommand!.CanExecute(null))
                        ViewModel.Btn_DeactivatePatientCommand.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearCommand!.CanExecute(null))
                        ViewModel.Btn_ClearCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }

        private void Button_ClickGotoServicePage(object sender, RoutedEventArgs e)
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(ServiceIdsPage));
        }

        //private void SearchBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        //{
        //    if (DataContext is PatientsViewModel vm)
        //    {
        //        vm.SearchQuery = SearchBox.Text;
        //        vm.ApplySearchFilter();
        //    }
        //}

        //private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (DataContext is PatientsViewModel vm)
        //    {
        //        vm.SearchQuery = SearchBox.Text;
        //        vm.ApplySearchFilter();
        //    }
        //}
    }
}
