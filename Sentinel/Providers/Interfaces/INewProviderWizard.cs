using System.Windows;

namespace Sentinel.Providers.Interfaces
{
    public interface INewProviderWizard
    {
        bool Display(Window parent);

        ProviderInfo Provider { get; } 
        IProviderSettings Settings { get; }
    }
}