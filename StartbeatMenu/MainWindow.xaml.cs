using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using WindowsSharp.DiskItems;
using Start9.UI.Wpf.Statics;
using WindowsSharp.Statics;
using Start9.UI.Wpf.Windows;

namespace StartbeatMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ShadowedWindow
    {
        public ObservableCollection<DiskItem> GetAllApps()
        {
            //ObservableCollection<DiskItem> items = new ObservableCollection<DiskItem>();

            ObservableCollection<DiskItem> AllAppsAppDataItems = new ObservableCollection<DiskItem>();
            foreach (var s in Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables(@"%appdata%\Microsoft\Windows\Start Menu\Programs")))
            {
                AllAppsAppDataItems.Add(new DiskItem(s));
            }
            foreach (var s in Directory.EnumerateDirectories(Environment.ExpandEnvironmentVariables(@"%appdata%\Microsoft\Windows\Start Menu\Programs")))
            {
                AllAppsAppDataItems.Add(new DiskItem(s));
            }

            ObservableCollection<DiskItem> AllAppsProgramDataItems = new ObservableCollection<DiskItem>();
            foreach (var s in Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables(@"%programdata%\Microsoft\Windows\Start Menu\Programs")))
            {
                AllAppsProgramDataItems.Add(new DiskItem(s));
            }
            foreach (var s in Directory.EnumerateDirectories(Environment.ExpandEnvironmentVariables(@"%programdata%\Microsoft\Windows\Start Menu\Programs")))
            {
                AllAppsProgramDataItems.Add(new DiskItem(s));
            }

            ObservableCollection<DiskItem> AllAppsItems = new ObservableCollection<DiskItem>();
            ObservableCollection<DiskItem> AllAppsReorgItems = new ObservableCollection<DiskItem>();
            foreach (DiskItem t in AllAppsAppDataItems)
            {
                var FolderIsDuplicate = false;

                foreach (DiskItem v in AllAppsProgramDataItems)
                {
                    ObservableCollection<DiskItem> SubItemsList = new ObservableCollection<DiskItem>();

                    if (Directory.Exists(t.ItemPath))
                    {
                        if (((t.ItemCategory == DiskItem.DiskItemCategory.Directory) & (v.ItemCategory == DiskItem.DiskItemCategory.Directory)) && ((t.ItemPath.Substring(t.ItemPath.LastIndexOf(@"\"))) == (v.ItemPath.Substring(v.ItemPath.LastIndexOf(@"\")))))
                        {
                            FolderIsDuplicate = true;
                            foreach (var i in Directory.EnumerateDirectories(t.ItemPath))
                            {
                                SubItemsList.Add(new DiskItem(i));
                            }

                            foreach (var j in Directory.EnumerateFiles(v.ItemPath))
                            {
                                SubItemsList.Add(new DiskItem(j));
                            }
                        }
                    }

                    if (!AllAppsItems.Contains(v))
                    {
                        AllAppsItems.Add(v);
                    }

                    return SubItemsList;
                }

                if ((!AllAppsItems.Contains(t)) && (!FolderIsDuplicate))
                {
                    AllAppsItems.Add(t);
                }
            }

            foreach (DiskItem x in AllAppsItems)
            {
                if (File.Exists(x.ItemPath))
                {
                    AllAppsReorgItems.Add(x);
                }
            }

            foreach (DiskItem x in AllAppsItems)
            {
                if (Directory.Exists(x.ItemPath))
                {
                    AllAppsReorgItems.Add(x);
                }
            }

            return AllAppsReorgItems;
        }

        public double RightColumnWidth
        {
            get => (double)GetValue(RightColumnWidthProperty);
            set => SetValue(RightColumnWidthProperty, value);
        }

        public static readonly DependencyProperty RightColumnWidthProperty =
            DependencyProperty.Register("RightColumnWidth", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(250);

        //Double TopAnimatedOut = 0;
        //Double TopAnimatedIn = 0;

        CircleEase CircleEase = new CircleEase()
        {
            EasingMode = EasingMode.EaseOut
        };

        String _pinnedItemsPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_PinnedApps.txt");
        String _placesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_Places.txt");

        /*public ObservableCollection<DiskItem> PinnedItems
        {
            get
            {
                var list = new ObservableCollection<DiskItem>();

                foreach (var s in File.ReadAllLines(_pinnedItemsPath))
                    list.Add(new DiskItem(s));
                /*{
                    var expS = Environment.ExpandEnvironmentVariables(s);
                    var item = new IconListViewItem()
                    {
                        Content = Path.GetFileNameWithoutExtension(expS),
                        Tag = expS
                    };
                    if (File.Exists(expS))
                    {
                        item.Icon = new Canvas()
                        {
                            Background = new SolidColorBrush(Colors.Gray),
                            Width = 24,
                            Height = 24
                        };
                        try
                        {
                            (item.Icon as Canvas).Background = new DiskItemToIconImageBrushConverter().Convert(new DiskItem(expS), null, 24, null) as ImageBrush;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    item.MouseLeftButtonUp += (sneder, args) =>
                    {
                        Hide();
                    };
                    list.Add(item);
                    Debug.WriteLine(expS);
                }*

                return list;
            }
            set
            {
                List<String> list = new List<String>();

                foreach (DiskItem i in value)
                    list.Add(i.ItemPath);

                File.WriteAllLines(_pinnedItemsPath, list.ToArray());
            }
        }*/

        public ObservableCollection<DiskItem> PinnedItems
        {
            get => (ObservableCollection<DiskItem>)GetValue(PinnedItemsProperty);
            set => SetValue(PinnedItemsProperty, value);
        }

        public static readonly DependencyProperty PinnedItemsProperty =
            DependencyProperty.Register("PinnedItems", typeof(ObservableCollection<DiskItem>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<DiskItem>()));

        /*public ObservableCollection<DiskItem> Places
        {
            get
            {
                var list = new ObservableCollection<DiskItem>();

                foreach (var s in File.ReadAllLines(_placesPath))
                    list.Add(new DiskItem(s));

                return list;
            }
            set
            {
                List<String> list = new List<String>();

                foreach (DiskItem i in value)
                    list.Add(i.ItemPath);

                File.WriteAllLines(_placesPath, list.ToArray());
            }
        }*/

        public ObservableCollection<DiskItem> AllApps
        {
            get => (ObservableCollection<DiskItem>)GetValue(AllAppsProperty);
            set => SetValue(AllAppsProperty, value);
        }

        public static readonly DependencyProperty AllAppsProperty =
            DependencyProperty.Register("AllApps", typeof(ObservableCollection<DiskItem>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<DiskItem>()));

        public ObservableCollection<DiskItem> Places
        {
            get => (ObservableCollection<DiskItem>)GetValue(PlacesProperty);
            set => SetValue(PlacesProperty, value);
        }

        public static readonly DependencyProperty PlacesProperty =
            DependencyProperty.Register("Places", typeof(ObservableCollection<DiskItem>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<DiskItem>()));

        public enum MenuMode
        {
            Normal,
            AllApps,
            Search,
            LeftColumnJumpList
        }

        public MenuMode CurrentMenuMode
        {
            get => (MenuMode)GetValue(CurrentMenuModeProperty);
            set => SetValue(CurrentMenuModeProperty, value);
        }

        public static readonly DependencyProperty CurrentMenuModeProperty = DependencyProperty.Register("CurrentMenuMode", typeof(MenuMode), typeof(MainWindow), new PropertyMetadata(MenuMode.Normal, OnCurrentMenuModePropertyChangedCallback));

        public static void OnCurrentMenuModePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var main = (sender as MainWindow);

            if ((main.CurrentMenuMode == MenuMode.Search) || (main.CurrentMenuMode == MenuMode.LeftColumnJumpList))
            {
                main.PlacesListView.Visibility = Visibility.Hidden;
            }
            else
            {
                main.PlacesListView.Visibility = Visibility.Visible;
            }

            if (main.CurrentMenuMode == MenuMode.Search)
            {
                main.SearchListView.Visibility = Visibility.Visible;
                main.FixedAppListsDockPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                main.SearchListView.Visibility = Visibility.Hidden;
                main.FixedAppListsDockPanel.Visibility = Visibility.Visible;
            }

            if (main.CurrentMenuMode == MenuMode.AllApps)
            {
                main.PinnedAppsListView.Visibility = Visibility.Hidden;
                main.AllAppsTreeView.Visibility = Visibility.Visible;
                if (main.AllAppsToggleButton.IsChecked != true)
                {
                    main.AllAppsToggleButton.IsChecked = true;
                }
            }
            else
            {
                main.PinnedAppsListView.Visibility = Visibility.Visible;
                main.AllAppsTreeView.Visibility = Visibility.Collapsed;
                if (main.AllAppsToggleButton.IsChecked != false)
                {
                    main.AllAppsToggleButton.IsChecked = false;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            //Application.Current.MainWindow = this;

            //Deactivated += (sender, e) => Hide();
            GetItems();

            //AllAppsTreeView.ItemsSource = DiskItem.AllApps;

            PinnedItems.CollectionChanged += Items_CollectionChanged;
            Places.CollectionChanged += Items_CollectionChanged;


            //Module.MessageReceived += Module_MessageReceived;
            //Show();
        }

        void GetItems()
        {
            //AllApps.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")));
            //AllApps = GetAllApps();
            var appData = new DiskItem(Environment.ExpandEnvironmentVariables(@"%appdata%\Microsoft\Windows\Start Menu\Programs"));
            int lastDirectoryIndex = -1;
            foreach (DiskItem d in appData.SubItems)
            {
                if (d.ItemCategory == DiskItem.DiskItemCategory.Directory)
                {
                    AllApps.Insert(lastDirectoryIndex + 1, d);
                    lastDirectoryIndex++;
                }
                else
                    AllApps.Add(d);
            }

            foreach (var s in File.ReadAllLines(_pinnedItemsPath))
                PinnedItems.Add(new DiskItem(s));

            foreach (var s in File.ReadAllLines(_placesPath))
                Places.Add(new DiskItem(s));
        }

        void SaveItems()
        {
            string[] pinnedWriteList = new string[PinnedItems.Count];
            for (int i = 0; i < PinnedItems.Count; i++)
                pinnedWriteList[i] = PinnedItems.ElementAt(i).ItemPath;
            File.WriteAllLines(_pinnedItemsPath, pinnedWriteList.ToArray());

            string[] placesWriteList = new string[Places.Count];
            for (int i = 0; i < Places.Count; i++)
                placesWriteList[i] = Places.ElementAt(i).ItemPath;
            File.WriteAllLines(_placesPath, placesWriteList.ToArray());
        }

        private void Module_MessageReceived(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //MessageBox.Show("MESSAGE RECEIVED BY STARTBEATMENU");
                DisplayMenu();
                //Background = new SolidColorBrush(Colors.Red);
            }));
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<DiskItem>)
            {
                foreach (DiskItem d in (sender as ObservableCollection<DiskItem>))
                {
                    Debug.WriteLine("PATH: " + d.ItemPath);
                }
            }
        }

        /*protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                DwmIsCompositionEnabled(out var enabled);
                if (enabled)
                {
                    DWM_BLURBEHIND blur = new DWM_BLURBEHIND()
                    {
                        dwFlags = DWM_BB.Enable,
                        fEnable = true,
                        hRgnBlur = IntPtr.Zero,
                        fTransitionOnMaximized = true
                    };
                    DwmEnableBlurBehindWindow(new WindowInteropHelper(this).EnsureHandle(), ref blur);
                }
            }
        }*/

        public void DisplayMenu()
        {
            Topmost = false;
            Topmost = true;
            Show();
            Focus();
            Activate();

            SearchTextBox.Focus();
            UpdatePosition();
            //Visibility = Visibility.Visible;

            /*DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                Duration = AnimationDuration,
                EasingFunction = CircleEase,
                To = 1
            };


            DoubleAnimation topAnimation = new DoubleAnimation()
            {
                Duration = AnimationDuration,
                EasingFunction = CircleEase,
                From = TopAnimatedOut,
                To = TopAnimatedIn
            };*/

            //BeginAnimation(MainWindow.OpacityProperty, opacityAnimation);
            //BeginAnimation(MainWindow.TopProperty, topAnimation);
        }

        public void UpdatePosition()
        {
            var p = SystemScaling.CursorPosition;
            var s = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)(p.X), (int)(p.Y))).WorkingArea;
            //MaxHeight = s.Height;
            Left = s.Left;
            Top = s.Bottom - ActualHeight;
        }

        /*new public void Hide()
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                Duration = AnimationDuration,
                EasingFunction = CircleEase,
                To = 0
            };


            DoubleAnimation topAnimation = new DoubleAnimation()
            {
                Duration = AnimationDuration,
                EasingFunction = CircleEase,
                From = TopAnimatedIn,
                To = TopAnimatedOut
            };
            topAnimation.Completed += delegate { base.Hide(); };

            BeginAnimation(MainWindow.OpacityProperty, opacityAnimation);
            BeginAnimation(MainWindow.TopProperty, topAnimation);
        }*/

        /*private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (Visibility == Visibility.Visible)
            {

            }
        }*/

        private void ListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var l = (sender as ListView);
            if (l.SelectedItem != null)
            {
                var s = Environment.ExpandEnvironmentVariables((l.SelectedItem as DiskItem).ItemPath);
                /*if (File.Exists(s) || Directory.Exists(s))
                {
                    Process.Start(s);
                }
                else
                {
                    Process.Start("cmd.exe", @"/C " + s);
                }*/
                (l.SelectedItem as DiskItem).Open();
                l.SelectedItem = null;
                //////Hide(); a
            }
        }

        private void TreeView_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var l = (sender as TreeView);
            if (l.SelectedItem != null)
            {
                var s = Environment.ExpandEnvironmentVariables((l.SelectedItem as DiskItem).ItemPath);
                if (File.Exists(s) || Directory.Exists(s))
                {
                    Process.Start(s);
                }
                else
                {
                    Process.Start("cmd.exe", @"/C " + s);
                }
                l.SelectedValuePath = null;
                //////Hide(); a
            }
        }

        private void AllAppsToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            if (AllAppsToggleButton.IsChecked == true)
                CurrentMenuMode = MenuMode.AllApps;
            else
                CurrentMenuMode = MenuMode.Normal;

            /*if (AllAppsToggleButton.IsChecked == true)
            {
                BeginStoryboard((Storyboard)Resources["ShowAllApps"]);
            }
            else
            {
                BeginStoryboard((Storyboard)Resources["HideAllApps"]);
            }*/
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(SearchTextBox.Text))
                CurrentMenuMode = MenuMode.Normal;
            else
                CurrentMenuMode = MenuMode.Search;
        }

        private void ShutDownContextMenu_IsVisibleChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as ContextMenu).Visibility == Visibility.Visible)
            {

            }
            else
            {

            }
        }

        private void ShutDownRightButton_Click(Object sender, RoutedEventArgs e)
        {
            /*var m = ShutDownRightButton.PointToScreen(new Point(0, 0));
            var c = SystemScaling.CursorPosition;*/
            var menu = (sender as Button).ContextMenu;
            /*menu.HorizontalOffset = ((c.X - m.X) * -1) + ShutDownRightButton.ActualWidth;
            menu.VerticalOffset = ((c.Y - m.Y) * -1) + ShutDownRightButton.ActualHeight;
            menu.IsOpen = true;
            menu.HorizontalOffset = ((c.X - m.X) * -1) + ShutDownRightButton.ActualWidth;
            menu.VerticalOffset = ((c.Y - m.Y) * -1) + ShutDownRightButton.ActualHeight;*/
            //(sender as Button).contextm
            menu.PlacementTarget = sender as Button;
            menu.Placement = PlacementMode.Right;
            /*menu.HorizontalOffset = 0;
            menu.VerticalOffset = 0;*/
            menu.IsOpen = true;
            ShutDownRightButton.IsEnabled = false;
        }

        private void ContextMenu_ContextMenuClosing(Object sender, ContextMenuEventArgs e)
        {

        }

        private void ContextMenu_Closed(Object sender, RoutedEventArgs e)
        {
            ShutDownRightButton.IsEnabled = true;
        }

        private void SwitchUser_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void LogOff_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.Instance.SignOut();
        }

        private void Lock_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.Instance.LockUserAccount();
        }

        private void Restart_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.Instance.RestartSystem();
        }

        private void Sleep_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.Instance.SleepSystem();
        }

        private void Hibernate_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void ListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /*private void PlacesOpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ownerListItem = ((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as FrameworkElement).TemplatedParent as FrameworkElement).TemplatedParent as FrameworkElement;
            Places[(VisualTreeHelper.GetParent(ownerListItem) as Panel).Children.IndexOf(ownerListItem)].Open();
            //////Hide(); a
        }

        private void PlacesRemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ownerListItem = ((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as FrameworkElement).TemplatedParent as FrameworkElement).TemplatedParent as FrameworkElement;
            Places.RemoveAt((VisualTreeHelper.GetParent(ownerListItem) as Panel).Children.IndexOf(ownerListItem));
        }*/

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePosition();
        }

        private void PinnedItemsListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string s in files)
                {
                    if (!Directory.Exists(s))
                        PinnedItems.Add(new DiskItem(s));
                }
            }
        }

        private void PlacesListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string s in files)
                {
                    if (Directory.Exists(s))
                        Places.Add(new DiskItem(s));
                }
            }
        }
    }
}