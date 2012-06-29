#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Highlighters.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for LogActivityControl.xaml
    /// </summary>
    public partial class LogActivityControl
    {
        public LogActivityControl()
        {
            InitializeComponent();
            Highlight = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
        }

        public IHighlightingService<IHighlighter> Highlight { get; private set; }
    }
}