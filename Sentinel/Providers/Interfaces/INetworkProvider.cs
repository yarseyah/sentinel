namespace Sentinel.Providers.Interfaces
{
    public interface INetworkProvider : ILogProvider
    {
        int Port { get; }
    }
}