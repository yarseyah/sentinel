using Sentinel.Interfaces;

namespace Sentinel.Classification.Interfaces
{
    public interface IClassifier
    {
        bool Enabled { get; set; }

        string Name { get; set; }

        string Type { get; }

        bool IsMatch(object parameter);

        ILogEntry Classify(ILogEntry entry);
    }
}