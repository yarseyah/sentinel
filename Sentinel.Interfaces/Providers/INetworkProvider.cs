namespace Sentinel.Interfaces.Providers
{
    public interface INetworkProvider : ILogProvider
    {
        int Port { get; }
    }
}