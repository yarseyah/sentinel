namespace Sentinel.StartUp
{
    using CommandLine;

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