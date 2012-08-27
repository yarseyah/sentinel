namespace MSBuildListener
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class MSBuildListenerProvider : IProviderInfo
    {
        public Guid Identifier
        {
            get
            {
                return new Guid("87270254-9EB6-4AF3-9008-0147DE849168");
            }
        }

        public string Name
        {
            get
            {
                return "MSBuild UDP Listener";
            }
        }

        public string Description
        {
            get
            {
                return "Listens for JSON serialized MSBuild logging events passed via UDP";
            }
        }
    }
}