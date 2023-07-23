using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class RibPlateModel : PlateModel
    {
        public float MiddleOffset { get; set; } = 0;
        public double XPosition { get; set; }
        public double YPosition { get; set; }
        public void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }
        public void MakeFaces(List<PointF> dp)
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
           
            points = new List<PointF>();
            foreach (PointF p in dp)
            {
                //-bottom-(float)YPosition+p.Y
                points.Add(new PointF(p.X - left - dx, -p.Y));
            }
            ClearShape();

            TriangulateSide(points, false);
        }

        internal override void SetPoints(List<PointF> dp)
        {
            if (dp != null)
            {
                rawPoints = new List<PointF>();
                //rawPoints.Clear();e
                // normalise the original points i.e. scale to 0..1
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
                float w = right - left;
                float h = top - bottom;
                if (w > 0 && h > 0)
                {
                    foreach (PointF p in dp)
                    {
                        rawPoints.Add(new PointF((p.X - left) / w, (p.Y - bottom) / h));
                    }
                }
            }
        }


        private void TriangulateSide(List<PointF> points, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(new Point3D(XPosition, t.Points[0].Y +  YPosition, t.Points[0].X));
                int c1 = AddVertice(new Point3D(XPosition, t.Points[1].Y +  YPosition, t.Points[1].X));
                int c2 = AddVertice(new Point3D(XPosition, t.Points[2].Y +  YPosition, t.Points[2].X));
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

        internal void SetPositionAndScale(double x, float yPosition,double lower1, double upper1, double lower2, double upper2)
        {
            
            XPosition = x;
            Log($"RibPlateModel xposition {XPosition}");
            YPosition = yPosition;
            float topDim = (float)(upper1 - lower1);
            float sideDim = (float)(upper2 - lower2);
            List<PointF> dp = new List<PointF>();
            foreach( PointF p in rawPoints)
            {
                PointF np = new PointF();
                np.X = (p.X * topDim) + (float)lower1;
                np.Y = (p.Y * sideDim) + (float)lower2;
                dp.Add(np);
            }
            MakeFaces(dp);
        }
    }
}