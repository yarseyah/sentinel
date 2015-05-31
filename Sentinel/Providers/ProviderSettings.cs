namespace Sentinel.Providers
{
    using Sentinel.Interfaces.Providers;
    using System.Runtime.Serialization;

    public class ProviderSettings : IProviderSettings
    {
        #region Implementation of IProviderSettings

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

        #endregion
    }
}