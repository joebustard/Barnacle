using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ReuleauxMaker : MakerBase
    {
        private int numberOfSides;
        private double radius;
        private double thickness;

        public ReuleauxMaker(int numberOfSides, double radius, double thickness)
        {
            this.numberOfSides = numberOfSides;
            this.radius = radius;
            this.thickness = thickness;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            double numberOfSteps = 120;
            List<System.Drawing.PointF> fps = new List<System.Drawing.PointF>();
            double dt = (Math.PI * 2) / numberOfSteps;
            double theta = 0;
            double pi = Math.PI;
            double n = numberOfSides;
            double pi2 = (2.0 * pi);
            double pin = (pi / n);
            for (int i = 0; i < numberOfSteps; i++)
            {
                double nt2p = (n * theta) / pi2;
                double flnt2p = Math.Floor(nt2p);
                double flnt2pp1 = 2 * flnt2p + 1;
                double pinflnt2pp1 = pin * flnt2pp1;
                double t1 = 0.5 * (theta + pinflnt2pp1);
                double x = 2 * Math.Cos(pi / (2 * n)) * Math.Cos(t1);
                x = x - Math.Cos(pin * flnt2pp1);
                x = (radius * x);

                double y = 2 * Math.Cos(pi / (2 * n)) * Math.Sin(t1);
                y = y - Math.Sin(pin * flnt2pp1);
                y = (radius * y);

                fps.Add(new System.Drawing.PointF((float)x, (float)y));
                theta += dt;
            }

            for (int i = 0; i < fps.Count; i++)
            {
                int j = i + 1;
                if (j >= fps.Count)
                {
                    j = 0;
                }
                int v1 = AddVertice(fps[i].X, 0, fps[i].Y);
                int v2 = AddVertice(fps[j].X, 0, fps[j].Y);
                int v3 = AddVertice(fps[j].X, thickness, fps[j].Y);
                int v4 = AddVertice(fps[i].X, thickness, fps[i].Y);

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v1);
                Faces.Add(v4);
                Faces.Add(v3);
            }
            TriangulatePerimiter(fps, thickness);
        }
    }
}