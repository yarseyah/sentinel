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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sentinel.Filters.Gui;
using Sentinel.Filters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Filters
{
    [Serializable]
    public class FilteringService
        : ViewModelBase
        , IFilteringService
        , IXmlSerializable
        , IDefaultInitialisation
    {
        private readonly CollectionChangeHelper<IFilter> collectionHelper =
            new CollectionChangeHelper<IFilter>();

        private IAddFilterService addFilterService = new AddFilter();

        private IEditFilterService editFilterService = new EditFilter();

        private IRemoveFilterService removeFilterService = new RemoveFilter();

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

        #region Implementation of IXmlSerializable

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable 
        /// interface, you should return null (Nothing in Visual Basic) from this method, and 
        /// instead, if specifying a custom schema is required, apply the 
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation 
        /// of the object that is produced by the 
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 
        /// method and consumed by the 
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the 
        /// object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("FilteringService");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(int));
            int count = (int)xmlSerializer.Deserialize(reader);
            for (int i = 0; i < count; i++)
            {
                xmlSerializer = new XmlSerializer(typeof(Filter));
                IFilter filter = (IFilter)xmlSerializer.Deserialize(reader);
                Filters.Add(filter);
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which 
        /// the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(int));
            xmlSerializer.Serialize(writer, Filters.Count, ns);
            foreach (IFilter filter in Filters)
            {
                xmlSerializer = new XmlSerializer(typeof(Filter));
                xmlSerializer.Serialize(writer, filter, ns);
            }
        }

        #endregion

        public void Initialise()
        {
            // Add the defaulted filters
            Filters.Add(new Filter("Trace", LogEntryField.Type, "TRACE"));
            Filters.Add(new Filter("Debug", LogEntryField.Type, "DEBUG"));
            Filters.Add(new Filter("Information", LogEntryField.Type, "INFO"));
            Filters.Add(new Filter("Warning", LogEntryField.Type, "WARN"));
            Filters.Add(new Filter("Error", LogEntryField.Type, "ERROR"));
            Filters.Add(new Filter("Fatal", LogEntryField.Type, "FATAL"));
        }
    }

    public interface IDefaultInitialisation
    {
        void Initialise();
    }
}