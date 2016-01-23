namespace Sentinel.Providers
{
    using Sentinel.Interfaces.Providers;

    public class ProviderSettings : IProviderSettings
    {
        public string Name { get; set; }

        public virtual string Summary => $"Provider named {Name}";

        /// <summary>
        /// Gets or sets reference back to the provider this setting is appropriate to.
        /// </summary>
        public IProviderInfo Info { get; set; }
    }
}