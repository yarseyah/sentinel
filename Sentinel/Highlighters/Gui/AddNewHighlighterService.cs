namespace Sentinel.Highlighters.Gui
{
    using System.Windows;
    using System.Windows.Media;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Services;

    public class AddNewHighlighterService : IAddHighlighterService
    {
        public void Add()
        {
            var window = new AddEditHighlighterWindow();
            var data = new AddEditHighlighter(window, false);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            var dialogResult = window.ShowDialog();
            if (dialogResult != null && (bool)dialogResult)
            {
                var highlighter = Construct(data);

                if (highlighter != null)
                {
                    var service = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
                    service?.Highlighters.Add(highlighter);
                }
            }
        }

        public Highlighter Construct(AddEditHighlighter data)
        {
            Color? background = null;
            Color? foreground = null;

            var highlighter = new Highlighter
                                  {
                                      Name = data.Name,
                                      Field = data.Field,
                                      Pattern = data.Pattern,
                                      Mode = data.Mode,
                                      Enabled = true,
                                  };

            if (data.OverrideBackgroundColour)
            {
                background = data.BackgroundColour;
            }

            if (data.OverrideForegroundColour)
            {
                foreground = data.ForegroundColour;
            }

            highlighter.Style = new HighlighterStyle { Background = background, Foreground = foreground };
            return highlighter;
        }
    }
}