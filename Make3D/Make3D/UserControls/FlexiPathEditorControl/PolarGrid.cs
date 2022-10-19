using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class PolarGrid : PointGrid
    {
        public override void CreateMarkers(DpiScale sc)
        {
            gridMarkers?.Clear();
            double x = 0;
            double y = 0;

            gridXPixels = (sc.PixelsPerInchX / 25.4) * gridXMM;

            gridYPixels = (sc.PixelsPerInchY / 25.4) * gridYMM;
            // make a largeish marker at the centre
            MakeSingleMarker(centre.X, centre.Y, 5);
            double theta;
            double rad = 10;
            double dt = Math.PI / 10;
            while ( rad < centre.X)
            {
                theta = 0;
                while ( theta < Math.PI * 2.0)
                {
                     x = (Math.Sin(theta) * rad);
                     y = (Math.Cos(theta) * rad);
                    MakeSingleMarker(x + centre.X, y + centre.Y,3);
                    theta += dt;
                }
                rad += 10;
            }
        }

        private void MakeSingleMarker(double x, double y, double sz)
        {
            Ellipse el = new Ellipse();
            Canvas.SetLeft(el, x - 1);
            Canvas.SetTop(el, y - 1);
            el.Width = sz;
            el.Height = sz;
            el.Fill = Brushes.AliceBlue;
            el.Stroke = Brushes.CadetBlue;
            if (gridMarkers != null)
            {
                gridMarkers.Add(el);
            }
        }

        public override void SetGridIntervals(double gridXMM, double gridYMM)
        {
            this.gridXMM = gridXMM;
            this.gridYMM = gridYMM;
        }

        /// <summary>
        /// Sets the bounds of the grid
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public override void SetSize(double v1, double v2)
        {
        }

        public override Point SnapPositionToMM(Point p)
        {
            return p;
        }

        private Point centre;
        internal void SetPolarCentre(Point centre)
        {
            this.centre = centre;
        }
    }
}