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
using Sentinel.Support.Mvvm;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Filters.Gui
{
    public class AddEditFilter : ViewModelBase
    {
        private readonly Window window;

        private string name = "Unnamed";

        private string pattern = "pattern";

        private LogEntryField field;

        private MatchMode mode;

        public AddEditFilter(Window window, bool editMode)
        {
            this.window = window;
            if (window != null)
            {
                window.Title = String.Format("{0} Filter", (editMode ? "Edit" : "Register"));
            }

            Accept = new DelegateCommand(AcceptDialog, Validates);
            Reject = new DelegateCommand(RejectDialog);
        }

        public ICommand Accept { get; private set; }

        public LogEntryField Field
        {
            get
            {
                return field;
            }
            set
            {
                field = value;
                OnPropertyChanged("Field");
            }
        }

        public MatchMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                OnPropertyChanged("Mode");
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