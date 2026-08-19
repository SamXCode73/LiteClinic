using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;
using Windows.Storage;
using Windows.UI.Text;

namespace LiteClinic.ViewModels
{
    public partial class MainPageViewModel : INotifyPropertyChanged
    {
        //private readonly AppState? _appState;
        public MainPageViewModel()
        {
            // Subscribe to GlobalState events
            App.GlobalState.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AppState.CanAccessMenu))
                    OnPropertyChanged(nameof(CanAccessMenu));
                if (e.PropertyName == nameof(AppState.CanAccesssAppMenu))
                    OnPropertyChanged(nameof(CanAccesssAppMenu));
                if (e.PropertyName == nameof(AppState.CanViewSettingsMenu))
                    OnPropertyChanged(nameof(CanViewSettingsMenu));
                if (e.PropertyName == nameof(AppState.LoggedUserName))
                    OnPropertyChanged(nameof(LoggedUserName));
            };

            // Keep your existing SubTitleChanged listener
            App.GlobalState.SubTitleChanged += (resourceKey) =>
            {
                try
                {
                    var loader = new ResourceLoader();
                    var value = loader.GetString(resourceKey);
                    this.SubTitle = string.IsNullOrEmpty(value)
                        ? $"Missing: {resourceKey}"
                        : value;
                }
                catch (Exception ex)
                {
                    this.SubTitle = $"Error loading resource: {ex.Message}";
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ===== Permissions / Visibility =====

        // Mirror AppState properties
        public bool CanAccessMenu => App.GlobalState.CanAccessMenu;
        public bool CanAccesssAppMenu => App.GlobalState.CanAccesssAppMenu;
        public bool CanViewSettingsMenu => App.GlobalState.CanViewSettingsMenu;
        public string LoggedUserName => App.GlobalState.LoggedUserName;


        //// ===== UI State =====
        private string _subTitle = string.Empty;
        public string SubTitle
        {
            get => _subTitle;
            set { _subTitle = value; OnPropertyChanged(nameof(SubTitle)); }
        }

    }
}
