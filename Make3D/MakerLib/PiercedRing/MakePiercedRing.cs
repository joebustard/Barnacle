using Barnacle.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PiercedRingMaker : MakerBase
    {
        private const int numberOfPointsAroundRim = 72;
        private const int numerOfPointsAoundHole = 20;
        private double diskHeight;
        private double diskInnerRadius;
        private double diskRadius;
        private double distanceFromEdge;
        private List<Point> holeInner;
        private List<Point> holeOuter;
        private double holeRadius;
        private double numberOfHoles;

        public PiercedRingMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
            holeInner = new List<Point>();
            holeOuter = new List<Point>();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            int c0;
            int c1;
            int c2;
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            CreateOctree(new Point3D(-diskRadius - 1, diskHeight - 0.1, -diskRadius - 1), new Point3D(diskRadius + 1, diskHeight + 0.1, diskRadius + 1));
            MakeMasterHole(holeRadius, holeOuter, holeInner);
            double hrad = diskRadius - holeRadius - distanceFromEdge;
            Point holeCentre = new Point(hrad, 0);
            Hole[] holes = new Hole[(int)numberOfHoles];

            double dtheta = (Math.PI * 2) / numberOfHoles;
            double theta = 0;
            InitialiseHoles(holeCentre, holes, dtheta, theta);

            // make a list of points for the disk rim
            List<Point> rim = new List<Point>();
            CalculateRimPoints(rim);

            // construct a polygon suitable for triangulation
            // start by adding the rim points
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (System.Windows.Point p in rim)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            pf.Add(new System.Drawing.PointF((float)rim[0].X, (float)rim[0].Y));
            // combine all hole outers into a single list but in reverse
            List<Point> outerPoly = new List<Point>();
            CombineHoleOuters(holes, outerPoly);

            // which point in the outer is closest to the end of the rim ones
            int closest = FindClosest(outerPoly, rim);

            // add these outerPoly points to the triangulation polygon starting AND ENDING
            // on the closest one
            int index = closest;
            do
            {
                Point p = outerPoly[index];
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                index++;
                if (index == outerPoly.Count)
                {
                    index = 0;
                }
            } while (index != closest);

            pf.Add(new System.Drawing.PointF((float)rim[0].X, (float)rim[0].Y));

            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                c0 = AddVerticeOctTree(t.Points[0].X, 0.0, t.Points[0].Y);
                c1 = AddVerticeOctTree(t.Points[1].X, 0.0, t.Points[1].Y);
                c2 = AddVerticeOctTree(t.Points[2].X, 0.0, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                c0 = AddVerticeOctTree(t.Points[0].X, diskHeight, t.Points[0].Y);
                c1 = AddVerticeOctTree(t.Points[1].X, diskHeight, t.Points[1].Y);
                c2 = AddVerticeOctTree(t.Points[2].X, diskHeight, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);
            }

            FillInHoleOnOutside(out c0, out c1, out c2, holes, rim);

            // now the inners
            pf.Clear();
            List<Point> innerPoly = new List<Point>();
            for (int i = 0; i < numberOfHoles; i++)
            {
                // the inners of the holes are added in reverse order
                for (int j = 0; j < holes[i].InnerPoints.Count; j++)
                {
                    innerPoly.Add(new Point(holes[i].InnerPoints[j].X, holes[i].InnerPoints[j].Y));
                }
            }

            // inner rim
            List<Point> innerRim = new List<Point>();
            Point point = new Point(diskInnerRadius, 0);
            dtheta = (Math.PI * 2) / (numberOfPointsAroundRim - 1);
            theta = 0;
            for (int i = 0; i < numberOfPointsAroundRim; i++)
            {
                innerRim.Add(Hole.Rotate(point, theta));
                theta += dtheta;
            }

            //   pf.Add(new System.Drawing.PointF((float)rim[0].X, (float)rim[0].Y));

            foreach (System.Windows.Point p in innerRim)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            closest = FindClosest(innerPoly, innerRim);
            index = closest;
            do
            {
                Point p = innerPoly[index];
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                index--;
                if (index < 0)
                {
                    index = innerPoly.Count - 1;
                }
            } while (index != closest);
            pf.Add(new System.Drawing.PointF((float)innerRim[0].X, (float)innerRim[0].Y));
            ply = new TriangulationPolygon();
            ply.Points = pf.ToArray();
            tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                c0 = AddVerticeOctTree(t.Points[0].X, 0.0, t.Points[0].Y);
                c1 = AddVerticeOctTree(t.Points[1].X, 0.0, t.Points[1].Y);
                c2 = AddVerticeOctTree(t.Points[2].X, 0.0, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
                c0 = AddVerticeOctTree(t.Points[0].X, diskHeight, t.Points[0].Y);
                c1 = AddVerticeOctTree(t.Points[1].X, diskHeight, t.Points[1].Y);
                c2 = AddVerticeOctTree(t.Points[2].X, diskHeight, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);
            }
            FillInHoleOnInside(out c0, out c1, out c2, holes, innerRim);
            innerRim.Reverse();
            CloseRim(innerRim);
            CloseRim(rim);

            CloseHoles(holes);
        }

        public void SetValues(double diskRadius,
                              double diskInnerRadius,
                              double holeRadius,
                              double distanceFromEdge,
                              double numberOfHoles,
                              double diskHeight)
        {
            this.diskRadius = diskRadius;
            this.diskInnerRadius = diskInnerRadius;
            this.holeRadius = holeRadius;
            this.distanceFromEdge = distanceFromEdge;
            this.numberOfHoles = numberOfHoles;
            this.diskHeight = diskHeight;
        }

        private void CalculateRimPoints(List<Point> rim)
        {
            double dtheta = (Math.PI * 2) / (numberOfPointsAroundRim - 1);
            double theta = 0;
            Point rimPoint = new Point(diskRadius, 0);
            for (int i = 0; i < numberOfPointsAroundRim; i++)
            {
                rim.Add(Hole.Rotate(rimPoint, theta));
                theta += dtheta;
            }
        }

        private void CloseHoles(Hole[] holes)
        {
            for (int h = 0; h < numberOfHoles; h++)
            {
                Hole hole = holes[h];
                List<Point> edge = new List<Point>();
                for (int i = 0; i < hole.OuterPoints.Count; i++)
                {
                    edge.Add(hole.OuterPoints[i]);
                }

                for (int i = hole.InnerPoints.Count - 1; i >= 0; i--)
                {
                    edge.Add(hole.InnerPoints[i]);
                }

                for (int i = 0; i < edge.Count; i++)
                {
                    int j = i + 1;
                    if (j == edge.Count)
                    {
                        j = 0;
                    }

                    int c0 = AddVerticeOctTree(edge[i].X, 0.0, edge[i].Y);
                    int c1 = AddVerticeOctTree(edge[j].X, 0.0, edge[j].Y);
                    int c2 = AddVerticeOctTree(edge[i].X, diskHeight, edge[i].Y);
                    int c3 = AddVerticeOctTree(edge[j].X, diskHeight, edge[j].Y);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c1);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }
            }
        }

        private void CloseRim(List<Point> rim)
        {
            // close the rim
            for (int i = 0; i < rim.Count; i++)
            {
                int j = i + 1;
                if (j == rim.Count)
                {
                    j = 0;
                }

                int c0 = AddVerticeOctTree(rim[i].X, 0.0, rim[i].Y);
                int c1 = AddVerticeOctTree(rim[j].X, 0.0, rim[j].Y);
                int c2 = AddVerticeOctTree(rim[i].X, diskHeight, rim[i].Y);
                int c3 = AddVerticeOctTree(rim[j].X, diskHeight, rim[j].Y);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                Faces.Add(c1);
                Faces.Add(c2);
                Faces.Add(c3);
            }
        }

        private void CombineHoleOuters(Hole[] holes, List<Point> outerPoly)
        {
            for (int i = 0; i < numberOfHoles; i++)
            {
                for (int j = 0; j < holes[i].OuterPoints.Count; j++)
                {
                    outerPoly.Insert(0, new Point(holes[i].OuterPoints[j].X, holes[i].OuterPoints[j].Y));
                }
            }
        }

        private double Distance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private void FillInHoleOnInside(out int c0, out int c1, out int c2, Hole[] holes, List<Point> innerRim)
        {
            // There is a missing triangle on the front and back that I can't ftrack down.
            // This is a dirty hack
            int l = holes[0].InnerPoints.Count;
            int m = l / 2;
            Point p0 = holes[0].InnerPoints[m - 1];
            Point p1 = holes[0].InnerPoints[m];
            Point p2 = innerRim[0];
            c0 = AddVerticeOctTree(p0.X, 0.0, p0.Y);
            c1 = AddVerticeOctTree(p1.X, 0.0, p1.Y);
            c2 = AddVerticeOctTree(p2.X, 0.0, p2.Y);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            c0 = AddVerticeOctTree(p0.X, diskHeight, p0.Y);
            c1 = AddVerticeOctTree(p1.X, diskHeight, p1.Y);
            c2 = AddVerticeOctTree(p2.X, diskHeight, p2.Y);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            p0 = holes[0].InnerPoints[m];
            p1 = holes[0].InnerPoints[m + 1];
            p2 = innerRim[0];
            c0 = AddVerticeOctTree(p0.X, 0.0, p0.Y);
            c1 = AddVerticeOctTree(p1.X, 0.0, p1.Y);
            c2 = AddVerticeOctTree(p2.X, 0.0, p2.Y);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            c0 = AddVerticeOctTree(p0.X, diskHeight, p0.Y);
            c1 = AddVerticeOctTree(p1.X, diskHeight, p1.Y);
            c2 = AddVerticeOctTree(p2.X, diskHeight, p2.Y);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);
        }

        private void FillInHoleOnOutside(out int c0, out int c1, out int c2, Hole[] holes, List<Point> rim)
        {
            // There is a missing triangle on the front and back that I can't ftrack down.
            // This is a dirty hack
            int l = holes[0].InnerPoints.Count;
            int m = l / 2;
            Point p0 = holes[0].OuterPoints[m - 1];
            Point p1 = holes[0].OuterPoints[m];
            Point p2 = rim[0];
            c0 = AddVerticeOctTree(p0.X, 0.0, p0.Y);
            c1 = AddVerticeOctTree(p1.X, 0.0, p1.Y);
            c2 = AddVerticeOctTree(p2.X, 0.0, p2.Y);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            c0 = AddVerticeOctTree(p0.X, diskHeight, p0.Y);
            c1 = AddVerticeOctTree(p1.X, diskHeight, p1.Y);
            c2 = AddVerticeOctTree(p2.X, diskHeight, p2.Y);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            p0 = holes[0].OuterPoints[m];
            p1 = holes[0].OuterPoints[m + 1];
            p2 = rim[0];
            c0 = AddVerticeOctTree(p0.X, 0.0, p0.Y);
            c1 = AddVerticeOctTree(p1.X, 0.0, p1.Y);
            c2 = AddVerticeOctTree(p2.X, 0.0, p2.Y);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            c0 = AddVerticeOctTree(p0.X, diskHeight, p0.Y);
            c1 = AddVerticeOctTree(p1.X, diskHeight, p1.Y);
            c2 = AddVerticeOctTree(p2.X, diskHeight, p2.Y);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);
        }

        private int FindClosest(List<Point> l1, List<Point> l2)
        {
            // which point in the outer is closest to the end of the rim ones
            int closest = -1;
            double closeDistance = double.MaxValue;

            for (int i = 0; i < l1.Count; i++)
            {
                double d = Distance(l1[i], l2[l2.Count - 1]);
                if (d < closeDistance)
                {
                    closest = i;
                    closeDistance = d;
                }
            }
            return closest;
        }

        private double InitialiseHoles(Point holeCentre, Hole[] holes, double dtheta, double theta)
        {
            for (int i = 0; i < numberOfHoles; i++)
            {
                holes[i] = new Hole();
                holes[i].AngleFromDiskCentre = theta;
                holes[i].Centre = Hole.Rotate(holeCentre, theta);
                holes[i].SetPoints(holeOuter, holeInner);

                theta += dtheta;
            }

            return theta;
        }

        private void MakeMasterHole(double holeRadius, List<Point> holeOuter, List<Point> holeInner)
        {
            double dt = (Math.PI * 2) / numerOfPointsAoundHole;
            double theta = -Math.PI / 2.0;
            Point point = new Point(holeRadius, 0);
            for (int i = 0; i < numerOfPointsAoundHole; i++)
            {
                if (i < numerOfPointsAoundHole / 2)
                {
                    holeOuter.Add(Hole.Rotate(point, theta));
                }
                else
                {
                    holeInner.Add(Hole.Rotate(point, theta));
                }
                theta += dt;
            }
            // sneaky trick
            holeOuter.Add(new Point(holeInner[0].X, holeInner[0].Y));
            holeInner.Add(new Point(holeOuter[0].X, holeOuter[0].Y));
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("DiskRadius", 0.1, 200);
            paramLimits.AddLimit("DiskInnerRadius", 0.1, 200);
            paramLimits.AddLimit("HoleRadius", 0.1, 200);
            paramLimits.AddLimit("DistanceFromEdge", 0.1, 200);
            paramLimits.AddLimit("NumberOfHoles", 3, 200);
            paramLimits.AddLimit("DiskHeight", 0.1, 200);
        }
    }
}