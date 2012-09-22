namespace Sentinel.FileMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    ///   Interaction logic for MessageFormatPage.xaml
    /// </summary>
    public partial class MessageFormatPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;
        private int selectedDecoderIndex;

        private IWizardPage customPage = null;

        public MessageFormatPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            PropertyChanged += PropertyChangedHandler;

            DecodingStyles = new List<string>
                                 {
                                     "nLog default message format decoder",
                                     "Custom"
                                 };
        }

        public IEnumerable<string> DecodingStyles
        {
            get;
            private set;
        }

        public int SelectedDecoderIndex
        {
            get
            {
                return selectedDecoderIndex;
            }
            set
            {
                if (selectedDecoderIndex == value) return;
                selectedDecoderIndex = value;
                OnPropertyChanged("SelectedDecoderIndex");
            }
        }

        private bool showCustomWarning = false;

        public bool ShowCustomWarning
        {
            get
            {
                return showCustomWarning;
            }

            private set
            {
                if (showCustomWarning != value)
                {
                    showCustomWarning = value;
                    OnPropertyChanged("ShowCustomWarning");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public string Title
        {
            get
            {
                return "Message Part Identification";
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
                return "Define how the entries in the log file are categorised.";
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
            Debug.Assert(saveData != null, "Expecting a valid save-data instance");
            Debug.Assert(saveData is IFileMonitoringProviderSettings, "Should be an IFileMonitoringProviderSettings");

            IFileMonitoringProviderSettings settings = saveData as IFileMonitoringProviderSettings;

            if ( settings != null )
            {
                if (!IsCustom)
                {
                    settings.MessageDecoder = GetDecoder();
                }
            }

            return saveData;
        }

        protected bool IsCustom
        {
            get
            {
                return SelectedDecoderIndex == DecodingStyles.Count() - 1;
            }
        }

        private string GetDecoder()
        {
            switch ( SelectedDecoderIndex )
            {
                case 0:
                    return "^(?<DateTime>[^|]+)\\|(?<Type>[^|]+)\\|(?<Logger>[^|]+)\\|(?<Description>[^$]*)$";
                default:
                    throw new NotSupportedException("Custom message formats are not handled on this page.");
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedDecoderIndex")
            {
                if (IsCustom)
                {
                    if (customPage == null)
                    {
                        customPage = new CustomMessageDecoderPage();
                    }

                    if (!Children.Contains(customPage))
                    {
                        AddChild(customPage);
                    }

                    ShowCustomWarning = true;
                }
                else
                {
                    ShowCustomWarning = false;

                    if (Children.Any())
                    {
                        children.Clear();
                        OnPropertyChanged("Children");
                    }
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Trigger the validation on these fields (work around a WPF 3.x issue).
            OnPropertyChanged("SelectedDecoderIndex");
        }
    }
}