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
using Sentinel.Classification.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Classification.Controls
{
    /// <summary>
    /// Interaction logic for ClassificationsControl.xaml
    /// </summary>
    public partial class ClassificationsControl : UserControl
    {
        public ClassificationsControl()
        {
            InitializeComponent();
            Classifier = ServiceLocator.Instance.Get<IClassifierService>();
            DataContext = this;
        }

        public IClassifierService Classifier { get; private set; }
    }
}