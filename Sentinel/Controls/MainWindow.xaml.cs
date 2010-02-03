#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Sentinel.Preferences;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.Controls
{

    #region Using directives

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private PreferencesWindow preferencesWindow;

        public MainWindow()
        {
            InitializeComponent();

            Add = new DelegateCommand(AddNewListener);

            // Append version number to caption (to save effort of producing an about screen)
            Title = string.Format(
                "{0} ({1})",
                Title,
                Assembly.GetExecutingAssembly().GetName().Version);

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            ViewManager = ServiceLocator.Instance.Get<IViewManager>();

            // Maintaining column widths is proving difficult in Xaml alone, so 
            // add an observer here and deal with it in code.
            if (Preferences != null && Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            DataContext = this;

            Exit = new DelegateCommand(e => Close());

            // When a new item is added, select the newest one.
            ViewManager.Viewers.CollectionChanged +=
                (s, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            tabControl.SelectedIndex = tabControl.Items.Count - 1;
                        }
                    };
        }

        public ICommand Add { get; private set; }

        public ICommand Exit { get; private set; }

        public IUserPreferences Preferences { get; private set; }

        public IViewManager ViewManager { get; private set; }

        /// <summary>
        /// AddNewListener method provides a mechanism for the user to add additional
        /// listeners to the log-viewer.
        /// </summary>
        /// <param name="obj">Object to add as a new listener.</param>
        private void AddNewListener(object obj)
        {
            AddNewUdpListenerWindow newWindow = new AddNewUdpListenerWindow();
            newWindow.Owner = this;
            bool? dialogResult = newWindow.ShowDialog();
            if (dialogResult == true)
            {
                Trace.WriteLine(
                    string.Format(
                        "New listener requested - Name {0}, Port {1}{2}",
                        newWindow.LogViewerName,
                        newWindow.Port,
                        newWindow.Enabled ? "- Enabled from start" : string.Empty));

                ViewManager.Create(
                    newWindow.LogViewerName,
                    newWindow.Port,
                    newWindow.Enabled);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Add.Execute(null);
        }

        private void PreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Preferences != null)
            {
                if (e.PropertyName == "Show")
                {
                    if (Preferences.Show)
                    {
                        preferencesWindow = new PreferencesWindow {Owner = this};
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
    }
}