using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Sentinel.Classification.Gui
{
    public class AddEditClassifier : ViewModelBase
    {
        private readonly Window window;

        private string name = "Unnamed";

        private string pattern = "pattern";

        private LogEntryField field;

        private MatchMode mode;

        private string type;

        public AddEditClassifier(Window window, bool editMode)
        {
            this.window = window;
            if (window != null)
            {
                window.Title = String.Format("{0} Classifier", (editMode ? "Edit" : "Register"));
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

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                if (value != type)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        //public string Image
        //{
        //    get
        //    {
        //        return image;
        //    }

        //    set
        //    {
        //        if (value != image)
        //        {
        //            image = value;
        //            OnPropertyChanged("Image");
        //        }
        //    }
        //}

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