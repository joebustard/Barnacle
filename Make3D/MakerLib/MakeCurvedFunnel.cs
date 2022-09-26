using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class CurvedFunnelMaker : MakerBase
    {

        private double radius;
        private double factorA;
        private double wallThickness;
        private double shapeHeight;

        public CurvedFunnelMaker(double radius, double factorA, double wallThickness, double shapeHeight)
        {
            this.radius = radius;
            this.factorA = factorA;
            this.wallThickness = wallThickness;
            this.shapeHeight = shapeHeight;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double theta;
            double a = 1;
            double da = 1.0 / 50.0;
            int numRotSteps = 50;
            double dt = (Math.PI * 2.0) / numRotSteps;
            theta = 0;
            List<int> innerTop = new List<int>();
            List<int> innerBottom = new List<int>();

            List<int> outerTop = new List<int>();
            List<int> outerBottom = new List<int>();

            while (a <= 2)
            {
                theta = 0;
                while (theta < 2.0 * Math.PI)
                {
                    // inner wall
                    Point3D pn0 = MakePoint(a, radius - wallThickness, theta);
                    Point3D pn1 = MakePoint(a, radius - wallThickness, theta + dt);
                    Point3D pn2 = MakePoint(a + da, radius - wallThickness, theta + dt);
                    Point3D pn3 = MakePoint(a + da, radius - wallThickness, theta);

                    int p0 = AddVertice(pn0);
                    int p1 = AddVertice(pn1);
                    int p2 = AddVertice(pn2);
                    int p3 = AddVertice(pn3);
                    if (a == 1)
                    {
                        innerBottom.Add(p0);
                    }

                    if (a + da > 2)
                    {
                        innerTop.Add(p2);
                    }
                    Faces.Add(p0);
                    Faces.Add(p1);
                    Faces.Add(p2);

                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    // outer wall
                    pn0 = MakePoint(a, radius, theta);
                    pn1 = MakePoint(a, radius, theta + dt);
                    pn2 = MakePoint(a + da, radius, theta + dt);
                    pn3 = MakePoint(a + da, radius, theta);

                    p0 = AddVertice(pn0);
                    p1 = AddVertice(pn1);
                    p2 = AddVertice(pn2);
                    p3 = AddVertice(pn3);
                    if (a == 1)
                    {
                        outerBottom.Add(p0);
                    }
                    if (a + da > 2)
                    {
                        outerTop.Add(p2);
                    }
                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p1);

                    Faces.Add(p0);
                    Faces.Add(p3);
                    Faces.Add(p2);
                    theta += dt;
                }
                a += da;
            }

            // close bottom
            for (int i = 0; i < innerBottom.Count; i++)
            {
                int j = i + 1;
                if (j >= innerBottom.Count)
                {
                    j = 0;
                }
                Faces.Add(innerBottom[i]);
                Faces.Add(outerBottom[i]);
                Faces.Add(innerBottom[j]);

                Faces.Add(innerBottom[j]);
                Faces.Add(outerBottom[i]);
                Faces.Add(outerBottom[j]);
            }


            // close top
            for (int i = 0; i < innerBottom.Count; i++)
            {
                int j = i + 1;
                if (j >= innerBottom.Count)
                {
                    j = 0;
                }

                Faces.Add(innerTop[i]);
                Faces.Add(innerTop[j]);
                Faces.Add(outerTop[i]);

                Faces.Add(innerTop[j]);
                Faces.Add(outerTop[j]);
                Faces.Add(outerTop[i]);
            }
        }

        private Point3D MakePoint(double a, double radius, double v)
        {
            double u = radius / Math.Pow(a, factorA);

            Point3D res = new Point3D()
            {
                X = u * Math.Cos(v),
                Z = u * Math.Sin(v),
                Y = (a - 1.0) * shapeHeight
            };
            return res;
        }
    }
}
