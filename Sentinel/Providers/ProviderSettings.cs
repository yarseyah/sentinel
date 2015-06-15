namespace Sentinel.Providers
{
    using Sentinel.Interfaces.Providers;

    public class ProviderSettings : IProviderSettings
    {
        public string Name { get; set; }

        public virtual string Summary
        {
            get
            {
                return string.Format("Provider named {0}", Name);
            }
        }

        /// <summary>
        /// Reference back to the provider this setting is appropriate to.
        /// </summary>
        public IProviderInfo Info { get; set; }
    }
}