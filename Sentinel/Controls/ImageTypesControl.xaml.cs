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
using Sentinel.Images;
using Sentinel.Services;

#endregion

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for ImageTypesControl.xaml
    /// </summary>
    public partial class ImageTypesControl : UserControl
    {
        public ImageTypesControl()
        {
            InitializeComponent();
            Images = ServiceLocator.Instance.Get<ITypeImageService>();
            DataContext = this;
        }

        public ITypeImageService Images { get; private set; }
    }
}