namespace Sentinel.NLog
{
    using System.Runtime.Serialization;

    using Sentinel.Interfaces.Providers;

    public interface INLogAppenderSettings : IProviderSettings
    {
        [DataMember]
        NetworkProtocol Protocol { get; set; }

        [DataMember]
        int Port { get; set; }
    }
}