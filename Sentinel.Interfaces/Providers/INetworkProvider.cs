using System.Runtime.Serialization;
namespace Sentinel.Interfaces.Providers
{
    public interface INetworkProvider : ILogProvider
    {
        [DataMember]
        int Port { get; }
    }
}