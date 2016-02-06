namespace Sentinel.EventLogMonitor
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option('b',"no-banner",DefaultValue = false,HelpText = "Hide the copyright banner shown on application startup")]
        public bool SuppressBanner { get; set; }
    }
}