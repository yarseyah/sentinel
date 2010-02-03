#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Classifying
{
    public abstract class IncomingClassifier : IIncomingClassifier
    {
        protected IncomingClassifier(string type)
        {
            Type = type;
        }

        #region IIncomingClassifier Members

        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Type { get; private set; }

        public abstract bool IsMatch(object parameter);

        #endregion
    }
}