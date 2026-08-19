using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models.Enums;
using LiteClinic.Models;
using LiteClinic.Repository;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Microsoft.Windows.ApplicationModel.Resources;

namespace LiteClinic.ViewModels
{
    public partial class SettingsPageViewModel : INotifyPropertyChanged
    {
        private CancellationTokenSource _cts = new();
        private readonly ResourceLoader _loader = new();


        public SettingsPageViewModel()
        {
            // Subscribe to GlobalState changes
            App.GlobalState.PropertyChanged += (s, e) =>
            {

                //Status messge adn Status color
                if (e.PropertyName == nameof(AppState.StatusMessage))
                    StatusMessage = App.GlobalState.StatusMessage;

                if (e.PropertyName == nameof(AppState.StatusColor))
                    StatusColor = App.GlobalState.StatusColor;

                // Wire all properties used in SettingsPage
                if (e.PropertyName == nameof(AppState.CanManageSettingsMenu))
                    OnPropertyChanged(nameof(CanManageSettingsMenu));
                if (e.PropertyName == nameof(AppState.CanViewSettingsMenu))
                    OnPropertyChanged(nameof(CanViewSettingsMenu));

            };
            

        }


        // Mirror AppState properties
        public bool CanManageSettingsMenu => App.GlobalState.CanManageSettingsMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;



        private SolidColorBrush? _statusColor;
        public SolidColorBrush? StatusColor 
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
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


        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed; 
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public async Task InitializeSettingsAsync()
        {
            try
            {
                // Show/ Hide date

                await GetHiriRomanDate();

                // Try to get theme from DB
            }
            catch (Exception ex)
            {                

                Logger.LogError(ex, "Failed to initialize settings from database");
            }
        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearSettingsMemory()
        {
            // Reset transient UI state
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            HijriDate = string.Empty;
            RomanDate = string.Empty;

            // Cancel CTS
            _cts.Cancel();
        }
    }
}

