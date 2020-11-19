namespace Sentinel.Interfaces.Providers
{
    public interface ILogProvider
    {
        IProviderInfo Information { get; }

        IProviderSettings ProviderSettings { get; }

        ILogger Logger { get; set; }

        string Name { get; set; }

        bool IsActive { get; }

        void Start();

        void Pause();

        void Close();
    }
}