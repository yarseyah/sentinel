namespace Sentinel.Interfaces
{
    using System.Windows.Media;

    public interface IHighlighterStyle
    {
        Color? Background { get; set; }

        Color? Foreground { get; set; }
    }
}