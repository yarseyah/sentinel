namespace Sentinel.StartUp
{
    using CommandLine;

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
}