namespace Sentinel.Providers.Interfaces
{
    using Sentinel.Interfaces.Providers;

    public class PendingProviderRecord
    {
        public IProviderInfo Info { get; set; }

        public IProviderSettings Settings { get; set; }
    }
}