using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Windows.ApplicationModel.Appointments;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SQLitePCL;
using Microsoft.Windows.ApplicationModel.Resources;

namespace LiteClinic.ViewModels
{
    public partial class MessagingIntegrationPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private string? _statusMessage;
        private SolidColorBrush? _statusColor;
        private string? _tokenName;
        private string? _friendlyTelegramName;
        private string? _adminChatId;
        private static readonly CancellationTokenSource _cts = new();
        private ProviderType _selectedProvider;
        private readonly ResourceLoader _loader = new();

        public ICommand ApplyTelegramCommand { get; }

        public MessagingIntegrationPageViewModel()
        {
            ApplyTelegramCommand = new RelayCommand(() => Btn_AddEncryptedToken(ProviderType.Telegram));
        }

        //public MessagingIntegrationPageViewModel()
        //{
        //}

        public ProviderType SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                if (_selectedProvider != value)
                {
                    _selectedProvider = value;
                    OnPropertyChanged(nameof(SelectedProvider));
                }
            }
        }

        public SolidColorBrush? StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string? TokenName
        {
            get => _tokenName;
            set
            {
                _tokenName = value;
                OnPropertyChanged(nameof(TokenName));
            }
        }

        public string? FriendlyTelegramName
        {
            get => _friendlyTelegramName;
            set
            {
                _friendlyTelegramName = value;
                OnPropertyChanged(nameof(FriendlyTelegramName));
            }
        }

        public string? AdminChatId
        {
            get => _adminChatId;
            set
            {
                _adminChatId = value;
                OnPropertyChanged(nameof(AdminChatId));
            }
        }


        public static long? GetAdminChatIds { get; set; }


        private async void Btn_AddEncryptedToken(ProviderType providerType)
        {
            if (string.IsNullOrWhiteSpace(TokenName))
            {
                StatusMessage = _loader.GetString("MiP_StatusMessage_TokenEmpty");
                return;
            }

            string encryptedToken = EncryptChatBot(TokenName.Trim());
            string encryptedFriendlyName = EncryptChatBot(FriendlyTelegramName?.Trim() ?? string.Empty);

            string loggedUsername = App.GlobalState.LoggedUserName;
            string windowsloggedUser = Environment.UserName;
            string dateNow = DateTime.Now.ToString("F");
            string adminChatId = AdminChatId!;
            string insertInfo = $"Logged User: {loggedUsername} - Windows User: {windowsloggedUser} - Inserted At: {dateNow}";

            if (App.GlobalState.LoggedUserRoleId > 1)
            {
                StatusColor = new SolidColorBrush(Colors.IndianRed);
                StatusMessage = _loader.GetString("MiP_StatusMessage_AdminOnly");
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                // Check if provider already exists
                cmd.CommandText = "SELECT COUNT(*) FROM AppChannel WHERE AppID1 = @AppID1";
                cmd.Parameters.AddWithValue("@AppID1", providerType.ToString());
                var exists = (long)cmd.ExecuteScalar()!;

                cmd.Parameters.Clear();
                if (exists > 0)
                {
                    cmd.CommandText = @"UPDATE AppChannel 
                                SET AppID2 = @AppID2, AppID3 = @AppID3, AppID7 = @AppID7, AppID8 = @AppID8
                                WHERE AppID1 = @AppID1";
                }
                else
                {
                    cmd.CommandText = @"INSERT INTO AppChannel (AppID1, AppID2, AppID3, AppID7, AppID8)
                                VALUES (@AppID1, @AppID2, @AppID3, @AppID7, AppID8)";
                }

                cmd.Parameters.AddWithValue("@AppID1", providerType.ToString());
                cmd.Parameters.AddWithValue("@AppID2", encryptedToken);
                cmd.Parameters.AddWithValue("@AppID3", encryptedFriendlyName);
                cmd.Parameters.AddWithValue("@AppID7", adminChatId);
                cmd.Parameters.AddWithValue("@AppID8", insertInfo);

                cmd.ExecuteNonQuery();
                transaction.Commit();

                StatusColor = new SolidColorBrush(Colors.Teal);
                StatusMessage = string.Format(
                    _loader.GetString(exists > 0 ? "MiP_StatusMessage_TokenUpdated" : "MiP_StatusMessage_TokenInserted"),
                    providerType);

                Logger.LogInfo($"{providerType} token {(exists > 0 ? "updated" : "inserted")}: {insertInfo}");

                await Task.Delay(2000);
                ClearData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error inserting {providerType} token.");
                transaction?.Rollback();
            }
            finally
            {
                DatabaseHelper.CloseConnection();
                _cts?.TryReset();
            }
        }
        private string EncryptChatBot(string tokenBot)
        {
            if (string.IsNullOrEmpty(_tokenName))
            {
                return string.Empty;
            }
            try
            {
                string encryptedToken = EncryptionHelper.EncryptToBase64(tokenBot);
                return encryptedToken;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error encrypting ChatBot token.");
                return string.Empty;
            }
        }
        private void ClearData()
        {
            StatusMessage = string.Empty;
            TokenName = string.Empty;
            FriendlyTelegramName = string.Empty;
            AdminChatId = string.Empty;
        }

        public (ProviderType ProviderType, string Token, string FriendlyName, string AdminId) DecryptFromBase64(ProviderType providerType)
        {
            string encryptedToken = string.Empty;
            string friendlyNameValue = string.Empty;
            string adminID = string.Empty;

            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                                SELECT 
                                    AppID2, AppID3, AppID7
                                FROM 
                                    AppChannel
                                WHERE 
                                    AppID1 = @AppID1;";

            cmd.Parameters.AddWithValue("@AppID1", providerType.ToString());

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                encryptedToken = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                friendlyNameValue = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                adminID = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            }

            if (string.IsNullOrEmpty(encryptedToken))
            {
                return (providerType, string.Empty, string.Empty, string.Empty);
            }

            try
            {
                string decryptedToken = EncryptionHelper.DecryptFromBase64(encryptedToken);
                string decryptedFriendlyName = EncryptionHelper.DecryptFromBase64(friendlyNameValue);

                return (providerType, decryptedToken, decryptedFriendlyName, adminID);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error decrypting token for {providerType}.");
                return (providerType, string.Empty, string.Empty, string.Empty);
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


        public void GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
        }

        public void ClearMemory()
        {
            _cts.Cancel();
        }
    }
}
