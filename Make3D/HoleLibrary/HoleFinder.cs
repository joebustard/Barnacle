using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;

namespace HoleLibrary
{
    public class HoleFinder
    {
        public List<Face> faces;
        public Int32Collection MeshFaces;
        public List<P3D> Points;

        // private List<Edge> edges;
        private EdgeTree edgeTree;

        public HoleFinder(List<P3D> meshPoints, Int32Collection mf)
        {
            Points = meshPoints;
            MeshFaces = mf;
            P3D centre = new P3D(0, 0, 0);
            if (Points.Count > 0)
            {
                foreach (P3D p in Points)
                {
                    centre.X += p.X;
                    centre.Y += p.Y;
                    centre.Z += p.Z;
                }
                centre.X = centre.X / Points.Count;
                centre.Y = centre.Y / Points.Count;
                centre.Z = centre.Z / Points.Count;
                faces = new List<Face>();

                edgeTree = new EdgeTree();
                edgeTree.Vertices = meshPoints;
                edgeTree.CentrePoint = centre;
                for (int i = 0; i <= mf.Count - 3; i += 3)
                {
                    Face nf = new Face(mf[i],
                        mf[i + 1],
                        mf[i + 2],
                        edgeTree);
                    faces.Add(nf);
                }
            }
        }

        public HoleFinder(List<P3D> meshPoints, Int32Collection mf, CancellationToken token)
        {
            Points = meshPoints;
            MeshFaces = mf;
            P3D centre = new P3D(0, 0, 0);
            if (Points.Count > 0)
            {
                foreach (P3D p in Points)
                {
                    centre.X += p.X;
                    centre.Y += p.Y;
                    centre.Z += p.Z;
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
                centre.X = centre.X / Points.Count;
                centre.Y = centre.Y / Points.Count;
                centre.Z = centre.Z / Points.Count;
                faces = new List<Face>();

                edgeTree = new EdgeTree();
                edgeTree.Vertices = meshPoints;
                edgeTree.CentrePoint = centre;
                for (int i = 0; i <= mf.Count - 3 && !token.IsCancellationRequested; i += 3)
                {
                    Face nf = new Face(mf[i],
                        mf[i + 1],
                        mf[i + 2],
                        edgeTree);
                    faces.Add(nf);
                }
            }
        }

        public Tuple<int, int> FindHoles(CancellationToken token)
        {
            int foundHoles = 0;
            int fixedHoles = 0;
            List<Edge> duffEdges = new List<Edge>();

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    for (int d = 0; d < 3; d++)
                    {
                        FetchDuff(edgeTree.Edgebucket[c, r, d], duffEdges);
                    }
                }
            }

            bool more = (duffEdges.Count >= 3);
            List<int> holePoints = new List<int>();
            while (more && !token.IsCancellationRequested)
            {
                int holeS = duffEdges[0].Start;
                int holeE = duffEdges[0].End;
                duffEdges.RemoveAt(0);

                holePoints.Add(holeS);
                holePoints.Add(holeE);

                bool closed = false;

                int maxi = duffEdges.Count;
                bool found = true;
                while (!closed && found && !token.IsCancellationRequested)
                {
                    found = false;
                    for (int i = 0; i < maxi && !token.IsCancellationRequested; i++)
                    {
                        if (i < duffEdges.Count)
                        {
                            // normal
                            if (duffEdges[i].Start == holeE)
                            {
                                holeE = duffEdges[i].End;
                                holePoints.Add(holeE);

                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                            if (duffEdges[i].End == holeS)
                            {
                                holeS = duffEdges[i].Start;
                                holePoints.Insert(0, holeS);

                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                             if (duffEdges[i].End == holeE)
                            {
                                holeE = duffEdges[i].Start;
                                holePoints.Add(holeE);

                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                            if (duffEdges[i].Start == holeS)
                            {
                                holeS = duffEdges[i].End;
                                holePoints.Insert(0, holeS);

                                duffEdges.RemoveAt(i);
                                found = true;
                            }

                            if (holeS == holeE)
                            {
                                closed = true;
                            }
                        }
                    }
                    maxi = duffEdges.Count;
                }

                foundHoles++;
                if (closed)
                {
                    if (FillHole(holePoints))
                    {
                        fixedHoles++;
                    }
                }
                more = (duffEdges.Count >= 3);
                holePoints.Clear();
            }
            return new Tuple<int, int>(foundHoles, fixedHoles);
        }

        private void FetchDuff(List<Edge> edges, List<Edge> duffEdges)
        {
            foreach (Edge e in edges)
            {
                if (e.Face2 == null)
                {
                    duffEdges.Add(e);
                    // Debug($"Edge {e.Start} to {e.End}");
                }
            }
        }

        private void Debug(string v)
        {
            System.Diagnostics.Debug.WriteLine(v);
        }

        private bool FillHole(List<int> holePoints)
        {
            bool res = false;
            if (holePoints.Count >= 3)
            {
                holePoints.RemoveAt(holePoints.Count - 1);

                switch (holePoints.Count)
                {
                    case 3:
                        {
                            MeshFaces.Add(holePoints[2]);
                            MeshFaces.Add(holePoints[1]);
                            MeshFaces.Add(holePoints[0]);
                            res = true;
                        }
                        break;

                    case 4:
                        {
                            MeshFaces.Add(holePoints[2]);
                            MeshFaces.Add(holePoints[1]);
                            MeshFaces.Add(holePoints[0]);

                            MeshFaces.Add(holePoints[3]);
                            MeshFaces.Add(holePoints[2]);
                            MeshFaces.Add(holePoints[0]);
                            res = true;
                        }
                        break;

                    default:
                        {
                            if (holePoints.Count < 150)
                            {
                                // find centroid
                                double cx = 0;
                                double cy = 0;
                                double cz = 0;
                                foreach (int ind in holePoints)
                                {
                                    cx += Points[ind].X;
                                    cy += Points[ind].Y;
                                    cz += Points[ind].Z;
                                }
                                cx = cx / holePoints.Count;
                                cy = cy / holePoints.Count;
                                cz = cz / holePoints.Count;
                                // add it as new point
                                Points.Add(new P3D(cx, cy, cz));

                                // create a simple triangle from each edge to the centroid. I know
                                // this isn't brilliant!
                                int cn = Points.Count - 1;
                                int j = holePoints.Count - 1;
                                while (j > 0)
                                {
                                    MeshFaces.Add(holePoints[j]);
                                    MeshFaces.Add(holePoints[j - 1]);
                                    MeshFaces.Add(cn);
                                    j--;
                                }
                                res = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Large hole {holePoints.Count}");
                            }
                        }
                        break;
                }
            }
            return res;
        }
    }
}