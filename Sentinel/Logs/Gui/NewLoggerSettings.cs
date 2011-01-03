using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;
using Sentinel.Providers;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using Sentinel.Support;
using Sentinel.Support.Mvvm;
using Sentinel.Views.Interfaces;

namespace Sentinel.Logs.Gui
{
    public class NewLoggerSettings : ViewModelBase
    {
        [ProtoContract]
        public class InternalSettings
        {
            [ProtoMember(1)]
            public string LogName { get; set; }

            [ProtoMember(2)]
            public string PrimaryView { get; set; }
            
            [ProtoMember(21)]
            public bool IsVertical { get; set; }

            [ProtoMember(22)]
            public string Layout { get; set; }
        }

        private InternalSettings settings = new InternalSettings();

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
                if (value == IsVertical) return;
                settings.IsVertical = value;
                OnPropertyChanged("IsVertical");
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
                if (Layout == value) return;
                settings.Layout = value;
                OnPropertyChanged("Layout");
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
                if (LogName == value) return;
                settings.LogName = value;
                OnPropertyChanged("LogName");
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

        public string PrimaryView
        {
            get
            {
                return settings.PrimaryView;
            }
            private set
            {
                if (PrimaryView == value) return;
                settings.PrimaryView = value;
                OnPropertyChanged("PrimaryView");
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

        public MemoryStream ProtobufPersist()
        {
#if PROTO_SAVING_SESSIONS
    // Testing.............................................................................
            MemoryStream ms = new MemoryStream();

            Trace.WriteLine("Settings");
            Serializer.Serialize(ms, settings);
            Trace.WriteLine(String.Format(" - Stream Length: {0}, Position: {1}", ms.Length, ms.Position));
            Serializer.Serialize(ms, providers.Count());
            Trace.WriteLine(String.Format(" - Stream Length: {0}, Position: {1}", ms.Length, ms.Position));

            Trace.WriteLine("Providers");
            foreach (var provider in Providers)
            {
                try
                {
                    Serializer.Serialize(ms, provider.Info);
                    Trace.WriteLine(String.Format(" - Stream Length: {0}, Position: {1}", ms.Length, ms.Position));

                    var wrappedProviderSettings = ProtoHelper.Wrap(provider.Settings);
                    Trace.WriteLine(
                        string.Format("Wrapped {0} into {1} bytes", provider.Settings, wrappedProviderSettings.Length));
                    Serializer.Serialize(ms, wrappedProviderSettings);
                    Trace.WriteLine(String.Format(" - Stream Length: {0}, Position: {1}", ms.Length, ms.Position));
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Exception caught trying to wrap {0}", provider.Info.Name);
                    throw;
                }
            }
            ms.Position = 0;
            Trace.WriteLine(String.Format(" - Stream Length: {0}, Position: {1}", ms.Length, ms.Position));

            // Testing.............................................................................
            try
            {
                InternalSettings s = Serializer.Deserialize<InternalSettings>(ms);
                int providerCount = Serializer.Deserialize<int>(ms);
                for (int i = 0; i < providerCount; i++)
                {
                    var info = Serializer.Deserialize<ProviderInfo>(ms);
                
                    object providerSettings;
                    ProtoHelper.Unwrap(ms, out providerSettings);

                    Trace.WriteLine(providerSettings.GetType().FullName);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            ms.Position = 0;
            return ms;
#else
            return null;
#endif
        }
    }
}