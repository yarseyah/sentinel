#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows.Controls;
using System.Windows.Threading;

#endregion

namespace Sentinel.Support.Wpf
{
    public class DataBoundToolbar : ToolBar
    {
        public override void OnApplyTemplate()
        {
            Dispatcher.BeginInvoke(
                new InvalidateMeasurementDelegate(InvalidateMeasure),
                DispatcherPriority.Background,
                null);
            base.OnApplyTemplate();
        }

        #region Nested type: InvalidateMeasurementDelegate

        private delegate void InvalidateMeasurementDelegate();

        #endregion
    }
}