using LiteClinic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public static class BlockedUsersManager
    {
        private static readonly HashSet<string> _blockedForever = new();

        public static async Task LoadBlockedUsers()
        {
            using var conn = await DatabaseHelper.GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ChatId FROM FloodProtection WHERE IsBlockedForever = 1";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _blockedForever.Add(reader.GetString(0));
            }
        }

        public static bool IsBlocked(string chatId) => _blockedForever.Contains(chatId);

        public static void AddBlocked(string chatId) => _blockedForever.Add(chatId);

        public static IEnumerable<string> GetAllBlocked() => _blockedForever;
    }
}
