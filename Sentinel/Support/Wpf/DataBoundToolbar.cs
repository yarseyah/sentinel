namespace Sentinel.Support.Wpf
{
    using System.Windows.Controls;
    using System.Windows.Threading;

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

        private delegate void InvalidateMeasurementDelegate();
    }
}