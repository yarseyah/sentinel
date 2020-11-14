namespace Sentinel.FileMonitor
{
    using System.Globalization;

    using Sentinel.Interfaces.Providers;

    public class FileMonitoringProviderSettings : IFileMonitoringProviderSettings
    {
        public FileMonitoringProviderSettings(
            IProviderInfo info,
            string providerName,
            string fileName,
            int refreshPeriod,
            bool loadExistingContent)
        {
            Info = info;
            Name = providerName;
            FileName = fileName;
            RefreshPeriod = refreshPeriod;
            LoadExistingContent = loadExistingContent;
        }

        public string FileName { get; private set; }

        /// <summary>
        /// Reference back to the provider this setting is appropriate to.
        /// </summary>
        public IProviderInfo Info { get; private set; }

        public bool LoadExistingContent { get; private set; }

        public string MessageDecoder { get; set; }

        public string Name { get; private set; }

        public int RefreshPeriod { get; private set; }

        public string Summary
            => string.Format(CultureInfo.InvariantCulture, "Monitor the file {0} for new log entries", FileName);

        public void Update(string fileName, int refreshPeriod, bool loadExistingContent)
        {
            FileName = fileName;
            RefreshPeriod = refreshPeriod;
            LoadExistingContent = loadExistingContent;
        }
    }
}