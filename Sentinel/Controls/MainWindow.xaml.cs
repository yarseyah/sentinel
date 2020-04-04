namespace Sentinel.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Controls.Ribbon;
    using System.Windows.Data;
    using System.Windows.Input;
    using CommandLine;
    using Common.Logging;
    using Microsoft.Win32;
    using Sentinel.Classification.Interfaces;
    using Sentinel.Extractors.Interfaces;
    using Sentinel.Filters.Interfaces;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Log4Net;
    using Sentinel.Logs.Interfaces;
    using Sentinel.NLog;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;
    using Sentinel.Services.Interfaces;
    using Sentinel.StartUp;
    using Sentinel.Support;
    using Sentinel.Upgrader;
    using Sentinel.Views.Interfaces;
    using WpfExtras;
    using WpfExtras.Converters;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger<MainWindow>();

        private readonly string persistingFilename;

        private readonly string persistingRecentFileName;

        private List<string> recentFilePathList;

        private PreferencesWindow preferencesWindow;

        private int preferencesWindowTabSelected;

        public MainWindow()
        {
            InitializeComponent();
            var savingDirectory = ServiceLocator.Instance.SaveLocation;
            persistingFilename = Path.Combine(savingDirectory, "MainWindow");
            persistingRecentFileName = Path.Combine(savingDirectory, "RecentFiles");
            var fileName = Path.ChangeExtension(persistingFilename, ".json");

            var settings = PersistingSettings.Load(fileName);

            // Restore persisted window placement if provided
            if (settings?.WindowPlacementInfo != null)
            {
                RestoreWindowPosition(settings.WindowPlacementInfo);
            }

            if (settings?.UserPreferences != null)
            {
                // TODO: is this already set?
                Preferences = settings.UserPreferences;
            }

            // Get recently opened files
            GetRecentlyOpenedFiles();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand Add { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand About { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand ShowPreferences { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand ExportLogs { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand Exit { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand NewSession { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand SaveSession { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand LoadSession { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public IUserPreferences Preferences { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public IViewManager ViewManager { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public IFilteringService<IFilter> Filters => ServiceLocator.Instance.Get<IFilteringService<IFilter>>();

        // ReSharper disable once MemberCanBePrivate.Global
        public IHighlightingService<IHighlighter> Highlighters
            => ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();

        // ReSharper disable once MemberCanBePrivate.Global
        public IClassifyingService<IClassifier> ClassifyingService
            => ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();

        // ReSharper disable once MemberCanBePrivate.Global
        public IExtractingService<IExtractor> Extractors
            => ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();

        // ReSharper disable once MemberCanBePrivate.Global
        public ISearchHighlighter Search => ServiceLocator.Instance.Get<ISearchHighlighter>();

        // ReSharper disable once MemberCanBePrivate.Global
        public ISearchFilter SearchFilter => ServiceLocator.Instance.Get<ISearchFilter>();

        // ReSharper disable once MemberCanBePrivate.Global
        public ISearchExtractor SearchExtractor => ServiceLocator.Instance.Get<ISearchExtractor>();

        public IUpgradeService UpgradeService => ServiceLocator.Instance.Get<IUpgradeService>();

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<string> RecentFiles { get; private set; }

        private static WindowPlacementInfo ValidateScreenPosition(WindowPlacementInfo wp)
        {
            if (wp == null)
            {
                return null;
            }

            try
            {
                var virtualScreen = new Rect(
                    SystemParameters.VirtualScreenLeft,
                    SystemParameters.VirtualScreenTop,
                    SystemParameters.VirtualScreenWidth,
                    SystemParameters.VirtualScreenHeight);
                var window = new Rect(wp.Left, wp.Top, wp.Width, wp.Height);
                return virtualScreen.IntersectsWith(window) ? wp : null;
            }
            catch (Exception e)
            {
                Log.Error("Unable to calculate rectangle or perform intersection with window", e);
            }

            return null;
        }

        private void ShowPreferencesAction(object obj)
        {
            preferencesWindowTabSelected = Convert.ToInt32(obj);
            Preferences.Show = true;
        }

        private void ExportLogsAction(object obj)
        {
            // Get Log
            var tab = (TabItem)tabControl.SelectedItem;
            var frame = (IWindowFrame)tab.Content;
            var restartLogging = false;

            // Notify user that log messages will be paused during this operation
            if (frame.Log.Enabled)
            {
                var messageBoxResult =
                    MessageBox.Show(
                        "The log viewer must be paused momentarily for this operation to continue. Is it OK to pause logging?",
                        "Sentinel",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    frame.Log.Enabled = false;
                    restartLogging = true;
                }
                else
                {
                    return;
                }
            }

            // Open a save file dialog
            var savefile = new SaveFileDialog
            {
                FileName = frame.Log.Name,
                DefaultExt = ".log",
                Filter = "Log documents (.log)|*.log|Text documents (.txt)|*.txt",
                FilterIndex = 0,
            };

            if (savefile.ShowDialog(this) == true)
            {
                var logFileExporter = ServiceLocator.Instance.Get<ILogFileExporter>();
                logFileExporter.SaveLogViewerToFile(frame, savefile.FileName);
            }

            frame.Log.Enabled = restartLogging;
        }

        private void SaveSessionAction(object obj)
        {
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            // Open a save file dialog
            var savefile = new SaveFileDialog
            {
                FileName = sessionManager.Name,
                DefaultExt = ".sntl",
                Filter = "Sentinel session (.sntl)|*.sntl",
                FilterIndex = 0,
            };

            if (savefile.ShowDialog(this) == true)
            {
                sessionManager.SaveSession(savefile.FileName);
                AddToRecentFiles(savefile.FileName);
            }
        }

        private void AddToRecentFiles(string fileName)
        {
            if (RecentFiles.Contains(fileName))
            {
                RecentFiles.Move(RecentFiles.IndexOf(fileName), 0);
            }
            else
            {
                RecentFiles.Insert(0, fileName);
            }

            // Keep list at no more than 13
            if (RecentFiles.Count > 13)
            {
                RecentFiles.Remove(RecentFiles.LastOrDefault());
            }
        }

        private void NewSessionAction(object obj)
        {
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            if (!sessionManager.IsSaved)
            {
                var userResult = MessageBox.Show(
                    "Do you want to save changes you made to " + sessionManager.Name + "?",
                    "Sentinel",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (userResult == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (userResult == MessageBoxResult.Yes)
                {
                    SaveSession.Execute(null);

                    // if the user clicked "Cancel" at the save dialog box
                    if (!sessionManager.IsSaved)
                    {
                        return;
                    }
                }
            }

            // Remove the tab control.
            if (tabControl.Items.Count > 0)
            {
                var tab = tabControl.SelectedItem;
                tabControl.Items.Remove(tab);
            }

            Add.Execute(null);
        }

        private void LoadSessionAction(object obj)
        {
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();
            var fileNameToLoad = (string)obj;

            if (!sessionManager.IsSaved)
            {
                var userResult = MessageBox.Show(
                    "Do you want to save changes you made to " + sessionManager.Name + "?",
                    "Sentinel",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (userResult == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (userResult == MessageBoxResult.Yes)
                {
                    SaveSession.Execute(null);

                    // if the user clicked "Cancel" at the save dialog box
                    if (!sessionManager.IsSaved)
                    {
                        return;
                    }
                }
            }

            if (fileNameToLoad == null)
            {
                // open a save file dialog
                var openFile = new OpenFileDialog
                {
                    FileName = sessionManager.Name,
                    DefaultExt = ".sntl",
                    Filter = "Sentinel session (.sntl)|*.sntl",
                    FilterIndex = 0,
                };

                if (openFile.ShowDialog(this) == true)
                {
                    fileNameToLoad = openFile.FileName;
                }
                else
                {
                    return;
                }
            }

            // Remove the tab control.
            if (tabControl.Items.Count > 0)
            {
                var tab = tabControl.SelectedItem;
                tabControl.Items.Remove(tab);
            }

            RemoveBindingReferences();

            sessionManager.LoadSession(fileNameToLoad);
            AddToRecentFiles(fileNameToLoad);

            BindViewToViewModel();

            if (!sessionManager.ProviderSettings.Any())
            {
                return;
            }

            var frame = ServiceLocator.Instance.Get<IWindowFrame>();

            // Add to the tab control.
            var newTab = new TabItem { Header = sessionManager.Name, Content = frame };
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

        /// <summary>
        /// AddNewListenerAction method provides a mechanism for the user to add additional
        /// listeners to the log-viewer.
        /// </summary>
        /// <param name="obj">Object to add as a new listener.</param>
        private void AddNewListenerAction(object obj)
        {
            // Load a new session
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            RemoveBindingReferences();

            sessionManager.LoadNewSession(this);

            BindViewToViewModel();

            if (!sessionManager.ProviderSettings.Any())
            {
                return;
            }

            var frame = ServiceLocator.Instance.Get<IWindowFrame>();

            // Add to the tab control.
            var tab = new TabItem { Header = sessionManager.Name, Content = frame };
            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Exit = new DelegateCommand(ee => Close());
            About = new DelegateCommand(
                ee =>
                {
                    var about = new AboutWindow(this);
                    about.ShowDialog();
                });

            Add = new DelegateCommand(AddNewListenerAction, b => tabControl.Items.Count < 1);
            ShowPreferences = new DelegateCommand(ShowPreferencesAction);
            ExportLogs = new DelegateCommand(ExportLogsAction, b => tabControl.Items.Count > 0);
            SaveSession = new DelegateCommand(SaveSessionAction);
            NewSession = new DelegateCommand(NewSessionAction);
            LoadSession = new DelegateCommand(LoadSessionAction);
            RecentFiles = new ObservableCollection<string>(recentFilePathList.Take(13));

            BindViewToViewModel();

            var commandLine = Environment.GetCommandLineArgs();
            if (UpgradeService != null)
            {
                // Determine whether anything passed on the command line, limited options
                // may be supplied and they will suppress the prompting of the new listener wizard.
                commandLine = UpgradeService.ParseCommandLine(commandLine);
            }

            if (commandLine.Length == 1)
            {
                Add.Execute(null);
            }
            else
            {
                ProcessCommandLine(commandLine.Skip(1));
            }

            // Debug the available loggers.
            var logManager = ServiceLocator.Instance.Get<Logs.Interfaces.ILogManager>();
            foreach (var logger in logManager)
            {
                Log.DebugFormat("Log: {0}", logger.Name);
            }

            var providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            foreach (var instance in providerManager.Instances)
            {
                Log.DebugFormat("Provider: {0}", instance.Name);
                Log.DebugFormat("   - is {0}active", instance.IsActive ? string.Empty : "not ");
                Log.DebugFormat("   - logger = {0}", instance.Logger);
            }

            // Provide the upgrade service with a Dispatcher for the main UI thread
            // which is needed for providing UI updates.
            if (UpgradeService != null)
            {
                UpgradeService.DispatcherUiThread = Dispatcher;
            }
        }

        private void ProcessCommandLine(IEnumerable<string> commandLine)
        {
            commandLine.ThrowIfNull(nameof(commandLine));

            var commandLineArguments = commandLine as string[] ?? commandLine.ToArray();
            if (!commandLineArguments.Any())
            {
                throw new ArgumentException("Collection must have at least one element", nameof(commandLine));
            }

            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            var unknownCommandLine = false;
            var args = commandLineArguments.ToArray();

            var options = Parser.Default.ParseArguments<Log4NetOptions, NLogOptions>(args).MapResult(
                (Log4NetOptions o) => Tuple.Create<string, IOptions>("log4net", o),
                (NLogOptions o) => Tuple.Create<string, IOptions>("nlog", o),
                _ => Tuple.Create<string, IOptions>(string.Empty, null));

            var verb = options.Item1;

            if (string.IsNullOrWhiteSpace(options.Item1))
            {
                var filePath = commandLineArguments.FirstOrDefault();
                if (!File.Exists(filePath) || Path.GetExtension(filePath)?.ToUpper() != ".SNTL")
                {
                    unknownCommandLine = true;
                }
            }

            if (unknownCommandLine)
            {
                // TODO: command line usage dialog
                MessageBox.Show(
                    "File does not exist or is not a Sentinel session file.",
                    "Sentinel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            RemoveBindingReferences();

            switch (verb)
            {
                case "nlog":
                    CreateDefaultNLogListener((NLogOptions)options.Item2, sessionManager);
                    break;
                case "log4net":
                    CreateDefaultLog4NetListener((Log4NetOptions)options.Item2, sessionManager);
                    break;
                default:
                    sessionManager.LoadSession(commandLineArguments.FirstOrDefault());
                    break;
            }

            BindViewToViewModel();

            var frame = ServiceLocator.Instance.Get<IWindowFrame>();

            // Add to the tab control.
            var newTab = new TabItem { Header = sessionManager.Name, Content = frame };
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

        private void CreateDefaultLog4NetListener(Log4NetOptions log4NetOptions, ISessionManager sessionManager)
        {
            var info = $"Using log4net listener on Udp port {log4NetOptions.Port}";
            Log.Debug(info);

            var providerSettings = new UdpAppenderSettings
            {
                Port = log4NetOptions.Port,
                Name = info,
                Info = Log4NetProvider.ProviderRegistrationInformation.Info,
            };

            var providers =
                Enumerable.Repeat(
                    new PendingProviderRecord
                    {
                        Info = Log4NetProvider.ProviderRegistrationInformation.Info,
                        Settings = providerSettings,
                    },
                    1);

            sessionManager.LoadProviders(providers);
        }

        private void CreateDefaultNLogListener(NLogOptions verbOptions, ISessionManager sessionManager)
        {
            var name = $"Using nlog listener on {(verbOptions.IsUdp ? "Udp" : "Tcp")} port {verbOptions.Port}";
            var info = NLogViewerProvider.ProviderRegistrationInformation.Info;
            Log.Debug(name);

            var providerSettings = new NetworkSettings
            {
                Protocol =
                    verbOptions.IsUdp
                        ? NetworkProtocol.Udp
                        : NetworkProtocol.Tcp,
                Port = verbOptions.Port,
                Name = name,
                Info = info,
            };
            var providers = Enumerable.Repeat(
                new PendingProviderRecord { Info = info, Settings = providerSettings },
                1);

            sessionManager.LoadProviders(providers);
        }

        private void RestoreWindowPosition(WindowPlacementInfo wp)
        {
            _ = wp ?? throw new ArgumentNullException(nameof(wp));

            // Validation routine will cope with Null being passed and if it finds an error, it returns null.
            wp = ValidateScreenPosition(wp);

            if (wp != null)
            {
                Log.TraceFormat(
                    "Window position being restored to ({0},{1})-({2},{3}) {4}",
                    wp.Top,
                    wp.Left,
                    wp.Top + wp.Height,
                    wp.Left + wp.Width,
                    wp.WindowState);

                Top = wp.Top;
                Left = wp.Left;
                Width = wp.Width;
                Height = wp.Height;

                // TODO: would it make sense to start up minimized if that was how it was terminated?
                WindowState = wp.WindowState;
            }
        }

        private void PreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Preferences != null)
            {
                if (e.PropertyName == "Show")
                {
                    if (Preferences.Show)
                    {
                        preferencesWindow = new PreferencesWindow(preferencesWindowTabSelected) { Owner = this };
                        preferencesWindow.Show();
                    }
                    else if (preferencesWindow != null)
                    {
                        preferencesWindow.Close();
                        preferencesWindow = null;
                    }
                }
            }
        }

        private void ViewManagerChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                tabControl.SelectedIndex = tabControl.Items.Count - 1;
            }
        }

        private void OnClosed(object sender, CancelEventArgs e)
        {
            var windowInfo = new WindowPlacementInfo
            {
                Height = (int)Height,
                Top = (int)Top,
                Left = (int)Left,
                Width = (int)Width,
                WindowState = WindowState,
            };

            var filename = Path.ChangeExtension(persistingFilename, ".json");
            PersistingSettings.Save(filename, windowInfo, Preferences);

            var recentFileInfo = new RecentFileInfo { RecentFilePaths = RecentFiles.ToList() };

            JsonHelper.SerializeToFile(recentFileInfo, Path.ChangeExtension(persistingRecentFileName, ".json"));
        }

        private void RetainOnlyStandardFilters(object sender, FilterEventArgs e)
        {
            sender.ThrowIfNull(nameof(sender));
            e.ThrowIfNull(nameof(e));
            e.Accepted = e.Item is IStandardDebuggingFilter;
        }

        private void ExcludeStandardFilters(object sender, FilterEventArgs e)
        {
            sender.ThrowIfNull(nameof(sender));
            e.ThrowIfNull(nameof(e));
            e.Accepted = !(e.Item is IStandardDebuggingFilter || e.Item is ISearchFilter);
        }

        private void RetainOnlyStandardHighlighters(object sender, FilterEventArgs e)
        {
            sender.ThrowIfNull(nameof(sender));
            e.ThrowIfNull(nameof(e));
            e.Accepted = e.Item is IStandardDebuggingHighlighter;
        }

        private void ExcludeStandardHighlighters(object sender, FilterEventArgs e)
        {
            sender.ThrowIfNull(nameof(sender));
            e.ThrowIfNull(nameof(e));
            e.Accepted = !(e.Item is IStandardDebuggingHighlighter);
        }

        private void SearchToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            Debug.Assert(
                sender.GetType() == typeof(RibbonToggleButton),
                $"A {sender.GetType()} accessed the wrong method");

            var button = (RibbonToggleButton)sender;
            switch (button.Label)
            {
                case "Highlight":
                    BindSearchToSearchHighlighter();
                    break;
                case "Filter":
                    BindSearchToSearchFilter();
                    break;
                case "Extract":
                    BindSearchToSearchExtractor();
                    break;
            }
        }

        private void BindSearchToSearchExtractor()
        {
            SearchRibbonTextBox.SetBinding(TextBox.TextProperty, CreateBinding("Pattern", SearchExtractor));
            SearchModeListBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Mode", SearchExtractor));
            SearchTargetComboBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Field", SearchExtractor));

            HighlightToggleButton.IsChecked = false;
            FilterToggleButton.IsChecked = false;
        }

        private Binding CreateBinding(string path, object source)
        {
            return new Binding
            {
                Source = source,
                Path = new PropertyPath(path),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
        }

        private void BindSearchToSearchFilter()
        {
            SearchRibbonTextBox.SetBinding(TextBox.TextProperty, CreateBinding("Pattern", SearchFilter));
            SearchModeListBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Mode", SearchFilter));
            SearchTargetComboBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Field", SearchFilter));
            HighlightToggleButton.IsChecked = false;
            ExtractToggleButton.IsChecked = false;
        }

        private void BindSearchToSearchHighlighter()
        {
            SearchRibbonTextBox.SetBinding(TextBox.TextProperty, CreateBinding("Search", Search));
            SearchModeListBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Mode", Search));
            SearchTargetComboBox.SetBinding(Selector.SelectedItemProperty, CreateBinding("Field", Search));
            FilterToggleButton.IsChecked = false;
            ExtractToggleButton.IsChecked = false;
        }

        private void RemoveBindingReferences()
        {
            var notifyPropertyChanged = Preferences as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= PreferencesChanged;
            }

            ViewManager.Viewers.CollectionChanged -= ViewManagerChanged;
        }

        private void BindViewToViewModel()
        {
            // Append version number to caption (to save effort of producing an about screen)
            Title =
                $"{Assembly.GetExecutingAssembly().GetName().Name} ({Assembly.GetExecutingAssembly().GetName().Version})"
                + $" {ServiceLocator.Instance.Get<ISessionManager>().Name}";

            // Preferences, if initialised, has come from persistence, so register current copy
            // otherwise, ask for one that will be shared.
            if (Preferences != null)
            {
                ServiceLocator.Instance.Register<IUserPreferences>(Preferences);
            }
            else
            {
                Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            }

            ViewManager = ServiceLocator.Instance.Get<IViewManager>();

            // Maintaining column widths is proving difficult in Xaml alone, so
            // add an observer here and deal with it in code.
            if (Preferences is INotifyPropertyChanged changed)
            {
                changed.PropertyChanged += PreferencesChanged;
            }

            DataContext = this;

            // When a new item is added, select the newest one.
            ViewManager.Viewers.CollectionChanged += ViewManagerChanged;

            // View-specific bindings
            var collapseIfZero = new CollapseIfZeroConverter();

            var standardHighlighters = new CollectionViewSource { Source = Highlighters.Highlighters };
            standardHighlighters.View.Filter = c => c is IStandardDebuggingHighlighter;

            var customHighlighters = new CollectionViewSource { Source = Highlighters.Highlighters };
            customHighlighters.View.Filter = c => !(c is IStandardDebuggingHighlighter);

            StandardHighlightersRibbonGroup.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = standardHighlighters });

            StandardHighlighterRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = standardHighlighters });
            var collapsingStandardHighlightersBinding = new Binding
            {
                Source = standardHighlighters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero,
            };
            StandardHighlighterRibbonGroupOnTab.SetBinding(VisibilityProperty, collapsingStandardHighlightersBinding);

            CustomHighlighterRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = customHighlighters });

            var collapsingCustomHighlightersBinding = new Binding
            {
                Source = customHighlighters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero,
            };
            CustomHighlighterRibbonGroupOnTab.SetBinding(VisibilityProperty, collapsingCustomHighlightersBinding);

            var standardFilters = new CollectionViewSource { Source = Filters.Filters };
            standardFilters.View.Filter = c => c is IStandardDebuggingFilter;

            var customFilters = new CollectionViewSource { Source = Filters.Filters };
            customFilters.View.Filter = c => !(c is IStandardDebuggingFilter);

            StandardFiltersRibbonGroup.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = standardFilters });

            StandardFiltersRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = standardFilters });

            StandardFiltersRibbonGroupOnTab.SetBinding(
                VisibilityProperty,
                new Binding { Source = standardFilters, Path = new PropertyPath("Count"), Converter = collapseIfZero });
            CustomFiltersRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = customFilters });
            CustomFiltersRibbonGroupOnTab.SetBinding(
                VisibilityProperty,
                new Binding { Source = customFilters, Path = new PropertyPath("Count"), Converter = collapseIfZero });

            var customExtractors = Extractors.Extractors;
            CustomExtractorsRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = customExtractors });

            var customClassifyiers = ClassifyingService.Classifiers;
            CustomClassifiersRibbonGroupOnTab.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = customClassifyiers });

            BindToSearchElements();

            // Column view buttons
            ExceptionRibbonToggleButton.SetBinding(
                ToggleButton.IsCheckedProperty,
                new Binding { Source = Preferences, Path = new PropertyPath("ShowExceptionColumn") });
            ThreadRibbonToggleButton.SetBinding(
                ToggleButton.IsCheckedProperty,
                new Binding { Source = Preferences, Path = new PropertyPath("ShowThreadColumn") });
            SourceHostRibbonToggleButton.SetBinding(
                ToggleButton.IsCheckedProperty,
                new Binding { Source = Preferences, Path = new PropertyPath("ShowSourceColumn") });
            DebugSourceRibbonToggleButton.SetBinding(
                ToggleButton.IsCheckedProperty,
                new Binding { Source = Preferences, Path = new PropertyPath("ShowSourceInformationColumns") });
            ContextRibbonToggleButton.SetBinding(
                ToggleButton.IsCheckedProperty,
                new Binding { Source = Preferences, Path = new PropertyPath("ShowContextColumn") });
        }

        private void BindToSearchElements()
        {
            // Bind search
            HighlightToggleButton.SetBinding(ToggleButton.IsCheckedProperty, CreateBinding("Enabled", Search));
            FilterToggleButton.SetBinding(ToggleButton.IsCheckedProperty, CreateBinding("Enabled", SearchFilter));
            ExtractToggleButton.SetBinding(ToggleButton.IsCheckedProperty, CreateBinding("Enabled", SearchExtractor));

            if (Search.Enabled)
            {
                BindSearchToSearchHighlighter();
            }
            else if (SearchFilter.Enabled)
            {
                BindSearchToSearchFilter();
            }
            else if (SearchExtractor.Enabled)
            {
                BindSearchToSearchExtractor();
            }
        }

        private void GetRecentlyOpenedFiles()
        {
            if (string.IsNullOrWhiteSpace(persistingRecentFileName))
            {
                return;
            }

            var fileName = Path.ChangeExtension(persistingRecentFileName, ".json");
            var recentFileInfo = JsonHelper.DeserializeFromFile<RecentFileInfo>(fileName);

            recentFilePathList = recentFileInfo?.RecentFilePaths.ToList() ?? new List<string>();
        }
    }
}