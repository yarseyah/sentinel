#define GITHUB_RELEASE

namespace Sentinel.Upgrader
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;

    using Sentinel.Services;

    using Squirrel;
    using WpfExtras;

    public class SquirrelUpgradeService : IUpgradeService, INotifyPropertyChanged
    {
        private string status;

        private bool? isFirstRun;

        private bool? isUpgradeAvailable;

        private bool isReadyForRestart;

        private bool showPanel;

        private bool userInhibitUpdateCheck = false;

        private UpdateInfo availableReleases;

        private Dispatcher dispatcherUiThread;

        private IUpgradeServicePreferences preferences;

        private string error;

        public SquirrelUpgradeService()
        {
            HidePanel = new DelegateCommand(o => ShowPanel = false);
            Upgrade = new DelegateCommand(
                a => Task.Run(() => DownloadReleases()),
                o => IsUpgradeAvailable != null && (bool)IsUpgradeAvailable);
            Restart = new DelegateCommand(a => Task.Run(() => RestartApplication()), o => IsReadyForRestart);

            bool showErrors = true;

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(IsReadyForRestart):
                    case nameof(IsUpgradeAvailable):
                    case nameof(IsFirstRun):
                        DispatcherUiThread?.Invoke(CommandManager.InvalidateRequerySuggested);
                        break;
                    case nameof(DispatcherUiThread):
                        if (!preferences?.IsDisabled ?? false)
                        {
                            // Set up an upgrade check to take place after 'CheckForUpgradesPeriod' seconds
                            Task.Delay(preferences?.DelayBeforeCheckingForUpgrades ?? TimeSpan.Zero)
                                .ContinueWith(t => CheckForUpgrades());
                        }

                        break;
                    case nameof(Error):
                        if (showErrors && !string.IsNullOrWhiteSpace(Error))
                        {
                            ShowPanel = true;
                        }

                        break;
                }
            };

            // Load preferences
            preferences = ServiceLocator.Instance.Get<IUpgradeServicePreferences>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the Dispatcher for the UI thread, this is useful because a lot of the
        /// activity here is run in a background thread and is unable to make the UI detect
        /// changes.  Need this dispatcher to force the UI thread to see changes.
        /// </summary>
        public Dispatcher DispatcherUiThread
        {
            get => dispatcherUiThread;
            set
            {
                if (dispatcherUiThread != value)
                {
                    dispatcherUiThread = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan CheckForUpgradesPeriod => TimeSpan.FromSeconds(10);

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

        public string Error
        {
            get => error;
            set
            {
                if (error != value)
                {
                    error = value;
                    Trace.WriteLine($"Squirrel upgrade error: {status}");
                    OnPropertyChanged();
                }
            }
        }

        public void CheckForUpgrades()
        {
            if (!userInhibitUpdateCheck)
            {
                try
                {
                    Status = "Checking for updates..";

                    using (var updateManager = UpdateManager())
                    {
                        var updateInfo = updateManager.CheckForUpdate().Result;

                        if (updateInfo?.ReleasesToApply?.Any() ?? false)
                        {
                            ShowPanel = true;

                            var installed = updateInfo.CurrentlyInstalledVersion?.Version.ToString()
                                            ?? "Unknown version";
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
                catch (AggregateException ae) when (ae.InnerExceptions?.All(e => e is WebException) ?? false)
                {
                    Error = "Unable to reach upgrade server";
                    Status = null;
                    IsUpgradeAvailable = false;
                }
                catch (AggregateException aggregateException)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Squirrel upgrader had the following errors");
                    foreach (var e in aggregateException.InnerExceptions)
                    {
                        sb.AppendLine(e.Message);
                    }

                    var message = sb.ToString();
                    Trace.WriteLine(message);
                    Status = null;
                    Error = message;
                }
                catch (Exception e)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Squirrel upgrader had the following errors");
                    sb.AppendLine(e.Message);

                    var message = sb.ToString();
                    Trace.WriteLine(message);
                    Status = null;
                    Error = message;
                }
            }
        }

        public void DownloadReleases()
        {
            if (availableReleases?.ReleasesToApply?.Any() ?? false)
            {
                using (var updateManager = UpdateManager())
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
            if (!userInhibitUpdateCheck)
            {
                Squirrel.UpdateManager.RestartApp("sentinel.exe");
            }
        }

        public string[] ParseCommandLine(string[] commandLineArguments)
        {
            var specialDirectives = new[]
            {
                "--squirrel-firstrun",
                "--inhibit-upgrade",
            };

            IsFirstRun = commandLineArguments.Any(a => a == specialDirectives[0]);
            Trace.WriteLine("SQUIRREL INSTALLER: 'FirstRun'");

            userInhibitUpdateCheck = commandLineArguments.Any(a => a == specialDirectives[1]);
            Trace.WriteLine($"SQUIRREL INSTALLER: Upgrade check {(userInhibitUpdateCheck ? "dis" : "en")}abled");

            return commandLineArguments.Where(a => !specialDirectives.Contains(a))
                                       .ToArray();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Trace.WriteLine($"OnPropertyChanged: {propertyName}");
        }

        private string GetUpgradeLocation()
        {
            var upgradeLocation = preferences?.UpgradeRepository ?? string.Empty;

            // For portability, if the upgradeLocation is a relative upgradeLocation, make into
            // an absolute upgradeLocation (this will allow development to avoid being hardcoded
            // to a specific folder).
            upgradeLocation = upgradeLocation.StartsWith("..")
                                  ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, upgradeLocation))
                                  : upgradeLocation;
            return upgradeLocation;
        }

        private UpdateManager UpdateManager()
        {
#if GITHUB_RELEASE
            return Squirrel.UpdateManager.GitHubUpdateManager("https://github.com/yarseyah/sentinel").Result;
#else
            return new UpdateManager(GetUpgradeLocation());
#endif
        }
    }
}