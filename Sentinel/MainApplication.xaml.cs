namespace Sentinel
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Windows;

    using Sentinel.Properties;
    using Sentinel.Services;
    using Sentinel.Services.Interfaces;

    /// <summary>
    /// Interaction logic for MainApplication.xaml.
    /// </summary>
    public partial class MainApplication : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainApplication"/> class.
        /// </summary>
        public MainApplication()
        {
            ////AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;
            Settings.Default.Upgrade();

            ServiceLocator locator = ServiceLocator.Instance;
            locator.ReportErrors = true;
            locator.Register<ISessionManager>(new SessionManager());

            // Request that the application close on main window close.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        ////private void FirstChanceExceptionHandler(object sender, FirstChanceExceptionEventArgs e)
        ////{
        ////    if (e.Exception is SocketException)
        ////    {
        ////        return;
        ////    }

        ////    var source = e.Exception.Source?.ToLower();
        ////    if (source == "mscorlib" || source == "squirrel")
        ////    {
        ////        return;
        ////    }

        ////    var sb = new StringBuilder();
        ////    sb.AppendLine($"Sender: {sender} FirstChanceException raised in {AppDomain.CurrentDomain.FriendlyName}");
        ////    sb.AppendLine($"Message - {e.Exception.Message}");
        ////    sb.AppendLine($"InnerException -- {e.Exception?.InnerException?.Message ?? string.Empty}");
        ////    sb.AppendLine($"TargetSite - {e.Exception?.TargetSite?.Name ?? string.Empty}");
        ////    sb.AppendLine($"StackTrace - {e.Exception?.StackTrace ?? string.Empty}");
        ////    sb.AppendLine($"HelpLink -- {e.Exception?.HelpLink ?? string.Empty} ");

        ////    MessageBox.Show(
        ////        sb.ToString(),
        ////        "Error " + e.Exception.GetType(),
        ////        MessageBoxButton.OK,
        ////        MessageBoxImage.Error,
        ////        MessageBoxResult.OK);
        ////}
    }
}