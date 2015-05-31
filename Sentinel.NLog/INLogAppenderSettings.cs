namespace Sentinel.NLog
{
    using Sentinel.Interfaces.Providers;
    using System.Runtime.Serialization;

    public interface INLogAppenderSettings : IProviderSettings
    {
        [DataMember]
        NetworkProtocol Protocol { get; set; }
        
        [DataMember]
        int Port { get; set; }
    }
}