namespace Sentinel.Providers.Interfaces
{
    public class PendingProviderRecord
    {
        public IProviderInfo Info { get; set; }
        public IProviderSettings Settings { get; set; }
    }
}