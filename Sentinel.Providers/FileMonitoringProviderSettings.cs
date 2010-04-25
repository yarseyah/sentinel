using System;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    public class FileMonitoringProviderSettings : IFileMonitoringProviderSettings
    {
        public FileMonitoringProviderSettings(IProviderInfo info, string providerName, string fileName, int refreshPeriod, bool loadExistingContent)
        {
            Info = info;
            Name = providerName;
            FileName = fileName;
            RefreshPeriod = refreshPeriod;
            LoadExistingContent = loadExistingContent;
        }

        #region Implementation of IProviderSettings

        public string Name { get; private set; }

        public string Summary
        {
            get
            {
                return string.Format("Monitor the file {0} for new log entries", FileName);
            }
        }

        /// <summary>
        /// Reference back to the provider this setting is appropriate to.
        /// </summary>
        public IProviderInfo Info { get; private set; }

        #endregion

        #region Implementation of IFileMonitoringProviderSettings

        public string FileName { get; private set; }

        public int RefreshPeriod { get; private set; }

        public bool LoadExistingContent { get; private set; }

        public string MessageDecoder { get; set; }

        public void Update(string fileName, int refreshPeriod, bool loadExistingContent)
        {
            FileName = fileName;
            RefreshPeriod = refreshPeriod;
            LoadExistingContent = loadExistingContent;
        }

        #endregion
    }
}