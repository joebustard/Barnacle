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
            public double Y;
            public bool vertical;
            public string barId;
            public int index;
        }

        public struct GapDef
        {
            public PolarPoint Start;
            public PolarPoint Finish;
        }

        private List<GapDef> spokePoints;

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
                        if (spokePoints[i].Finish.theta > spokePoints[i + 1].Start.theta)
                        {
                            swapped = true;
                            GapDef tmp = spokePoints[i];
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
                    double vdx1 = cx - vx1;
                    double vdx2 = cx - vx2;

                    //Uppers
                    double h1 = Math.Sqrt((innerRadius * innerRadius) - (vdx1 * vdx1));
                    double h2 = Math.Sqrt((innerRadius * innerRadius) - (vdx2 * vdx2));
                    PolarPoint p1 = new PolarPoint();
                    p1.barId = "V" + i.ToString();
                    p1.X = vx1;
                    p1.Y = cy + h1;
                    double t1 = Math.Atan2(h1 + cy, vdx1);

                    p1.theta = t1;

                    p1.r = innerRadius;
                    p1.vertical = true;
                    p1.index = 0;

                    //spokePoints.Add(p1);

                    PolarPoint p2 = new PolarPoint();
                    p2.barId = "V" + i.ToString();
                    p2.X = vx2;
                    p2.Y = cy + h2;
                    double t2 = Math.Atan2(cy - h1, vdx2);

                    p2.theta = t2;
                    p2.r = innerRadius;
                    p2.vertical = true;
                    p2.index = 1;

                    GapDef upper = new GapDef();
                    if (p1.theta > p2.theta)
                    {
                        upper.Start = p2;
                        upper.Finish = p1;
                    }
                    else
                    {
                        upper.Start = p1;
                        upper.Finish = p2;
                    }
                    spokePoints.Add(upper);

                    PolarPoint p3 = new PolarPoint();
                    p3.barId = "V" + i.ToString();
                    p3.X = vx1;
                    p3.Y = cy - h1;
                    double t3 = Math.Atan2(cy - h1, vdx1);

                    p3.theta = t3;
                    p3.r = innerRadius;
                    p3.vertical = true;
                    p3.index = 3;
                    //  spokePoints.Add(p2);

                    PolarPoint p4 = new PolarPoint();
                    p4.barId = "V" + i.ToString();
                    p4.X = vx2;
                    p4.Y = cy - h2;
                    double t4 = Math.Atan2(cy - h2, vdx2);

                    p4.theta = t4;
                    p4.r = innerRadius;
                    p4.vertical = true;
                    p4.index = 4;
                    GapDef lower = new GapDef();
                    if (p3.theta > p4.theta)
                    {
                        lower.Start = p4;
                        lower.Finish = p3;
                    }
                    else
                    {
                        lower.Start = p3;
                        lower.Finish = p4;
                    }
                    spokePoints.Add(lower);
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
            spokePoints = new List<GapDef>();

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
            DumpSpokes();
            //    SortSpokesByTheta();

            GenerateOuterEdge();
            GenerateInnerEdge();
        }

        private void DumpSpokes()
        {
            for (int i = 0; i < spokePoints.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("----");
                System.Diagnostics.Debug.WriteLine($" Start {spokePoints[i].Start.barId},{spokePoints[i].Start.index},{spokePoints[i].Start.theta},{spokePoints[i].Start.X},{spokePoints[i].Start.Y}");
                System.Diagnostics.Debug.WriteLine($" Finish {spokePoints[i].Finish.barId},{spokePoints[i].Finish.index},{spokePoints[i].Finish.theta},{spokePoints[i].Finish.X},{spokePoints[i].Finish.Y}");
            }
        }

        private void GenerateInnerEdge()
        {
            double theta = -Math.PI;
            double dt = Math.PI / 100;
            double theta2 = theta + dt;
            Point pn0;
            int p0 = 0;
            int p1 = 0;
            int p2 = 0;
            int p3 = 0;

            Point pn1;
            while (Math.PI - theta2 > 0.0001)
            {
                double phi = theta;
                double phi2 = theta2;
                if (ValidInner(ref phi, ref phi2))
                {
                    pn0 = CalcPoint(phi, innerRadius);
                    pn1 = CalcPoint(phi2, innerRadius);

                    p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                    p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                    p2 = AddVertice(cx + pn0.X, cy + edgeThickness, cz + pn0.Y);
                    p3 = AddVertice(cx + pn1.X, cy + edgeThickness, cz + pn1.Y);
                    AddFace(p0, p2, p1);
                    AddFace(p1, p2, p3);
                }

                theta = theta + dt;
                theta2 = theta + dt;
            }
        }

        private bool ValidInner(ref double phi, ref double phi2)
        {
            bool res = true;
            for (int i = 0; i < spokePoints.Count - 1 && res; i++)
            {
                if (phi < spokePoints[i].Start.theta && phi2 >= spokePoints[i].Start.theta && phi2 < spokePoints[i].Finish.theta)
                {
                    phi2 = spokePoints[i].Start.theta;
                }
                else
                {
                    if (phi > spokePoints[i].Start.theta && phi <= spokePoints[i].Finish.theta && phi2 > spokePoints[i].Finish.theta)
                    {
                        phi = spokePoints[i].Finish.theta;
                    }
                    else
                    {
                        if (phi > spokePoints[i].Start.theta &&
                            phi <= spokePoints[i].Finish.theta &&
                            phi2 > spokePoints[i].Start.theta &&
                            phi2 <= spokePoints[i].Finish.theta)
                        {
                            res = false;
                        }
                    }
                }
            }

            return res;
        }

        private void GenerateOuterEdge()
        {
            double theta = -Math.PI;
            double dt = Math.PI / 100;
            double theta2 = theta + dt;
            Point pn0;
            int p0;
            int p1;
            int p2;
            int p3;

            Point pn1;
            while (Math.PI - theta2 > 0.0001)
            {
                pn0 = CalcPoint(theta, grilleRadius);
                pn1 = CalcPoint(theta2, grilleRadius);

                p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                p2 = AddVertice(cx + pn0.X, cy + edgeThickness, cz + pn0.Y);
                p3 = AddVertice(cx + pn1.X, cy + edgeThickness, cz + pn1.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                theta = theta + dt;
                theta2 = theta + dt;
            }

            // close last step of ring
            theta2 = 2.0 * Math.PI;
            pn0 = CalcPoint(theta, grilleRadius);
            pn1 = CalcPoint(theta2, grilleRadius);

            p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
            p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

            p2 = AddVertice(cx + pn0.X, cy + edgeThickness, cz + pn0.Y);
            p3 = AddVertice(cx + pn1.X, cy + edgeThickness, cz + pn1.Y);
            AddFace(p0, p2, p1);
            AddFace(p1, p2, p3);
        }

        private void GenerateWithoutEdge()
        {
        }
    }
}