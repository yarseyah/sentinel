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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sentinel.Support;

#endregion

namespace Sentinel.Filters
{

    #region Using directives

    #endregion

    public class AddEditFilter : ViewModelBase
    {
        private readonly List<string> fields = new List<string>
                                                   {
                                                       "Type",
                                                       "System"
                                                   };

        private readonly List<string> filterMethods = new List<string>
                                                          {
                                                              "Exact",
                                                              "Substring",
                                                              "RegularExpression"
                                                          };

        private readonly Window window;

        private int fieldIndex = 0;

        private int filterMethod = 0;

        private string name = "Unnamed";

        private string pattern = "pattern";

        public AddEditFilter(Window window, bool editMode)
        {
            this.window = window;
            if (window != null)
            {
                window.Title = String.Format("{0} Filter", (editMode ? "Edit" : "Add"));
            }

            Accept = new DelegateCommand(AcceptDialog, Validates);
            Reject = new DelegateCommand(RejectDialog);
        }

        public ICommand Accept { get; private set; }

        public int FieldIndex
        {
            get
            {
                return fieldIndex;
            }

            set
            {
                if (fieldIndex != value)
                {
                    fieldIndex = value;
                    OnPropertyChanged("FieldIndex");
                }
            }
        }

        public IEnumerable<string> Fields
        {
            get
            {
                return fields;
            }
        }

        public int FilterMethod
        {
            get
            {
                return filterMethod;
            }

            set
            {
                if (filterMethod != value)
                {
                    filterMethod = value;
                    OnPropertyChanged("FilterMethod");
                }
            }
        }

        public IEnumerable<string> FilterMethods
        {
            get
            {
                return filterMethods;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Pattern
        {
            get
            {
                return pattern;
            }

            set
            {
                if (value != pattern)
                {
                    pattern = value;
                    OnPropertyChanged("Pattern");
                }
            }
        }

        public ICommand Reject { get; private set; }

        private void AcceptDialog(object obj)
        {
            window.DialogResult = true;
            window.Close();
        }

        private void RejectDialog(object obj)
        {
            window.DialogResult = false;
            window.Close();
        }

        private bool Validates(object obj)
        {
            return Name.Length > 0
                   && Pattern.Length > 0;
        }
    }
}