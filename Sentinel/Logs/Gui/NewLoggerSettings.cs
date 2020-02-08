namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;

    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;
    using Sentinel.Views.Interfaces;

    using WpfExtras;

    public class NewLoggerSettings : ViewModelBase
    {
        private readonly InternalSettings settings = new InternalSettings();

        private ObservableCollection<PendingProviderRecord> providers;

        private string secondaryView;

        public NewLoggerSettings()
        {
            Providers = new ObservableCollection<PendingProviderRecord>();
            Views = new ObservableCollection<string>();

            Views.CollectionChanged += ViewsCollectionChanged;
        }

        public bool IsVertical
        {
            get
            {
                return settings.IsVertical;
            }

            set
            {
                if (IsVertical != value)
                {
                    settings.IsVertical = value;
                    OnPropertyChanged(nameof(IsVertical));
                }
            }
        }

        public string Layout
        {
            get
            {
                return settings.Layout;
            }

            set
            {
                if (Layout != value)
                {
                    settings.Layout = value;
                    OnPropertyChanged(nameof(Layout));
                }
            }
        }

        public string LogName
        {
            get
            {
                return settings.LogName;
            }

            set
            {
                if (LogName != value)
                {
                    settings.LogName = value;
                    OnPropertyChanged(nameof(LogName));
                }
            }
        }

        public ObservableCollection<PendingProviderRecord> Providers
        {
            get
            {
                return providers;
            }

            private set
            {
                if (providers != value)
                {
                    providers = value;
                    OnPropertyChanged(nameof(Providers));
                }
            }
        }

        public string PrimaryView
        {
            get
            {
                return settings.PrimaryView;
            }

            private set
            {
                if (PrimaryView != value)
                {
                    settings.PrimaryView = value;
                    OnPropertyChanged(nameof(PrimaryView));
                }
            }
        }

        public string SecondaryView
        {
            get
            {
                return secondaryView;
            }

            private set
            {
                if (secondaryView != value)
                {
                    secondaryView = value;
                    OnPropertyChanged(nameof(SecondaryView));
                }
            }
        }

        public ObservableCollection<string> Views { get; set; }

        public string ProviderSummary
        {
            get
            {
                var sb = new StringBuilder();

                if (Providers != null && Providers.Count > 0)
                {
                    for (var index = 0; index < Providers.Count; index++)
                    {
                        var p = Providers[index];
                        sb.Append($"{p.Settings.Name} - {p.Settings.Info.Name} - {p.Settings.Summary}");

                        if (index < (providers.Count - 1))
                        {
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.Append("No providers configured.");
                }

                return sb.ToString();
            }
        }

        private static string LookupViewInformation(string identifier)
        {
            var vm = ServiceLocator.Instance.Get<IViewManager>();
            var info = vm.Get(identifier);
            return info.Name;
        }

        private void ViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Views.Count >= 1)
            {
                PrimaryView = LookupViewInformation(Views.ElementAt(0));
            }

            SecondaryView = Views.Count >= 2 ? LookupViewInformation(Views.ElementAt(1)) : "Not used.";
        }

        private class InternalSettings
        {
            public string LogName { get; set; }

            public string PrimaryView { get; set; }

            public bool IsVertical { get; set; }

            public string Layout { get; set; }
        }
    }
}