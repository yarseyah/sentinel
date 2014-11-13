#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Sentinel.Classification.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;
using System.Runtime.Serialization;
using System.ComponentModel;
using Sentinel.Classification.Gui;

#endregion

namespace Sentinel.Classification
{
    /// <summary>
    /// View Model for classifier collection.  This has been written to operate
    /// as a ServiceLocator provided resource, so there is only one collection
    /// across the whole of the system.
    /// </summary>
    //[Export(typeof(IClassifierService))]
    [DataContract]
    public class ClassifyingService<T> : ViewModelBase, IClassifyingService<T>, IDefaultInitialisation
        where T : class, IClassifier
    {
        private readonly CollectionChangeHelper<T> collectionHelper = new CollectionChangeHelper<T>();

        private readonly IAddClassifyingService addClassifyingService = new AddClassifier();

        private readonly IEditClassifyingService editClassifyingService = new EditClassifier();

        private readonly IRemoveClassifyingService removeClassifyingService = new RemoveClassifier();

        private string displayName = "ClassifyingService";

        private int selectedIndex = -1;

        /// <summary>
        /// Initializes a new instance of the Classifiers class.
        /// </summary>
        public ClassifyingService()
        {
            Add = new DelegateCommand(AddClassifier);
            Edit = new DelegateCommand(EditClassifier, e => SelectedIndex != -1);
            Remove = new DelegateCommand(RemoveClassifier, e => SelectedIndex != -1);
            OrderEarlier = new DelegateCommand(MoveItemUp, e => SelectedIndex > 0);
            OrderLater = new DelegateCommand(
                MoveItemDown,
                e => SelectedIndex < Classifiers.Count - 1 && SelectedIndex != -1);

            Classifiers = new ObservableCollection<T>();

            // Register self as an observer of the collection.
            collectionHelper.ManagerName = "Classifiers";
            collectionHelper.OnPropertyChanged += CustomClassifierPropertyChanged;
            collectionHelper.NameLookup += e => e.Name;
            Classifiers.CollectionChanged += collectionHelper.AttachDetach;
        }

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

        private void CustomClassifierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IClassifier)
            {
                var filter = sender as IClassifier;
                Trace.WriteLine(
                    string.Format(
                        "ClassifyingService saw some activity on {0} (IsEnabled = {1})", filter.Name, filter.Enabled));
            }

