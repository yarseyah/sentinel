#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows;
using Sentinel.Classifying;
using Sentinel.Filters;
using Sentinel.Highlighting;
using Sentinel.Images;
using Sentinel.Logger;
using Sentinel.Networking;
using Sentinel.Preferences;
using Sentinel.Properties;
using Sentinel.Services;

#endregion

namespace Sentinel
{
    /// <summary>
    /// Interaction logic for MainApplication.xaml
    /// </summary>
    public partial class MainApplication : Application
    {
        /// <summary>
        /// Initializes a new instance of the MainApplication class.
        /// </summary>
        public MainApplication()
        {
            Settings.Default.Upgrade();

            ServiceLocator locator = ServiceLocator.Instance;
            locator.ReportErrors = true;

            locator.Load("settings.xml");
            locator.Register(typeof(IViewManager), typeof(ViewManager), false);
            locator.Register(typeof(ILogManager), typeof(LogManager), false);
            locator.Register(typeof(IUserPreferences), typeof(UserPreferences), false);
            locator.Register(typeof(IConnectionsManager), typeof(ConnectionsManager), false);
            locator.Register(typeof(IHighlightingService), typeof(HighlightingService), false);
            locator.Register(typeof(IQuickHighlighter), typeof(QuickHighlighter), false);

            locator.Register<IAddHighlighterService>(new AddNewHighlighterService());
            locator.Register<IEditHighlighterService>(new EditHighlighterService());
            locator.Register<IRemoveHighlighterService>(new RemoveHighlighterService());

            // Filter stuff.
            if (!locator.IsRegistered<IFilteringService>())
            {
                locator.Register<IFilteringService>(new FilteringService());
            }

            locator.Register<IAddFilterService>(new AddFilter());
            locator.Register<IEditFilterService>(new EditFilter());
            locator.Register<IRemoveFilterService>(new RemoveFilter());

            // Image stuff.
            locator.Register(typeof(ITypeImageService), typeof(TypeToImageService), false);
            locator.Register<IAddTypeImageService>(new AddTypeImageService());

            locator.Register<LogWriter>(new LogWriter());

            // Do this last so that other services have registered, e.g. the 
            // TypeImageService is called by some classifiers!
            if (!locator.IsRegistered<IClassifierService>())
            {
                locator.Register<IClassifierService>(new ClassifiersViewModel());
            }

            // Request that the application close on main window close.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        /// <summary>
        /// Override of the <c>Application.OnExit</c> method.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Instance.Save("settings.xml");
            base.OnExit(e);
        }
    }
}