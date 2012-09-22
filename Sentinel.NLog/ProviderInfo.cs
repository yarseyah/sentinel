namespace Sentinel.NLog
{
    using System;

    using Sentinel.Interfaces.Providers;

    internal class ProviderInfo : IProviderInfo
    {
        public Guid Identifier
        {
            get
            {
                return new Guid("F12581A5-64C0-4B35-91FC-81C9A09C1E0B");
            }
        }

        public string Name
        {
            get
            {
                return "NLog Viewer Provider";
            }
        }

        public string Description
        {
            get
            {
                return "Handler for nLog's log4j networking protocol log target.";
            }
        }
    }
}