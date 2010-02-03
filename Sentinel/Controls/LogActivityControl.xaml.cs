#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Highlighting;
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
            Highlight = ServiceLocator.Instance.Get<IHighlightingService>();
        }

        public IHighlightingService Highlight { get; private set; }
    }
}