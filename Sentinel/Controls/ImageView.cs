namespace Sentinel.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class ImageView : ViewBase
    {
        protected override object DefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(GetType(), "ImageView");
            }
        }

        protected override object ItemContainerDefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(GetType(), "ImageViewItem");
            }
        }
    }
}