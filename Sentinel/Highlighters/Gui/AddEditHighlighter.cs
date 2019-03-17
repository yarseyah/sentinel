namespace Sentinel.Highlighters.Gui
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    using WpfExtras;

    public class AddEditHighlighter : ViewModelBase
    {
        private readonly Window window;

        private readonly Dictionary<string, Color> colours = GetColours();

        private int backgroundColourIndex = 1;

        private bool coloursAreClose;

        private int foregroundColourIndex;

        private bool overrideBackgroundColour = false;

        private bool overrideForegroundColour = false;

        private string name = "Untitled";

        private string pattern = "pattern";

        private LogEntryFields field;

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
                var find = colours.FirstOrDefault(r => r.Value == value);
                find.Key.ThrowIfNull(nameof(find.Key));

                var index = colours.Keys.OrderBy(n => n).ToList().IndexOf(find.Key);
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
                    OnPropertyChanged(nameof(BackgroundColourIndex));
                }
            }
        }

        public IEnumerable<string> BackgroundColours => colours.Keys;

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
                    OnPropertyChanged(nameof(ColoursClose));
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
                var find = colours.FirstOrDefault(r => r.Value == value);
                find.Key.ThrowIfNull(nameof(find.Key));

                var index = colours.Keys.OrderBy(n => n).ToList().IndexOf(find.Key);
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
                    OnPropertyChanged(nameof(ForegroundColourIndex));
                }
            }
        }

        public IEnumerable<string> ForegroundColours => colours.Keys;

        public LogEntryFields Field
        {
            get
            {
                return field;
            }

            set
            {
                field = value;
                OnPropertyChanged(nameof(Field));
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
                OnPropertyChanged(nameof(Mode));
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
                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(OverrideBackgroundColour));
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
                    OnPropertyChanged(nameof(OverrideForegroundColour));
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
                    OnPropertyChanged(nameof(Pattern));
                }
            }
        }

        public ICommand Reject { get; private set; }

        private static Dictionary<string, Color> GetColours()
        {
            var colours = new Dictionary<string, Color>();
            foreach (var propertyInfo in typeof(Colors).GetProperties())
            {
                var colour = ColorConverter.ConvertFromString(propertyInfo.Name);
                if (colour != null)
                {
                    colours.Add(propertyInfo.Name, (Color)colour);
                }
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
                    ColoursClose = OverrideBackgroundColour && OverrideForegroundColour && Color.AreClose(ForegroundColour, BackgroundColour);
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