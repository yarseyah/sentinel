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

    using WpfExtras;

    [DataContract]
    public class HighlightingService<T> : ViewModelBase, IHighlightingService<T>, IDefaultInitialisation
        where T : class, IHighlighter
    {
        private readonly CollectionChangeHelper<T> collectionHelper = new CollectionChangeHelper<T>();

        private int selectedIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingService{T}"/> class.
        /// </summary>
        public HighlightingService()
        {
            Add = new DelegateCommand(AddHighlighter);
            Edit = new DelegateCommand(EditHighlighter, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveHighlighter, e => selectedIndex != -1);
            OrderEarlier = new DelegateCommand(MoveItemUp, e => selectedIndex > 0);
            OrderLater = new DelegateCommand(
                MoveItemDown,
                e => selectedIndex < (Highlighters.Count - 1) && selectedIndex != -1);

            Highlighters = new ObservableCollection<T>();

            // Register self as an observer of the collection.
            collectionHelper.ManagerName = "Highlighters";
            collectionHelper.OnPropertyChanged += CustomHighlighterPropertyChanged;
            collectionHelper.NameLookup += e => e.Name;
            Highlighters.CollectionChanged += collectionHelper.AttachDetach;
        }

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
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public IHighlighterStyle IsHighlighted(ILogEntry logEntry)
        {
            return Highlighters.Where(h => h.IsMatch(logEntry)).Select(h => h.Style).FirstOrDefault();
        }

        public void Initialise()
        {
            Debug.Assert(!Highlighters.Any(), "Should not have any contents at initialisation");

            Highlighters.Add(
                new StandardHighlighter(
                    "Trace",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "TRACE",
                    new HighlighterStyle { Background = Colors.LightGray }) as T);
            Highlighters.Add(
                new StandardHighlighter(
                    "Debug",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "DEBUG",
                    new HighlighterStyle { Background = Colors.LightGreen }) as T);
            Highlighters.Add(
                new StandardHighlighter(
                    "Info",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "INFO",
                    new HighlighterStyle { Foreground = Colors.White, Background = Colors.Blue }) as T);
            Highlighters.Add(
                new StandardHighlighter(
                    "Warn",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "WARN",
                    new HighlighterStyle { Background = Colors.Yellow }) as T);
            Highlighters.Add(
                new StandardHighlighter(
                    "Error",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "ERROR",
                    new HighlighterStyle { Foreground = Colors.White, Background = Colors.Red }) as T);
            Highlighters.Add(
                new StandardHighlighter(
                    "Fatal",
                    true,
                    LogEntryFields.Type,
                    MatchMode.Exact,
                    "FATAL",
                    new HighlighterStyle { Foreground = Colors.Yellow, Background = Colors.Black }) as T);
        }

        private void AddHighlighter(object obj)
        {
            IAddHighlighterService addHighlighterService = new AddNewHighlighterService();
            addHighlighterService.Add();
        }

        private void CustomHighlighterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var highlighter = sender as IHighlighter;
            if (highlighter != null)
            {
                Trace.WriteLine(
                    $"HighlightingService saw some activity on {highlighter.Name} (IsEnabled = {highlighter.Enabled})");
            }

            OnPropertyChanged(string.Empty);
        }

        private void EditHighlighter(object obj)
        {
            IEditHighlighterService editService = new EditHighlighterService();
            var highlighter = Highlighters.ElementAt(SelectedIndex);
            if (highlighter != null)
            {
                editService.Edit(highlighter);
            }
        }

        private void MoveItemDown(object obj)
        {
            if (selectedIndex == -1)
            {
                return;
            }

            lock (this)
            {
                Debug.Assert(selectedIndex >= 0, "SelectedIndex must be valid, not -1, for moving.");
                Debug.Assert(
                    selectedIndex < (Highlighters.Count - 1),
                    "SelectedIndex must be a value less than the max index of the collection.");
                Debug.Assert(
                    Highlighters.Count > 1,
                    "Can only move an item if there are more than one items in the collection.");

                var oldIndex = selectedIndex;
                SelectedIndex = -1;
                lock (Highlighters)
                {
                    Highlighters.Swap(oldIndex, oldIndex + 1);
                }

                SelectedIndex = oldIndex + 1;
            }
        }

        private void MoveItemUp(object obj)
        {
            if (selectedIndex == -1)
            {
                return;
            }

            lock (this)
            {
                Debug.Assert(selectedIndex >= 0, "SelectedIndex must be valid, e.g not -1.");
                Debug.Assert(
                    Highlighters.Count > 1, "Can not move item unless more than one item in the collection.");

                var oldIndex = selectedIndex;
                SelectedIndex = -1;
                lock (Highlighters)
                {
                    Highlighters.Swap(oldIndex, oldIndex - 1);
                }

                SelectedIndex = oldIndex - 1;
            }
        }

        private void RemoveHighlighter(object obj)
        {
            IRemoveHighlighterService service = new RemoveHighlighterService();
            var highlighter = Highlighters.ElementAt(SelectedIndex);

            Debug.Assert(highlighter != null, "Should not be able to run this if no highlighter is selected!");

            service.Remove(highlighter);
        }
    }
}