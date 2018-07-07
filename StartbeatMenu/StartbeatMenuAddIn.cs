using StartbeatMenu.Views;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace StartbeatMenu
{
    [AddIn("Startbeat Menu", Description = "Keeps your computer beating", Version = "1.0.0.0", Publisher = "Start9")]
    public class StartbeatMenuAddIn : IModule
    {
        public static StartbeatMenuAddIn Instance { get; private set; }

        public IConfiguration Configuration { get; set; } = new StartbeatMenuConfiguration();

        public IMessageContract MessageContract => null;

        public IReceiverContract ReceiverContract { get; } = new StartbeatMenuReceiverContract();

        public IHost Host { get; private set; }

        public void Initialize(IHost host)
        {
            void Start()
            {
                Instance = this;
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => MessageBox.Show(e.ExceptionObject.ToString(), "Uh Oh Exception!");

                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
                App.Main();
            }

            var t = new Thread(Start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

    }

    public class StartbeatMenuReceiverContract : IReceiverContract
    {
        public StartbeatMenuReceiverContract()    
        {
            StartMenuOpenedEntry.MessageReceived += (sender, e) =>
            {
                ((MainWindow)Application.Current.MainWindow).Topmost = true;
                ((MainWindow)Application.Current.MainWindow).Show();
            };
        }
        public IList<IReceiverEntry> Entries => new[] { StartMenuOpenedEntry };
        public IReceiverEntry StartMenuOpenedEntry { get; } = new ReceiverEntry("Open menu");
    }


    public class StartbeatMenuConfiguration : IConfiguration
    {
        public IList<IConfigurationEntry> Entries => new[] 
        {
            new ConfigurationEntry(PinnedItems, "Pinned Items"),
            new ConfigurationEntry(Places, "Places"),
            new ConfigurationEntry(MRU, "Most Recently Used"),
            new ConfigurationEntry(PreferredShutdownOptions, "Preferred shutdown option")
        };

        public IList<String> PinnedItems { get; } = new List<String>();

        public IList<String> MRU { get; } = new List<String>();

        public IList<String> Places { get; } = new List<String>();

        public PreferredShutdownOptions PreferredShutdownOptions { get; } = PreferredShutdownOptions.Shutdown;
    }

    public enum PreferredShutdownOptions
    {
        Shutdown,
        Sleep,
        Hibernate,
        Restart,
        Lock,
        Logoff
    }
}