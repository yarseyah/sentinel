namespace Sentinel.Upgrader
{
    using System;

    public class UpgradeServicePreferences : IUpgradeServicePreferences
    {
        //// = "https://github.com/yarseyah/sentinel/updates";
        //// private string upgradeLocation = @"..\..\..\Releases";
        public string UpgradeRepository { get; } = "http://localhost:5000";

        public bool IsDisabled => false;

        public TimeSpan DelayBeforeCheckingForUpgrades => TimeSpan.FromSeconds(30);

        public UpgradeServicePreferences()
        {
            var locationOverride = Environment.GetEnvironmentVariable("SENTINEL::DOWNLOAD-LOCATION");
            UpgradeRepository = locationOverride ?? UpgradeRepository;
        }
    }
}