            OnPropertyChanged(string.Empty);
        }

        #region IClassifierService Members

        /// <summary>
        /// Gets the <c>ICommand</c> providing the add-classifier functionality.
        /// </summary>
        public ICommand Add { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the edit-classifier functionality.
        /// </summary>
        public ICommand Edit { get; private set; }

        /// <summary>
        /// Gets the <c>ObservableCollection</c> of items representing the
        /// collection of Classifiers.
        /// </summary>
        public ObservableCollection<T> Classifiers { get; set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality to 
        /// move the currently selected element to an earlier position
        /// in the ordered list of classifiers.
        /// </summary>
        public ICommand OrderEarlier { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality to 
        /// move the currently selected element to a later position
        /// in the ordered list of classifiers.
        /// </summary>
        public ICommand OrderLater { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> providing the functionality to
        /// remove the selected element from the list of classifiers.
        /// </summary>
        public ICommand Remove { get; private set; }

        /// <summary>
        /// Gets or sets the index for the selected item in the list
        /// of classifiers.  The selected index is used for commands
        /// such as <seealso cref="OrderEarlier">OrderEarlier</seealso>,
        /// <seealso cref="OrderLater">OrderLater</seealso>
        /// and <seealso cref="Remove">Remove</seealso>.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ILogEntry Classify(ILogEntry entry)
        {
            return Classifiers
                .Where(classifier => classifier.Enabled)
                .Aggregate(entry, (current, classifier) => classifier.Classify(current));
        }

        #endregion

        /// <summary>
        /// Add a new classifier to the collection of classifiers.
        /// <para>
        /// Private method that implements the <c>ICommand</c> <seealso cref="Add">
        /// Add</seealso> through a <c>DelegateCommand</c>.
        /// </para>
        /// </summary>
        /// <param name="obj">Delegate object data - unused.</param>
        private void AddClassifier(object obj)
        {
            addClassifyingService.Add();
        }

        /// <summary>
        /// Edit the currently selected classifier (defined by the
        /// <seealso cref="SelectedIndex">SelectedIndex</seealso> 
        /// property).
        /// <para>
        /// Private method that implements the <c>ICommand</c> <seealso cref="Edit">
        /// Edit</seealso> through a <c>DelegateCommand</c>.
        /// </para>
        /// </summary>
        /// <param name="obj">Delegate object data - unused.</param>
        private void EditClassifier(object obj)
        {
            var classifier = Classifiers.ElementAt(SelectedIndex);
            if (classifier != null)
            {
                editClassifyingService.Edit(classifier);
            }
        }

        /// <summary>
        /// Move the currently selected classifier (defined by the
        /// <seealso cref="SelectedIndex">SelectedIndex</seealso> 
        /// property) to later in the ordered list.
        /// <para>
        /// Private method that implements the <c>ICommand</c> <seealso cref="OrderLater">
        /// OrderLater</seealso> through a <c>DelegateCommand</c>.
        /// </para>
        /// </summary>
        /// <param name="obj">Delegate object data - unused.</param>
        private void MoveItemDown(object obj)
        {
            if (selectedIndex != -1)
            {
                lock (this)
                {
                    Debug.Assert(
                        selectedIndex >= 0,
                        "SelectedIndex must be >= 0.");
                    Debug.Assert(
                        selectedIndex < Classifiers.Count - 1,
                        "SelectedIndex must be within the index range of Items collection");
                    Debug.Assert(
                        Classifiers.Count > 1,
                        "Can not move an item unless there is more than one.");

                    lock (Classifiers)
                    {
                        Classifiers.Swap(selectedIndex, selectedIndex + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Move the currently selected classifier (defined by the
        /// <seealso cref="SelectedIndex">SelectedIndex</seealso> 
        /// property) to earlier in the ordered list.
        /// <para>
        /// Private method that implements the <c>ICommand</c> <seealso cref="OrderEarlier">
        /// OrderEarlier</seealso> through a <c>DelegateCommand</c>.
        /// </para>
        /// </summary>
        /// <param name="obj">Delegate object data - unused.</param>
        private void MoveItemUp(object obj)
        {
            if (selectedIndex != -1)
            {
                lock (this)
                {
                    Debug.Assert(selectedIndex >= 0, "SelectedIndex must be valid, e.g. >= 0");
                    Debug.Assert(Classifiers.Count > 1, "Can only move item if more than one.");

                    lock (Classifiers)
                    {
                        Classifiers.Swap(selectedIndex, selectedIndex - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the currently selected classifier (defined by the
        /// <seealso cref="SelectedIndex">SelectedIndex</seealso> 
        /// property).
        /// <para>
        /// Private method that implements the <c>ICommand</c> <seealso cref="Remove">
        /// Remove</seealso> through a <c>DelegateCommand</c>.
        /// </para>
        /// </summary>
        /// <param name="obj">Delegate object data - unused.</param>
        private void RemoveClassifier(object obj)
        {
            var classifier = Classifiers.ElementAt(SelectedIndex);
            removeClassifyingService.Remove(classifier);
        }

        public void Initialise()
        {
            Classifiers.Add(new Classifier("Timing messages", true, LogEntryField.Description, MatchMode.RegularExpression, @"^\[SimulationTime\] (?<description>[^$]+)$", "TIMING") as T);
            Classifiers.Add(new Classifier("Smp messages", true, LogEntryField.Description, MatchMode.RegularExpression, "Src:'(?<system>[^']+)', Msg:'(?<description>.*)'$", "TIMING") as T);           
            Classifiers.Add(new Classifier("SimSat messages", true, LogEntryField.Description, MatchMode.RegularExpression, "SIMSAT:'(?<system>[^']+)', Msg:'(?<description>.*)'$", "TIMING") as T);

        }
    }
}