namespace Sentinel.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class ImageView : ViewBase
    {
        protected override object DefaultStyleKey => new ComponentResourceKey(GetType(), "ImageView");

        protected override object ItemContainerDefaultStyleKey => new ComponentResourceKey(GetType(), "ImageViewItem");
    }
}