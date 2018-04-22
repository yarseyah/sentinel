namespace Sentinel.StartUp
{
    using CommandLine;

    public interface IOptions
    {
        [Option('p', "port")]
        int Port { get; set; }

        [Option('u', "udp", SetName = "protocols")]
        bool IsUdp { get; set; }
    }
}