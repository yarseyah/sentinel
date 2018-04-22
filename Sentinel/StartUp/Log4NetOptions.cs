namespace Sentinel.StartUp
{
    using CommandLine;

    [Verb("log4net", HelpText = "Use log4net listener")]
    public class Log4NetOptions : IOptions
    {
        public Log4NetOptions()
        {
            IsUdp = true;
            Port = 9998;
        }

        public int Port { get; set; }

        public bool IsUdp { get; set; }
    }
}