namespace Sentinel.FileMonitor
{
    using Sentinel.Interfaces.Providers;

    public interface IFileMonitoringProviderSettings : IProviderSettings
    {
        string FileName { get; }

        int RefreshPeriod { get; }

        bool LoadExistingContent { get; }

        string MessageDecoder { get; set; }

        void Update(string fileName, int refresh, bool loadExisting);
    }
}