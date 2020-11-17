using System.Drawing;

namespace PolygonTriangulationLib
{
    public class Triangle : TriangulationPolygon
    {
        public Triangle(PointF p0, PointF p1, PointF p2)
        {
            Points = new PointF[] { p0, p1, p2 };
        }
    }
}