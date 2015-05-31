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
using Sentinel.Extractors.Interfaces;
using Sentinel.Extractors;
using Sentinel.Services.Interfaces;

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
            
            locator.Register<ISessionManager>(new SessionManager());

            // Request that the application close on main window close.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        /// <summary>
        /// Override of the <c>Application.OnExit</c> method.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {          
            base.OnExit(e);
        }
    }
}