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
using Sentinel.Highlighters.Gui;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Highlighters
{
    [Serializable]
    public class HighlightingService
        : ViewModelBase
        , IHighlightingService
        , IXmlSerializable
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

            Highlighters = new ObservableCollection<IHighlighter>();

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
        public ObservableCollection<IHighlighter> Highlighters { get; set; }

        public ICommand OrderEarlier { get; private set; }

        public ICommand OrderLater { get; private set; }

        public ICommand Remove { get; private set; }

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
            foreach (IHighlighter highlighter in Highlighters)
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
            IAddHighlighterService addHighlighterService = new AddNewHighlighterService();
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
            IEditHighlighterService editService = new EditHighlighterService();
            if (editService != null)
            {
                IHighlighter highlighter = Highlighters.ElementAt(SelectedIndex);
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
            IRemoveHighlighterService service = new RemoveHighlighterService();
            if (service != null)
            {
                IHighlighter highlighter = Highlighters.ElementAt(SelectedIndex);

                Debug.Assert(
                    highlighter != null,
                    "Should not be able to run this if no highlighter is selected!");

                service.Remove(highlighter);
            }
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
            reader.ReadStartElement("HighlightingService");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(int));
            int count = (int) xmlSerializer.Deserialize(reader);
            for (int i = 0; i < count; i++)
            {
                xmlSerializer = new XmlSerializer(typeof(Highlighter));
                IHighlighter highlighter = (IHighlighter)xmlSerializer.Deserialize(reader);
                Highlighters.Add(highlighter);
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
            xmlSerializer.Serialize(writer, Highlighters.Count, ns);
            foreach (IHighlighter highlighter in Highlighters)
            {
                xmlSerializer = new XmlSerializer(typeof(Highlighter));
                xmlSerializer.Serialize(writer, highlighter, ns);
            }
        }

        #endregion
    }
}