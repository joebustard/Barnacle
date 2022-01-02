using System.Windows;

namespace Barnacle.Dialogs
{
    public class Dimension
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }

        public Point Mid
        {
            get
            {
                if (P1 != null)
                {
                    Point p = new Point(P1.X + (P2.X - P1.X) / 2.0, P1.Y + (P2.Y - P1.Y) / 2.0);
                    return p;
                }
                else
                {
                    return new Point(0, 0);
                }
            }
        }

        public double Width
        {
            get
            {
                double res = 0;
                if (P1 != null && P2 != null)
                {
                    res = (P2.X - P1.X);
                }
                return res;
            }
        }

        public double Height
        {
            get
            {
                double res = 0;
                if (P1 != null && P2 != null)
                {
                    res = (P2.Y - P1.Y);
                }
                return res;
            }
        }

        public Dimension()
        {
            P1 = new Point(0, 0);
            P2 = new Point(0, 0);
        }

        public Dimension(Point a, Point b)
        {
            P1 = a;
            P2 = b;
        }
    }
}