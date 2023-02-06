using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace HoleLibrary
{
    public class HoleFinder
    {
        public List<Face> faces;
        public Int32Collection MeshFaces;
        public List<P3D> Points;
        private List<Edge> edges;
        private EdgeTree edgeTree;

        public HoleFinder(List<P3D> meshPoints, Int32Collection mf)
        {
            Points = meshPoints;
            MeshFaces = mf;
            double centre = 0;
            foreach (P3D p in Points)
            {
                centre += p.X;
            }
            centre = centre / Points.Count;
            faces = new List<Face>();
            /*
                        edges = new List<Edge>();
                        for (int i = 0; i <= mf.Count - 3; i += 3)
                        {
                            Face nf = new Face(mf[i],
                                mf[i + 1],
                                mf[i + 2],
                                edges);
                            faces.Add(nf);
                        }
            */
            edgeTree = new EdgeTree();
            edgeTree.Vertices = meshPoints;
            edgeTree.CentrePoint = (float)centre;
            for (int i = 0; i <= mf.Count - 3; i += 3)
            {
                Face nf = new Face(mf[i],
                    mf[i + 1],
                    mf[i + 2],
                    edgeTree);
                faces.Add(nf);
            }
        }

        public Tuple<int, int> FindHoles()
        {
            int foundHoles = 0;
            int fixedHoles = 0;
            List<Edge> duffEdges = new List<Edge>();
            //FetchDuff(edges, duffEdges);

            FetchDuff(edgeTree.BothLeft, duffEdges);
            FetchDuff(edgeTree.BothRight, duffEdges);
            FetchDuff(edgeTree.Mixed, duffEdges);
            bool more = (duffEdges.Count >= 3);
            while (more)
            {
                List<Edge> hole = new List<Edge>();
                List<int> holePoints = new List<int>();
                hole.Add(duffEdges[0]);
                duffEdges.RemoveAt(0);

                int holeS = hole[0].Start;
                int holeE = hole[0].End;
                holePoints.Add(holeS);
                holePoints.Add(holeE);

                bool closed = false;

                int maxi = duffEdges.Count;
                bool found = true;
                while (!closed && found)
                {
                    found = false;
                    for (int i = 0; i < maxi; i++)
                    {
                        if (i < duffEdges.Count)
                        {
                            // normal
                            if (duffEdges[i].Start == holeE)
                            {
                                holeE = duffEdges[i].End;
                                holePoints.Add(holeE);

                                hole.Add(duffEdges[i]);
                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                            if (duffEdges[i].End == holeS)
                            {
                                holeS = duffEdges[i].Start;
                                holePoints.Insert(0, holeS);

                                hole.Add(duffEdges[i]);
                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                             if (duffEdges[i].End == holeE)
                            {
                                holeE = duffEdges[i].Start;
                                holePoints.Add(holeE);

                                hole.Add(duffEdges[i]);
                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                            if (duffEdges[i].Start == holeS)
                            {
                                holeS = duffEdges[i].End;
                                holePoints.Insert(0, holeS);

                                hole.Add(duffEdges[i]);
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
                Debug($"Closed {closed} Points {holePoints.Count}");
                foundHoles++;
                if (closed)
                {
                    if (FillHole(holePoints))
                    {
                        fixedHoles++;
                    }
                }
                more = (duffEdges.Count >= 3);
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
            if (holePoints.Count > 3)
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
                            if (holePoints.Count < 20)
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

                                // create a simple triangle from each edge to the centroid.
                                // I know this  isn't brilliant!
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
                        }
                        break;
                }
            }
            return res;
        }
    }
}