using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for AeroProfileDisplayer.xaml
    /// </summary>
    public partial class AeroProfileDisplayer : UserControl
    {
        private List<Point> profilePnts;

        public List<Point> ProfilePnts
        {
            get { return profilePnts; }
            set
            {
                if (value != profilePnts)
                {
                    profilePnts = value;
                    if (profilePnts != null && profilePnts.Count > 0)
                    {
                        Refresh();
                    }
                }
            }
        }

        public void Refresh()
        {
            profileCanvas.Children.Clear();
            double border = 8;
            double h = (profileCanvas.ActualHeight - 2 * border);
            double w = (profileCanvas.ActualWidth - 2 * border);

            double hc = h / 2.0;

            Line ln = new Line();
            ln.Stroke = Brushes.Blue;
            ln.StrokeThickness = 2;
            DoubleCollection dc = new DoubleCollection();
            dc.Add(0.25);
            dc.Add(0.5);
            dc.Add(0.25);
            ln.StrokeDashArray = dc;
            ln.X1 = border;
            ln.Y1 = hc;
            ln.X2 = w - border;
            ln.Y2 = hc;

            profileCanvas.Children.Add(ln);

            Polyline pln = new Polyline();
            pln.Stroke = Brushes.Black;
            pln.StrokeThickness = 2;
            pln.Points = new PointCollection();
            pln.Fill = Brushes.Red;
            foreach (Point p in profilePnts)
            {
                Point n = new Point(p.X * w + border, ((1 - p.Y) * h) - hc);
                pln.Points.Add(n);
            }
            profileCanvas.Children.Add(pln);
        }

        public AeroProfileDisplayer()
        {
            InitializeComponent();
            ProfilePnts = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (profilePnts != null)
            {
                Refresh();
            }
        }
    }
}