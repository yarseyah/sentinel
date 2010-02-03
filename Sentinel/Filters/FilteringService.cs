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

namespace Sentinel.Filters
{

    #region Using directives

    #endregion

    [Serializable]
    public class FilteringService
        : ViewModelBase, IFilteringService
    {
        private readonly CollectionChangeHelper<Filter> collectionHelper =
            new CollectionChangeHelper<Filter>();

        private string displayName = "FilteringService";

        private int selectedIndex = -1;

        public FilteringService()
        {
            Add = new DelegateCommand(AddFilter);
            Edit = new DelegateCommand(EditFilter, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveFilter, e => selectedIndex != -1);

            Filters = new ObservableCollection<Filter>();

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
        public ObservableCollection<Filter> Filters { get; set; }

        public bool IsFiltered(ILogEntry entry)
        {
            // If a system check finds a match, it avoids checking the type
            // as it could cause a conflict.
            bool overrideTypeCheck = false;
            bool isFiltered = false;

            // Start with the systems
            foreach (Filter filter in Filters.Where(f => f.Field == LogEntryField.System))
            {
                if (filter.IsMatch(entry))
                {
                    isFiltered = filter.Enabled;
                    overrideTypeCheck = true;
                    break;
                }
            }

            if (!overrideTypeCheck)
            {
                foreach (Filter filter in Filters.Where(f => f.Field == LogEntryField.Type))
                {
                    if (filter.IsMatch(entry) && filter.Enabled)
                    {
                        isFiltered = true;
                        break;
                    }
                }
            }

            return isFiltered;
        }

        #endregion

        private static void AddFilter(object obj)
        {
            IAddFilterService addService = ServiceLocator.Instance.Get<IAddFilterService>();
            if (addService != null)
            {
                addService.Add();
            }
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
            IEditFilterService editService = ServiceLocator.Instance.Get<IEditFilterService>();

            if (editService != null)
            {
                Filter filter = Filters.ElementAt(SelectedIndex);
                if (filter != null)
                {
                    editService.Edit(filter);
                }
            }
        }

        private void RemoveFilter(object obj)
        {
            IRemoveFilterService removeFilter = ServiceLocator.Instance.Get<IRemoveFilterService>();
            if (removeFilter != null)
            {
                Filter filter = Filters.ElementAt(SelectedIndex);
                removeFilter.Remove(filter);
            }
        }
    }
}