namespace Sentinel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Squirrel;

    public class Upgrader
    {
        public static IEnumerable<ReleaseEntry> CheckForUpgrades()
        {
            try
            {
                var location = @"C:\Users\Ray\Development\sentinel\Releases";
                //// var location = "https://github.com/yarseyah/sentinel/updates";

                using (var updateManager = new UpdateManager(location))
                {
                    var updateInfo = updateManager.CheckForUpdate(ignoreDeltaUpdates: true)
                                                  .Result;

                    if (updateInfo?.ReleasesToApply?.Any() ?? false)
                    {
                        updateManager.DownloadReleases(updateInfo.ReleasesToApply)
                                     .ContinueWith(
                                         t =>
                                             {
                                                 if (t.IsFaulted)
                                                 {
                                                     Trace.WriteLine("Download failure");
                                                 }
                                                 else if (t.IsCompleted)
                                                 {
                                                     Trace.WriteLine("Download complete");
                                                 }
                                             });

                        updateManager.UpdateApp()
                                     .ContinueWith(t => Trace.WriteLine("Application updated"));

                        return updateInfo.ReleasesToApply;
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                Trace.WriteLine("Squirrel upgrader had the following errors");
                foreach (var e in aggregateException.InnerExceptions)
                {
                    Trace.WriteLine(e.Message);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Squirrel upgrader had the following error");
                Trace.WriteLine(e.Message);
            }

            return Enumerable.Empty<ReleaseEntry>();
        }

        public static string[] ParseCommandLine(string[] commandLineArguments)
        {
            FirstRun = commandLineArguments.Any(a => a == "--squirrel-firstrun");
            Trace.WriteLine("SQUIRREL INSTALLER: 'FirstRun'");

            return commandLineArguments.Where(a => a != "--squirrel-firstrun")
                                       .ToArray();
        }

        public static bool FirstRun { get; private set; }
    }
}