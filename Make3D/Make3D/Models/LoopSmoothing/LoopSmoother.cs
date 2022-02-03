using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopSmoother
    {
        private Int32Collection faces;
        private List<LoopEdge> loopEdges;
        private List<LoopFace> loopFaces;
        private List<LoopPoint> loopPoints;
        private Point3DCollection vertices;

        public LoopSmoother()
        {
            loopPoints = new List<LoopPoint>();
            loopEdges = new List<LoopEdge>();
            loopFaces = new List<LoopFace>();
            vertices = new Point3DCollection();
            faces = new Int32Collection();
        }

        internal void Smooth(ref Point3DCollection p3col, ref Int32Collection icol)
        {
            InitialiseData(p3col, icol);

            //  GenerateNewFacePoints();
            GenerateNewEdgePoints();
            CalculateNewPoints();

            GenerateNewFaces();
            p3col = vertices;
            icol = faces;
        }

        internal void Subdivide(ref Point3DCollection p3col, ref Int32Collection icol)
        {
            InitialiseData(p3col, icol);

            //  GenerateNewFacePoints();
            GenerateNewUnweightedEdgePoints();
            CalculateNewUnweightedPoints();

            GenerateNewFaces();
            p3col = vertices;
            icol = faces;
        }

        protected int AddVertice(double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (PointUtils.equals(vertices[i], x, y, z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(x, y, z));
                res = vertices.Count - 1;
            }
            return res;
        }

        private void CalculateNewPoints()
        {
            int n;
            LoopCoord lc = new LoopCoord();
            for (int i = 0; i < loopPoints.Count; i++)
            {
                n = 0;
                lc.X = 0;
                lc.Y = 0;
                lc.Z = 0;
                LoopPoint p = loopPoints[i];
                foreach (int index in p.Edges)
                {
                    LoopEdge ed = loopEdges[index];

                    if (ed.Start == i)
                    {
                        n++;
                        /*
                        lc.X += loopPoints[ed.End].X;
                        lc.Y += loopPoints[ed.End].Y;
                        lc.Z += loopPoints[ed.End].Z;
                        */
                        lc.X += ed.Ep.X;
                        lc.Y += ed.Ep.Y;
                        lc.Z += ed.Ep.Z;
                    }
                    else
                    {
                        if (ed.End == i)
                        {
                            n++;
                            /*
                            lc.X += loopPoints[ed.Start].X;
                            lc.Y += loopPoints[ed.Start].Y;
                            lc.Z += loopPoints[ed.Start].Z;
                            */
                            lc.X += ed.Ep.X;
                            lc.Y += ed.Ep.Y;
                            lc.Z += ed.Ep.Z;
                        }
                    }
                }

                double beta = LoopUtils.Beta((double)n);
                double ob = 1.0 - ((double)n * beta);
                p.UpdatedPosition = new LoopCoord();
                p.UpdatedPosition.X = (p.X * ob) + (lc.X * beta);
                p.UpdatedPosition.Y = (p.Y * ob) + (lc.Y * beta);
                p.UpdatedPosition.Z = (p.Z * ob) + (lc.Z * beta);

                //  Log($"P{i} n={n} beta={beta} lc.X={lc.X}  lc.Y={lc.Y}  lc.Z={lc.Z} => { p.UpdatedPosition.X}, { p.UpdatedPosition.Y}, { p.UpdatedPosition.Z}");
            }
        }

        private void CalculateNewUnweightedPoints()
        {
            for (int i = 0; i < loopPoints.Count; i++)
            {
                LoopPoint p = loopPoints[i];
                p.UpdatedPosition = new LoopCoord();
                p.UpdatedPosition.X = p.X;
                p.UpdatedPosition.Y = p.Y;
                p.UpdatedPosition.Z = p.Z;
            }
        }

        private void Dump()
        {
            for (int i = 0; i < loopPoints.Count; i++)
            {
                LoopPoint p = loopPoints[i];
                Log($"P{i}  {p.X}, {p.Y}, {p.Z} ");
            }

            for (int i = 0; i < loopFaces.Count; i++)
            {
                LoopFace p = loopFaces[i];
                Log($"F{i}  P1 {p.P1}, P2 {p.P2}, P3  {p.P3}  E1 {p.E1} E2 {p.E2} E3 {p.E3}");
            }

            for (int i = 0; i < loopEdges.Count; i++)
            {
                LoopEdge e = loopEdges[i];
                Log($"E{i}  {e.Start}, {e.End}  F1 {e.F1}  F2 {e.F2} ");
            }
        }

        private int FindEdge(int p1, int p2)
        {
            int e = -1;
            int i = 0;
            foreach (LoopEdge ce in loopEdges)
            {
                if (ce.Start == p1 && ce.End == p2)
                {
                    e = i;
                    break;
                }
                if (ce.Start == p2 && ce.End == p1)
                {
                    e = i;
                    break;
                }
                i++;
            }
            return e;
        }

        private void GenerateNewEdgePoints()
        {
            foreach (LoopEdge ce in loopEdges)
            {
                ce.MakeEdgePoint(loopFaces, loopPoints);
            }
        }

        private void GenerateNewFaces()
        {
            foreach (LoopFace fc in loopFaces)
            {
                // make new centre face
                MakeFace(loopEdges[fc.E1].Ep, loopEdges[fc.E2].Ep, loopEdges[fc.E3].Ep);

                // make face from p1
                MakeFace(loopPoints[fc.P1].UpdatedPosition, loopEdges[fc.E1].Ep, loopEdges[fc.E3].Ep);

                // make face from p2
                MakeFace(loopPoints[fc.P2].UpdatedPosition, loopEdges[fc.E2].Ep, loopEdges[fc.E1].Ep);

                // make face from p3
                MakeFace(loopPoints[fc.P3].UpdatedPosition, loopEdges[fc.E3].Ep, loopEdges[fc.E2].Ep);
            }
        }

        private void GenerateNewUnweightedEdgePoints()
        {
            foreach (LoopEdge ce in loopEdges)
            {
                ce.MakeUnweightedEdgePoint(loopFaces, loopPoints);
            }
        }

        private void InitialiseData(Point3DCollection p3col, Int32Collection icol)
        {
            loopPoints.Clear();
            loopEdges.Clear();
            loopFaces.Clear();

            foreach (Point3D p in p3col)
            {
                LoopPoint cp = new LoopPoint(p);
                loopPoints.Add(cp);
            }

            for (int i = 0; i < icol.Count; i += 3)
            {
                LoopFace f = new LoopFace();
                loopFaces.Add(f);
                f.P1 = icol[i];
                f.P2 = icol[i + 1];
                f.P3 = icol[i + 2];

                int ei = FindEdge(f.P1, f.P2);

                if (ei == -1)
                {
                    LoopEdge e1 = new LoopEdge();
                    e1.Start = f.P1;
                    e1.End = f.P2;
                    e1.F1 = loopFaces.Count - 1;
                    e1.OppositePoint1 = f.P3;
                    loopPoints[f.P1].Edges.Add(loopEdges.Count);
                    loopPoints[f.P2].Edges.Add(loopEdges.Count);
                    ei = loopEdges.Count;
                    loopEdges.Add(e1);
                }
                else
                {
                    loopEdges[ei].F2 = loopFaces.Count - 1;
                    loopEdges[ei].OppositePoint2 = f.P3;
                }

                f.E1 = ei;

                ei = FindEdge(f.P2, f.P3);
                if (ei == -1)
                {
                    LoopEdge e2 = new LoopEdge();
                    e2.Start = f.P2;
                    e2.End = f.P3;
                    e2.F1 = loopFaces.Count - 1;
                    e2.OppositePoint1 = f.P1;
                    loopPoints[f.P2].Edges.Add(loopEdges.Count);
                    loopPoints[f.P3].Edges.Add(loopEdges.Count);
                    ei = loopEdges.Count;
                    loopEdges.Add(e2);
                }
                else
                {
                    loopEdges[ei].F2 = loopFaces.Count - 1;
                    loopEdges[ei].OppositePoint2 = f.P1;
                }

                f.E2 = ei;

                ei = FindEdge(f.P3, f.P1);
                if (ei == -1)
                {
                    LoopEdge e3 = new LoopEdge();
                    e3.Start = f.P3;
                    e3.End = f.P1;
                    e3.F1 = loopFaces.Count - 1;
                    e3.OppositePoint1 = f.P2;
                    loopPoints[f.P3].Edges.Add(loopEdges.Count);
                    loopPoints[f.P1].Edges.Add(loopEdges.Count);
                    ei = loopEdges.Count;
                    loopEdges.Add(e3);
                }
                else
                {
                    loopEdges[ei].F2 = loopFaces.Count - 1;
                    loopEdges[ei].OppositePoint2 = f.P2;
                }
                f.E3 = ei;
            }
            //   Dump();
        }

        private void Log(String s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        private void MakeFace(LoopCoord ep1, LoopCoord ep2, LoopCoord ep3)
        {
            int v1 = AddVertice(ep1.X, ep1.Y, ep1.Z);
            int v2 = AddVertice(ep2.X, ep2.Y, ep2.Z);
            int v3 = AddVertice(ep3.X, ep3.Y, ep3.Z);
            faces.Add(v1);
            faces.Add(v2);
            faces.Add(v3);
        }
    }
}