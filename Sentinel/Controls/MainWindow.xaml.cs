#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using Common.Logging;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;
    using Sentinel.Log4Net;
    using Sentinel.Logs.Gui;
    using Sentinel.Logs.Interfaces;
    using Sentinel.NLog;
    using Sentinel.Providers;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;
    using Sentinel.Support;
    using Sentinel.Support.Mvvm;
    using Sentinel.Views.Gui;
    using Sentinel.Views.Interfaces;
    using System.Windows.Controls.Ribbon;
    using Sentinel.Extractors.Interfaces;
    using Microsoft.Win32;
    using Sentinel.Services.Interfaces;
    using System.Collections.ObjectModel;
    using Sentinel.Classification.Interfaces;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        private PreferencesWindow preferencesWindow;

        private int preferencesWindowTabSelected;

        private readonly string persistingFilename;

        private readonly string persistingRecentFileName;

        private List<string> _recentFilePathList;

        public MainWindow()
        {
            InitializeComponent();
            var savingDirectory = ServiceLocator.Instance.SaveLocation;
            persistingFilename = Path.Combine(savingDirectory, "MainWindow");
            persistingRecentFileName = Path.Combine(savingDirectory, "RecentFiles");

            // Restore persisted window placement
            RestoreWindowPosition();
            // Get recently opened files
            GetRecentlyOpenedFiles();
        }

        private void GetRecentlyOpenedFiles()
        {
            if (string.IsNullOrWhiteSpace(persistingRecentFileName))
            {
                return;
            }

            var fileName = Path.ChangeExtension(persistingRecentFileName, ".json");
            var recentFileInfo = JsonHelper.DeserializeFromFile<RecentFileInfo>(fileName);

            if (recentFileInfo != null)
            {
                _recentFilePathList = recentFileInfo.RecentFilePaths.ToList();
            }
            else
            {
                _recentFilePathList = new List<string>();
            }
        }

        public ICommand Add { get; private set; }

        public ICommand About { get; private set; }

        public ICommand ShowPreferences { get; private set; }

        public ICommand ExportLogs { get; private set; }

        public ICommand Exit { get; private set; }

        public ICommand NewSession { get; private set; }

        public ICommand SaveSession { get; private set; }

        public ICommand LoadSession { get; private set; }

        public IUserPreferences Preferences { get; private set; }

        public IViewManager ViewManager { get; private set; }

        public IFilteringService<IFilter> Filters
        {
            get
            {
                return ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            }
        }

        public IHighlightingService<IHighlighter> Highlighters
        {
            get
            {
                return ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
            }
        }

        public IClassifyingService<IClassifier> ClassifyingService
        {
            get
            {
                return ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();
            }
        }
        
        public IExtractingService<IExtractor> Extractors
        {
            get
            {
                return ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
            }
        }

        public ISearchHighlighter Search
        {
            get
            {
                return ServiceLocator.Instance.Get<ISearchHighlighter>();
            }
        }

        public ISearchFilter SearchFilter
        {
            get
            {
                return ServiceLocator.Instance.Get<ISearchFilter>();
            }
        }

        public ISearchExtractor SearchExtractor
        {
            get
            {
                return ServiceLocator.Instance.Get<ISearchExtractor>();
            }
        }

        public ObservableCollection<string> RecentFiles { get; private set; }

        private static WindowPlacementInfo ValidateScreenPosition(WindowPlacementInfo wp)
        {
            if (wp != null)
            {
                var virtualScreen = new Rect(
                    SystemParameters.VirtualScreenLeft,
                    SystemParameters.VirtualScreenTop,
                    SystemParameters.VirtualScreenWidth,
                    SystemParameters.VirtualScreenHeight);
                var window = new Rect(wp.Left, wp.Top, wp.Width, wp.Height);
                return virtualScreen.IntersectsWith(window) ? wp : null;
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
            //Get Log
            TabItem tab = (TabItem)tabControl.SelectedItem;
            IWindowFrame frame = (IWindowFrame)tab.Content;
            bool restartLogging = false;
            //Notify user that log messages will be paused during this operation
            if (frame.Log.Enabled)
            {
                if (MessageBox.Show("The log viewer must be paused momentarily for this operation to continue. Is it OK to pause logging?", "Sentinel", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    frame.Log.Enabled = false;
                    restartLogging = true;
                }
                else
                {
                    return;
                }
            }

            //open a save file dialog
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = frame.Log.Name;
            savefile.DefaultExt = ".log";
            savefile.Filter = "Log documents (.log)|*.log|Text documents (.txt)|*.txt";
            savefile.FilterIndex = 0;

            var result = savefile.ShowDialog(this);
            if (result == true)
            {
                var logFileExporter = ServiceLocator.Instance.Get<ILogFileExporter>();
                logFileExporter.SaveLogViewerToFile(frame, savefile.FileName);
            }

            frame.Log.Enabled = restartLogging;
        }

        private void SaveSessionAction(object obj)
        {
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            //open a save file dialog
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = sessionManager.Name;
            savefile.DefaultExt = ".sntl";
            savefile.Filter = "Sentinel session (.sntl)|*.sntl";
            savefile.FilterIndex = 0;

            var result = savefile.ShowDialog(this);
            if (result == true)
            {                
                sessionManager.SaveSession(savefile.FileName);
                AddToRecentFiles(savefile.FileName);
            }
        }

        private void AddToRecentFiles(string fileName)
        {
            if (RecentFiles.Contains(fileName)) RecentFiles.Move(RecentFiles.IndexOf(fileName), 0);
            else RecentFiles.Insert(0, fileName);
            if (RecentFiles.Count > 13) RecentFiles.Remove(RecentFiles.LastOrDefault());
        }

        private void NewSessionAction(object obj)
        {
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            if (!sessionManager.IsSaved)
            {
                var userResult = MessageBox.Show("Do you want to save changes you made to " + sessionManager.Name + "?", "Sentinel", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (userResult == MessageBoxResult.Cancel) return;
                else if (userResult == MessageBoxResult.Yes)
                {
                    SaveSession.Execute(null);
                    
                    // if the user clicked "Cancel" at the save dialog box
                    if (!sessionManager.IsSaved) return;
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
                var userResult = MessageBox.Show("Do you want to save changes you made to " + sessionManager.Name + "?", "Sentinel", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (userResult == MessageBoxResult.Cancel) return;
                else if (userResult == MessageBoxResult.Yes)
                {
                    SaveSession.Execute(null);

                    // if the user clicked "Cancel" at the save dialog box
                    if (!sessionManager.IsSaved) return;
                }
            }

            if (fileNameToLoad == null)
            {
                // Open file dialog
                //open a save file dialog
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.FileName = sessionManager.Name;
                openFile.DefaultExt = ".sntl";
                openFile.Filter = "Sentinel session (.sntl)|*.sntl";
                openFile.FilterIndex = 0;

                var result = openFile.ShowDialog(this);
                if (result == true) fileNameToLoad = openFile.FileName;
                else return;
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

                if (sessionManager.ProviderSettings.Count() == 0) return;

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
            //Load a new session
            var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

            RemoveBindingReferences();

            sessionManager.LoadNewSession(this);

            BindViewToViewModel();

            if (sessionManager.ProviderSettings.Count() == 0) return;

            var frame = ServiceLocator.Instance.Get<IWindowFrame>();

            // Add to the tab control.
            var tab = new TabItem { Header = sessionManager.Name, Content = frame };
            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Exit = new DelegateCommand(ee => Close());
            About = new DelegateCommand(ee =>
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
            RecentFiles = new ObservableCollection<string>(_recentFilePathList.Take(13));

            BindViewToViewModel();

            // Determine whether anything passed on the command line, limited options
            // may be supplied and they will suppress the prompting of the new listener wizard.
            var commandLine = Environment.GetCommandLineArgs();
            if (commandLine.Count() == 1)
            {
                Add.Execute(null);
            }
            else
            {
                ProcessCommandLine(commandLine.Skip(1));
            }

            // Debug the available loggers.
            var logManager = ServiceLocator.Instance.Get<ILogManager>();
            foreach (var logger in logManager)
            {
                log.DebugFormat("Log: {0}", logger.Name);
            }

            var providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            foreach (var instance in providerManager.GetInstances())
            {
                log.DebugFormat("Provider: {0}", instance.Name);
                log.DebugFormat("   - is {0}active", instance.IsActive ? string.Empty : "not ");
                log.DebugFormat("   - logger = {0}", instance.Logger);
            }            
        }

        private void ProcessCommandLine(IEnumerable<string> commandLine)
        {
            var filePath = commandLine.FirstOrDefault();
            if (System.IO.File.Exists(filePath) && Path.GetExtension(filePath).ToUpper() == ".SNTL")
            {
                var sessionManager = ServiceLocator.Instance.Get<ISessionManager>();

                RemoveBindingReferences();

                sessionManager.LoadSession(filePath);

                BindViewToViewModel();

                if (sessionManager.ProviderSettings.Count() == 0) return;

                var frame = ServiceLocator.Instance.Get<IWindowFrame>();

                // Add to the tab control.
                var newTab = new TabItem { Header = sessionManager.Name, Content = frame };
                tabControl.Items.Add(newTab);
                tabControl.SelectedItem = newTab;
            }
            else
            {
                MessageBox.Show("File does not exist or is not a Sentinel session file.", "Sentinel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreWindowPosition()
        {
            if (string.IsNullOrWhiteSpace(persistingFilename))
            {
                return;
            }

            var fileName = Path.ChangeExtension(persistingFilename, ".json");
            var wp = JsonHelper.DeserializeFromFile<WindowPlacementInfo>(fileName);

            if (wp != null)
            {
                wp = ValidateScreenPosition(wp);

                Top = wp.Top;
                Left = wp.Left;
                Width = wp.Width;
                Height = wp.Height;
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
                    WindowState = WindowState
                };

            var filename = Path.ChangeExtension(persistingFilename, ".json");
            JsonHelper.SerializeToFile(windowInfo, filename);

            var recentFileInfo = new RecentFileInfo
            {
                RecentFilePaths = RecentFiles.ToList(),
            };
            JsonHelper.SerializeToFile(recentFileInfo, Path.ChangeExtension(persistingRecentFileName, ".json"));
        }

        private void RetainOnlyStandardFilters(object sender, FilterEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            e.Accepted = e.Item is IStandardDebuggingFilter;
        }

        private void ExcludeStandardFilters(object sender, FilterEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            e.Accepted = !(e.Item is IStandardDebuggingFilter || e.Item is ISearchFilter);
        }

        private void RetainOnlyStandardHighlighters(object sender, FilterEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentException("e");
            }
            var type = e.Item.GetType();
            e.Accepted = e.Item is IStandardDebuggingHighlighter;
        }

        private void ExcludeStandardHighlighters(object sender, FilterEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentException("e");
            }

            e.Accepted = !(e.Item is IStandardDebuggingHighlighter);
        }

        private void SearchToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender.GetType() == typeof(RibbonToggleButton), string.Format("A {0} accessed the wrong method", sender.GetType()));

            var button = sender as RibbonToggleButton;

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
                default:
                    break;
            }
        }

        private void BindSearchToSearchExtractor()
        {
            SearchRibbonTextBox.SetBinding(RibbonTextBox.TextProperty, new Binding()
            {
                Source = SearchExtractor,
                Path = new PropertyPath("Pattern"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchModeListBox.SetBinding(ListBox.SelectedItemProperty, new Binding()
            {
                Source = SearchExtractor,
                Path = new PropertyPath("Mode"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchTargetComboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding()
            {
                Source = SearchExtractor,
                Path = new PropertyPath("Field"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            HighlightToggleButton.IsChecked = false;
            FilterToggleButton.IsChecked = false;
        }

        private void BindSearchToSearchFilter()
        {
            SearchRibbonTextBox.SetBinding(RibbonTextBox.TextProperty, new Binding()
            {
                Source = SearchFilter,
                Path = new PropertyPath("Pattern"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchModeListBox.SetBinding(ListBox.SelectedItemProperty, new Binding()
            {
                Source = SearchFilter,
                Path = new PropertyPath("Mode"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchTargetComboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding()
            {
                Source = SearchFilter,
                Path = new PropertyPath("Field"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            HighlightToggleButton.IsChecked = false;
            ExtractToggleButton.IsChecked = false;
        }

        private void BindSearchToSearchHighlighter()
        {
            SearchRibbonTextBox.SetBinding(RibbonTextBox.TextProperty, new Binding()
            {
                Source = Search,
                Path = new PropertyPath("Search"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchModeListBox.SetBinding(ListBox.SelectedItemProperty, new Binding()
            {
                Source = Search,
                Path = new PropertyPath("Mode"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            SearchTargetComboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding()
            {
                Source = Search,
                Path = new PropertyPath("Field"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            FilterToggleButton.IsChecked = false;
            ExtractToggleButton.IsChecked = false;
        }

        private void RemoveBindingReferences()
        {
            (Preferences as INotifyPropertyChanged).PropertyChanged -= PreferencesChanged;
            ViewManager.Viewers.CollectionChanged -= ViewManagerChanged;
        }

        private void BindViewToViewModel()
        {
            // Append version number to caption (to save effort of producing an about screen)
            Title = string.Format("{0} ({1}) {2}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version, ServiceLocator.Instance.Get<ISessionManager>().Name);
            
            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            ViewManager = ServiceLocator.Instance.Get<IViewManager>();

            // Maintaining column widths is proving difficult in Xaml alone, so 
            // add an observer here and deal with it in code.
            if (Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            DataContext = this;

            // When a new item is added, select the newest one.
            ViewManager.Viewers.CollectionChanged += ViewManagerChanged;         

            //View-specific bindings
            var collapseIfZero = new WpfExtras.Converters.CollapseIfZeroConverter();

            var standardHighlighters = new CollectionViewSource() { Source = Highlighters.Highlighters };
            standardHighlighters.View.Filter = c =>
            {
                if (c is IStandardDebuggingHighlighter) return true;
                return false;
            };
                        
            var customHighlighters = new CollectionViewSource() { Source = Highlighters.Highlighters };
            customHighlighters.View.Filter = c =>
            {
                if (c is IStandardDebuggingHighlighter) return false;
                return true;
            };

            StandardHighlightersRibbonGroup.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = standardHighlighters
            });

            StandardHighlighterRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = standardHighlighters
            });
            StandardHighlighterRibbonGroupOnTab.SetBinding(RibbonGroup.VisibilityProperty, new Binding()
            {
                Source = standardHighlighters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero
            });
            CustomHighlighterRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = customHighlighters
            });
            CustomHighlighterRibbonGroupOnTab.SetBinding(RibbonGroup.VisibilityProperty, new Binding()
            {
                Source = customHighlighters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero
            });

            var standardFilters = new CollectionViewSource() { Source = Filters.Filters };
            standardFilters.View.Filter = c =>
            {
                if (c is IStandardDebuggingFilter) return true;
                return false;
            };
            var customFilters = new CollectionViewSource() { Source = Filters.Filters };
            customFilters.View.Filter = c =>
            {
                if (c is IStandardDebuggingFilter) return false;
                return true;
            };

            StandardFiltersRibbonGroup.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = standardFilters               
            });
            
            StandardFiltersRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = standardFilters
            });
            
            StandardFiltersRibbonGroupOnTab.SetBinding(RibbonGroup.VisibilityProperty, new Binding()
            {
                Source = standardFilters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero
            });
            CustomFiltersRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = customFilters
            });
            CustomFiltersRibbonGroupOnTab.SetBinding(RibbonGroup.VisibilityProperty, new Binding()
            {
                Source = customFilters,
                Path = new PropertyPath("Count"),
                Converter = collapseIfZero
            });

            var customExtractors = Extractors.Extractors;
            CustomExtractorsRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = customExtractors
            });

            var customClassifyiers = ClassifyingService.Classifiers;
            CustomClassifiersRibbonGroupOnTab.SetBinding(RibbonGroup.ItemsSourceProperty, new Binding()
            {
                Source = customClassifyiers
            });

            //Bind search
            HighlightToggleButton.SetBinding(RibbonToggleButton.IsCheckedProperty, new Binding()
            {
                Source = Search,
                Path = new PropertyPath("Enabled"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            FilterToggleButton.SetBinding(RibbonToggleButton.IsCheckedProperty, new Binding()
            {
                Source = SearchFilter,
                Path = new PropertyPath("Enabled"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            ExtractToggleButton.SetBinding(RibbonToggleButton.IsCheckedProperty, new Binding()
            {
                Source = SearchExtractor,
                Path = new PropertyPath("Enabled"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            if (Search.Enabled) BindSearchToSearchHighlighter();
            else if (SearchFilter.Enabled) BindSearchToSearchFilter();
            else if (SearchExtractor.Enabled) BindSearchToSearchExtractor();

            //Column view buttons
            ExceptionRibbonToggleButton.SetBinding(RibbonToggleButton.IsCheckedProperty, new Binding()
            {
                Source = Preferences,
                 Path = new PropertyPath("ShowExceptionColumn")
            });
            ThreadRibbonToggleButton.SetBinding(RibbonToggleButton.IsCheckedProperty, new Binding()
            {
                Source = Preferences,
                Path = new PropertyPath("ShowThreadColumn")
            });
        }       
    }
}
