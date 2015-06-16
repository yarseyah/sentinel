namespace Sentinel
{
    using System.Reflection;
    using System.Windows;
    using CommandLine;

    public class Options
    {
        public Options()
        {
        }

        [VerbOption("nlog", HelpText = "Use log4net listener")]
        public Log4NetOptions Log4NetVerb { get; set; }

        [VerbOption("nlog", HelpText = "Use nlog listener")]
        public NLogOptions NLogVerb { get; set; }
    }

    public class NLogOptions
    {
        public NLogOptions()
        {
            IsUdp = true;
            Port = 9999;
        }

        [Option('u', "udp", MutuallyExclusiveSet = "protocols")]
        public bool IsUdp { get; set; }

        [Option('t', "tcp", MutuallyExclusiveSet = "protocols")]
        public bool IsTcp
        {
            get
            {
                return !IsUdp;
            }

            set
            {
                IsUdp = !IsUdp;
            }
        }

        [Option('p', "port")]
        public int Port { get; set; }
    }

    public class Log4NetOptions
    {
        public Log4NetOptions()
        {
            IsUdp = true;
            Port = 9998;
        }

        [Option('p', "port")]
        public int Port { get; set; }

        [Option('u', "udp", MutuallyExclusiveSet = "protocols")]
        public bool IsUdp { get; set; }
    }
}
