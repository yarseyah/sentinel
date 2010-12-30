#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters
{
    public class HighlighterData
    {
        private bool enabled = true;

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }

        public LogEntryField Field { get; set; }

        public MatchMode Mode { get; set; }

        public string Name { get; set; }

        public IHighlighterStyle Style { get; set; }

        public string TypeMatch { get; set; }
    }
}