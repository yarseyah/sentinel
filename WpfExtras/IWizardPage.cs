namespace WpfExtras
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    public interface IWizardPage
        : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IWizardPage> Children { get; }

        string Description { get; }

        bool IsValid { get; }

        Control PageContent { get; }

        string Title { get; }

        // Hierarchical Support.
        void AddChild(IWizardPage child);

        void RemoveChild(IWizardPage child);

        object Save(object saveData);
    }
}
