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

namespace Sentinel.Controls
{
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