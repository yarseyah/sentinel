namespace Sentinel.Providers.Interfaces
{
    using System.Windows;

    using Sentinel.Interfaces.Providers;

    public interface INewProviderWizard
    {
        IProviderInfo Provider { get; }

        IProviderSettings Settings { get; }

        bool Display(Window parent);
    }
}