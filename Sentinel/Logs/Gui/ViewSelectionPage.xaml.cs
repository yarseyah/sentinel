#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Sentinel.Services;
using Sentinel.Views.Interfaces;
using WpfExtras;

#endregion

namespace Sentinel.Logs.Gui
{
    /// <summary>
    /// Interaction logic for ViewSelectionPage.xaml
    /// </summary>
    public partial class ViewSelectionPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private bool horizontal;

        private bool multipleView;

        private int primaryIndex;

        private int secondaryIndex;

        private bool singleView = true;

        private bool vertical = true;

        private bool multipleViewsSupported = false;

        private IEnumerable<IViewInformation> registeredViews;

        public ViewSelectionPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);


            IViewManager vm = ServiceLocator.Instance.Get<IViewManager>();
            if (vm != null)
            {
                registeredViews = new List<IViewInformation>(vm.GetRegistered());
                MultipleViewsSupported = registeredViews.Count() > 1;
                SecondaryIndex = registeredViews.Count() > 1 ? 1 : PrimaryIndex;
            }

            PropertyChanged += PropertyChangedHandler;
        }

        public bool Horizontal
        {
            get
            {
                return horizontal;
            }
            set
            {
                if (horizontal == value) return;
                horizontal = value;
                OnPropertyChanged("Horizontal");
            }
        }

        public bool Vertical
        {
            get
            {
                return vertical;
            }
            set
            {
                if (vertical == value) return;
                vertical = value;
                OnPropertyChanged("Vertical");
            }
        }

        public bool MultipleViewsSupported
        {
            get
            {
                return multipleViewsSupported;
            }
            private set
            {
                if (multipleViewsSupported == value) return;
                multipleViewsSupported = value;
                OnPropertyChanged("MultipleViewsSupported");
            }
        }

        public bool MultipleView
        {
            get
            {
                return multipleView;
            }
            set
            {
                if (multipleView == value) return;
                multipleView = value;
                OnPropertyChanged("MultipleView");
            }
        }

        public bool SingleView
        {
            get
            {
                return singleView;
            }
            set
            {
                if (singleView == value) return;
                singleView = value;
                OnPropertyChanged("SingleView");
            }
        }

        public IEnumerable<IViewInformation> RegisteredViews
        {
            get
            {
                return registeredViews;
            }
            private set
            {
                if (registeredViews == value) return;
                registeredViews = value;
                OnPropertyChanged("RegisteredViews");
            }
        }

        public int PrimaryIndex
        {
            get
            {
                return primaryIndex;
            }
            set
            {
                if (primaryIndex == value) return;
                primaryIndex = value;
                OnPropertyChanged("PrimaryIndex");
            }
        }

        public int SecondaryIndex
        {
            get
            {
                return secondaryIndex;
            }
            set
            {
                if (secondaryIndex == value) return;
                secondaryIndex = value;
                OnPropertyChanged("SecondaryIndex");
            }
        }

        #region Implementation of IWizardPage

        public string Title
        {
            get
            {
                return "Visualising the Log";
            }
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
            }
        }

        public string Description
        {
            get
            {
                return "Select the desired views to visualise the logger and its providers.";
            }
        }

        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        public Control PageContent
        {
            get
            {
                return this;
            }
        }

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged("Children");
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged("Children");
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting a non-null instance of a class to save settings into");
            Debug.Assert(saveData is NewLoggerSettings, "Expecting save data structure to be a NewLoggerSettings");

            NewLoggerSettings settings = saveData as NewLoggerSettings;
            if (settings != null)
            {
                settings.Views.Clear();
                settings.Views.Add(registeredViews.ElementAt(PrimaryIndex).Identifier);
                if (MultipleView)
                {
                    settings.Views.Add(registeredViews.ElementAt(SecondaryIndex).Identifier);
                    settings.IsVertical = Vertical;
                }
            }

            return saveData;
        }

        #endregion

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Horizontal")
            {
                Vertical = !Horizontal;
            }
            else if (e.PropertyName == "Vertical")
            {
                Horizontal = !Vertical;
            }
            else if (e.PropertyName == "SingleView")
            {
                MultipleView = !SingleView;
            }
            else if (e.PropertyName == "MultipleView")
            {
                SingleView = !MultipleView;
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}