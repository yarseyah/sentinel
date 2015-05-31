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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Highlighters.Gui
{
    public class AddEditHighlighter : ViewModelBase
    {
        private readonly Window window;

        private int backgroundColourIndex = 1;

        private Dictionary<string, Color> colours = GetColours();

        private bool coloursAreClose;

        private int foregroundColourIndex;                

        private bool overrideBackgroundColour = false;

        private bool overrideForegroundColour = false;

        private string name = "Untitled";

        private string pattern = "pattern";

        private LogEntryField field;

        private MatchMode mode;

        public AddEditHighlighter(Window window, bool editMode)
        {
            this.window = window;
            if (window != null)
            {
                window.Title = editMode ? "Edit Highlighter" : "Add Highlighter";
            }

            PropertyChanged += CloseColourCheck;

            Accept = new DelegateCommand(AcceptDialog, Validates);
            Reject = new DelegateCommand(RejectDialog);
        }

        public ICommand Accept { get; private set; }

        public Color BackgroundColour
        {
            get
            {
                string key = colours.Keys.OrderBy(e => e).ToList()[backgroundColourIndex];
                return colours[key];
            }

            set
            {
                KeyValuePair<string, Color> find = colours.Where(r => r.Value == value).FirstOrDefault();
                if (find.Key == null)
                { 
                    throw new NotSupportedException(string.Format("Match for {0} not found in system colours", value));
                }

                int index = colours.Keys.OrderBy(n => n).ToList().IndexOf(find.Key);
                BackgroundColourIndex = index;
            }
        }

        public int BackgroundColourIndex
        {
            get
            {
                return backgroundColourIndex;
            }

            set
            {
                if (value != backgroundColourIndex)
                {
                    backgroundColourIndex = value;
                    OnPropertyChanged("BackgroundColourIndex");
                }
            }
        }

        public IEnumerable<string> BackgroundColours
        {
            get
            {
                return colours.Keys;
            }
        }

        public bool ColoursClose
        {
            get
            {
                return coloursAreClose;
            }

            private set
            {
                if (coloursAreClose != value)
                {
                    coloursAreClose = value;
                    OnPropertyChanged("ColoursClose");
                }
            }
        }

        public Color ForegroundColour
        {
            get
            {
                string key = colours.Keys.OrderBy(e => e).ToList()[foregroundColourIndex];
                return colours[key];
            }

            set
            {
                KeyValuePair<string, Color> find = colours.Where(r => r.Value == value).FirstOrDefault();
                if (find.Key == null)
                {
                    throw new NotSupportedException(string.Format("Match for {0} not found in system colours", value));
                }

                int index = colours.Keys.OrderBy(n => n).ToList().IndexOf(find.Key);
                ForegroundColourIndex = index;
            }
        }

        public int ForegroundColourIndex
        {
            get
            {
                return foregroundColourIndex;
            }

            set
            {
                if (value != foregroundColourIndex)
                {
                    foregroundColourIndex = value;
                    OnPropertyChanged("ForegroundColourIndex");
                }
            }
        }

        public IEnumerable<string> ForegroundColours
        {
            get
            {
                return colours.Keys;
            }
        }

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

        public bool OverrideBackgroundColour
        {
            get
            {
                return overrideBackgroundColour;
            }

            set
            {
                if (value != overrideBackgroundColour)
                {
                    overrideBackgroundColour = value;
                    OnPropertyChanged("OverrideBackgroundColour");
                }
            }
        }

        public bool OverrideForegroundColour
        {
            get
            {
                return overrideForegroundColour;
            }

            set
            {
                if (value != overrideForegroundColour)
                {
                    overrideForegroundColour = value;
                    OnPropertyChanged("OverrideForegroundColour");
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

        private static Dictionary<string, Color> GetColours()
        {
            Dictionary<string, Color> colours = new Dictionary<string, Color>();
            foreach (PropertyInfo propertyInfo in typeof(Colors).GetProperties())
            {
                colours.Add(propertyInfo.Name, (Color) ColorConverter.ConvertFromString(propertyInfo.Name));
            }

            return colours;
        }

        private void AcceptDialog(object obj)
        {
            window.DialogResult = true;
            window.Close();
        }

        private void CloseColourCheck(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "OverrideForegroundColour":
                case "OverrideBackgroundColour":
                case "BackgroundColourIndex":
                case "ForegroundColourIndex":
                    ColoursClose = !OverrideBackgroundColour || !OverrideForegroundColour
                                       ? false
                                       : Color.AreClose(ForegroundColour, BackgroundColour);
                    break;
            }
        }

        private void RejectDialog(object obj)
        {
            window.DialogResult = false;
            window.Close();
        }

        private bool Validates(object obj)
        {
            return !ColoursClose
                   && Name.Length > 0
                   && Pattern.Length > 0;
        }
    }
}