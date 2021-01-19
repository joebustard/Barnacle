using System.Collections.Generic;
using System.Drawing;

namespace PlacementLib
{
    public class Workspace
    {
        private System.Windows.Point centre;
        private double width;

        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                centre.X = width / 2;
            }
        }

        private double height;

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                centre.Y = height / 2;
            }
        }

        public List<Component> PlacedComponents;

        public Workspace()
        {
            PlacedComponents = new List<Component>();
        }

        internal void Remove(Component component)
        {
            if (PlacedComponents.Contains(component))
            {
                PlacedComponents.Remove(component);
            }
        }

        internal void Place(Component component, double px, double py, bool rotated)
        {
            component.Position = new System.Windows.Point(px, py);
            component.Rotated = rotated;
            PlacedComponents.Add(component);
        }

        internal bool CanPlace(Component component, double px, double py, bool rotate)
        {
            bool result = true;
            RectangleF r1;
            if (!rotate)
            {
                if ((px + component.Width > Width) || (py + component.Height > Height))
                {
                    return false;
                }
                if ((px + component.Width < 0) || (py + component.Height < 0))
                {
                    return false;
                }
                r1 = new RectangleF((float)px, (float)py, (float)component.Width, (float)component.Height);
            }
            else
            {
                if ((px + component.Height > Width) || (py + component.Width > Height))
                {
                    return false;
                }
                if ((px + component.Height < 0) || (py + component.Width < 0))
                {
                    return false;
                }
                r1 = new RectangleF((float)px, (float)py, (float)component.Height, (float)component.Width);
            }
            // basic test

            // if a corner of component is in side the others then they overlap
            foreach (Component prt in PlacedComponents)
            {
                if (!prt.Rotated && (r1.IntersectsWith(prt.Rect) || prt.Rect.Contains(r1)))
                {
                    result = false;
                    break;
                }
                if (prt.Rotated && (r1.IntersectsWith(prt.RotatedRect) || prt.Rect.Contains(r1)))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public double Evaluate()
        {
            double result;

            double lx = double.MaxValue;
            double ly = double.MaxValue;
            double tx = double.MinValue;
            double ty = double.MinValue;

            foreach (Component com in PlacedComponents)
            {
                if (com.Position.X < lx)
                {
                    lx = com.Position.X;
                }
                if (com.Position.Y < ly)
                {
                    ly = com.Position.Y;
                }
                if (!com.Rotated)
                {
                    if (com.Position.X + com.Width > tx)
                    {
                        tx = com.Position.X + com.Width;
                    }
                    if (com.Position.Y + com.Height > ty)
                    {
                        ty = com.Position.Y + com.Height;
                    }
                }
                else
                {
                    if (com.Position.X + com.Height > tx)
                    {
                        tx = com.Position.X + com.Height;
                    }
                    if (com.Position.Y + com.Width > ty)
                    {
                        ty = com.Position.Y + com.Width;
                    }
                }
            }
            double xdiff = (tx - lx);
            double ydiff = (ty = ly);
            result = (tx - lx) * (ty - ly);
            double ratio = 1;
            if (xdiff > ydiff)
            {
                if (ydiff > 0)
                {
                    ratio = xdiff / ydiff;
                }
            }
            else
            {
                if (xdiff > 0)
                {
                    ratio = ydiff / xdiff;
                }
            }
            result *= ratio;
            return result;
        }
    }
}