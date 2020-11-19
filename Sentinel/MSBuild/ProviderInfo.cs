namespace Sentinel.MSBuild
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class ProviderInfo : IProviderInfo
    {
        public Guid Identifier => new Guid("87270254-9EB6-4AF3-9008-0147DE849168");

        public string Name => "MSBuild UDP Listener";

        public string Description => "Listens for JSON serialized MSBuild logging events passed via UDP";
    }
}