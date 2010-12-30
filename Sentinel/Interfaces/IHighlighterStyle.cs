using System.Windows.Media;

namespace Sentinel.Interfaces
{
    public interface IHighlighterStyle
    {
        Color? Background { get; set; }
        Color? Foreground { get; set; }
    }
}