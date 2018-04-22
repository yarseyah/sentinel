namespace Sentinel.StartUp
{
    using CommandLine;

    [Verb("nlog", HelpText = "Use nlog listener")]
    public class NLogOptions : IOptions
    {
        public NLogOptions()
        {
            IsUdp = true;
            Port = 9999;
        }

        [Option('t', "tcp", SetName = "protocols")]
        public bool IsTcp
        {
            get => !IsUdp;
            set => IsUdp = !value;
        }

        public int Port { get; set; }

        public bool IsUdp { get; set; }
    }
}