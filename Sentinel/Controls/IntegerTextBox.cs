namespace Sentinel.Controls
{
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class IntegerTextBox : TextBox
    {
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);

            base.OnPreviewTextInput(e);
        }
    }
}