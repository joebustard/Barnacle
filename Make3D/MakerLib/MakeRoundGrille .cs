using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RoundGrilleMaker : MakerBase
    {
        private double edgeThickness;

        //  private double grilleHeight;
        private double grilleRadius;

        private double grilleWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private bool makeEdge;
        private double verticalBars;
        private double verticalBarThickness;
        private double innerRadius;
        private double cx = 0.0;
        private double cy = 0.0;
        private double cz = 0.0;
        private double innerDiameter;

        public struct PolarPoint
        {
            public double theta;
            public double r;
            public double X;
            public double y;
            public bool vertical;
            public string barId;
            public int index;
        }

        private List<PolarPoint> spokePoints;

        private void SortSpokesByTheta()
        {
            if (spokePoints != null && spokePoints.Count > 1)
            {
                bool swapped = true;
                while (swapped)
                {
                    swapped = false;
                    for (int i = 0; i < spokePoints.Count - 1; i++)
                    {
                        if (spokePoints[i].theta > spokePoints[i + 1].theta)
                        {
                            swapped = true;
                            PolarPoint tmp = spokePoints[i];
                            spokePoints[i] = spokePoints[i + 1];
                            spokePoints[i + 1] = tmp;
                        }
                    }
                }
            }
        }

        private List<double> verticalXs;

        private void MakeVerticalBarPolarCoords()
        {
            verticalXs = new List<double>();
            if (verticalBars > 0)
            {
                double vspacing = innerDiameter / (verticalBars + 1);
                for (int i = 0; i < verticalBars; i++)
                {
                    // work out where vertical bar crosses the x axies.
                    double vx = (cx - innerRadius) + ((i + 1) * vspacing);
                    verticalXs.Add(vx);
                    double vx1 = vx - (verticalBarThickness / 2.0);
                    double vx2 = vx + (verticalBarThickness / 2.0);
                    // distance of this from the centre
                    double vdx1 = Math.Abs(cx - vx1);
                    double vdx2 = Math.Abs(cx - vx2);

                    double h1 = Math.Sqrt((innerRadius * innerRadius) - (vdx1 * vdx1));

                    PolarPoint p1 = new PolarPoint();
                    p1.barId = "V" + i.ToString();
                    p1.X = vx1;
                    p1.y = cy + h1;
                    double t = Math.Atan2(vdx1, h1 + cy);
                    p1.theta = t;
                    p1.r = innerRadius;
                    p1.vertical = true;
                    p1.index = 0;
                    spokePoints.Add(p1);

                    p1 = new PolarPoint();
                    p1.barId = "V" + i.ToString();
                    p1.X = vx1;
                    p1.y = cy - h1;
                    t = Math.Atan2(vdx1, cy - h1);
                    p1.theta = t;
                    p1.r = innerRadius;
                    p1.vertical = true;
                    p1.index = 1;
                    spokePoints.Add(p1);

                    double h2 = Math.Sqrt((innerRadius * innerRadius) - (vdx2 * vdx2));

                    PolarPoint p2 = new PolarPoint();
                    p2.barId = "V" + i.ToString();
                    p2.X = vx2;
                    p2.y = cy + h2;
                    t = Math.Atan2(vdx2, h2 + cy);
                    p2.theta = t;
                    p2.r = innerRadius;
                    p2.vertical = true;
                    p2.index = 3;
                    spokePoints.Add(p2);

                    p2 = new PolarPoint();
                    p2.barId = "V" + i.ToString();
                    p2.X = vx2;
                    p2.y = cy - h1;
                    t = Math.Atan2(vdx2, cy - h2);
                    p2.theta = t;
                    p2.r = innerRadius;
                    p2.vertical = true;
                    p2.index = 4;
                    spokePoints.Add(p2);
                }

                // so we should have a list
            }
        }

        public RoundGrilleMaker(double grilleRadius, double grillWidth, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grilleRadius = grilleRadius;
            //  this.grilleHeight = grillHeight;
            this.grilleWidth = grillWidth;
            this.makeEdge = makeEdge;
            this.edgeThickness = edgeThickness;
            this.verticalBars = verticalBars;
            this.verticalBarThickness = verticalBarThickness;
            this.horizontalBars = horizontalBars;
            this.horizontalBarThickness = horizontalBarThickness;
            this.innerRadius = grilleRadius - edgeThickness;
            this.innerDiameter = 2.0 * this.innerRadius;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            spokePoints = new List<PolarPoint>();

            if (makeEdge)
            {
                GenerateWithEdge();
            }
            else
            {
                GenerateWithoutEdge();
            }
        }

        private void Box(double x, double y, double z, double l, double h, double w, bool left, bool right, bool top, bool bottom)
        {
            // back
            int v0 = AddVertice(x, y, z);
            int v1 = AddVertice(x, y + h, z);
            int v2 = AddVertice(x + l, y + h, z);
            int v3 = AddVertice(x + l, y, z);
            AddFace(v0, v1, v2);
            AddFace(v0, v2, v3);

            // front
            int v4 = AddVertice(x, y, z + w);
            int v5 = AddVertice(x, y + h, z + w);
            int v6 = AddVertice(x + l, y + h, z + w);
            int v7 = AddVertice(x + l, y, z + w);
            AddFace(v4, v6, v5);
            AddFace(v4, v7, v6);
            if (left)
            {
                AddFace(v0, v5, v1);
                AddFace(v0, v4, v5);
            }

            if (right)
            {
                AddFace(v3, v2, v6);
                AddFace(v3, v6, v7);
            }

            if (top)
            {
                AddFace(v1, v6, v2);
                AddFace(v1, v5, v6);
            }

            if (bottom)
            {
                AddFace(v0, v3, v7);
                AddFace(v0, v7, v4);
            }
        }

        private void GenerateWithEdge()
        {
            MakeVerticalBarPolarCoords();
            SortSpokesByTheta();
            GenerateOuterEdge();
        }

        private void GenerateOuterEdge()
        {
            double theta = 0;
            double dt = Math.PI / 100;
            double theta2 = theta + dt;
            while (Math.PI * 2 - theta2 > 0.0001)
            {
                Point pn0 = CalcPoint(theta, grilleRadius);
                Point pn1 = CalcPoint(theta2, grilleRadius);

                int p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                int p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                int p2 = AddVertice(cx + pn0.X, cy + edgeThickness, cz + pn0.Y);
                int p3 = AddVertice(cx + pn1.X, cy + edgeThickness, cz + pn1.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                theta = theta + dt;
                theta2 = theta + dt;
            }
        }

        private void GenerateWithoutEdge()
        {
        }
    }
}