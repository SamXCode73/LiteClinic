using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Storage;


namespace LiteClinic.Services
{
    internal static class LanguageManager
    {
        public static event Action<string>? LanguageChanged;

        private static string currentLanguage =
            ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string ?? "en-US";

        public static string CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                if (currentLanguage != value)
                {
                    currentLanguage = value;
                    ApplyLanguage(currentLanguage);
                    LanguageChanged?.Invoke(currentLanguage);
                }
            }
        }

        public static void ApplyLanguage(string languageName)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Application.Current.Resources["SelectedLanguage"] = languageName;
            localSettings.Values["SelectedLanguage"] = languageName;

            // Apply font family based on language
            if (languageName.StartsWith("ar"))
            {
                Application.Current.Resources["AppFontFamily"] = new FontFamily("Calibri, Segoe UI, Segoe UI Historic, Arial, Times New Roman"); // or a good Arabic font                                
                
            }
            else
            {
                Application.Current.Resources["AppFontFamily"] = new FontFamily("Segoe UI, Segoe UI Historic, Arial");
                
            }
        }
    }
}
