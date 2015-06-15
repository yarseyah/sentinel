namespace Sentinel.Logs.Interfaces
{
    using Sentinel.Views.Interfaces;

    public interface ILogFileExporter
    {
        void SaveLogViewerToFile(IWindowFrame windowFrame, string filePath);
    }
}
