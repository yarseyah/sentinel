namespace Sentinel.Providers.Interfaces
{
    public interface IProviderSettings
    {
        string Name { get; }

        string Summary { get; }

        /// <summary>
        /// Reference back to the provider this setting is appropriate to.
        /// </summary>
        IProviderInfo Info { get; }
    }
}