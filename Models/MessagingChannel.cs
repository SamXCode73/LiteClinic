using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class MessagingChannel
    {
        public int Id { get; set; }

        /// <summary>
        /// The type of provider (Telegram, WhatsApp, Email, etc.)
        /// </summary>
        public string ProviderType { get; set; } = string.Empty;

        /// <summary>
        /// Encrypted token or API key stored in the database
        /// </summary>
        public string EncryptedToken { get; set; } = string.Empty;

        /// <summary>
        /// Friendly name (e.g., Clinic name, Bot name)
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Chat ID or recipient ID for the messaging app
        /// </summary>
        public string ChatId { get; set; } = string.Empty;

        /// <summary>
        /// Whether this channel is active and should be used
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional: last time a message was sent through this channel
        /// </summary>
        //public DateTime? LastUsed { get; set; }
    }
}
