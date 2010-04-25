using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;
using Sentinel.Views.Interfaces;

namespace Sentinel.Logs.Gui
{
    public class NewLoggerSettings : ViewModelBase
    {
        private bool isVertical;

        private string layout;

        private string logName;

        private string primaryView;

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
                return isVertical;
            }
            set
            {
                if (value == isVertical) return;
                isVertical = value;
                OnPropertyChanged("IsVertical");
            }
        }

        public string Layout
        {
            get
            {
                return layout;
            }
            set
            {
                if (layout == value) return;
                layout = value;
                OnPropertyChanged("Layout");
            }
        }

        public string LogName
        {
            get
            {
                return logName;
            }
            set
            {
                if (logName == value) return;
                logName = value;
                OnPropertyChanged("LogName");
            }
        }

        public string PrimaryView
        {
            get
            {
                return primaryView;
            }
            private set
            {
                if (primaryView == value) return;
                primaryView = value;
                OnPropertyChanged("PrimaryView");
            }
        }

        public string ProviderSummary
        {
            get
            {
                var sb = new StringBuilder();

                if (Providers != null
                    && Providers.Count > 0)
                {
                    for (int index = 0; index < Providers.Count; index++)
                    {
                        PendingProviderRecord p = Providers[index];
                        sb.AppendFormat(
                            "{0} - {1} - {2}",
                            p.Settings.Name,
                            p.Settings.Info.Name,
                            p.Settings.Summary);

                        if (index < providers.Count - 1) sb.AppendLine();
                    }
                }
                else
                {
                    sb.Append("No providers configured.");
                }

                return sb.ToString();
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
                if (providers == value) return;
                providers = value;
                OnPropertyChanged("Providers");
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
                if (secondaryView == value) return;
                secondaryView = value;
                OnPropertyChanged("SecondaryView");
            }
        }

        public ObservableCollection<string> Views { get; set; }

        private static string LookupViewInformation(string identifier)
        {
            var vm = ServiceLocator.Instance.Get<IViewManager>();
            IViewInformation info = vm.Get(identifier);
            return info.Name;
        }

        private void ViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Views.Count >= 1)
            {
                PrimaryView = LookupViewInformation(Views.ElementAt(0));
            }

            if (Views.Count >= 2)
            {
                SecondaryView = LookupViewInformation(Views.ElementAt(1));
            }
            else
            {
                SecondaryView = "Not used.";
            }
        }
    }
}