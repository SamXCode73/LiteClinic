using LiteClinic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    internal class BackgroundService
    {
        // Event raised when greeting starts, passing GreetingEventArgs
        public event Action<GreetingEventArgs>? GreetingStarted;

        // Event raised when greeting finishes
        //public event Action? GreetingFinished;

        // Method to start greeting
        public void StartGreeting(string greetingText)
        {
            var args = new GreetingEventArgs(greetingText);
            GreetingStarted?.Invoke(args);
        }

        // Method to finish greeting
        //public void FinishGreeting()
        //{
        //    GreetingFinished?.Invoke();
        //}
    }
}    
