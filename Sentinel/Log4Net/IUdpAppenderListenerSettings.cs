namespace Sentinel.Log4Net
{
    using Sentinel.Interfaces.Providers;

    public interface IUdpAppenderListenerSettings : IProviderSettings
    {
        int Port { get; set; }
    }
}