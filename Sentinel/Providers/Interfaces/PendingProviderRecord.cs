namespace Sentinel.Providers.Interfaces
{
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    using IProviderInfo = Sentinel.Interfaces.Providers.IProviderInfo;

    public class PendingProviderRecord
    {
        public IProviderInfo Info { get; set; }
        public IProviderSettings Settings { get; set; }
    }
}