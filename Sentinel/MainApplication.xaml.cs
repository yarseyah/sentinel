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
using Sentinel.Classification;
using Sentinel.Classification.Interfaces;
using Sentinel.Filters;
using Sentinel.Filters.Interfaces;
using Sentinel.Highlighters;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Images;
using Sentinel.Images.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logger;
using Sentinel.Logs;
using Sentinel.Logs.Interfaces;
using Sentinel.Preferences;
using Sentinel.Properties;
using Sentinel.Providers;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using Sentinel.Views;
using Sentinel.Views.Gui;
using Sentinel.Views.Interfaces;

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

            locator.RegisterOrLoad<FilteringService>(typeof(IFilteringService), "Filters");

            // TODO: this is being phased out...
            locator.Load("settings.xml");

            locator.Register(typeof(IUserPreferences), typeof(UserPreferences), false);
            locator.Register(typeof(IHighlighterStyle), typeof(HighlighterStyle), false);
            locator.Register(typeof(ITypeImageService), typeof(TypeToImageService), false);
            locator.Register(typeof(IHighlightingService), typeof(HighlightingService), false);
            locator.Register(typeof(IQuickHighlighter), typeof(QuickHighlighter), false);
            locator.Register<ILogManager>(new LogManager());
            locator.Register<LogWriter>(new LogWriter());
            locator.Register(typeof(IViewManager), typeof(ViewManager), false);
            locator.Register<IProviderManager>(new ProviderManager());
            locator.Register<IWindowFrame>(new MultipleViewFrame());

            locator.Register<INewProviderWizard>(new NewProviderWizard());

            // Do this last so that other services have registered, e.g. the 
            // TypeImageService is called by some classifiers!);););
            if (!locator.IsRegistered<IClassifierService>())
            {
                locator.Register<IClassifierService>(new Classifiers());
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
            ServiceLocator.Instance.Save();
            base.OnExit(e);
        }
    }
}