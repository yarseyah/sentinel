namespace Sentinel.Interfaces
{
    public interface ILogProvider
    {
        IProviderInfo Information { get; }

        ILogger Logger { get; set; }

        string Name { get; set; }

        bool IsActive { get; }

        void Start();

        void Pause();

        void Close();

    }
}