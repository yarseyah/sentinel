namespace Sentinel.Support.Wpf
{
    using System.Windows;
    using System.Windows.Controls;

    public class FixedWidthColumn : GridViewColumn
    {
        public static readonly DependencyProperty FixedWidthProperty =
            DependencyProperty.Register(
                "FixedWidth",
                typeof(double),
                typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(
                typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        public double FixedWidth
        {
            get
            {
                return (double) GetValue(FixedWidthProperty);
            }

            set
            {
                SetValue(FixedWidthProperty, value);
            }
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc != null)
            {
                return fwc.FixedWidth;
            }

            return baseValue;
        }

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc != null)
            {
                fwc.CoerceValue(WidthProperty);
            }
        }
    }
}