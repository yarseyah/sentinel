namespace Sentinel.Providers.Interfaces
{
    public class PendingProviderRecord
    {
        public ProviderInfo Info { get; set; }
        public IProviderSettings Settings { get; set; }
    }
}