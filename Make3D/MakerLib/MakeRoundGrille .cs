using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RoundGrilleMaker : MakerBase
    {
        private const int divisions = 100;
        private double cx = 0.0;
        private double cy = 0.0;
        private double cz = 0.0;
        private double edgeThickness;
        private double grilleRadius;
        private double grilleWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private double innerDiameter;
        private double innerRadius;
        private bool makeEdge;
        private List<GapDef> spokePoints;
        private double verticalBars;
        private double verticalBarThickness;
        private List<double> verticalXs;

        public RoundGrilleMaker(double grilleRadius, double grillWidth, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grilleRadius = grilleRadius;
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
            double dt = Math.PI / divisions;
            double theta2 = theta + dt;
            Point pn0;
            int p0 = 0;
            int p1 = 0;
            int p2 = 0;
            int p3 = 0;

            Point pn1;
            while (theta < Math.PI)
            {
                double phi = theta;
                double phi2 = theta2;
                if (phi2 > Math.PI)
                {
                    phi2 = Math.PI;
                }
                if (ValidInner(ref phi, ref phi2))
                {

                    // inner
                    pn0 = CalcPoint(phi, innerRadius);
                    pn1 = CalcPoint(phi2, innerRadius);

                    p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                    p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                    p2 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                    p3 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);
                    AddFace(p0, p1, p2);
                    AddFace(p1, p3, p2);

                    pn0 = CalcPoint(phi, grilleRadius);
                    pn1 = CalcPoint(phi2, grilleRadius);

                    p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                    p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                    p2 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                    p3 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);
                    AddFace(p0, p2, p1);
                    AddFace(p1, p2, p3);
                    
                }

                theta = theta + dt;
                theta2 = theta + dt;
            }
        }

        private void GenerateOuterEdge()
        {
            double theta = -Math.PI;

            double dt = Math.PI / divisions;
            double theta2 = theta + dt;
            Point pn0;
            int p0;
            int p1;
            int p2;
            int p3;

            Point pn1;
            while (theta < Math.PI)
            {
                theta2 = theta + dt;
                if (theta2 > Math.PI)
                {
                    theta2 = Math.PI;
                }
                pn0 = CalcPoint(theta, grilleRadius);
                pn1 = CalcPoint(theta2, grilleRadius);

                p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                p2 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                p3 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                theta = theta + dt;
            }
        }

        private void GenerateRing()
        {
            double theta = -Math.PI;

            double dt = Math.PI / divisions;
            double theta2 = theta + dt;
            Point pn0;
            int p0;
            int p1;
            int p2;
            int p3;

            Point pn1;
            Point pn2;
            Point pn3;
            while (theta < Math.PI)
            {
                theta2 = theta + dt;
                if (theta2 > Math.PI)
                {
                    theta2 = Math.PI;
                }

                double phi = theta;
                double phi2 = theta2;
                if (phi2 > Math.PI)
                {
                    phi2 = Math.PI;
                }
                if (ValidInner(ref phi, ref phi2))
                {
                    pn0 = CalcPoint(phi, innerRadius);
                    pn1 = CalcPoint(phi2, innerRadius);
                    pn2 = CalcPoint(phi, grilleRadius);
                    pn3 = CalcPoint(phi2, grilleRadius);

                    // bottom
                    p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                    p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                    p2 = AddVertice(cx + pn2.X, cy, cz + pn2.Y);
                    p3 = AddVertice(cx + pn3.X, cy, cz + pn3.Y);
                    AddFace(p0, p2, p1);
                    AddFace(p1, p2, p3);

                    // top
                    p0 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                    p1 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);

                    p2 = AddVertice(cx + pn2.X, cy + grilleWidth, cz + pn2.Y);
                    p3 = AddVertice(cx + pn3.X, cy + grilleWidth, cz + pn3.Y);
                    AddFace(p0, p1, p2);
                    AddFace(p1, p3, p2);
                }
                theta = theta + dt;
            }
        }

        private List<Bar> vBars;

        private void GenerateWithEdge()
        {
            MakeVerticalBarPolarCoords();
            SortSpokesByTheta();
            DumpSpokes();
        //    GenerateOuterEdge();
            GenerateInnerEdge();
            GenerateRing();
            if (verticalBars > 0 && horizontalBars > 0)
            {
                GenerateCrossingGrid();
            }
            else if (verticalBars > 0)
            {
                GenerateVerticalBars();
            }
            else
            if (horizontalBars > 0)
            {
                GenerateHorizontalBars();
            }
        }

        private void GenerateHorizontalBars()
        {
        }

        private void GenerateVerticalBars()
        {
            if (vBars != null && vBars.Count > 0)
            {
                foreach (Bar b in vBars)
                {
                    Point p1 = new Point(b.G1.Start.X, b.G1.Start.Y);
                    Point p2 = new Point(b.G1.Finish.X, b.G1.Finish.Y);
                    Point p3 = new Point(b.G2.Start.X, b.G2.Start.Y);
                    Point p4 = new Point(b.G2.Finish.X, b.G2.Finish.Y);
                    SideWall(p1, p4);
                    SideWall(p3, p2);
                    VTopAndBottom(b.G1, b.G2);
                    RingGap(b.G1, b.G2);
                }
            }
        }

        private void RingGap(GapDef a, GapDef b)
        {
            double dt = (a.Finish.theta - a.Start.theta) / 10.0;

            double theta = a.Start.theta;
            while (theta < a.Finish.theta)
            {
                Point pn0 = CalcPoint(theta, innerRadius);
                Point pn1 = CalcPoint(theta + dt, innerRadius);
                Point pn2 = CalcPoint(theta, grilleRadius);
                Point pn3 = CalcPoint(theta + dt, grilleRadius);
                int p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                int p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);
                int p2 = AddVertice(cx + pn2.X, cy, cz + pn2.Y);
                int p3 = AddVertice(cx + pn3.X, cy, cz + pn3.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                int up0 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                int up1 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);
                int up2 = AddVertice(cx + pn2.X, cy + grilleWidth, cz + pn2.Y);
                int up3 = AddVertice(cx + pn3.X, cy + grilleWidth, cz + pn3.Y);
                AddFace(up0, up1, up2);
                AddFace(up1, up3, up2);
                AddFace(p2, up2, p3);
                AddFace(up2, up3, p3);              
                theta += dt;
            }
            theta = b.Start.theta;
            while (theta <b.Finish.theta)
            {
                Point pn0 = CalcPoint(theta, innerRadius);
                Point pn1 = CalcPoint(theta + dt, innerRadius);
                Point pn2 = CalcPoint(theta, grilleRadius);
                Point pn3 = CalcPoint(theta + dt, grilleRadius);
                int p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                int p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);
                int p2 = AddVertice(cx + pn2.X, cy, cz + pn2.Y);
                int p3 = AddVertice(cx + pn3.X, cy, cz + pn3.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                int up0 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                int up1 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);
                int up2 = AddVertice(cx + pn2.X, cy + grilleWidth, cz + pn2.Y);
                int up3 = AddVertice(cx + pn3.X, cy + grilleWidth, cz + pn3.Y);
                AddFace(up0, up1, up2);
                AddFace(up1, up3, up2);
                AddFace(p2, up2, p3);
                AddFace(up2, up3, p3);
                theta += dt;
            }
        }

        private void VTopAndBottom(GapDef a, GapDef b)
        {
            double dt = (a.Finish.theta - a.Start.theta) / 10.0;

            double theta = a.Start.theta;
            while (theta < a.Finish.theta)
            {
                double g = dt;
                if ( theta +g > a.Finish.theta)
                {
                    g = a.Finish.theta - theta;
                }
                Point pn0 = CalcPoint(theta, innerRadius);

                Point pn1 = CalcPoint(theta + g, innerRadius);
                Point pn2 = CalcPoint(-theta, innerRadius);
                Point pn3 = CalcPoint(-theta - g, innerRadius);
                int p0 = AddVertice(cx + pn0.X, cy, cz + pn0.Y);
                int p1 = AddVertice(cx + pn1.X, cy, cz + pn1.Y);

                int p2 = AddVertice(cx + pn2.X, cy, cz + pn2.Y);
                int p3 = AddVertice(cx + pn3.X, cy, cz + pn3.Y);
                AddFace(p0, p1, p2);
                AddFace(p1, p3, p2);

                p0 = AddVertice(cx + pn0.X, cy + grilleWidth, cz + pn0.Y);
                p1 = AddVertice(cx + pn1.X, cy + grilleWidth, cz + pn1.Y);

                p2 = AddVertice(cx + pn2.X, cy + grilleWidth, cz + pn2.Y);
                p3 = AddVertice(cx + pn3.X, cy + grilleWidth, cz + pn3.Y);
                AddFace(p0, p2, p1);
                AddFace(p1, p2, p3);
                theta += dt;
            }
        }
        private void SideWall(Point p1, Point p2)
        {
            int v0 = AddVertice(p1.X, 0, p1.Y);
            int v1 = AddVertice(p2.X, 0, p2.Y);
            int v2 = AddVertice(p2.X, grilleWidth, p2.Y);
            int v3 = AddVertice(p1.X, grilleWidth, p1.Y);
            AddFace(v0, v2, v1);
            AddFace(v0, v3, v2);
        }

        private void GenerateCrossingGrid()
        {
        }

        private void GenerateWithoutEdge()
        {
        }

        private void MakeVerticalBarPolarCoords()
        {
            verticalXs = new List<double>();
            vBars = new List<Bar>();
            if (verticalBars > 0)
            {
                double vspacing = innerDiameter / (verticalBars + 1);
                for (int i = 0; i < verticalBars; i++)
                {
                    Bar bar = new Bar();

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
                    double t2 = Math.Atan2(h2 + cy, vdx2);

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
                    bar.G1 = upper;

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
                    bar.G2 = lower;

                    vBars.Add(bar);
                }

                // so we should have a list
            }
        }

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

        private bool ValidInner(ref double phi, ref double phi2)
        {
            bool res = true;
            for (int i = 0; i < spokePoints.Count && res; i++)
            {
                // does segment overlap start of gap, if so clip end
                if (phi < spokePoints[i].Start.theta && phi2 >= spokePoints[i].Start.theta && phi2 < spokePoints[i].Finish.theta)
                {
                    phi2 = spokePoints[i].Start.theta;
                }
                else
                {
                    // does segment overlap end of gap, if so clip start
                    if (phi >= spokePoints[i].Start.theta && phi <= spokePoints[i].Finish.theta && phi2 > spokePoints[i].Finish.theta)
                    {
                        phi = spokePoints[i].Finish.theta;
                    }
                    else
                    {
                        // is entire seqment inside a gap
                        if (phi >= spokePoints[i].Start.theta &&
                            phi <= spokePoints[i].Finish.theta &&
                            phi2 >= spokePoints[i].Start.theta &&
                            phi2 <= spokePoints[i].Finish.theta)
                        {
                            // tell caller not use do this seqment at all
                            res = false;
                        }
                    }
                }
            }

            return res;
        }

        public struct Bar
        {
            public GapDef G1;
            public GapDef G2;
        }

        public struct GapDef
        {
            public PolarPoint Finish;
            public PolarPoint Start;
        }

        public struct PolarPoint
        {
            public string barId;
            public int index;
            public double r;
            public double theta;
            public bool vertical;
            public double X;
            public double Y;
        }
    }
}