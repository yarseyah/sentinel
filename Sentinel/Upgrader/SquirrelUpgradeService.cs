using System.IO;

namespace Sentinel.Upgrader
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
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

#if DEBUG_UPGRADE
        // A copy of update.exe is required to be placed in the sentinel\bin folder
        // in order to be able to debug the upgrade process.
        // Use the the file in ./packages/squirrel.windows.n.n.n/tools/squirrel.exe
        // renamed as update.exe
        private string applicationName = "Bin";
#else
        private string applicationName = "Sentinel";
#endif

        //// = "https://github.com/yarseyah/sentinel/updates";
        private string location = @"..\..\..\Releases";

        private ReleaseEntry availableRelease;

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

        public string AvailableRelease
        {
            get => availableRelease?.Version?.ToString();
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

                // For portability, if the location is a relative location, make into
                // an absolute location (this will allow development to avoid being hardcoded
                // to a specific folder).
                location = location.StartsWith("..")
                    ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, location))
                    : location;

                using (var updateManager = new UpdateManager(location))
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
                        availableRelease = updateInfo.FutureReleaseEntry;

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
            if (availableRelease != null)
            {
                using (var updateManager = new UpdateManager(location))
                {
                    var mgr = updateManager;

                    updateManager.UpdateApp(i => Status = $"Update progress {i}")
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