namespace Sentinel.Extractors
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Input;

    using Sentinel.Extractors.Gui;
    using Sentinel.Extractors.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    [DataContract]
    public class ExtractingService<T> : ViewModelBase, IExtractingService<T>, IDefaultInitialisation
        where T : class, IExtractor
    {
        private readonly CollectionChangeHelper<T> collectionHelper = new CollectionChangeHelper<T>();

        private readonly IAddExtractorService addExtractorService = new AddExtractor();

        private readonly IEditExtractorService editExtractorService = new EditExtractor();

        private readonly IRemoveExtractorService removeExtractorService = new RemoveExtractor();

        private int selectedIndex = -1;

        public ExtractingService()
        {
            Add = new DelegateCommand(AddExtractor);
            Edit = new DelegateCommand(EditExtractor, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveExtractor, e => selectedIndex != -1);

            Extractors = new ObservableCollection<T>();
            SearchExtractors = new ObservableCollection<T>();

            // Register self as an observer of the collection.
            collectionHelper.OnPropertyChanged += CustomExtractorPropertyChanged;

            collectionHelper.ManagerName = "ExtractingService";
            collectionHelper.NameLookup += e => e.Name;

            Extractors.CollectionChanged += collectionHelper.AttachDetach;
            SearchExtractors.CollectionChanged += collectionHelper.AttachDetach;

            var searchExtractor = ServiceLocator.Instance.Get<ISearchExtractor>();

            if (searchExtractor != null)
            {
                SearchExtractors.Add(searchExtractor as T);
            }
            else
            {
                Debug.Fail("The search extractor is null.");
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand Add { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand Edit { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand Remove { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
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
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public ObservableCollection<T> Extractors { get; set; }

        public ObservableCollection<T> SearchExtractors { get; set; }

        public void Initialise()
        {
            // For adding standard extractors
        }

        public bool IsFiltered(ILogEntry entry)
        {
            return Extractors.Any(filter => filter.Enabled && filter.IsMatch(entry))
                   || SearchExtractors.Any(filter => filter.Enabled && filter.IsMatch(entry));
        }

        private void AddExtractor(object obj)
        {
            addExtractorService.Add();
        }

        private void CustomExtractorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var extractor = sender as Extractor;
            if (extractor != null)
            {
                Trace.WriteLine(
                    $"ExtractingService saw some activity on {extractor.Name} (IsEnabled = {extractor.Enabled})");
            }

            OnPropertyChanged(string.Empty);
        }

        private void EditExtractor(object obj)
        {
            var extractor = Extractors.ElementAt(SelectedIndex);
            if (extractor != null)
            {
                editExtractorService.Edit(extractor);
            }
        }

        private void RemoveExtractor(object obj)
        {
            var extractor = Extractors.ElementAt(SelectedIndex);
            removeExtractorService.Remove(extractor);
        }
    }
}