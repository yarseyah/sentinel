namespace Sentinel.Upgrader
{
    using System;

    public interface IUpgradeServicePreferences
    {
        string UpgradeRepository { get; }

        bool IsDisabled { get; }

        TimeSpan DelayBeforeCheckingForUpgrades { get; }
    }
}