using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace LiteClinic.ViewModels;

public partial class PatientsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<PatientsModel> PatientsList { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);
    private readonly PatientsRepository _patientsRepository = new();
    private readonly ResourceLoader _loader = new();


    public ICommand? Btn_SavePatientCommand { get; }
    public ICommand? Btn_UpdatePatientCommand { get; }
    public ICommand? Btn_ClearCommand { get; }
    public ICommand? Btn_DeactivatePatientCommand { get; }

    public bool CanAddPatient => PermissionHelper.CanManageRecords; // Add New Record
    public bool CanEditPatient => PermissionHelper.CanEditRecords; // Edit existing Record
    public bool CanDeactivatePatient => PermissionHelper.CanEditRecords; // Deactivate Record


    public PatientsViewModel()
    {

        Btn_SavePatientCommand = new RelayCommand(SavePatient);
        Btn_UpdatePatientCommand = new RelayCommand(UpdatePatient);
        Btn_DeactivatePatientCommand = new RelayCommand(DeactivatePatient);
        Btn_ClearCommand = new RelayCommand(ClearPatientFields);
        

    }

    private List<PatientsModel> _allPatients = new();

    private PatientsModel? _selectesPatient;
    public PatientsModel? SelectedPatient
    {
        get => _selectesPatient;
        set
        {
            _selectesPatient = value;
            OnPropertyChanged(nameof(SelectedPatient));
        }
    }

    public SolidColorBrush StatusColor

    {
        get => _statusColor;
        set
        {
            _statusColor = value;
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    private string? _statusMessage {get; set; }
    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    // 🔹 PatientAutoId
    private int _patientAutoId;
    public int PatientAutoId
    {
        get => _patientAutoId;
        set
        {
            _patientAutoId = value;
            OnPropertyChanged(nameof(PatientAutoId));
        }
    }

    // 🔹 PatientId
    private string? _patientId;
    public string? PatientId
    {
        get => _patientId;
        set
        {
            _patientId = value;
            OnPropertyChanged(nameof(PatientId));
        }
    }

    // 🔹 FirstName
    private string? _firstName;
    public string? FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    // 🔹 MiddleName
    private string? _middleName;
    public string? MiddleName
    {
        get => _middleName;
        set
        {
            _middleName = value;
            OnPropertyChanged(nameof(MiddleName));
        }
    }

    // 🔹 LastName
    private string? _lastName;
    public string? LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
        }
    }

    // 🔹 FullMotherName
    private string? _fullMotherName;
    public string? FullMotherName
    {
        get => _fullMotherName;
        set
        {
            _fullMotherName = value;
            OnPropertyChanged(nameof(FullMotherName));
        }
    }

    // 🔹 CivilRecord
    private string? _civilRecord;
    public string? CivilRecord
    {
        get => _civilRecord;
        set
        {
            _civilRecord = value;
            OnPropertyChanged(nameof(CivilRecord));
        }
    }

    // 🔹 Gender
    private string? _gender;
    public string? Gender
    {
        get => _gender;
        set
        {
            _gender = value;
            OnPropertyChanged(nameof(Gender));
        }
    }

    // 🔹 DateOfBirth
    private string? _StringDay;
    public string? StringDay
    { 
        get => _StringDay;
        set
        { 
            _StringDay = value;
            OnPropertyChanged(nameof(StringDay));
            UpdateDateOfBirth();
            UpdateAge();
        }
    }

    private string? _StringMonth;
    public string? StringMonth
    { 
        get => _StringMonth;
        set
        { 
            _StringMonth = value;
            OnPropertyChanged(nameof(StringMonth));
            UpdateDateOfBirth();
            UpdateAge();
        }
    }

    private string? _StringYear;
    public string? StringYear
    { 
        get => _StringYear;
        set
        {
            _StringYear = value;
            OnPropertyChanged(nameof(StringYear));
            UpdateDateOfBirth();
            UpdateAge();
        }
    }

    private string? _DateOfBirth;
    public string? DateOfBirth
    {
        get => _DateOfBirth;
        set 
        { 
            _DateOfBirth = value;
            OnPropertyChanged(nameof(DateOfBirth));

        }
    }

    private int _PatientAge;
    public int PatientAge
    { 
        get => _PatientAge;
        set
        { 
            _PatientAge = value;
            OnPropertyChanged(nameof(PatientAge));
        }
    }

    // 🔹 PhoneNumber
    private string? _phoneNumber;
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged(nameof(PhoneNumber));
        }
    }

    // 🔹 Email
    private string? _email;
    public string? Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    // 🔹 Address
    private string? _address;
    public string? Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged(nameof(Address));
        }
    }

    // 🔹 City
    private string? _city;
    public string? City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged(nameof(City));
        }
    }

    // 🔹 Country
    private string? _country;
    public string? Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged(nameof(Country));
        }
    }

    // 🔹 GotInsurance
    private bool _gotInsurance;
    public bool GotInsurance
    {
        get => _gotInsurance;
        set
        {
            _gotInsurance = value;
            OnPropertyChanged(nameof(GotInsurance));
        }
    }

    // 🔹 Insurance Name
    private string? _InsuranceName;
    public string? InsuranceName
    {
        get => _InsuranceName;
        set
        {
            _InsuranceName = value;
            OnPropertyChanged(nameof(InsuranceName));
        }
    }

    // 🔹 InsuranceNumber
    private string? _insuranceNumber;
    public string? InsuranceNumber
    {
        get => _insuranceNumber;
        set
        {
            _insuranceNumber = value;
            OnPropertyChanged(nameof(InsuranceNumber));
        }
    }

    // 🔹 GotNSN
    private bool _gotNSN;
    public bool GotNSN
    {
        get => _gotNSN;
        set
        {
            _gotNSN = value;
            OnPropertyChanged(nameof(GotNSN));
        }
    }

    // 🔹 NSNName
    private string? _nsnName;
    public string? NSNName
    {
        get => _nsnName;
        set
        {
            _nsnName = value;
            OnPropertyChanged(nameof(NSNName));
        }
    }

    // 🔹 NSNNumber
    private string? _nsnNumber;
    public string? NSNNumber
    {
        get => _nsnNumber;
        set
        {
            _nsnNumber = value;
            OnPropertyChanged(nameof(NSNNumber));
        }
    }

    // 🔹 BloodType
    private string? _bloodType;
    public string? BloodType
    {
        get => _bloodType;
        set
        {
            _bloodType = value;
            OnPropertyChanged(nameof(BloodType));
        }
    }

    // 🔹 Allergies
    private string? _allergies;
    public string? Allergies
    {
        get => _allergies;
        set
        {
            _allergies = value;
            OnPropertyChanged(nameof(Allergies));
        }
    }

    // 🔹 MedicalHistory
    private string? _medicalHistory;
    public string? MedicalHistory
    {
        get => _medicalHistory;
        set
        {
            _medicalHistory = value;
            OnPropertyChanged(nameof(MedicalHistory));
        }
    }

    // 🔹 Language
    private string _language = "English";
    public string Language
    {
        get => _language;
        set
        {
            _language = value;
            OnPropertyChanged(nameof(Language));
        }
    }

    // 🔹 IsActive
    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    // 🔹 CreatedBy
    private string? _createdBy;
    public string? CreatedBy
    {
        get => _createdBy;
        set
        {
            _createdBy = value;
            OnPropertyChanged(nameof(CreatedBy));
        }
    }

    // 🔹 CreatedAt
    private DateTime? _createdAt;
    public DateTime? CreatedAt
    {
        get => _createdAt;
        set
        {
            _createdAt = value;
            OnPropertyChanged(nameof(CreatedAt));
        }
    }

    // 🔹 UpdatedAt
    private DateTime? _updatedAt;
    public DateTime? UpdatedAt
    {
        get => _updatedAt;
        set
        {
            _updatedAt = value;
            OnPropertyChanged(nameof(UpdatedAt));
        }
    }

    // 🔹 UpdatedBy
    private string? _updatedBy;
    public string? UpdatedBy
    {
        get => _updatedBy;
        set
        {
            _updatedBy = value;
            OnPropertyChanged(nameof(UpdatedBy));
        }
    }
    private bool _isBusy {get;set;}
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged(nameof(_isBusy));
        }
    }

    private string? _fullPatientName { get; set; }
    public string? FullPatientName
    {
        get => _fullMotherName;
        set 
        {
            _fullMotherName = value;
            OnPropertyChanged(nameof(_fullMotherName));
        }
    }


        // 🔹 Computed PatientFullName
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

    private string? _searchQuery;
    public string? SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                ApplySearchFilter(); // Trigger filtering automatically
            }
        }
    }

    private string? _hijriDate;
    public string? HijriDate
    {
        get => _hijriDate;
        set
        {
            _hijriDate = value;
            OnPropertyChanged(nameof(HijriDate));
        }
    }

    private string? _romanDate;
    public string? RomanDate
    {
        get => _romanDate;
        set
        {
            _romanDate = value;
            OnPropertyChanged(nameof(RomanDate));
        }
    }

    // -------------------------
    // Show/Hide Date
    // -------------------------
    private bool _showGregorianDate = App.GlobalState.ShowGregorianDate;
    public bool ShowGregorianDate
    {
        get => _showGregorianDate;
        set
        {
            if (_showGregorianDate != value)
            {
                _showGregorianDate = value;
                OnPropertyChanged(nameof(ShowGregorianDate));
            }
        }
    }

    private bool _showHijriDate = App.GlobalState.ShowHijriDate;
    public bool ShowHijriDate
    {
        get => _showHijriDate;
        set
        {
            if (_showHijriDate != value)
            {
                _showHijriDate = value;
                OnPropertyChanged(nameof(ShowHijriDate));

            }
        }
    }

    public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

    public void LoadPatients()
    {
        IsBusy = true;
        PatientsList.Clear();

        try
        {

            _allPatients = _patientsRepository.GetAllPatients();
            
            foreach (var patient in _allPatients)
            {
                PatientsList.Add(patient);                

            }

            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(_loader.GetString("StatusMessageLoaded"), PatientsList.Count);
        }
        catch (Exception ex)
        {
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = $"Error loading patients: {ex.Message}"; 
            
        }
        finally
        {
            IsBusy = false;
        
        }
        OnPropertyChanged(nameof(StatusMessage));
    }

    public async Task LoadPatientsAsync()
    {
        IsBusy = true;
        PatientsList.Clear();
        try
        {
            _allPatients = await Task.Run(() => _patientsRepository.GetAllPatients());

            foreach (var patient in _allPatients)
            {
                PatientsList.Add(patient);
            }

            StatusColor = new SolidColorBrush(Colors.Black);
            StatusMessage = string.Format(_loader.GetString("StatusMessageLoaded"), PatientsList.Count);
        }
        catch (Exception ex)
        {
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = string.Format(_loader.GetString("StatusMessageErrorLoadingPatients"),ex.Message);
        }
        finally
        {
            IsBusy = false;            
        }
        OnPropertyChanged(nameof(StatusMessage));
    }


    public async Task InitializeAsync()
    {
        await LoadPatientsAsync();
         await GetHiriRomanDate();
    }


    public async void SavePatient()
    {

        //bool isDobMissing = DateOfBirth == DateTime.Now.Date;

        if (string.IsNullOrWhiteSpace(FirstName) ||
            string.IsNullOrWhiteSpace(MiddleName) ||
            string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(FullMotherName) ||
            string.IsNullOrWhiteSpace(Gender) || string.IsNullOrWhiteSpace(Language))
        {
            // your code here
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessageRequiredFields");
            return;

        }

        IsBusy = true;



        var patientsModel = new PatientsModel
        {
            PatientId = GenerateNextPatientId(),
            FirstName = this.FirstName ?? "",
            MiddleName = this.MiddleName ?? "",
            LastName = this.LastName ?? "",
            FullMotherName = this.FullMotherName ?? "",
            CivilRecord = this.CivilRecord ?? "",
            Gender = this.Gender ?? "",
            StringDay = this.StringDay ?? "",
            StringMonth = this.StringMonth ?? "",
            StringYear = this.StringYear ?? "",
            DateOfBirth = this.DateOfBirth,
            PatientAge = this.PatientAge,
            PhoneNumber = this.PhoneNumber ?? "",
            Email = this.Email ?? "",
            Address = this.Address ?? "",
            City = this.City ?? "",
            Country = this.Country ?? "",
            GotInsurance = this.GotInsurance,
            InsuranceName = this.InsuranceName ?? "",
            InsuranceNumber = this.InsuranceNumber ?? "",
            GotNSN = this.GotNSN,
            NSNName = this.NSNName ?? "",
            NSNNumber = this.NSNNumber ?? "",
            BloodType = this.BloodType ?? "",
            Allergies = this.Allergies ?? "",
            MedicalHistory = this.MedicalHistory ?? "",
            IsActive = this.IsActive,
            CreatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}",
            CreatedAt = DateTime.Now,
        };

        bool success = _patientsRepository.SavePatient(patientsModel);

        if (success)
        {
            PatientsList.Add(patientsModel);
            StatusColor = new SolidColorBrush(Colors.Teal);
            StatusMessage = _loader.GetString("StatusMessagePatientAdded");

        }

        try
        {
            await Task.Delay(2000, _cts.Token);
            //LoadPatients();
            ClearPatientFields();
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in PatientsViewModel - Patient Added Error");
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessagePatientAddedError");
        }
        finally
        {
            _cts.TryReset(); // Reset the cancellation token for future use
        }
    }

    public async void UpdatePatient()
    {
        if (SelectedPatient == null) return;
        //bool isDobMissing = SelectedPatient.DateOfBirth == DateTime.Now.Date;

        if (string.IsNullOrWhiteSpace(SelectedPatient.FirstName) ||
            string.IsNullOrWhiteSpace(SelectedPatient.MiddleName) ||
            string.IsNullOrWhiteSpace(SelectedPatient.LastName) ||
            string.IsNullOrWhiteSpace(SelectedPatient.FullMotherName) ||
            string.IsNullOrWhiteSpace(SelectedPatient.Gender))
        {
            // your code here
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessageRequiredFields");
            return;

        }

        IsBusy = true;

        SelectedPatient.FirstName = FirstName ?? "";
        SelectedPatient.MiddleName = MiddleName ?? "";
        SelectedPatient.LastName = LastName ?? "";
        SelectedPatient.FullMotherName = FullMotherName ?? "";
        SelectedPatient.CivilRecord = CivilRecord ?? "";
        SelectedPatient.Gender = Gender ?? "";
        SelectedPatient.PhoneNumber = PhoneNumber;
        SelectedPatient.DateOfBirth = DateOfBirth;
        SelectedPatient.PatientAge = PatientAge;
        SelectedPatient.Email = Email ?? "";
        SelectedPatient.Address = Address ?? "";
        SelectedPatient.City = City ?? "";
        SelectedPatient.Country = Country ?? "";
        SelectedPatient.GotInsurance = GotInsurance;
        SelectedPatient.InsuranceName = InsuranceName ?? "";
        SelectedPatient.InsuranceNumber = InsuranceNumber ?? "";
        SelectedPatient.GotNSN = GotNSN;
        SelectedPatient.NSNName = NSNName ?? "";
        SelectedPatient.NSNNumber = NSNNumber ?? "";
        SelectedPatient.BloodType = BloodType ?? "";
        SelectedPatient.Allergies = Allergies ?? "";
        SelectedPatient.MedicalHistory = MedicalHistory ?? "";
        SelectedPatient.IsActive = IsActive;
        SelectedPatient.UpdatedAt = DateTime.Now;
        SelectedPatient.UpdatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}";

        bool success = _patientsRepository.UpdatePatient(SelectedPatient);
        if (success)
        {
            StatusColor = new SolidColorBrush(Colors.RoyalBlue);
            StatusMessage = _loader.GetString("StatusMessagePatientUpdated");
        }

        try
        {
            await Task.Delay(2000, _cts.Token);
            //LoadPatients();
            ClearPatientFields();
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in PatientsViewModel - Patient Added Error");
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessagePatientUpdatedError");
        }
        finally
        {
            _cts.TryReset(); // Reset the cancellation token for future use        

        }
    }

    public async void DeactivatePatient()
    {
        if (SelectedPatient == null) return;

        IsBusy = true;
        if (SelectedPatient == null || string.IsNullOrEmpty(SelectedPatient.PatientId)) return;

        SelectedPatient.IsActive = false;
        SelectedPatient.UpdatedAt = DateTime.Now;
        SelectedPatient.UpdatedBy = $"Windows User: {Environment.UserName} - Active User: {App.GlobalState.LoggedUserName}";

        var success = _patientsRepository.DeactivatePatient(SelectedPatient);

        if (success)
        {
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessagePatientDeactivated");
        }

        try
        {
            await Task.Delay(2000, _cts.Token);
            ClearPatientFields();
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in PatientsViewModel - Patient Deactivated Error");
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusMessage = _loader.GetString("StatusMessagePatientDeactivatedError");
        }
        finally
        {
            _cts.TryReset(); // Reset the cancellation token for future use
        }

    }

    public void ApplySearchFilter()
    {
        if (_allPatients == null || _allPatients.Count == 0) return;

        PatientsList.Clear();

        var query = SearchQuery?.Trim().ToLower();

        foreach (var patient in _allPatients)
        {
            if (string.IsNullOrWhiteSpace(query) ||
                (patient.FirstName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.MiddleName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.LastName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.PatientFullName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.Email?.ToLower().Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.PhoneNumber?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.PatientId?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (patient.FullMotherName?.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false))
            {
                
                PatientsList.Add(patient);

            }
        }
    }

    public void ClearPatientFields()
    {
        
        FirstName = string.Empty;
        MiddleName = string.Empty;
        LastName = string.Empty;
        FullMotherName = string.Empty;
        CivilRecord = string.Empty;
        Gender = string.Empty;
        StringDay = string.Empty;
        StringMonth = string.Empty;
        StringYear = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        Country = string.Empty;
        GotInsurance = false;
        InsuranceName = string.Empty;
        InsuranceNumber = string.Empty;
        GotNSN = false;
        NSNName = string.Empty;
        NSNNumber = string.Empty;
        BloodType = string.Empty;
        Allergies = string.Empty;
        MedicalHistory = string.Empty;
        CreatedBy =  string.Empty;
        CreatedAt = DateTime.Now;
        UpdatedBy = string.Empty;
        UpdatedAt = null;
        SelectedPatient = new PatientsModel();
        StatusMessage = string.Empty;
        SearchQuery = string.Empty;
        LoadPatients();
    }

    private string GenerateNextPatientId()
    {
        var patient = _patientsRepository.GetAllPatients();
        var lastId = patient
            .Select(u => u.PatientId)
            .Where(id => id!.StartsWith("PT"))
            .Select(id => int.Parse(id!.Substring(2)))
            .DefaultIfEmpty(0)
            .Max();

        int nextId = lastId + 1;
        return $"PT{nextId:D6}"; // D4 for zero-padded to 4 digits like LC0001
    }

    private void UpdateDateOfBirth()
    {
        if (!string.IsNullOrWhiteSpace(StringYear))
        {
            // Normalize day and month to two digits if provided
            string? day = string.IsNullOrWhiteSpace(StringDay) ? null : StringDay.PadLeft(2, '0');
            string? month = string.IsNullOrWhiteSpace(StringMonth) ? null : StringMonth.PadLeft(2, '0');

            if (day != null && month != null)
            {
                DateOfBirth = $"{day}/{month}/{StringYear}";
            }
            else if (month != null)
            {
                DateOfBirth = $"01/{month}/{StringYear}"; // Year + Month only
            }
            else
            {
                DateOfBirth = $"01/01/{StringYear}"; // Year only
            }
        }
        else
        {
            DateOfBirth = DateTime.MinValue.ToString("dd/MM/yyyy");
        }
    }


    private void UpdateAge()
    {
        var today = DateTime.Today;
        

        // Full date: Day + Month + Year
        if (int.TryParse(StringDay, out int day) &&
            int.TryParse(StringMonth, out int month) &&
            int.TryParse(StringYear, out int year))
        {
            try
            {
                var dob = new DateTime(year, month, day);
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;
                PatientAge = age;
                return;
            }
            catch (Exception ex)
            {
                // Invalid date (e.g., Feb 30)
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("StatusMessageErrorDate");
                Logger.LogError(ex, "Error in PatientsViewModel - Patient Date Error");
            }
        }

        // Partial date: Month + Year
        if (int.TryParse(StringMonth, out month) &&
            int.TryParse(StringYear, out year))
        {
            var dob = new DateTime(year, month, 1);
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            PatientAge = age;
            return;
        }

        // Year only
        if (int.TryParse(StringYear, out year))
        {
            PatientAge = today.Year - year;
            return;
        }

        // No valid date
        PatientAge = 0;
    }

    private void OnNavigatedFrom()
    {
        ClearPatientMemory();
    }

    public async Task GetHiriRomanDate()
    {
        RomanDate = DateHelper.GetRomanDate();
        HijriDate = DateHelper.GetHijriDate();
        await Task.CompletedTask;
    }

    public void ClearPatientMemory()
    {
        PatientAutoId = 0;
        PatientId = string.Empty;
        FirstName = string.Empty;
        MiddleName = string.Empty;
        LastName = string.Empty;
        FullMotherName = string.Empty;
        CivilRecord = string.Empty;
        Gender = string.Empty;
        StringDay = string.Empty;
        StringMonth = string.Empty;
        StringYear = string.Empty;
        DateOfBirth = string.Empty;
        PatientAge = 0;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        Country = string.Empty;
        GotInsurance = false;
        InsuranceName = string.Empty;
        InsuranceNumber = string.Empty;
        GotNSN = false;
        NSNName = string.Empty;
        NSNNumber = string.Empty;
        BloodType = string.Empty;
        Allergies = string.Empty;
        MedicalHistory = string.Empty;
        Language = "English"; // reset to default
        IsActive = true;
        CreatedBy = string.Empty;
        CreatedAt = null;
        UpdatedBy = string.Empty;
        UpdatedAt = null;

        StatusMessage = string.Empty;
        StatusColor = new SolidColorBrush(Colors.Black);
        SearchQuery = string.Empty;

        SelectedPatient = null;
        PatientsList.Clear();
        _allPatients.Clear();

        _cts.Cancel();   // stop any pending delays
    }

}

