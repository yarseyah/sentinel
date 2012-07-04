using System.Windows;

namespace Sentinel.Providers.Interfaces
{
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    using IProviderInfo = Sentinel.Interfaces.Providers.IProviderInfo;

    public interface INewProviderWizard
    {
        IProviderInfo Provider { get; } 

        IProviderSettings Settings { get; }

        bool Display(Window parent);
    }
}