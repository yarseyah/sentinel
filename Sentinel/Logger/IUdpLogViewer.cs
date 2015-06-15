namespace Sentinel.Logger
{
    using Sentinel.Views.Interfaces;

    public interface IUdpLogViewer : ILogViewer
    {
        int Port { get; set; }
    }
}