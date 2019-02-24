using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StartbeatMenu
{
    public class Config : DependencyObject
    {
        public bool UseSmallIcons
        {
            get => (bool)GetValue(UseSmallIconsProperty);
            set => SetValue(UseSmallIconsProperty, value);
        }

        public static readonly DependencyProperty UseSmallIconsProperty =
            DependencyProperty.Register("UseSmallIcons", typeof(bool), typeof(Config), new PropertyMetadata(false));

        public bool RightColumnIcons
        {
            get => (bool)GetValue(RightColumnIconsProperty);
            set => SetValue(RightColumnIconsProperty, value);
        }

        public static readonly DependencyProperty RightColumnIconsProperty =
            DependencyProperty.Register("RightColumnIcons", typeof(bool), typeof(Config), new PropertyMetadata(false));

        public static SettingsWindow SettingsWindow = new SettingsWindow();

        public static Config Instance = new Config();

        private Config()
        { }
    }
}
