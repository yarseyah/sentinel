namespace Sentinel.Upgrader
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Squirrel;
    using WpfExtras;

    public class SquirrelUpgradeService : IUpgradeService, INotifyPropertyChanged
    {
        private string status;

        private bool? isFirstRun;

        private bool? isUpgradeAvailable;

        private bool isReadyForRestart;

        private bool showPanel;

        //// = "https://github.com/yarseyah/sentinel/updates";
        //// private string upgradeLocation = @"..\..\..\Releases";
        private string upgradeLocation = "http://localhost:5000";

        private UpdateInfo availableReleases;

        /// <summary>
        /// Gets or sets the Dispatcher for the UI thread, this is useful because a lot of the
        /// activity here is run in a background thread and is unable to make the UI detect
        /// changes.  Need this dispatcher to force the UI thread to see changes.
        /// </summary>
        public Dispatcher DispatcherUiThread { get; set; }

        public SquirrelUpgradeService()
        {
            HidePanel = new DelegateCommand(o => ShowPanel = false);
            Upgrade = new DelegateCommand(
                a => Task.Run(() => DownloadReleases()),
                o => IsUpgradeAvailable != null && (bool)IsUpgradeAvailable);
            Restart = new DelegateCommand(a => Task.Run(() => RestartApplication()), o => IsReadyForRestart);

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(IsReadyForRestart):
                    case nameof(IsUpgradeAvailable):
                    case nameof(IsFirstRun):
                        DispatcherUiThread?.Invoke(CommandManager.InvalidateRequerySuggested);
                        break;
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand HidePanel { get; private set; }

        public ICommand Upgrade { get; }

        public ICommand Restart { get; }

        public bool? IsFirstRun
        {
            get => isFirstRun;
            private set
            {
                if (isFirstRun != value)
                {
                    isFirstRun = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? IsUpgradeAvailable
        {
            get => isUpgradeAvailable;
            set
            {
                if (isUpgradeAvailable != value)
                {
                    isUpgradeAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsReadyForRestart
        {
            get => isReadyForRestart;
            set
            {
                if (isReadyForRestart != value)
                {
                    isReadyForRestart = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowPanel
        {
            get => showPanel;
            set
            {
                if (showPanel != value)
                {
                    showPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    Trace.WriteLine($"Squirrel upgrade status: {status}");
                    OnPropertyChanged();
                }
            }
        }

        public void CheckForUpgrades()
        {
            try
            {
                Status = "Checking for updates..";

                // For portability, if the upgradeLocation is a relative upgradeLocation, make into
                // an absolute upgradeLocation (this will allow development to avoid being hardcoded
                // to a specific folder).
                upgradeLocation = upgradeLocation.StartsWith("..")
                    ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, upgradeLocation))
                    : upgradeLocation;

                using (var updateManager = new UpdateManager(upgradeLocation))
                {
                    var updateInfo = updateManager.CheckForUpdate(ignoreDeltaUpdates: true)
                                                  .Result;

                    if (updateInfo?.ReleasesToApply?.Any() ?? false)
                    {
                        ShowPanel = true;

                        var installed = updateInfo.CurrentlyInstalledVersion?.Version.ToString() ?? "Unknown version";
                        var available = updateInfo.FutureReleaseEntry?.Version.ToString();
                        var msg = $"New version available (Intalled: {installed}, available: {available})";
                        Status = msg;

                        IsUpgradeAvailable = true;
                        availableReleases = updateInfo;
                    }
                    else
                    {
                        Status = "No updates available";
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Squirrel upgrader had the following errors");
                foreach (var e in aggregateException.InnerExceptions)
                {
                    sb.AppendLine(e.Message);
                }

                var error = sb.ToString();
                Trace.WriteLine(error);
                Status = error;
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Squirrel upgrader had the following errors");
                sb.AppendLine(e.Message);
                var error = sb.ToString();
                Trace.WriteLine(error);
                Status = error;
            }
        }

        public void DownloadReleases()
        {
            if (availableReleases?.ReleasesToApply?.Any() ?? false)
            {
                using (var updateManager = new UpdateManager(upgradeLocation))
                {
                    updateManager.DownloadReleases(
                        availableReleases.ReleasesToApply,
                        i => Status = $"Download progress {i}").Wait();

                    updateManager.ApplyReleases(availableReleases, i => Status = $"Update progress {i}").Wait();

                    var key = updateManager.CreateUninstallerRegistryEntry().Result;

                    Status = "Upgrade installed, please 'Restart' to use upgraded application";
                    Trace.WriteLine(key);

                    IsReadyForRestart = true;
                    IsUpgradeAvailable = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public void RestartApplication()
        {
            UpdateManager.RestartApp("sentinel.exe");
        }

        public string[] ParseCommandLine(string[] commandLineArguments)
        {
            IsFirstRun = commandLineArguments.Any(a => a == "--squirrel-firstrun");
            Trace.WriteLine("SQUIRREL INSTALLER: 'FirstRun'");

            return commandLineArguments.Where(a => a != "--squirrel-firstrun")
                                       .ToArray();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Trace.WriteLine($"OnPropertyChanged: {propertyName}");
        }
    }
}