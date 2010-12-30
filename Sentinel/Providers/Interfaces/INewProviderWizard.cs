using System.Windows;

namespace Sentinel.Providers.Interfaces
{
    public interface INewProviderWizard
    {
        bool Display(Window parent);

        IProviderInfo Provider { get; } 
        IProviderSettings Settings { get; }
    }
}