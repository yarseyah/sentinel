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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using Sentinel.Filters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Filters
{
    [Serializable]
    [Export(typeof(IFilteringService))]
    public class FilteringService
        : ViewModelBase
        , IFilteringService
    {
        private readonly CollectionChangeHelper<IFilter> collectionHelper =
            new CollectionChangeHelper<IFilter>();

        [Import(typeof(IAddFilterService))]
        private IAddFilterService addFilterService;

        [Import(typeof(IEditFilterService))]
        private IEditFilterService editFilterService;

        [Import(typeof(IRemoveFilterService))]
        private IRemoveFilterService removeFilterService;

        private string displayName = "FilteringService";

        private int selectedIndex = -1;

        public FilteringService()
        {
            Add = new DelegateCommand(AddFilter);
            Edit = new DelegateCommand(EditFilter, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveFilter, e => selectedIndex != -1);

            Filters = new ObservableCollection<IFilter>();

            // Register self as an observer of the collection.
            collectionHelper.OnPropertyChanged += CustomFilterPropertyChanged;
            collectionHelper.ManagerName = "FilteringService";
            collectionHelper.NameLookup += e => e.Name;
            Filters.CollectionChanged += collectionHelper.AttachDetach;
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

        [XmlIgnore]
        public ICommand Add { get; private set; }

        [XmlIgnore]
        public ICommand Edit { get; private set; }

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
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        #region IFilteringService Members

        [XmlArray]
        [XmlArrayItem("Filter", typeof(Filter))]
        public ObservableCollection<IFilter> Filters { get; set; }

        public bool IsFiltered(LogEntry entry)
        {
            return (Filters.Any(filter => filter.Enabled && filter.IsMatch(entry)));
        }

        #endregion

        private void AddFilter(object obj)
        {
            addFilterService.Add();
        }

        private void CustomFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Filter)
            {
                Filter filter = sender as Filter;
                Trace.WriteLine(
                    string.Format(
                        "FilterServer saw some activity on {0} (IsEnabled = {1})",
                        filter.Name,
                        filter.Enabled));
            }

            OnPropertyChanged(string.Empty);
        }

        private void EditFilter(object obj)
        {
            IFilter filter = Filters.ElementAt(SelectedIndex);
            if (filter != null)
            {
                editFilterService.Edit(filter);
            }
        }

        private void RemoveFilter(object obj)
        {
            IFilter filter = Filters.ElementAt(SelectedIndex);
            removeFilterService.Remove(filter);
        }
    }
}