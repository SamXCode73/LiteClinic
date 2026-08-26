using Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using WinRT.Interop;

namespace LiteClinic.Services
{
    public class FolderPickerService : IFolderPickerService
    {
        public async Task<string?> PickFolderAsync()
        {
            var folderPicker = new FolderPicker();
            var hwnd = WindowNative.GetWindowHandle(App.MainAppWindow);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder? selectedFolder = await folderPicker.PickSingleFolderAsync();
            return selectedFolder?.Path;
        }
    }
}
