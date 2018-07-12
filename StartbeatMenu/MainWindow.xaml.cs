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
using Start9.Api.Controls;
using Start9.Api.Tools;
using Start9.Api.DiskItems;
using static Start9.Api.SystemContext;
using static Start9.Api.SystemScaling;
using static Start9.Api.Extensions;
using System.Collections.ObjectModel;

namespace StartbeatMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : QuadContentWindow
    {
        [DllImport("dwmapi.dll")]
        static extern Int32 DwmIsCompositionEnabled(out Boolean enabled);

        [DllImport("dwmapi.dll")]
        static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [StructLayout(LayoutKind.Sequential)]
        struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;
            public Boolean fEnable;
            public IntPtr hRgnBlur;
            public Boolean fTransitionOnMaximized;

            public DWM_BLURBEHIND(Boolean enabled)
            {
                fEnable = enabled ? true : false;
                hRgnBlur = IntPtr.Zero;
                fTransitionOnMaximized = false;
                dwFlags = DWM_BB.Enable;
            }

            public System.Drawing.Region Region
            {
                get { return System.Drawing.Region.FromHrgn(hRgnBlur); }
            }

            public Boolean TransitionOnMaximized
            {
                get { return fTransitionOnMaximized != false; }
                set
                {
                    fTransitionOnMaximized = value ? true : false;
                    dwFlags |= DWM_BB.TransitionMaximized;
                }
            }

            public void SetRegion(System.Drawing.Graphics graphics, System.Drawing.Region region)
            {
                hRgnBlur = region.GetHrgn(graphics);
                dwFlags |= DWM_BB.BlurRegion;
            }
        }

        [Flags]
        enum DWM_BB
        {
            Enable = 1,
            BlurRegion = 2,
            TransitionMaximized = 4
        }

        TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(250);

        Double TopAnimatedOut = 0;
        Double TopAnimatedIn = 0;

        CircleEase CircleEase = new CircleEase()
        {
            EasingMode = EasingMode.EaseOut
        };

        String _pinnedItemsPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_PinnedApps.txt");
        String _placesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_Places.txt");

        public ObservableCollection<DiskItem> PinnedItems
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
                }*/

                return list;
            }
            set
            {
                List<String> list = new List<String>();

                foreach (DiskItem i in value)
                    list.Add(i.ItemPath);

                File.WriteAllLines(_pinnedItemsPath, list.ToArray());
            }
        }

        public ObservableCollection<DiskItem> Places
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
        }

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
                main.AllAppsTreeView.Visibility = Visibility.Hidden;
                if (main.AllAppsToggleButton.IsChecked != false)
                {
                    main.AllAppsToggleButton.IsChecked = false;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;

            Deactivated += (sender, e) => Hide();

            AllAppsTreeView.ItemsSource = DiskItem.AllApps;

            PinnedItems.CollectionChanged += Items_CollectionChanged;
            Places.CollectionChanged += Items_CollectionChanged;

            IsVisibleChanged += MainWindow_IsVisibleChanged;
        }

        public void DisplayMenu()
        {
            Topmost = true;
            Show();
            Focus();
            Activate();
            SearchTextBox.Focus();
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                var p = CursorPosition;
                var s = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)(p.X), (int)(p.Y))).WorkingArea;
                MaxHeight = s.Height;
                Left = s.Left;

                TopAnimatedIn = s.Bottom - Height;
                TopAnimatedOut = TopAnimatedIn + 50;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        protected override void OnSourceInitialized(EventArgs e)
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
        }

        new public void Show()
        {
            base.Show();
            //Visibility = Visibility.Visible;

            DoubleAnimation opacityAnimation = new DoubleAnimation()
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
            };

            BeginAnimation(MainWindow.OpacityProperty, opacityAnimation);
            BeginAnimation(MainWindow.TopProperty, topAnimation);
        }

        new public void Hide()
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
        }

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
                if (File.Exists(s) || Directory.Exists(s))
                {
                    Process.Start(s);
                }
                else
                {
                    Process.Start("cmd.exe", @"/C " + s);
                }
                l.SelectedItem = null;
                Hide();
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
                Hide();
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
            var m = ShutDownRightButton.PointToScreenInWpfUnits(new Point(0,0));
            var c = Start9.Api.SystemScaling.CursorPosition;
            var menu = (sender as Button).ContextMenu;
            menu.HorizontalOffset = ((c.X - m.X) * -1) + ShutDownRightButton.ActualWidth;
            menu.VerticalOffset = ((c.Y - m.Y) * -1) + ShutDownRightButton.ActualHeight;
            menu.IsOpen = true;
            menu.HorizontalOffset = ((c.X - m.X) * -1) + ShutDownRightButton.ActualWidth;
            menu.VerticalOffset = ((c.Y - m.Y) * -1) + ShutDownRightButton.ActualHeight;
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
            //AAAAA
        }

        private void LogOff_Click(Object sender, RoutedEventArgs e)
        {
            SignOut();
        }

        private void Lock_Click(Object sender, RoutedEventArgs e)
        {
            LockUserAccount();
        }

        private void Restart_Click(Object sender, RoutedEventArgs e)
        {
            RestartSystem();
        }

        private void Sleep_Click(Object sender, RoutedEventArgs e)
        {
            SleepSystem();
        }

        private void Hibernate_Click(Object sender, RoutedEventArgs e)
        {
            //BBBBB
        }
    }
}
