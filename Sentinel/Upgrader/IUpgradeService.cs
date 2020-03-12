namespace Sentinel.Upgrader
{
    using System.Windows.Threading;

    public interface IUpgradeService
    {
        /// <summary>
        /// Gets whether the currently running instance the first run of a newly installed instance.
        /// </summary>
        /// <returns>True/False if it knows, Null if not yet known/unable to determine.</returns>
        bool? IsFirstRun { get; }

        bool? IsUpgradeAvailable { get; }

        Dispatcher DispatcherUiThread { get; set; }

        /// <summary>
        /// Allow the upgrade service to review (and alter) the supplied command line arguments.
        /// Useful if upgrade bootstrapper passes some well known arguments.
        /// </summary>
        /// <param name="commandLine">Supplied command line.</param>
        /// <returns>Procesed/Cleaned command line.</returns>
        string[] ParseCommandLine(string[] commandLine);

        void CheckForUpgrades();
    }
}