using LiteClinic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    internal class GreetingEventArgs
    {
        public string UserName { get; set; } = App.GlobalState.LoggedUserName;
        public string GreetingText { get; set; }

        AppState _appState = new AppState();

        public GreetingEventArgs(string userName, string greetingText)
        {
            UserName = userName;
            GreetingText = greetingText;
        }

        public GreetingEventArgs(string greetingText)
        {
            GreetingText = greetingText;
        }
    }
}
