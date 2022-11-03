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

        public override void CreateMarkers(DpiScale sc)
        {
            gridMarkers?.Clear();
            pixelsPerInchX = sc.PixelsPerInchX;
            pixelsPerInchY = sc.PixelsPerInchY;
            double x = 0;
            double y = 0;
            double drad = polarRadius * (sc.PixelsPerInchX / 25.4);
            double rad = drad;
            gridXPixels = (sc.PixelsPerInchX / 25.4) * gridXMM;

            gridYPixels = (sc.PixelsPerInchY / 25.4) * gridYMM;
            // make a largeish marker at the centre
            MakeSingleMarker(centre.X, centre.Y, 9);
            double maxrad = Math.Sqrt((centre.X * centre.X) + (centre.Y * centre.Y));
            double theta;

            double dt = (polarAngle / 360.0) * Math.PI * 2.0;
            while (rad < maxrad)
            {
                theta = 0;
                while (theta < Math.PI * 2.0)
                {
                    x = (Math.Sin(theta) * rad);
                    y = (Math.Cos(theta) * rad);
                    MakeSingleMarker(x + centre.X, y + centre.Y, 5);
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
            this.centre = centre;
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