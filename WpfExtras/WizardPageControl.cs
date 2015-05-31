#region Using directives

using System.Windows.Controls;

#endregion

namespace WpfExtras
{
    public interface IPosition
    {
        double Left { get; set; }
        void MoveTo(double left, double top);
        double Top { get; set; }
    }

    public class PositionHelper : IPosition
    {
        private readonly UserControl userControl;

        public PositionHelper(UserControl userControl)
        {
            this.userControl = userControl;
        }

        public double Left
        {
            get
            {
                return (double)userControl.GetValue(Canvas.LeftProperty);
            }
            set
            {
                userControl.SetValue(Canvas.LeftProperty, value);
            }
        }

        public double Top
        {
            get
            {
                return (double)userControl.GetValue(Canvas.TopProperty);
            }
            set
            {
                userControl.SetValue(Canvas.TopProperty, value);

            }
        }

        public void MoveTo(double left, double top)
        {
            Left = left;
            Top = top;
        }

    }  
}