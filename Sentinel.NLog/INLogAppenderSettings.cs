namespace Sentinel.NLog
{
    using Sentinel.Interfaces.Providers;

    public interface INLogAppenderSettings : IProviderSettings
    {
        NetworkProtocol Protocol { get; set; }
        
        int Port { get; set; }
    }
}