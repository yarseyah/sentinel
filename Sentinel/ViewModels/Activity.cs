#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using Sentinel.Logger;
using Sentinel.Support;

#endregion

namespace Sentinel.ViewModels
{

    #region Using directives

    #endregion

    public class Activity : ViewModelBase
    {
        private string classification;

        private string description;

        private DateTime lastActivity;

        private string lastActivityType;

        private DateTime? lastError;

        private DateTime? lastWarning;

        private string system;

        /// <summary>
        /// Initializes a new instance of the Activity class.
        /// </summary>
        /// <param name="entry">Log entry to wrap as an activity.</param>
        public Activity(LogEntry entry)
        {
            System = entry.System;
            Classification = entry.Classification;
            lastError = null;
            Update(entry);
        }

        public string Classification
        {
            get
            {
                return classification;
            }

            private set
            {
                if (value != classification)
                {
                    classification = value;
                    OnPropertyChanged("Classification");
                }
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            private set
            {
                if (value != description)
                {
                    description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public DateTime LastActivity
        {
            get
            {
                return lastActivity;
            }

            private set
            {
                if (value != lastActivity)
                {
                    lastActivity = value;
                    OnPropertyChanged("LastActivity");
                }
            }
        }

        public string LastActivityType
        {
            get
            {
                return lastActivityType;
            }

            private set
            {
                if (value != lastActivityType)
                {
                    lastActivityType = value;
                    OnPropertyChanged("LastActivityType");
                }
            }
        }

        public DateTime? LastError
        {
            get
            {
                return lastError;
            }

            private set
            {
                if (value != lastError)
                {
                    lastError = value;
                    OnPropertyChanged("LastError");
                }
            }
        }

        public DateTime? LastWarning
        {
            get
            {
                return lastWarning;
            }

            private set
            {
                if (value != lastWarning)
                {
                    lastWarning = value;
                    OnPropertyChanged("LastWarning");
                }
            }
        }

        public string System
        {
            get
            {
                return system;
            }

            private set
            {
                if (value != system)
                {
                    system = value;
                    OnPropertyChanged("System");
                }
            }
        }

        public void Tick()
        {
            OnPropertyChanged("LastActivity");
        }

        public void Update(LogEntry entry)
        {
            LastActivityType = entry.Type;
            LastActivity = entry.DateTime;
            Description = entry.Description;

            if (LastActivityType == "ERROR")
            {
                LastError = entry.DateTime;
            }
            else if (LastActivityType == "WARN")
            {
                LastWarning = entry.DateTime;
            }
        }
    }
}