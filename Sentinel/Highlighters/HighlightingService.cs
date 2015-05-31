#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Highlighters
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Input;
    using System.Windows.Media;

    using Sentinel.Highlighters.Gui;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Support.Mvvm;

    [DataContract]
    public class HighlightingService<T> : ViewModelBase, IHighlightingService<T>, IDefaultInitialisation
        where T : class, IHighlighter
    {
        private readonly CollectionChangeHelper<T> collectionHelper = new CollectionChangeHelper<T>();

        private string displayName = "HighlightingService";

        private int selectedIndex = -1;

        /// <summary>
        /// Initializes a new instance of the HighlightingService class.
        /// </summary>
        public HighlightingService()
        {
            Add = new DelegateCommand(AddHighlighter);
            Edit = new DelegateCommand(EditHighligter, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveHighlighter, e => selectedIndex != -1);
            OrderEarlier = new DelegateCommand(MoveItemUp, e => selectedIndex > 0);
            OrderLater = new DelegateCommand(
                MoveItemDown, e => selectedIndex < Highlighters.Count - 1 && selectedIndex != -1);

            Highlighters = new ObservableCollection<T>();

            // Register self as an observer of the collection.
            collectionHelper.ManagerName = "Highlighers";
            collectionHelper.OnPropertyChanged += CustomHighlighterPropertyChanged;
            collectionHelper.NameLookup += e => e.Name;
            Highlighters.CollectionChanged += collectionHelper.AttachDetach;
        }

        /// <summary>
        /// Gets or sets the display name for the view-model.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return displayName;
            }

            set
            {
                displayName = value;
            }
        }

        #region IHighlightingService Members

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality for the adding of a new highlighter.
        /// </summary>
        public ICommand Add { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality for editing a highlighter.
        /// </summary>
        public ICommand Edit { get; private set; }

        /// <summary>
        /// Gets or sets the observable collection of highlighters.
        /// </summary>
        [DataMember]
        public ObservableCollection<T> Highlighters { get; set; }

        public ICommand OrderEarlier { get; private set; }

        public ICommand OrderLater { get; private set; }

        public ICommand Remove { get; private set; }

        [DataMember]
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        public IHighlighterStyle IsHighlighted(LogEntry logEntry)
        {
            return Highlighters.Where(h => h.IsMatch(logEntry)).Select(h => h.Style).FirstOrDefault();
        }

        #endregion

        public void Initialise()
        {
            Debug.Assert(!Highlighters.Any(), "Should not have any contents at initialisation");            

            Highlighters.Add(new StandardHighlighter("Trace", true, LogEntryField.Type, MatchMode.Exact, "TRACE", new HighlighterStyle { Background = Colors.LightGray }) as T);
            Highlighters.Add(new StandardHighlighter("Debug", true, LogEntryField.Type,MatchMode.Exact,"DEBUG",new HighlighterStyle { Background = Colors.LightGreen }) as T);
            Highlighters.Add(new StandardHighlighter("Info", true, LogEntryField.Type, MatchMode.Exact, "INFO", new HighlighterStyle { Foreground = Colors.White, Background = Colors.Blue }) as T);
            Highlighters.Add(new StandardHighlighter("Warn", true, LogEntryField.Type, MatchMode.Exact, "WARN", new HighlighterStyle { Background = Colors.Yellow }) as T);
            Highlighters.Add(new StandardHighlighter("Error", true, LogEntryField.Type, MatchMode.Exact, "ERROR", new HighlighterStyle { Foreground = Colors.White, Background = Colors.Red }) as T);
            Highlighters.Add(new StandardHighlighter("Fatal", true, LogEntryField.Type, MatchMode.Exact, "FATAL", new HighlighterStyle { Foreground = Colors.Yellow, Background = Colors.Black }) as T);
        }

        private void AddHighlighter(object obj)
        {
            IAddHighlighterService addHighlighterService = new AddNewHighlighterService();
            addHighlighterService.Add();
        }

        private void CustomHighlighterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IHighlighter)
            {
                var filter = sender as IHighlighter;
                Trace.WriteLine(
                    string.Format(
                        "HighlightingService saw some activity on {0} (IsEnabled = {1})", filter.Name, filter.Enabled));
            }

            OnPropertyChanged(string.Empty);
        }

        private void EditHighligter(object obj)
        {
            IEditHighlighterService editService = new EditHighlighterService();
            var highlighter = this.Highlighters.ElementAt(this.SelectedIndex);
            if (highlighter != null)
            {
                editService.Edit(highlighter);
            }
        }

        private void MoveItemDown(object obj)
        {
            if (selectedIndex != -1)
            {
                lock (this)
                {
                    Debug.Assert(selectedIndex >= 0, "SelectedIndex must be valid, not -1, for moving.");
                    Debug.Assert(
                        selectedIndex < Highlighters.Count - 1,
                        "SelectedIndex must be a value less than the max index of the collection.");
                    Debug.Assert(
                        Highlighters.Count > 1,
                        "Can only move an item if there are more than one items in the collection.");

                    int oldIndex = selectedIndex;
                    SelectedIndex = -1;
                    lock (Highlighters)
                    {
                        Highlighters.Swap(oldIndex, oldIndex + 1);
                    }

                    SelectedIndex = oldIndex + 1;
                }
            }
        }

        private void MoveItemUp(object obj)
        {
            if (selectedIndex != -1)
            {
                lock (this)
                {
                    Debug.Assert(selectedIndex >= 0, "SelectedIndex must be valid, e.g not -1.");
                    Debug.Assert(
                        Highlighters.Count > 1, "Can not move item unless more than one item in the collection.");

                    int oldIndex = selectedIndex;
                    SelectedIndex = -1;
                    lock (Highlighters)
                    {
                        Highlighters.Swap(oldIndex, oldIndex - 1);
                    }

                    SelectedIndex = oldIndex - 1;
                }
            }
        }

        private void RemoveHighlighter(object obj)
        {
            IRemoveHighlighterService service = new RemoveHighlighterService();
            var highlighter = this.Highlighters.ElementAt(this.SelectedIndex);

            Debug.Assert(highlighter != null, "Should not be able to run this if no highlighter is selected!");

            service.Remove(highlighter);
        }
    }
}