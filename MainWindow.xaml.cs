using LiteClinic.Services;
using LiteClinic.ViewModels;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.DataContracts;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //private readonly string _appTitle = "LiteClinic Healthcare Manager";
        //public LoginPageViewModel? ViewModel { get; }
        public MainWindow()
        {
            InitializeComponent();
            //var appWindow = this.AppWindow;
            //if (AppWindowTitleBar.IsCustomizationSupported())
            //{
            //    var titleBar = appWindow.TitleBar;
            //    titleBar.ExtendsContentIntoTitleBar = true;
            //    titleBar.ButtonBackgroundColor = Colors.Transparent;
            //    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //    titleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);
            //    titleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 241, 241, 241);

            //}
            //this.Closed  += MainWindow_Closed;         
        }

        //private void MainWindow_Closed(object sender, WindowEventArgs e)
        //{
        //    try
        //    {
        //        Logger.LogUserEvent($"User session ended by closing window. " +
        //            $"Role: {App.GlobalState.LoggedUserRoleId} | " +
        //            $"ID: {App.GlobalState.LoggedUserId} | User: {App.GlobalState.LoggedUserName} | " +
        //            $"Windows User: {Environment.UserName}, " +
        //            $"Action: Main Window Closed", "LOG OUT AND EXIT");

        //        // Clear Botlient when closing for safty.
        //        ViewModel?.DisposeBotClient();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, $"Error occurred while closing the main window. | {GetType().Name}");
        //    }
        //}

    }
}
