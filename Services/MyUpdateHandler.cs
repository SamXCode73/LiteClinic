using LiteClinic.Models;
using LiteClinic.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LiteClinic.Services
{
    public class MyUpdateHandler : IUpdateHandler
    {
        // ✅ Commands defined once, static and readonly
        private static readonly Dictionary<string, Func<ITelegramBotClient, long, string, CancellationToken, Task>> Commands =
            new()
            {
                ["/admin"] = async (botClient, chatId, firstName, ct) =>
                {
                    Logger.LogInfo($"Get Admin : ChatId={chatId}, Name={firstName}");
                    await botClient.SendMessage(chatId,
                        $"Hello Admin {firstName}, your chatId is {chatId}.",
                        cancellationToken: ct);
                },
                ["/doctor"] = async (botClient, chatId, firstName, ct) =>
                {
                    Logger.LogTelegramEvent(chatId.ToString(), "Doctor command received");
                    await botClient.SendMessage(chatId,
                        $"Hello Doctor {firstName}, your chatId is {chatId}.",
                        cancellationToken: ct);
                },
                ["/start"] = async (botClient, chatId, firstName, ct) =>
                {
                    Logger.LogTelegramEvent(chatId.ToString(), "Start command received");
                    await botClient.SendMessage(chatId,
                        $"Welcome {firstName}, your chatId is {chatId}.",
                        cancellationToken: ct);
                }
            };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message || message.Type != MessageType.Text) return;

            var chatId = message.Chat.Id;
            var firstName = message.Chat.FirstName ?? "";
            var text = message.Text?.ToLowerInvariant();

            if (BlockedUsersManager.IsBlocked(chatId.ToString())) return;
            if (!CanProcessMessage(chatId.ToString())) return;

            if (text != null && Commands.TryGetValue(text, out var command))
            {
                if (CanExecuteCommand(chatId.ToString(), text, out bool isBlockedForever))
                {
                    await command(botClient, chatId, firstName, cancellationToken);
                }
                else if (!isBlockedForever)
                {
                    await botClient.SendMessage(chatId,
                        $"You can only use {text} once every 24 hours.",
                        cancellationToken: cancellationToken);
                }
            }
        }

        public static bool CanExecuteCommand(string chatId, string command, out bool isBlockedForever)
        {
            isBlockedForever = false;
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT LastUsedUtc, FailCount FROM CommandUsage WHERE ChatId = @chatId";
                checkCmd.Parameters.AddWithValue("@chatId", chatId);

                using var reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    var lastUsed = DateTime.Parse(reader.GetString(0));
                    var failCount = reader.GetInt32(1);

                    if ((DateTime.UtcNow - lastUsed).TotalHours < 24)
                    {
                        failCount++;
                        using var updateCmd = conn.CreateCommand();
                        updateCmd.CommandText = "UPDATE CommandUsage SET FailCount = @failCount WHERE ChatId = @chatId";
                        updateCmd.Parameters.AddWithValue("@failCount", failCount);
                        updateCmd.Parameters.AddWithValue("@chatId", chatId);
                        updateCmd.ExecuteNonQuery();

                        if (failCount == 3)
                            isBlockedForever = true;

                        return false;
                    }
                }

                using var upsertCmd = conn.CreateCommand();
                upsertCmd.CommandText = @"
                    INSERT INTO CommandUsage (ChatId, Command, LastUsedUtc, FailCount)
                    VALUES (@chatId, @command, @lastUsedUtc, 0)
                    ON CONFLICT(ChatId)
                    DO UPDATE SET Command = excluded.Command, LastUsedUtc = excluded.LastUsedUtc, FailCount = 0;";
                upsertCmd.Parameters.AddWithValue("@chatId", chatId);
                upsertCmd.Parameters.AddWithValue("@command", command);
                upsertCmd.Parameters.AddWithValue("@lastUsedUtc", DateTime.UtcNow.ToString("o"));
                upsertCmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error checking command usage for ChatId={chatId}, Command={command}");
                return false;
            }
        }

        private static readonly ConcurrentDictionary<string, int> _messageCounts = new();

        public static bool CanProcessMessage(string chatId)
        {
            try
            {
                var count = _messageCounts.AddOrUpdate(chatId, 1, (_, old) => old + 1);

                if (count == 5)
                {
                    using var conn = DatabaseHelper.GetConnection();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO FloodProtection (ChatId, MessageCount, IsBlockedForever, LastMessageUtc) " +
                                      "VALUES (@chatId, @count, 1, @LastMessageUtc) " +
                                      "ON CONFLICT(ChatId) DO UPDATE SET MessageCount=@count, IsBlockedForever=1";
                    cmd.Parameters.AddWithValue("@chatId", chatId);
                    cmd.Parameters.AddWithValue("@count", count);
                    cmd.Parameters.AddWithValue("@LastMessageUtc", DateTime.UtcNow.ToString("o"));
                    cmd.ExecuteNonQuery();

                    BlockedUsersManager.AddBlocked(chatId);
                    Logger.LogTelegramEvent(chatId.ToString(), "Blocked user spammer");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in CanProcessMessage");
                return false;
            }
        }

        Task IUpdateHandler.HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Logger.LogError(exception, $"Telegram error from {source}");
            return Task.CompletedTask;
        }
    }
}