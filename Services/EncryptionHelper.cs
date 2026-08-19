using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LiteClinic.Services
{
    public static class EncryptionHelper
    {
        // Optional: add a purpose-specific entropy to strengthen protection
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Liteclinic-TelegramToken-v1");

        public static string EncryptToBase64(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentException("Value cannot be empty.", nameof(plainText));

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] protectedData = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }

        public static string DecryptFromBase64(string base64Cipher)
        {
            if (string.IsNullOrWhiteSpace(base64Cipher))
                throw new ArgumentException("Value cannot be empty.", nameof(base64Cipher));



            byte[] protectedData = Convert.FromBase64String(base64Cipher);
            byte[] data = ProtectedData.Unprotect(protectedData, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }


    }
}
