
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

        //// = "https://github.com/yarseyah/sentinel/updates";
        private string location = @"..\..\..\Releases";

        private ReleaseEntry[] availableReleases;

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
                        IsUpgradeAvailable = true;
                        availableReleases = updateInfo.ReleasesToApply.ToArray();
                        Status = $"{availableReleases.Length} update(s) available";
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
            if (availableReleases != null && availableReleases.Any())
            {
                using (var updateManager = new UpdateManager(location))
                {
                    updateManager.DownloadReleases(availableReleases, i => Status = $"Download progress {i}")
                                 .ContinueWith(
                                     t =>
                                         {
                                             if (t.IsFaulted)
                                             {
                                                 Trace.WriteLine("Download failure");
                                                 Status = "Failure checking for downloads";
                                             }
                                             else if (t.IsCompleted)
                                             {
                                                 Trace.WriteLine("Download complete");
                                                 Status = "Checking for downloads complete";
                                             }
                                         });

                    updateManager.UpdateApp(i => Status = $"Update progress {i}")
                                 .ContinueWith(t => Trace.WriteLine("Application updated"));
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