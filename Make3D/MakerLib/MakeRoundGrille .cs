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
        private List<Bar> hBars;
        private double horizontalBars;
        private double horizontalBarThickness;
        private List<double> horizontalYs;
        private double innerDiameter;
        private double innerRadius;
        private bool makeEdge;
        private double maxGridX;
        private double maxGridY;
        private double minGridX;
        private double minGridY;
        private List<GapDef> spokePoints;
        private List<Bar> vBars;
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

        private void CreateHorizontalBarEdges()
        {
            for (int i = 0; i < horizontalBars; i++)
            {
                // connect central horizontal bars to ring to their left
                if (hBars[i].G2.Start.X < cx)
                {
                    // left of center, top and bottom
                    double startAngle = hBars[i].G2.Start.theta;
                    double endAngle = hBars[i].G2.Finish.theta;
                    if (startAngle > 0 && endAngle < 0)
                    {
                        endAngle += Math.PI * 2.0;
                    }
                    double dt = (endAngle - startAngle) / 10.0;
                    double dy = horizontalBarThickness / 10.0;
                    double y = hBars[(int)horizontalBars - i - 1].G2.Start.Y;
                    y += horizontalBarThickness;
                    double theta = startAngle;
                    Point p0;
                    Point p1;
                    Point p2;
                    Point p3;
                    int v0;
                    int v1;
                    int v2;
                    int v3;
                    while (theta < endAngle)
                    {
                        p0 = CalcPoint(theta, innerRadius);
                        p1 = CalcPoint(theta + dt, innerRadius);
                        p2 = new Point(minGridX, p0.Y);
                        p3 = new Point(minGridX, p1.Y);

                        v0 = AddVertice(p0.X, cy, p0.Y);
                        v1 = AddVertice(p1.X, cy, p1.Y);
                        v2 = AddVertice(p2.X, cy, p2.Y);
                        v3 = AddVertice(p3.X, cy, p3.Y);

                        AddFace(v0, v1, v2);
                        AddFace(v1, v3, v2);

                        v0 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                        v1 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                        v2 = AddVertice(p2.X, cy + grilleWidth, p2.Y);
                        v3 = AddVertice(p3.X, cy + grilleWidth, p3.Y);

                        AddFace(v0, v2, v1);
                        AddFace(v1, v2, v3);

                        y -= dy;
                        theta += dt; ;
                    }
                    y += dy;
                    p0 = CalcPoint(endAngle, innerRadius);
                    p1 = new Point(minGridX, p0.Y);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);

                    p0 = CalcPoint(startAngle, innerRadius);
                    p1 = new Point(minGridX, p0.Y);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);

                    // right of center
                    startAngle = hBars[i].G1.Start.theta;
                    endAngle = hBars[i].G1.Finish.theta;
                    if (startAngle > 0 && endAngle < 0)
                    {
                        endAngle += Math.PI * 2.0;
                    }
                    dt = (endAngle - startAngle) / 10.0;
                    dy = horizontalBarThickness / 10.0;
                    y = hBars[(int)horizontalBars - i - 1].G1.Start.Y;
                    y -= horizontalBarThickness;
                    theta = startAngle;
                    while (theta < endAngle)
                    {
                        p2 = CalcPoint(theta, innerRadius);
                        p3 = CalcPoint(theta + dt, innerRadius);
                        p0 = new Point(maxGridX, p2.Y);
                        p1 = new Point(maxGridX, p3.Y);

                        v0 = AddVertice(p0.X, cy, p0.Y);
                        v1 = AddVertice(p1.X, cy, p1.Y);
                        v2 = AddVertice(p2.X, cy, p2.Y);
                        v3 = AddVertice(p3.X, cy, p3.Y);

                        AddFace(v0, v2, v1);
                        AddFace(v1, v2, v3);

                        v0 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                        v1 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                        v2 = AddVertice(p2.X, cy + grilleWidth, p2.Y);
                        v3 = AddVertice(p3.X, cy + grilleWidth, p3.Y);

                        AddFace(v0, v1, v2);
                        AddFace(v1, v3, v2);

                        y += dy;
                        theta += dt; ;
                    }
                    y -= dy;
                    p0 = CalcPoint(endAngle, innerRadius);
                    p1 = new Point(maxGridX, p0.Y);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);

                    p0 = CalcPoint(startAngle, innerRadius);
                    p1 = new Point(maxGridX, p0.Y);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }

                RingGap(hBars[i].G1, hBars[i].G2);
            }
        }

        private void CreateHorizontalCentralBars(Crossing[,] crossings)
        {
            for (int i = 0; i < verticalBars - 1; i++)
            {
                for (int j = 0; j < horizontalBars; j++)
                {
                    // create a box from the right of the current crossing to the left of the next one
                    int v4 = AddVertice(crossings[i, j].p1.X, cy, crossings[i, j].p1.Y);
                    int v5 = AddVertice(crossings[i + 1, j].p0.X, cy, crossings[i + 1, j].p0.Y);
                    int v6 = AddVertice(crossings[i + 1, j].p3.X, cy, crossings[i + 1, j].p3.Y);
                    int v7 = AddVertice(crossings[i, j].p2.X, cy, crossings[i, j].p2.Y);

                    int v8 = AddVertice(crossings[i, j].p1.X, cy + grilleWidth, crossings[i, j].p1.Y);
                    int v9 = AddVertice(crossings[i + 1, j].p0.X, cy + grilleWidth, crossings[i + 1, j].p0.Y);
                    int v10 = AddVertice(crossings[i + 1, j].p3.X, cy + grilleWidth, crossings[i + 1, j].p3.Y);
                    int v11 = AddVertice(crossings[i, j].p2.X, cy + grilleWidth, crossings[i, j].p2.Y);

                    // bottom
                    AddFace(v4, v6, v5);
                    AddFace(v4, v6, v7);

                    // top
                    AddFace(v8, v9, v10);
                    AddFace(v8, v10, v11);

                    //front
                    AddFace(v4, v5, v9);
                    AddFace(v4, v9, v8);

                    //Back
                    AddFace(v7, v10, v6);
                    AddFace(v7, v11, v10);
                }
            }
        }

        private void CreateVerticalBarEdges()
        {
            for (int i = 0; i < verticalBars; i++)
            {
                // connect central vertical bars to ring to their top
                // if (vBars[i].G2.Start.X < cx)
                {
                    double startAngle = vBars[i].G2.Start.theta;
                    double endAngle = vBars[i].G2.Finish.theta;
                    if (startAngle > 0 && endAngle < 0)
                    {
                        endAngle += Math.PI * 2.0;
                    }
                    double dt = (endAngle - startAngle) / 10.0;
                    double dx = verticalBarThickness / 10.0;
                    double x = vBars[(int)verticalBars - i - 1].G2.Finish.X;
                    // x += verticalBarThickness;
                    double theta = startAngle;
                    Point p0;
                    Point p1;
                    Point p2;
                    Point p3;
                    int v0;
                    int v1;
                    int v2;
                    int v3;
                    while (theta < endAngle)
                    {
                        p0 = CalcPoint(theta, innerRadius);
                        p1 = CalcPoint(theta + dt, innerRadius);

                        p2 = new Point(p0.X, minGridY);
                        p3 = new Point(p1.X, minGridY);

                        v0 = AddVertice(p0.X, cy, p0.Y);
                        v1 = AddVertice(p1.X, cy, p1.Y);
                        v2 = AddVertice(p2.X, cy, p2.Y);
                        v3 = AddVertice(p3.X, cy, p3.Y);

                        AddFace(v0, v1, v2);
                        AddFace(v1, v3, v2);

                        v0 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                        v1 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                        v2 = AddVertice(p2.X, cy + grilleWidth, p2.Y);
                        v3 = AddVertice(p3.X, cy + grilleWidth, p3.Y);

                        AddFace(v0, v2, v1);
                        AddFace(v1, v2, v3);

                        x += dx;
                        theta += dt;
                    }
                    x -= dx;
                    p0 = CalcPoint(endAngle, innerRadius);

                    p1 = new Point(p0.X, minGridY);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);

                    p0 = CalcPoint(startAngle, innerRadius);
                    p1 = new Point(x - verticalBarThickness, minGridY);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);

                    startAngle = vBars[i].G1.Start.theta;
                    endAngle = vBars[i].G1.Finish.theta;
                    if (startAngle > 0 && endAngle < 0)
                    {
                        endAngle += Math.PI * 2.0;
                    }
                    dt = (endAngle - startAngle) / 10.0;

                    x = vBars[i].G1.Start.X;
                    // x += verticalBarThickness;
                    theta = startAngle;

                    while (theta < endAngle)
                    {
                        p0 = CalcPoint(theta, innerRadius);
                        p1 = CalcPoint(theta + dt, innerRadius);
                        //   p2 = new Point(x, maxGridY);
                        //   p3 = new Point(x - dx, maxGridY);
                        p2 = new Point(p0.X, maxGridY);
                        p3 = new Point(p1.X, maxGridY);

                        v0 = AddVertice(p0.X, cy, p0.Y);
                        v1 = AddVertice(p1.X, cy, p1.Y);
                        v2 = AddVertice(p2.X, cy, p2.Y);
                        v3 = AddVertice(p3.X, cy, p3.Y);

                        AddFace(v0, v1, v2);
                        AddFace(v1, v3, v2);

                        v0 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                        v1 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                        v2 = AddVertice(p2.X, cy + grilleWidth, p2.Y);
                        v3 = AddVertice(p3.X, cy + grilleWidth, p3.Y);

                        AddFace(v0, v2, v1);
                        AddFace(v1, v2, v3);

                        x -= dx;
                        theta += dt;
                    }
                    x += dx;
                    p0 = CalcPoint(endAngle, innerRadius);
                    p1 = new Point(p0.X, maxGridY);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);

                    p0 = CalcPoint(startAngle, innerRadius);
                    p1 = new Point(p0.X, maxGridY);
                    v0 = AddVertice(p0.X, cy, p0.Y);
                    v1 = AddVertice(p1.X, cy, p1.Y);
                    v2 = AddVertice(p1.X, cy + grilleWidth, p1.Y);
                    v3 = AddVertice(p0.X, cy + grilleWidth, p0.Y);
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }

                RingGap(vBars[i].G1, vBars[i].G2);
            }
        }

        private void CreateVerticalCentralBars(Crossing[,] crossings)
        {
            for (int i = 0; i < verticalBars; i++)
            {
                for (int j = 0; j < horizontalBars - 1; j++)
                {
                    // create a box from the right of the current crossing to the left of the next one
                    int v4 = AddVertice(crossings[i, j].p0.X, cy, crossings[i, j].p0.Y);
                    int v5 = AddVertice(crossings[i, j].p1.X, cy, crossings[i, j].p1.Y);
                    int v6 = AddVertice(crossings[i, j + 1].p2.X, cy, crossings[i, j + 1].p2.Y);
                    int v7 = AddVertice(crossings[i, j + 1].p3.X, cy, crossings[i, j + 1].p3.Y);

                    int v8 = AddVertice(crossings[i, j].p0.X, cy + grilleWidth, crossings[i, j].p0.Y);
                    int v9 = AddVertice(crossings[i, j].p1.X, cy + grilleWidth, crossings[i, j].p1.Y);
                    int v10 = AddVertice(crossings[i, j + 1].p2.X, cy + grilleWidth, crossings[i, j + 1].p2.Y);
                    int v11 = AddVertice(crossings[i, j + 1].p3.X, cy + grilleWidth, crossings[i, j + 1].p3.Y);

                    // bottom
                    AddFace(v4, v5, v6);
                    AddFace(v4, v7, v6);

                    // top
                    AddFace(v8, v10, v9);
                    AddFace(v8, v11, v10);

                    //left
                    AddFace(v4, v11, v8);
                    AddFace(v4, v7, v11);

                    //Right
                    AddFace(v5, v9, v10);
                    AddFace(v5, v10, v6);
                }
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

        private void GenerateCrossingGrid()
        {
            double w = verticalBarThickness / 2.0;
            double h = horizontalBarThickness / 2.0;
            // do the front and back of the crossings
            Crossing[,] crossings = new Crossing[(int)verticalBars, (int)horizontalBars];

            // just add the litle front and back squares where the verticals and horizontals cross.
            // Record where they are in the crossing structs.
            // These are useful later
            for (int i = 0; i < verticalBars; i++)
            {
                double px = verticalXs[i];
                for (int j = 0; j < horizontalBars; j++)
                {
                    double py = horizontalYs[j];
                    crossings[i, j] = new Crossing();

                    crossings[i, j].p0 = new Point(px - w, py + h);
                    crossings[i, j].p1 = new Point(px + w, py + h);
                    crossings[i, j].p2 = new Point(px + w, py - h);
                    crossings[i, j].p3 = new Point(px - w, py - h);

                    int v0 = AddVertice(px - w, cy + grilleWidth, py + h);
                    int v1 = AddVertice(px + w, cy + grilleWidth, py + h);
                    int v2 = AddVertice(px + w, cy + grilleWidth, py - h);
                    int v3 = AddVertice(px - w, cy + grilleWidth, py - h);

                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);

                    v0 = AddVertice(px - w, cy, py + h);
                    v1 = AddVertice(px + w, cy, py + h);
                    v2 = AddVertice(px + w, cy, py - h);
                    v3 = AddVertice(px - w, cy, py - h);

                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);
                }
            }

            // add the crossing boxes in the middle
            if (verticalBars > 1)
            {
                CreateHorizontalCentralBars(crossings);
                CreateVerticalBarEdges();
            }

            if (horizontalBars > 1)
            {
                CreateVerticalCentralBars(crossings);
                CreateHorizontalBarEdges();
            }
        }

        private void GenerateHorizontalBars()
        {
            if (hBars != null && hBars.Count > 0)
            {
                foreach (Bar b in hBars)
                {
                    Point p1 = new Point(b.G1.Start.X, b.G1.Start.Y);
                    Point p2 = new Point(b.G1.Finish.X, b.G1.Finish.Y);
                    Point p3 = new Point(b.G2.Start.X, b.G2.Start.Y);
                    Point p4 = new Point(b.G2.Finish.X, b.G2.Finish.Y);
                    SideWall(p1, p4);
                    SideWall(p3, p2);
                    HTopAndBottom(b.G1, b.G2);
                    RingGap(b.G1, b.G2);
                }
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

        private void GenerateRing()
        {
            double theta = -Math.PI;
            double dt = Math.PI / divisions;
            double theta2 = theta + dt;

            int p0;
            int p1;
            int p2;
            int p3;
            Point pn0;
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
                    SideWall(p3, p1);
                    SideWall(p2, p4);
                    VTopAndBottom(b.G1, b.G2);
                    RingGap(b.G1, b.G2);
                }
            }
        }

        private void GenerateWithEdge()
        {
            this.innerRadius = grilleRadius - edgeThickness;
            this.innerDiameter = 2.0 * this.innerRadius;
            if (verticalBars > 0 && horizontalBars > 0)
            {
                MakeVerticalBarPolarCoords();
                MakeHorizontalBarPolarCoords();
                System.Diagnostics.Debug.WriteLine("Before Sort");
                DumpSpokes();
                SortSpokesByTheta();

                GenerateInnerEdge();
                GenerateRing();
                GenerateCrossingGrid();
            }
            else if (verticalBars > 0)
            {
                MakeVerticalBarPolarCoords();
                SortSpokesByTheta();
                DumpSpokes();
                GenerateInnerEdge();
                GenerateRing();
                GenerateVerticalBars();
            }
            else
            if (horizontalBars > 0)
            {
                MakeHorizontalBarPolarCoords();
                SortSpokesByTheta();
                DumpSpokes();
                GenerateInnerEdge();
                GenerateRing();
                GenerateHorizontalBars();
            }
        }

        private void GenerateWithoutEdge()
        {
            this.innerRadius = grilleRadius;
            this.innerDiameter = 2.0 * this.innerRadius;

            if (verticalBars > 0 && horizontalBars > 0)
            {
                MakeVerticalBarPolarCoords();
                MakeHorizontalBarPolarCoords();
                System.Diagnostics.Debug.WriteLine("Before Sort");

                SortSpokesByTheta();
                GenerateCrossingGrid();
            }
            else if (verticalBars > 0)
            {
                MakeVerticalBarPolarCoords();
                SortSpokesByTheta();
                GenerateVerticalBars();
            }
            else
            if (horizontalBars > 0)
            {
                MakeHorizontalBarPolarCoords();
                SortSpokesByTheta();
                GenerateHorizontalBars();
            }
        }

        private void HTopAndBottom(GapDef a, GapDef b)
        {
            double startAngle = a.Start.theta;
            double endAngle = a.Finish.theta;
            if (startAngle > 0 && endAngle < 0)
            {
                endAngle += Math.PI * 2.0;
            }

            double startAngle2 = b.Start.theta;
            double endAngle2 = b.Finish.theta;
            if (startAngle2 > 0 && endAngle2 < 0)
            {
                endAngle2 += Math.PI * 2.0;
            }

            double dt = (endAngle - startAngle) / 10.0;

            double theta = startAngle;
            double phi = endAngle2;
            while (theta < endAngle)
            {
                double g = dt;
                if (theta + g > endAngle)
                {
                    g = endAngle - theta;
                }
                Point pn0 = CalcPoint(theta, innerRadius);
                Point pn1 = CalcPoint(theta + g, innerRadius);

                Point pn2 = CalcPoint(phi, innerRadius);
                Point pn3 = CalcPoint(phi - g, innerRadius);
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
                phi -= dt;
            }
        }

        private void MakeHorizontalBarPolarCoords()
        {
            maxGridY = Double.MinValue;
            minGridY = Double.MaxValue;
            horizontalYs = new List<double>();
            hBars = new List<Bar>();
            if (horizontalBars > 0)
            {
                double availableSpace = (innerDiameter - 2 * verticalBarThickness);
                double hspacing = (availableSpace) / (horizontalBars + 1);
                double origin = cy - innerRadius + verticalBarThickness;
                for (int i = 0; i < horizontalBars; i++)
                {
                    Bar bar = new Bar();

                    // work out where vertical bar crosses the x axies.
                    double hy = origin + ((i + 1) * hspacing);
                    horizontalYs.Add(hy);
                    double hy1 = hy - (horizontalBarThickness / 2.0);
                    double hy2 = hy + (horizontalBarThickness / 2.0);
                    // distance of this from the centre
                    double hdy1 = cy - hy1;
                    double hdy2 = cy - hy2;

                    double h1 = Math.Sqrt((innerRadius * innerRadius) - (hdy1 * hdy1));
                    double h2 = Math.Sqrt((innerRadius * innerRadius) - (hdy2 * hdy2));
                    PolarPoint p1 = new PolarPoint();
                    p1.barId = "H" + i.ToString();
                    p1.X = cx + h1;
                    p1.Y = hy1;
                    double t1 = Math.Atan2(hdy1, h1);

                    p1.theta = t1;

                    p1.r = innerRadius;
                    p1.vertical = false;
                    p1.index = 0;

                    PolarPoint p2 = new PolarPoint();
                    p2.barId = "H" + i.ToString();
                    p2.X = cx + h2;
                    p2.Y = hy2;
                    double t2 = Math.Atan2(hdy2, h2);

                    p2.theta = t2;
                    p2.r = innerRadius;
                    p2.vertical = false;
                    p2.index = 1;

                    GapDef right = new GapDef();
                    if (p1.theta > p2.theta)
                    {
                        right.Start = p2;
                        right.Finish = p1;
                    }
                    else
                    {
                        right.Start = p1;
                        right.Finish = p2;
                    }
                    spokePoints.Add(right);
                    bar.G1 = right;

                    PolarPoint p3 = new PolarPoint();
                    p3.barId = "H" + i.ToString();
                    p3.X = cx - h1;
                    p3.Y = hy1;
                    double t3 = Math.Atan2(hdy1, -h1);

                    p3.theta = t3;
                    p3.r = innerRadius;
                    p3.vertical = false;
                    p3.index = 3;

                    PolarPoint p4 = new PolarPoint();
                    p4.barId = "H" + i.ToString();
                    p4.X = cx - h2;
                    p4.Y = hy2;
                    double t4 = Math.Atan2(hdy2, -h2);

                    p4.theta = t4;
                    p4.r = innerRadius;
                    p4.vertical = false;
                    p4.index = 4;
                    GapDef left = new GapDef();
                    if (Math.Sign(p3.theta) == Math.Sign(p4.theta))
                    {
                        if (p3.theta > p4.theta)
                        {
                            left.Start = p4;
                            left.Finish = p3;
                        }
                        else
                        {
                            left.Start = p3;
                            left.Finish = p4;
                        }
                    }
                    else
                    {
                        if (Math.Sign(p3.theta) == 1)
                        {
                            left.Start = p3;
                            left.Finish = p4;
                        }
                        else
                        {
                            {
                                left.Start = p4;
                                left.Finish = p3;
                            }
                        }
                    }
                    spokePoints.Add(left);
                    bar.G2 = left;

                    hBars.Add(bar);
                    minGridY = Math.Min(minGridY, hy1);
                    minGridY = Math.Min(minGridY, hy2);
                    maxGridY = Math.Max(maxGridY, hy1);
                    maxGridY = Math.Max(maxGridY, hy2);
                }
            }
        }

        private void MakeVerticalBarPolarCoords()
        {
            maxGridX = Double.MinValue;
            minGridX = Double.MaxValue;
            verticalXs = new List<double>();
            vBars = new List<Bar>();
            double halfBarWidth = (verticalBarThickness / 2.0);
            if (verticalBars > 0)
            {
                double availableSpace = (innerDiameter - 2 * horizontalBarThickness);
                double vspacing = (availableSpace) / (verticalBars + 1);
                double origin = cx - innerRadius + horizontalBarThickness;

                for (int i = 0; i < verticalBars; i++)
                {
                    Bar bar = new Bar();

                    // work out where vertical bar crosses the x axies.
                    double vx = origin + ((i + 1) * vspacing);
                    verticalXs.Add(vx);
                    double vx1 = vx - halfBarWidth;
                    double vx2 = vx + halfBarWidth;
                    // distance of this from the centre
                    double vdx1 = cx - vx1;
                    double vdx2 = cx - vx2;
                    if (Math.Abs(vdx1) > innerRadius || Math.Abs(vdx2) > innerRadius)
                    {
                        bool breakhere = true;
                    }

                    //Uppers
                    double h1 = Math.Sqrt((innerRadius * innerRadius) - (vdx1 * vdx1));
                    double h2 = Math.Sqrt((innerRadius * innerRadius) - (vdx2 * vdx2));
                    PolarPoint p1 = new PolarPoint();
                    p1.barId = "V" + i.ToString();
                    p1.X = vx1;
                    p1.Y = cy + h1;
                    // double t1 = Math.Atan2(h1 + cy, vdx1);
                    double t1 = Math.Atan2(h1, vx1);
                    p1.theta = t1;

                    p1.r = innerRadius;
                    p1.vertical = true;
                    p1.index = 0;

                    PolarPoint p2 = new PolarPoint();
                    p2.barId = "V" + i.ToString();
                    p2.X = vx2;
                    p2.Y = cy + h2;

                    double t2 = Math.Atan2(h2, vx2);

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
                    minGridX = Math.Min(minGridX, vx1);
                    minGridX = Math.Min(minGridX, vx2);
                    maxGridX = Math.Max(maxGridX, vx1);
                    maxGridX = Math.Max(maxGridX, vx2);
                }
            }
        }

        private void RingGap(GapDef a, GapDef b)
        {
            double startAngle = a.Start.theta;
            double endAngle = a.Finish.theta;
            if (startAngle > 0 && endAngle < 0)
            {
                endAngle += Math.PI * 2.0;
            }
            double dt = (endAngle - startAngle) / 10.0;
            double theta = startAngle;
            while (theta < endAngle)
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

            startAngle = b.Start.theta;
            endAngle = b.Finish.theta;
            if (startAngle > 0 && endAngle < 0)
            {
                endAngle += Math.PI * 2.0;
            }
            theta = startAngle;
            while (theta < endAngle)
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

        private void SideWall(Point p1, Point p2)
        {
            int v0 = AddVertice(p1.X, 0, p1.Y);
            int v1 = AddVertice(p2.X, 0, p2.Y);
            int v2 = AddVertice(p2.X, grilleWidth, p2.Y);
            int v3 = AddVertice(p1.X, grilleWidth, p1.Y);
            AddFace(v0, v2, v1);
            AddFace(v0, v3, v2);
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
                        if (spokePoints[i].Start.theta > spokePoints[i + 1].Start.theta)
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
                //check for special case where horizontal bar starts close to PI and ends just
                // over -PI
                if (spokePoints[i].Start.theta > 0 && spokePoints[i].Finish.theta < 0)
                {
                    double TwoPi = Math.PI * 2.0;
                    if (phi < spokePoints[i].Start.theta && phi2 >= spokePoints[i].Start.theta && phi2 < spokePoints[i].Finish.theta + TwoPi)
                    {
                        phi2 = spokePoints[i].Start.theta;
                    }
                    else
                    if (phi < spokePoints[i].Finish.theta && phi2 >= spokePoints[i].Finish.theta)
                    {
                        phi = spokePoints[i].Finish.theta;
                    }
                    else
                         if (phi >= spokePoints[i].Start.theta &&
                                phi <= spokePoints[i].Finish.theta + TwoPi &&
                                phi2 >= spokePoints[i].Start.theta &&
                                phi2 <= spokePoints[i].Finish.theta + TwoPi)
                    {
                        // tell caller not use do this seqment at all
                        res = false;
                    }
                    else
                        if (phi >= -Math.PI &&
                                phi <= spokePoints[i].Finish.theta &&
                                phi2 >= -Math.PI &&
                                phi2 <= spokePoints[i].Finish.theta + TwoPi)
                    {
                        // tell caller not use do this seqment at all
                        res = false;
                    }
                }
                else
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
            }

            return res;
        }

        private void VTopAndBottom(GapDef a, GapDef b)
        {
            double dt = (a.Finish.theta - a.Start.theta) / 10.0;

            double theta = a.Start.theta;
            while (theta < a.Finish.theta)
            {
                double g = dt;
                if (theta + g > a.Finish.theta)
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

        internal struct Crossing
        {
            internal Point p0;
            internal Point p1;
            internal Point p2;
            internal Point p3;
        }
    }
}