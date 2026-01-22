using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models.Enums
{
    public enum ProviderType
    {
        Telegram = 0,
        WhatsApp = 1,
        Discord = 2,
        Email = 3,
        Sms = 4,
        Slack = 5,
        Viber = 6,
        MicrosoftTeams = 7,
        FacebookMessenger = 8,
        Undefined = 99
    }
}
