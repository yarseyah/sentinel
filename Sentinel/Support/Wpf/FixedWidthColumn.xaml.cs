#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Sentinel.Support.Wpf
{
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
            FixedWidthColumn fwc = o as FixedWidthColumn;
            if (fwc != null)
            {
                return fwc.FixedWidth;
            }

            return baseValue;
        }

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FixedWidthColumn fwc = o as FixedWidthColumn;
            if (fwc != null)
            {
                fwc.CoerceValue(WidthProperty);
            }
        }
    }
}