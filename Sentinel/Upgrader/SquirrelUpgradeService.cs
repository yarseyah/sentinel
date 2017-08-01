namespace Sentinel.Upgrader
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Input;

    using Squirrel;

    using WpfExtras;

    public class SquirrelUpgradeService : IUpgradeService, INotifyPropertyChanged
    {
        private bool? isFirstRun;

        private string status;

        private bool? isUpgradeAvailable;

        private bool showPanel = true;

        //// = "https://github.com/yarseyah/sentinel/updates";
        //// private string upgradeLocation = @"..\..\..\Releases";
        private string upgradeLocation = "http://localhost:5000";

        private UpdateInfo availableReleases;

        public SquirrelUpgradeService()
        {
            HidePanel = new DelegateCommand(o => ShowPanel = false);
            Upgrade = new DelegateCommand(
                a => DownloadReleases(),
                o => IsUpgradeAvailable != null && (bool)IsUpgradeAvailable);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand HidePanel { get; private set; }

        public ICommand Upgrade { get; }

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

        public UpdateInfo AvailableReleases { get; private set; }

        public string AvailableRelease
        {
            get
            {
                return AvailableReleases?.ReleasesToApply?.OrderByDescending(r => r.Version)
                    .FirstOrDefault()?.Version?.ToString();
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
                        var msg = "New version available!\n\n"
                                  + $"Current version: {updateInfo.CurrentlyInstalledVersion?.Version}\n"
                                  + $"New version: {updateInfo.FutureReleaseEntry?.Version}";
                        Status = msg;

                        IsUpgradeAvailable = true;
                        AvailableReleases = updateInfo;

                        // ReSharper disable once ExplicitCallerInfoArgument
                        OnPropertyChanged(nameof(AvailableRelease));

                        CommandManager.InvalidateRequerySuggested();
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
            if (AvailableReleases?.ReleasesToApply?.Any() ?? false)
            {
                using (var updateManager = new UpdateManager(upgradeLocation))
                {
                    var mgr = updateManager;

                    updateManager.DownloadReleases(
                            AvailableReleases.ReleasesToApply,
                            i => Status = $"Download progress {i}")
                        .ContinueWith(t2 =>
                            mgr.ApplyReleases(AvailableReleases, i => Status = $"Update progress {i}"))
                        .ContinueWith(t => mgr.CreateUninstallerRegistryEntry())
                        .ContinueWith(
                            t =>
                            {
                                var x = mgr;
                                if (t.IsCompleted)
                                {
                                    UpdateManager.RestartApp("sentinel.exe");
                                }
                                else
                                {
                                    Status = t.Status.ToString();
                                }
                            });
                }
            }
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
        }
    }
}