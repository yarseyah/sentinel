namespace Sentinel.Views.Heartbeat
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using Sentinel.Support.Wpf;

    /// <summary>
    ///   Interaction logic for HeartbeatControl.xaml.
    /// </summary>
    public partial class HeartbeatControl : INotifyPropertyChanged
    {
        private ObservableDictionary<string, ObservableCollection<int>> data;

        public HeartbeatControl()
        {
            InitializeComponent();
            DataContext = this;

            var samplePeriodTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(1000),
            };
            samplePeriodTimer.Tick += SampleTick;
            samplePeriodTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableDictionary<string, ObservableCollection<int>> Data
        {
            get => data;
            set
            {
                if (data != value)
                {
                    data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }
        }

        public IEnumerable<Point> CreatePoints(IEnumerable<int> values, int width, int height, double heightScale)
        {
            IList<Point> returnCollection = new List<Point>();

            var stride = width / (values.Count() - 1);

            var x = -stride;

            // Populate structure with entry off to side and down to origin.
            returnCollection.Add(new Point(x, height));
            returnCollection.Add(new Point(x, height - (values.ElementAt(0) * heightScale)));
            x += stride;

            foreach (var value in values)
            {
                var y = (int)(value * heightScale);
                returnCollection.Add(new Point(x, height - y));
                x += stride;
            }

            // Post populate with values right off to the side (so null values don't enter visibility).
            returnCollection.Add(new Point(x, height));

            return returnCollection;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void SampleTick(object sender, EventArgs e)
        {
            // Dimensions of the canvas.
            // Trace.WriteLine(string.Format("Canvas dimensions {0}x{1}", canvas.Width, canvas.Height));

            // Remove any polylines registered with the canvas.
            canvas.Children.Clear();

            if (Data != null)
            {
                int maxValue = Data.Count() > 0
                    ? Data.Max(kvp => kvp.Value != null && kvp.Value.Count() > 0 ? kvp.Value.Max() : 0)
                    : 0;
                double scale = maxValue > 0 ? canvas.Height / maxValue : 1.0d;

                foreach (KeyValuePair<string, ObservableCollection<int>> d in Data)
                {
                    var pc = new PointCollection(
                        CreatePoints(
                            d.Value,
                            (int)canvas.Width,
                            (int)canvas.Height,
                            scale));
                    var pl = new Polyline
                    {
                        Points = pc,
                        StrokeThickness = 3.0d,
                    };

                    switch (d.Key)
                    {
                        case "DEBUG":
                            pl.Stroke = Brushes.Green;
                            break;
                        case "ERROR":
                            pl.Stroke = Brushes.Red;
                            break;
                        case "INFO":
                            pl.Stroke = Brushes.Blue;
                            break;
                        case "WARN":
                            pl.Stroke = Brushes.Yellow;
                            break;
                        case "FATAL":
                            pl.Stroke = Brushes.Purple;
                            break;
                        default:
                            pl.Stroke = Brushes.Black;
                            break;
                    }

                    pl.Name = d.Key;
                    canvas.Children.Add(pl);
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCanvasDimensions();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetCanvasDimensions();
        }

        private void SetCanvasDimensions()
        {
            canvas.Width = ActualWidth;
            canvas.Height = ActualHeight;
        }
    }
}