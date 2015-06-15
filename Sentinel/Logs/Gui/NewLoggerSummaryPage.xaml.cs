namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for NewLoggerSummaryPage.xaml
    /// </summary>
    public partial class NewLoggerSummaryPage : IWizardPage 
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();
        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        public NewLoggerSummaryPage()
        {
            InitializeComponent();

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            DataContext = this;
        }

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

        public string Title
        {
            get
            {
                return "Summary";
            }
        }

        public string Description
        {
            get
            {
                return "Review the selections made in this Wizard.";
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
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
        }

        public object Save(object saveData)
        {
            return saveData;
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
            }
        }
    }
}
