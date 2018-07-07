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

        public List<IconListViewItem> PinnedItems
        {
            get
            {
                var list = new List<IconListViewItem>();
                foreach(var s in File.ReadAllLines(Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_PinnedApps.txt")))
                {
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
                            Background = new ImageBrush(MiscTools.GetIconFromFilePath(expS, 24, 24)),
                            Width = 24,
                            Height = 24
                        };
                    }
                    item.MouseLeftButtonUp += (sneder, args) =>
                    {
                        Hide();
                    };
                    list.Add(item);
                    Debug.WriteLine(expS);
                }
                return list;
            }
            set
            {
                List<String> list = new List<String>();
                foreach (IconListViewItem i in value)
                {
                    list.Add(i.Tag.ToString());
                }
                File.WriteAllLines(Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\StartbeatMenu_PinnedApps.txt"), list.ToArray());
            }
        }

        ToggleButton tempStart;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            Left = SystemParameters.WorkArea.Left;

            TopAnimatedIn = SystemParameters.WorkArea.Bottom - Height;
            TopAnimatedOut = TopAnimatedIn + 50;

            Deactivated += (sender, e) => Hide();

            foreach (var s in Directory.EnumerateDirectories(Environment.ExpandEnvironmentVariables(@"%userprofile%")))
            {
                ListViewItem item = new ListViewItem()
                {
                    Content = System.IO.Path.GetFileName(s),
                    Tag = s
                };
                item.MouseLeftButtonUp += delegate { Process.Start(s); };
                if (!(item.Content.ToString().StartsWith(".")))
                {
                    PlacesListView.Items.Add(item);
                }
            }

            
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

        private void PinnedListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem != null)
            {
                var s = ((sender as ListView).SelectedItem as ListViewItem).Tag.ToString();
                if (File.Exists(s))
                {
                    Process.Start(s);
                }
                else
                {
                    Process.Start("cmd.exe", @"/C " + s);
                }
                (sender as ListView).SelectedItem = null;
            }
        }

        private void AllAppsToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            if (AllAppsToggleButton.IsChecked == true)
            {
                BeginStoryboard((Storyboard)Resources["ShowAllApps"]);
            }
            else
            {
                BeginStoryboard((Storyboard)Resources["HideAllApps"]);
            }
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
            var m = MainTools.GetDpiScaledGlobalControlPosition(ShutDownRightButton);
            var c = MainTools.GetDpiScaledCursorPosition();
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
            SystemPowerTools.SignOut();
        }

        private void Lock_Click(Object sender, RoutedEventArgs e)
        {
            SystemPowerTools.LockUserAccount();
        }

        private void Restart_Click(Object sender, RoutedEventArgs e)
        {
            SystemPowerTools.RestartSystem();
        }

        private void Sleep_Click(Object sender, RoutedEventArgs e)
        {
            SystemPowerTools.SleepSystem();
        }

        private void Hibernate_Click(Object sender, RoutedEventArgs e)
        {
            //BBBBB
        }
    }
}
