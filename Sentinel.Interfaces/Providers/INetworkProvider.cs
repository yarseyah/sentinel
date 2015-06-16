namespace Sentinel.Interfaces.Providers
{
    using System.Runtime.Serialization;

    public interface INetworkProvider : ILogProvider
    {
        [DataMember]
        int Port { get; }
    }
}