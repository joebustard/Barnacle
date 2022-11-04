using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PolygonLibrary
{
    public class ConvexPolygon2D
    {
        public Point[] Corners;

        public ConvexPolygon2D(Point[] corners)
        {
            Corners = corners;
        }
    }
}