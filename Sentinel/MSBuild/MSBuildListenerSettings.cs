namespace Sentinel.MSBuild
{
    using Sentinel.Interfaces.Providers;

    public class MsBuildListenerSettings : IMsBuildListenerSettings
    {
        public MsBuildListenerSettings(IProviderSettings providerSettings)
        {
            this.ProviderSettings = providerSettings;
        }

        public string Name => ProviderSettings.Name;

        public string Summary => ProviderSettings.Summary;

        public IProviderInfo Info => ProviderSettings.Info;

        private IProviderSettings ProviderSettings { get; set; }
    }
}