using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace StartbeatMenu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow = new MainWindow();
            ShowMenu();
        }

        void ShowMenu()
        {
            var win = MainWindow as MainWindow;
            win.DisplayMenu();
        }
    }
}
