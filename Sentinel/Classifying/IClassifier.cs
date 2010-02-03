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
    public interface IClassifier
    {
        bool Enabled { get; set; }

        string Name { get; set; }

        string Type { get; }

        bool IsMatch(object parameter);
    }
}