#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

using Sentinel.Interfaces;

namespace Sentinel.Logger
{
    public static class LogEntryFieldHelper
    {
        public static LogEntryField FieldNameToEnumeration(string field)
        {
            switch (field)
            {
                case "Type":
                    return LogEntryField.Type;
                case "System":
                    return LogEntryField.System;
                //case "Source":
                //    return LogEntryField.Source;
                case "Classification":
                    return LogEntryField.Classification;
                case "Description":
                    return LogEntryField.Description;
                //case "Host":
                //    return LogEntryField.Host;
                default:
                    return LogEntryField.None;
            }
        }
    }
}