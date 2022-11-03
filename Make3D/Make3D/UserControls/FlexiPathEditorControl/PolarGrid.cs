using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class PolarGrid : PointGrid
    {
        private Point centre;
        private DpiScale dpiScale;

        public override void CreateMarkers(DpiScale sc)
        {
            dpiScale = sc;
            gridMarkers?.Clear();

            double x = 0;
            double y = 0;
            double drad = polarRadius;
            double rad = drad;

            // make a largeish marker at the centre
            MakeSingleMarker(ToPixelX(centre.X), ToPixelY(centre.Y), 9);
            double maxrad = 2 * Math.Sqrt((actualWidth * actualWidth) + (actualHeight * actualHeight));
            double theta;

            double dt = (polarAngle / 360.0) * Math.PI * 2.0;
            while (rad < maxrad)
            {
                theta = 0;
                while (theta < Math.PI * 2.0)
                {
                    x = (Math.Sin(theta) * rad);
                    y = (Math.Cos(theta) * rad);
                    double px = ToPixelX((x + centre.X));
                    double py = ToPixelY((y + centre.Y));
                    if (px >= 0 && px <= actualWidth && py >= 0 && py <= actualHeight)
                    {
                        MakeSingleMarker(px, py, 5);
                    }
                    theta += dt;
                }
                rad += drad;
            }
        }

        private double polarRadius = 10;
        private double polarAngle = 36;

        public override void SetGridIntervals(double rad, double angle)
        {
            polarRadius = rad;
            polarAngle = angle;
        }

        /// <summary>
        /// Sets the bounds of the grid
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public override void SetSize(double v1, double v2)
        {
        }

        public override Point SnapPositionToMM(Point pos)
        {
            double gx = pos.X;
            double gy = pos.Y;
            double minDist = double.MaxValue;
            foreach (Shape sh in gridMarkers)
            {
                double x = Canvas.GetLeft(sh) + sh.Width / 2;
                double y = Canvas.GetTop(sh) + sh.Height / 2;
                double dist = Math.Sqrt((pos.X - x) * (pos.X - x) + (pos.Y - y) * (pos.Y - y));
                if (dist < minDist)
                {
                    minDist = dist;
                    gx = x;
                    gy = y;
                }
            }

            return new Point(ToMMX(gx), ToMMY(gy));
        }

        internal void SetPolarCentre(Point centre)
        {
            this.centre = new Point(centre.X, centre.Y);
        }

        private void MakeSingleMarker(double x, double y, double sz)
        {
            Ellipse el = new Ellipse();
            Canvas.SetLeft(el, x - 1);
            Canvas.SetTop(el, y - 1);
            el.Width = sz;
            el.Height = sz;
            el.Fill = Brushes.CadetBlue;
            el.Stroke = Brushes.Black;
            if (gridMarkers != null)
            {
                gridMarkers.Add(el);
            }
        }
    }
}