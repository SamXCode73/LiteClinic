using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace LiteClinic.Services
{
    public class LogFolderService : ILogFolderService
    {
        public async Task<bool> OpenLogFolderAsync()
        {
            string logFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "AppLogs");
            

            try
            {
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(logFolder);
                await Launcher.LaunchFolderAsync(folder);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ERROR_OPEN_LOG_FOLDER: Failed to open log folder");
                return false;
            }
        }
    }
}
