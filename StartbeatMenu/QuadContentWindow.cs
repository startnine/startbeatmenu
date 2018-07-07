using System;
using System.Windows;

namespace StartbeatMenu
{
    public class QuadContentWindow : Window
    {
        public QuadContentWindow()
        {

        }

        public Object SecondContent
        {
            get => GetValue(SecondContentProperty);
            set => SetValue(SecondContentProperty, (value));
        }

        public static readonly DependencyProperty SecondContentProperty =
            DependencyProperty.Register("SecondContent", typeof(Object), typeof(QuadContentWindow), new PropertyMetadata());

        public Object ThirdContent
        {
            get => GetValue(ThirdContentProperty);
            set => SetValue(ThirdContentProperty, (value));
        }

        public static readonly DependencyProperty ThirdContentProperty =
            DependencyProperty.Register("ThirdContent", typeof(Object), typeof(QuadContentWindow), new PropertyMetadata());

        public Object FourthContent
        {
            get => GetValue(FourthContentProperty);
            set => SetValue(FourthContentProperty, (value));
        }

        public static readonly DependencyProperty FourthContentProperty =
            DependencyProperty.Register("FourthContent", typeof(Object), typeof(QuadContentWindow), new PropertyMetadata());
    }
}
