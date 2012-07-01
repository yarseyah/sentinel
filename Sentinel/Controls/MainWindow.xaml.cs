#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

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
using System.Windows.Input;
using Sentinel.Interfaces;
using Sentinel.Logs.Gui;
using Sentinel.Logs.Interfaces;
using Sentinel.Providers;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using Sentinel.Support;
using Sentinel.Support.Mvvm;
using Sentinel.Views.Gui;
using Sentinel.Views.Interfaces;

#endregion

namespace Sentinel.Controls
{
    using System.Windows.Data;

    using Newtonsoft.Json;

    using Sentinel.Filters;
    using Sentinel.Filters.Interfaces;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private PreferencesWindow preferencesWindow;

        private static ServiceLocator services;

        private readonly string persistingFilename;

        public MainWindow()
        {
            InitializeComponent();
            string savingDirectory = ServiceLocator.Instance.SaveLocation;
            persistingFilename = Path.Combine(savingDirectory, "MainWindow");

            // Restore persisted window placement
            RestoreWindowPosition();
        }

        public ICommand Add { get; private set; }

        public ICommand Exit { get; private set; }

        public IUserPreferences Preferences { get; private set; }

        public IViewManager ViewManager { get; private set; }

        public IFilteringService<IFilter> Filters
        {
            get
            {
                return ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            }
        }

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

        /// <summary>
        /// AddNewListener method provides a mechanism for the user to add additional
        /// listeners to the log-viewer.
        /// </summary>
        /// <param name="obj">Object to add as a new listener.</param>
        private void AddNewListener(object obj)
        {
            var wizard = new NewLoggerWizard();

            if (!wizard.Display(this))
            {
                return;
            }

            var settings = wizard.Settings;

            // Create the logger.
            var logManager = services.Get<ILogManager>();
            ILogger log = logManager.Add(settings.LogName);

            // Create the frame view
            Debug.Assert(
                ViewManager != null,
                "A ViewManager should be registered with service locator for the IViewManager interface");
            var frame = services.Get<IWindowFrame>();
            frame.Log = log;
            frame.SetViews(settings.Views);
            ViewManager.Viewers.Add(frame);

            // Create the providers.
            var providerManager = services.Get<IProviderManager>();
            foreach (var providerRecord in settings.Providers)
            {
                var provider = providerManager.Create(providerRecord.Info.Identifier, providerRecord.Settings);
                provider.Logger = log;
                provider.Start();
            }

            // Add to the tab control.
            var tab = new TabItem { Header = log.Name, Content = frame };
            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Add = new DelegateCommand(AddNewListener, b => tabControl.Items.Count < 1);

            // Append version number to caption (to save effort of producing an about screen)
            Title = string.Format("{0} ({1})", Title, Assembly.GetExecutingAssembly().GetName().Version);

            services = ServiceLocator.Instance;
            Preferences = services.Get<IUserPreferences>();
            ViewManager = services.Get<IViewManager>();

            // Maintaining column widths is proving difficult in Xaml alone, so 
            // add an observer here and deal with it in code.
            if (Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            DataContext = this;

            Exit = new DelegateCommand(ee => Close());

            // When a new item is added, select the newest one.
            ViewManager.Viewers.CollectionChanged += (s, ee) =>
                {
                    if (ee.Action == NotifyCollectionChangedAction.Add)
                    {
                        tabControl.SelectedIndex = tabControl.Items.Count - 1;
                    }
                };

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
        }

        private void ProcessCommandLine(IEnumerable<string> commandLine)
        {
            if (commandLine == null)
            {
                throw new ArgumentNullException("commandLine");
            }

            int port;

            if (commandLine.Count() != 3 || !commandLine.ElementAt(0).IsRegexMatch("nlog|log4net")
                || !commandLine.ElementAt(1).IsRegexMatch("udp|tcp")
                || !Int32.TryParse(commandLine.ElementAt(2), out port))
            {
                // Syntax expected to be "<nlog|log4net> <udp|tcp> <portNumber>"
                MessageBox.Show(
                    this,
                    "Command line arguments should be <nlog|log4net> <udp|tcp> <portNumber>",
                    "Command Line Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                var loggerType = commandLine.ElementAt(0);
                var protocol = commandLine.ElementAt(1);

                if (loggerType == "log4net" && protocol == "tcp")
                {
                    MessageBox.Show(
                        this,
                        "Log4net does not support TCP",
                        "Command Line Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else
                {
                    Trace.WriteLine(
                        string.Format("Requested listener {0}, {1} on port {2}", loggerType, protocol, port));

                    // Create the logger.
                    var logManager = services.Get<ILogManager>();
                    var log = logManager.Add(string.Format("{0} listening on {1} port {2}", loggerType, protocol, port));

                    // Create the frame view
                    Debug.Assert(
                        ViewManager != null,
                        "A ViewManager should be registered with service locator for the IViewManager interface");
                    var frame = services.Get<IWindowFrame>();
                    frame.Log = log;
                    frame.SetViews(new List<string> { LogMessages.Info.Identifier });
                    ViewManager.Viewers.Add(frame);

                    // Create the providers.
                    var providerManager = services.Get<IProviderManager>();
                    IProviderSettings providerSettings = new NetworkSettings { IsUdp = protocol == "udp", Port = port };

                    var provider =
                        providerManager.Create(
                            loggerType == "nlog" ? NLogViewerProvider.Info.Identifier : Log4NetProvider.Info.Identifier,
                            providerSettings);

                    provider.Logger = log;
                    provider.Start();

                    // Add to the tab control.
                    var tab = new TabItem { Header = log.Name, Content = frame };
                    tabControl.Items.Add(tab);
                    tabControl.SelectedItem = tab;
                }
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
                        preferencesWindow = new PreferencesWindow { Owner = this };
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

            e.Accepted = !(e.Item is IStandardDebuggingFilter);
        }
    }
}