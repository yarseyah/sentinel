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
    ///   Interaction logic for MessageFormatPage.xaml.
    /// </summary>
    public partial class MessageFormatPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private bool showCustomWarning = false;

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
                "Custom",
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> DecodingStyles { get; private set; }

        public int SelectedDecoderIndex
        {
            get => selectedDecoderIndex;
            set
            {
                if (selectedDecoderIndex != value)
                {
                    selectedDecoderIndex = value;
                    OnPropertyChanged(nameof(SelectedDecoderIndex));
                }
            }
        }

        public bool ShowCustomWarning
        {
            get => showCustomWarning;

            private set
            {
                if (showCustomWarning != value)
                {
                    showCustomWarning = value;
                    OnPropertyChanged(nameof(ShowCustomWarning));
                }
            }
        }

        public string Title => "Message Part Identification";

        public ReadOnlyObservableCollection<IWizardPage> Children => readonlyChildren;

        public string Description => "Define how the entries in the log file are categorised.";

        public bool IsValid => true;

        public Control PageContent => this;

        protected bool IsCustom => SelectedDecoderIndex == DecodingStyles.Count() - 1;

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged(nameof(Children));
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged(nameof(Children));
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting a valid save-data instance");
            Debug.Assert(saveData is IFileMonitoringProviderSettings, "Should be an IFileMonitoringProviderSettings");

            if (saveData is IFileMonitoringProviderSettings settings)
            {
                if (!IsCustom)
                {
                    settings.MessageDecoder = GetDecoder();
                }
            }

            return saveData;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            var e = new PropertyChangedEventArgs(propertyName);
            handler?.Invoke(this, e);
        }

        private string GetDecoder()
        {
            switch (SelectedDecoderIndex)
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
                        OnPropertyChanged(nameof(Children));
                    }
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Trigger the validation on these fields (work around a WPF 3.x issue).
            OnPropertyChanged(nameof(SelectedDecoderIndex));
        }
    }
}