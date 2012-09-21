namespace Sentinel.MSBuild
{
    using Sentinel.Interfaces.Providers;

    public class MSBuildListenerSettings : IMSBuildListenerSettings
    {
        public MSBuildListenerSettings(IProviderSettings providerSettings)
        {
            this.ProviderSettings = providerSettings;
        }

        public string Name
        {
            get
            {
                return ProviderSettings.Name;
            }
        }

        public string Summary
        {
            get
            {
                return ProviderSettings.Summary;
            }
        }

        public IProviderInfo Info
        {
            get
            {
                return ProviderSettings.Info;
            }
        }

        private IProviderSettings ProviderSettings { get; set; }
    }
}