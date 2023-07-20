using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    class SidePlateModel : PlateModel
    {

        private void TriangulateSide(List<PointF> points, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(new Point3D(t.Points[0].X, t.Points[0].Y, 0.0));
                int c1 = AddVertice(new Point3D(t.Points[1].X, t.Points[1].Y, 0.0));
                int c2 = AddVertice(new Point3D(t.Points[2].X, t.Points[2].Y, 0.0));
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }
        public float MiddleOffset { get; set; } = 0;
        internal override void SetPoints(List<PointF> dp)
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;
            foreach (PointF p in dp)
            {
                left = Math.Min(left, p.X);
                right = Math.Max(right, p.X);
                bottom = Math.Min(bottom, p.Y);
                top = Math.Max(top, p.Y);
            }
            float dx = (right - left) / 2;
            float dy = (top - bottom);
            MiddleOffset = dy / 2;
            points = new List<PointF>();
            foreach (PointF p in dp)
            {
                points.Add(new PointF(p.X - left - dx, -(p.Y - bottom)+dy));
            }
            ClearShape();

            TriangulateSide(points, false);

        }
    }
}
