using System;
using System.Threading.Tasks;
using LiteClinic.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace LiteClinic
{
    public class Program
    {
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            // Initialize COM wrappers for WinRT interop
            WinRT.ComWrappersSupport.InitializeComWrappers();

            // Unique key for LiteClinic single-instance enforcement
            string appKey = "b53d312f-2253-4890-87f0-8821b16e3b5d";
            var keyInstance = AppInstance.FindOrRegisterForKey(appKey);

            if (keyInstance.IsCurrent)
            {
                // First instance: start the application normally
                Application.Start((p) =>
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    if (dispatcherQueue != null)
                    {
                        var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                        System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                    }

                    new App();
                });
            }
            else
            {
                // Another instance is already running
                try
                {
                    var argsActivated = AppInstance.GetCurrent().GetActivatedEventArgs();
                    await keyInstance.RedirectActivationToAsync(argsActivated);
                }
                catch (ObjectDisposedException)
                {
                    // Dispatcher or activation args were disposed during shutdown
                    // Safe to ignore since the main instance is closing
                }
                catch (Exception ex)
                {
                    // Log or handle other unexpected errors gracefully
                    Logger.LogError(ex, "Failed to redirect activation to the main instance.");

                }
            }

            return 0;
        }
    }
}