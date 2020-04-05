namespace Sentinel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows;

    using Newtonsoft.Json.Linq;

    using Sentinel.Classification;
    using Sentinel.Classification.Interfaces;
    using Sentinel.Extractors;
    using Sentinel.Extractors.Interfaces;
    using Sentinel.FileMonitor;
    using Sentinel.Filters;
    using Sentinel.Filters.Interfaces;
    using Sentinel.Highlighters;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Images;
    using Sentinel.Images.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;
    using Sentinel.Log4Net;
    using Sentinel.Logger;
    using Sentinel.Logs;
    using Sentinel.Logs.Gui;
    using Sentinel.Logs.Interfaces;
    using Sentinel.NLog;
    using Sentinel.Preferences;
    using Sentinel.Providers;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services.Interfaces;
    using Sentinel.Support;
    using Sentinel.Upgrader;
    using Sentinel.Views;
    using Sentinel.Views.Gui;
    using Sentinel.Views.Interfaces;

    using WpfExtras;

    [DataContract]
    public class SessionManager : ISessionManager
    {
        private const char ObjectSeparator = '~';

        private bool serviceLocatorIsFresh;

        public SessionManager()
        {
            Name = "Untitled";
            RefreshServiceLocator();
        }

        public IEnumerable<ViewModelBase> ChangingViewModelBases { get; set; }

        public bool IsSaved { get; set; }

        public string Name { get; private set; }

        public IEnumerable<IProviderSettings> ProviderSettings
        {
            get
            {
                var providerManager = ServiceLocator.Instance.Get<IProviderManager>();
                return providerManager.Instances.Select(c => c.ProviderSettings);
            }
        }

        public void LoadNewSession(Window parent)
        {
            if (!serviceLocatorIsFresh)
            {
                CleanUpResources();
                Name = "Untitled";
                RefreshServiceLocator();
            }

            var wizard = new NewLoggerWizard();

            if (!wizard.Display(parent))
            {
                return;
            }

            var settings = wizard.Settings;

            // Set session properties
            Name = settings.LogName;

            ConfigureLoggerServices(settings.LogName, settings.Views, settings.Providers);

            IsSaved = false;
            serviceLocatorIsFresh = false;
        }

        public void LoadProviders(IEnumerable<PendingProviderRecord> providers)
        {
            CleanUpResources();

            var views = new List<string> { ServiceLocator.Instance.Get<IViewManager>().Registered.First().Identifier };

            ConfigureLoggerServices("Untitled", views, providers);

            IsSaved = false;
        }

        public void LoadSession(string fileName)
        {
            var fileText = File.ReadAllText(fileName);
            var jsonObjects = fileText.Split(ObjectSeparator);

            CleanUpResources();
            LoadServiceLocator(jsonObjects);

            IsSaved = false;
        }

        public void SaveSession(string filePath)
        {
            var stringToSave = new StringBuilder();
            var services = ServiceLocator.Instance.RegisteredServices;
            foreach (var value in services)
            {
                if (value == null)
                {
                    Trace.TraceError("Unexpected null");
                    continue;
                }

                if (value.HasAttribute<DataContractAttribute>())
                {
                    stringToSave.AppendLine(JsonHelper.SerializeToString(value));
                    stringToSave.AppendLine(ObjectSeparator.ToString());
                }
            }

            using (var fs = File.Create(filePath))
            {
                var info = new UTF8Encoding(true).GetBytes(stringToSave.ToString());
                fs.Write(info, 0, info.Length);
            }

            IsSaved = true;
        }

        private static void ConfigureLoggerServices(
            string logName,
            IEnumerable<string> viewIdentifiers,
            IEnumerable<PendingProviderRecord> pendingProviderRecords)
        {
            // Create the logger.
            var logManager = ServiceLocator.Instance.Get<ILogManager>();
            var log = logManager.Add(logName);

            // Create the frame view
            var viewManager = ServiceLocator.Instance.Get<IViewManager>();
            Debug.Assert(
                viewManager != null,
                "A ViewManager should be registered with service locator for the IViewManager interface");

            var frame = ServiceLocator.Instance.Get<IWindowFrame>();
            frame.Log = log;
            frame.SetViews(viewIdentifiers);
            viewManager.Viewers.Add(frame);

            // Create the providers.
            var providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            foreach (var providerRecord in pendingProviderRecords)
            {
                var provider = providerManager.Create(providerRecord.Info.Identifier, providerRecord.Settings);
                provider.Logger = log;
                provider.Start();
            }
        }

        private void CleanUpResources()
        {
            // Close all open providers
            var providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            foreach (var provider in providerManager.Instances)
            {
                provider.Close();
            }

            // Unregister changing viewmodelbases
            foreach (var viewmodel in ChangingViewModelBases)
            {
                viewmodel.PropertyChanged -= ViewModelProperty_Changed;
            }
        }

        private void LoadChangingViewModelBases()
        {
            var viewModelBases = new List<ViewModelBase>();
            var locator = ServiceLocator.Instance;
            viewModelBases.Add((SearchExtractor)locator.Get<ISearchExtractor>());
            viewModelBases.Add((SearchFilter)locator.Get<ISearchFilter>());
            viewModelBases.Add((HighlightingService<IHighlighter>)locator.Get<IHighlightingService<IHighlighter>>());
            viewModelBases.Add((ExtractingService<IExtractor>)locator.Get<IExtractingService<IExtractor>>());
            viewModelBases.Add((FilteringService<IFilter>)locator.Get<IFilteringService<IFilter>>());
            viewModelBases.Add((ClassifyingService<IClassifier>)locator.Get<IClassifyingService<IClassifier>>());

            ChangingViewModelBases = viewModelBases;

            foreach (var item in ChangingViewModelBases)
            {
                item.PropertyChanged += ViewModelProperty_Changed;
            }
        }

        private void LoadServiceLocator(IEnumerable<string> jsonObjectStrings)
        {
            if (jsonObjectStrings == null)
            {
                return;
            }

            var locator = ServiceLocator.Instance;
            var pendingProviderRecords = new List<PendingProviderRecord>();

            foreach (var objString in jsonObjectStrings)
            {
                if (!string.IsNullOrWhiteSpace(objString))
                {
                    var deserializedObj = JObject.Parse(objString);
                    var typeString = deserializedObj["$type"].ToString();

                    if (typeString.Contains(typeof(UserPreferences).ToString()))
                    {
                        locator.Register<IUserPreferences>(JsonHelper.DeserializeFromString<UserPreferences>(objString));
                    }
                    else if (typeString.Contains(typeof(SearchFilter).Name))
                    {
                        locator.Register<ISearchFilter>(JsonHelper.DeserializeFromString<SearchFilter>(objString));
                    }
                    else if (typeString.Contains(typeof(SearchExtractor).Name))
                    {
                        locator.Register<ISearchExtractor>(JsonHelper.DeserializeFromString<SearchExtractor>(objString));
                    }
                    else if (typeString.Contains(typeof(FilteringService<>).Name))
                    {
                        locator.Register<IFilteringService<IFilter>>(
                            JsonHelper.DeserializeFromString<FilteringService<IFilter>>(objString));
                    }
                    else if (typeString.Contains(typeof(ExtractingService<>).Name))
                    {
                        locator.Register<IExtractingService<IExtractor>>(
                            JsonHelper.DeserializeFromString<ExtractingService<IExtractor>>(objString));
                    }
                    else if (typeString.Contains(typeof(HighlightingService<>).Name))
                    {
                        locator.Register<IHighlightingService<IHighlighter>>(
                            JsonHelper.DeserializeFromString<HighlightingService<IHighlighter>>(objString));
                    }
                    else if (typeString.Contains(typeof(SearchHighlighter).Name))
                    {
                        locator.Register<ISearchHighlighter>(
                            JsonHelper.DeserializeFromString<SearchHighlighter>(objString));
                    }
                    else if (typeString.Contains(typeof(ClassifyingService<>).Name))
                    {
                        locator.Register<IClassifyingService<IClassifier>>(
                            JsonHelper.DeserializeFromString<ClassifyingService<IClassifier>>(objString));
                    }
                    else if (typeString.Contains(typeof(TypeToImageService).Name))
                    {
                        locator.Register<ITypeImageService>(
                            JsonHelper.DeserializeFromString<TypeToImageService>(objString));
                    }
                    else if (typeString.Contains(typeof(SessionManager).Name))
                    {
                        Name = deserializedObj["Name"].ToString();

                        LoadChangingViewModelBases();

                        var providerSettingsObj = deserializedObj["ProviderSettings"].HasValues
                                                      ? deserializedObj["ProviderSettings"].Values()
                                                      : null;

                        if (providerSettingsObj == null)
                        {
                            continue;
                        }

                        var providerInstances = providerSettingsObj.Last();
                        foreach (var providerSetting in providerInstances)
                        {
                            var settings = providerSetting.ToString();

                            var name = providerSetting["$type"].ToString();
                            if (name.Contains(typeof(NetworkSettings).Name))
                            {
                                var thisSetting = JsonHelper.DeserializeFromString<NetworkSettings>(settings);
                                pendingProviderRecords.Add(new PendingProviderRecord
                                                               {
                                                                   Info = thisSetting.Info,
                                                                   Settings = thisSetting,
                                                               });
                            }
                            else if (name.Contains(typeof(UdpAppenderSettings).Name))
                            {
                                var thisSetting = JsonHelper.DeserializeFromString<UdpAppenderSettings>(settings);
                                pendingProviderRecords.Add(new PendingProviderRecord
                                                               {
                                                                   Info = thisSetting.Info,
                                                                   Settings = thisSetting,
                                                               });
                            }
                            else if (name.Contains(typeof(FileMonitoringProviderSettings).Name))
                            {
                                var thisSetting =
                                    JsonHelper.DeserializeFromString<FileMonitoringProviderSettings>(settings);
                                pendingProviderRecords.Add(new PendingProviderRecord { Info = thisSetting.Info, Settings = thisSetting, });
                            }
                            else
                            {
                                Trace.TraceError($"No PendingProviderRecord for type of {name}");
                            }
                        }
                    }
                    else
                    {
                        Trace.TraceError($"No deconstruction supplied for type {typeString}");
                    }
                }
            }

            // Load new objects for the rest.
            locator.Register<ILogManager>(new LogManager());
            locator.Register<LogWriter>(new LogWriter());
            locator.Register(typeof(IViewManager), typeof(ViewManager), false);
            locator.Register<IProviderManager>(new ProviderManager());
            locator.Register<IWindowFrame>(new MultipleViewFrame()); // needs IUserPreferences, IViewManager
            locator.Register<ILogFileExporter>(new LogFileExporter());

            locator.Register<IUpgradeServicePreferences>(new UpgradeServicePreferences());
            locator.Register<IUpgradeService>(new SquirrelUpgradeService());

            locator.Register<INewProviderWizard>(new NewProviderWizard());

            // Do this last so that other services have registered, e.g. the
            // TypeImageService is called by some classifiers!
            if (!locator.IsRegistered<IClassifyingService<IClassifier>>())
            {
                locator.Register(
                    typeof(IClassifyingService<IClassifier>),
                    typeof(ClassifyingService<IClassifier>),
                    true);
            }

            var viewIDs = new List<string> { locator.Get<IViewManager>().Registered.First().Identifier };

            ConfigureLoggerServices(Name, viewIDs, pendingProviderRecords);

            GC.Collect();

            serviceLocatorIsFresh = false;
        }

        private void RefreshServiceLocator()
        {
            var locator = ServiceLocator.Instance;

            locator.Register(typeof(IUserPreferences), typeof(UserPreferences), true);
            locator.Register(typeof(ISearchFilter), typeof(SearchFilter), true);
            locator.Register(typeof(ISearchExtractor), typeof(SearchExtractor), true);
            locator.Register(typeof(IFilteringService<IFilter>), typeof(FilteringService<IFilter>), true);
            locator.Register(typeof(IExtractingService<IExtractor>), typeof(ExtractingService<IExtractor>), true);
            locator.Register(
                typeof(IHighlightingService<IHighlighter>),
                typeof(HighlightingService<IHighlighter>),
                true);
            locator.Register(typeof(ISearchHighlighter), typeof(SearchHighlighter), true);
            locator.Register(typeof(IClassifyingService<IClassifier>), typeof(ClassifyingService<IClassifier>), true);

            locator.Register(typeof(ITypeImageService), typeof(TypeToImageService), true);
            locator.Register<ILogManager>(new LogManager());
            locator.Register<LogWriter>(new LogWriter());
            locator.Register(typeof(IViewManager), typeof(ViewManager), false);
            locator.Register<IProviderManager>(new ProviderManager());
            locator.Register<IWindowFrame>(new MultipleViewFrame()); // needs IUserPreferences, IViewManager
            locator.Register<ILogFileExporter>(new LogFileExporter());

            locator.Register<IUpgradeServicePreferences>(new UpgradeServicePreferences());
            locator.Register<IUpgradeService>(new SquirrelUpgradeService());

            locator.Register<INewProviderWizard>(new NewProviderWizard());

            LoadChangingViewModelBases();

            GC.Collect(); // collect all things without a reference

            serviceLocatorIsFresh = true;
        }

        private void ViewModelProperty_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsSaved = false;
            serviceLocatorIsFresh = false;
        }
    }
}