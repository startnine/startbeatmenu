using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using WindowsSharp.DiskItems;

namespace StartbeatMenu
{
    public class PlaceContextMenuBehavior : Behavior<ContextMenu>
    {
        public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(PlaceContextMenuBehavior), new PropertyMetadata(null));

        public MainWindow BaseWindow
        {
            get => (MainWindow)GetValue(BaseWindowProperty);
            set => SetValue(BaseWindowProperty, value);
        }

        public static readonly DependencyProperty BaseWindowProperty =
            DependencyProperty.Register("BaseWindow", typeof(MainWindow), typeof(PlaceContextMenuBehavior), new PropertyMetadata(null));

        public MenuItem OpenMenuItem
        {
            get => (MenuItem)GetValue(OpenMenuItemProperty);
            set => SetValue(OpenMenuItemProperty, value);
        }

        public static readonly DependencyProperty OpenMenuItemProperty =
            DependencyProperty.Register("OpenMenuItem", typeof(MenuItem), typeof(PlaceContextMenuBehavior), new PropertyMetadata(null, OnOpenMenuItemChangedCallback));

        static void OnOpenMenuItemChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as PlaceContextMenuBehavior).OpenMenuItem_Click;

            if (e.OldValue != null)
                (e.OldValue as MenuItem).Click -= (sender as PlaceContextMenuBehavior).OpenMenuItem_Click;
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("OpenMenuItem_Click " + (BaseWindow != null).ToString() + " " + (TargetItem != null).ToString() + " " + BaseWindow.Places.Contains(TargetItem).ToString());
            /*if ((BaseWindow != null) && (TargetItem != null))
            {
                if (BaseWindow.Places.Contains(TargetItem))
                    BaseWindow.PlacesListView.SelectedItem = TargetItem;
                //.SelectedIndex = BaseWindow.Places.IndexOf(TargetItem);
            }*/
            if (TargetItem != null)
                TargetItem.Open();
        }

        public MenuItem RemoveMenuItem
        {
            get => (MenuItem)GetValue(RemoveMenuItemProperty);
            set => SetValue(RemoveMenuItemProperty, value);
        }

        public static readonly DependencyProperty RemoveMenuItemProperty =
            DependencyProperty.Register("RemoveMenuItem", typeof(MenuItem), typeof(PlaceContextMenuBehavior), new PropertyMetadata(null, OnRemoveMenuItemChangedCallback));

        static void OnRemoveMenuItemChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as PlaceContextMenuBehavior).RemoveMenuItem_Click;

            if (e.OldValue != null)
                (e.OldValue as MenuItem).Click -= (sender as PlaceContextMenuBehavior).RemoveMenuItem_Click;
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("RemoveMenuItem_Click " + (BaseWindow != null).ToString() + " " + (TargetItem != null).ToString());
            if ((BaseWindow != null) && (TargetItem != null))
            {
                foreach (DiskItem d in BaseWindow.Places)
                {
                    if (TargetItem.ItemPath.ToLowerInvariant() == d.ItemPath.ToLowerInvariant()) //(BaseWindow.Places.Contains(TargetItem))
                    {
                        Debug.WriteLine("FOUND: " + d.ItemPath);
                        //(BaseWindow.PlacesListView.ItemsSource as ObservableCollection<DiskItem>).Remove(d);
                        BaseWindow.Places.Remove(d);
                        break;
                    }
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            //BaseWindow = Window.GetWindow(AssociatedObject.PlacementTarget) as MainWindow;
            Binding baseWindowBinding = new Binding()
            {
                Source = Window.GetWindow(AssociatedObject.PlacementTarget) as MainWindow,
                //Path = new PropertyPath("ShadowStyle"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, PlaceContextMenuBehavior.BaseWindowProperty, baseWindowBinding);
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }
    }
}
