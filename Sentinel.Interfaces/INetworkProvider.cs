namespace Sentinel.Interfaces
{
    public interface INetworkProvider : ILogProvider
    {
        int Port { get; }
    }
}