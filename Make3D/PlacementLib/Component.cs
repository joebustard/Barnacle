using System;
using System.Drawing;

namespace PlacementLib
{
    public class Component
    {
        public bool Rotated { get; set; }
        private bool canBeRotated;
        public bool CanBeRotated { get { return canBeRotated; } }

        private System.Windows.Point position;

        public System.Windows.Point Position
        {
            get { return position; }
            set
            {
                position = value;
                rect = new RectangleF((float)position.X, (float)position.Y, (float)Width, (float)Height);
                if (canBeRotated)
                {
                    rotatedRect = new RectangleF((float)position.X, (float)position.Y, (float)Height, (float)Width);
                }
            }
        }

        public System.Windows.Point OriginalPosition { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public object Tag { get; set; }
        public RectangleF rect;

        public RectangleF Rect
        {
            get
            {
                return rect;
            }
        }

        public RectangleF rotatedRect;

        public RectangleF RotatedRect
        {
            get
            {
                return rotatedRect;
            }
        }

        private Boundary Boundary { get; set; }

        public double Area
        {
            get
            {
                if (Boundary != null)
                {
                    return Boundary.Area;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Component()
        {
            Tag = null;
            Boundary = null;
        }

        public Component(object t, System.Windows.Point l, System.Windows.Point u)
        {
            Tag = t;
            Boundary = new Boundary(l, u);
            Width = u.X - l.X;
            Height = u.Y - l.Y;
            if (Math.Abs(Width - Height) > 1.0)
            {
                canBeRotated = true;
            }
            else
            {
                canBeRotated = false;
            }
            Rotated = false;
            Position = new System.Windows.Point(l.X, l.Y);
            OriginalPosition = new System.Windows.Point(l.X, l.Y);
        }
    }
}