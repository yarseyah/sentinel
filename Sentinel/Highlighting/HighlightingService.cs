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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using Sentinel.Logger;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.Highlighting
{

    #region Using directives

    #endregion

    [Serializable]
    public class HighlightingService : ViewModelBase, IHighlightingService
    {
        private readonly CollectionChangeHelper<Highlighter> collectionHelper =
            new CollectionChangeHelper<Highlighter>();

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
                MoveItemDown,
                e => selectedIndex < Highlighters.Count - 1 && selectedIndex != -1);

            Highlighters = new ObservableCollection<Highlighter>();

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
        [XmlIgnore]
        public ICommand Add { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality for editing a highlighter.
        /// </summary>
        [XmlIgnore]
        public ICommand Edit { get; private set; }

        /// <summary>
        /// Gets or sets the observable collection of highlighters.
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Highlighter", typeof(Highlighter))]
        public ObservableCollection<Highlighter> Highlighters { get; set; }

        [XmlIgnore]
        public ICommand OrderEarlier { get; private set; }

        [XmlIgnore]
        public ICommand OrderLater { get; private set; }

        [XmlIgnore]
        public ICommand Remove { get; private set; }

        [XmlIgnore]
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

        public HighlighterStyle IsHighlighted(ILogEntry logEntry)
        {
            foreach (Highlighter highlighter in Highlighters)
            {
                if (highlighter.IsMatch(logEntry))
                {
                    return highlighter.Style;
                }
            }

            return null;
        }

        #endregion

        private void AddHighlighter(object obj)
        {
            IAddHighlighterService addHighlighterService =
                ServiceLocator.Instance.Get<IAddHighlighterService>();
            if (addHighlighterService != null)
            {
                addHighlighterService.Add();
            }
        }

        private void CustomHighlighterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Highlighter)
            {
                Highlighter filter = sender as Highlighter;
                Trace.WriteLine(
                    string.Format(
                        "FilterServer saw some activity on {0} (IsEnabled = {1})",
                        filter.Name,
                        filter.Enabled));
            }

            OnPropertyChanged(string.Empty);
        }

        private void EditHighligter(object obj)
        {
            IEditHighlighterService editService =
                ServiceLocator.Instance.Get<IEditHighlighterService>();

            if (editService != null)
            {
                Highlighter highlighter = Highlighters.ElementAt(SelectedIndex);
                if (highlighter != null)
                {
                    editService.Edit(highlighter);
                }
            }
        }

        private void MoveItemDown(object obj)
        {
            if (selectedIndex != -1)
            {
                lock (this)
                {
                    Debug.Assert(
                        selectedIndex >= 0,
                        "SelectedIndex must be valid, not -1, for moving.");
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
                    Debug.Assert(
                        selectedIndex >= 0,
                        "SelectedIndex must be valid, e.g not -1.");
                    Debug.Assert(
                        Highlighters.Count > 1,
                        "Can not move item unless more than one item in the collection.");

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
            IRemoveHighlighterService service = ServiceLocator.Instance.Get<IRemoveHighlighterService>();

            if (service != null)
            {
                Highlighter highlighter = Highlighters.ElementAt(SelectedIndex);

                Debug.Assert(
                    highlighter != null,
                    "Should not be able to run this if no highlighter is selected!");

                service.Remove(highlighter);
            }
        }
    }
}