#region Using directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

#endregion

namespace WpfExtras
{
    public class PageNavigationTreeEntry : INotifyPropertyChanged
    {
        private readonly ObservableCollection<PageNavigationTreeEntry> children;

        private bool isCurrent;

        public PageNavigationTreeEntry(IWizardPage page)
        {
            Page = page;

            children = new ObservableCollection<PageNavigationTreeEntry>();
            Children = new ReadOnlyObservableCollection<PageNavigationTreeEntry>(children);

            page.PropertyChanged += PagePropertyChanged;
            (page.Children as INotifyCollectionChanged).CollectionChanged += PageChildCollectionChanged;


            foreach (IWizardPage c in page.Children)
            {
                children.Add(new PageNavigationTreeEntry(c));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<PageNavigationTreeEntry> Children { get; private set; }

        public bool IsCurrent
        {
            get
            {
                return isCurrent;
            }
            private set
            {
                if (isCurrent == value) return;
                isCurrent = value;
                OnPropertyChanged("IsCurrent");
            }
        }


        public IWizardPage Page { get; private set; }

        public void SetCurrentPage(IWizardPage wizardPage)
        {
            IsCurrent = wizardPage == Page;
            foreach (var child in children)
            {
                child.SetCurrentPage(wizardPage);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void PageChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action
                == NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    children.Add(new PageNavigationTreeEntry(newItem as IWizardPage));
                }
            }
            else if (e.Action
                     == NotifyCollectionChangedAction.Remove)
            {
                List<PageNavigationTreeEntry> itemsToRemove = Children
                    .Join(e.OldItems.OfType<IWizardPage>(), n => n.Page, p => p, (n, p) => n)
                    .ToList();
                foreach (var c in itemsToRemove)
                {
                    children.Remove(c);
                }
            }
            else if (e.Action
                     == NotifyCollectionChangedAction.Reset)
            {
                children.Clear();
            }
        }

        private void PagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}