namespace Sentinel.Providers.Interfaces
{
    public interface INetworkProviderSettings : IProviderSettings
    {
        int Port { get; set; }

        bool IsUdp { get; set; }
    }
}