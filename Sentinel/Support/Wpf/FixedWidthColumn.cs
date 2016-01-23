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
                new FrameworkPropertyMetadata(double.NaN, OnFixedWidthChanged));

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(
                typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, OnCoerceWidth));
        }

        public double FixedWidth
        {
            get
            {
                return (double)GetValue(FixedWidthProperty);
            }

            set
            {
                SetValue(FixedWidthProperty, value);
            }
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            var fwc = o as FixedWidthColumn;
            return fwc?.FixedWidth ?? baseValue;
        }

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            fwc?.CoerceValue(WidthProperty);
        }
    }
}