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
using Sentinel.Highlighting;
using Sentinel.Services;

#endregion

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for HighlightersControl.xaml
    /// </summary>
    public partial class HighlightersControl : UserControl
    {
        public HighlightersControl()
        {
            InitializeComponent();
            Highlighters = ServiceLocator.Instance.Get<IHighlightingService>();
            DataContext = this;
        }

        public IHighlightingService Highlighters { get; private set; }
    }
}