namespace Sentinel.Upgrader
{
    using System;

    public class UpgradeServicePreferences : IUpgradeServicePreferences
    {
        public UpgradeServicePreferences()
        {
            var locationOverride = Environment.GetEnvironmentVariable("SENTINEL::DOWNLOAD-LOCATION");
            UpgradeRepository = locationOverride ?? UpgradeRepository;
        }

        //// = "https://github.com/yarseyah/sentinel/updates";
        //// private string upgradeLocation = @"..\..\..\Releases";

        public string UpgradeRepository { get; } = "http://localhost:5000";

#if !STANDALONE_BUILD
        public bool IsDisabled => false;
#else
        public bool IsDisabled => true;
#endif

        public TimeSpan DelayBeforeCheckingForUpgrades => TimeSpan.FromSeconds(30);
    }
}