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
                faces = new List<Face>();

                edgeTree = new EdgeTree(Points.Count);
                edgeTree.Vertices = meshPoints;

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
                faces = new List<Face>();

                edgeTree = new EdgeTree(Points.Count);
                edgeTree.Vertices = meshPoints;

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
            //List<Edge> processedEdges = new List<Edge>();

            foreach (List<Edge> edges in edgeTree.Edgebucket)
            {
                FetchDuff(edges, duffEdges);
            }
            // can we find any combinations of these duffedges that just form simple triangles
            int simples = FillSimpleTriangles(duffEdges);

            bool more = (duffEdges.Count >= 3);
            List<int> holePoints = new List<int>();
            while (more && !token.IsCancellationRequested)
            {
                int holeS = duffEdges[0].Start;
                int holeE = duffEdges[0].End;
                //   processedEdges.Add(duffEdges[0]);
                duffEdges.RemoveAt(0);

                holePoints.Add(holeS);
                holePoints.Add(holeE);

                bool closed = false;

                int maxi = duffEdges.Count;
                bool found = true;
                while (!closed && found && !token.IsCancellationRequested)
                {
                    found = false;
                    for (int i = 0; i < maxi && !found && !token.IsCancellationRequested; i++)
                    {
                        if (i < duffEdges.Count)
                        {
                            // normal
                            if (duffEdges[i].Start == holeE)
                            {
                                holeE = duffEdges[i].End;
                                holePoints.Add(holeE);
                                //         processedEdges.Add(duffEdges[i]);
                                duffEdges.RemoveAt(i);
                                found = true;
                            }
                            else
                            if (duffEdges[i].End == holeS)
                            {
                                holeS = duffEdges[i].Start;
                                holePoints.Insert(0, holeS);
                                //         processedEdges.Insert(0, duffEdges[i]);
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
            return new Tuple<int, int>(foundHoles + simples, fixedHoles + simples);
        }

        private void Debug(string v)
        {
            System.Diagnostics.Debug.WriteLine(v);
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

        private int FillSimpleTriangles(List<Edge> duffEdges)
        {
            int res = 0;
            if (duffEdges.Count > 2)
            {
                bool again = true;

                Edge edge1 = null;
                Edge edge2 = null;
                Edge edge3 = null;
                while (again)
                {
                    again = false;
                    for (int i = 0; i < duffEdges.Count && again == false; i++)
                    {
                        edge1 = duffEdges[i];
                        for (int j = 0; j < duffEdges.Count && again == false; j++)
                        {
                            if (i != j)
                            {
                                edge2 = duffEdges[j];
                                if (edge2.Start == edge1.End)
                                {
                                    for (int k = 0; k < duffEdges.Count && again == false; k++)
                                    {
                                        if (i != k && j != k)
                                        {
                                            edge3 = duffEdges[k];

                                            // if they form a triangle then edge2 must connect to edge1 somehow
                                            // so edge3 should fit between the end of edge2 and the start of edge 1
                                            if (edge3.Start == edge2.End)
                                            {
                                                if (edge3.End == edge1.Start)
                                                {
                                                    MeshFaces.Add(edge3.Start);
                                                    MeshFaces.Add(edge2.Start);
                                                    MeshFaces.Add(edge1.Start);

                                                    duffEdges.Remove(edge1);
                                                    duffEdges.Remove(edge2);
                                                    duffEdges.Remove(edge3);

                                                    res++;
                                                    again = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                if (edge2.End == edge1.Start)
                                {
                                    for (int k = 0; k < duffEdges.Count && again == false; k++)
                                    {
                                        if (i != k && j != k)
                                        {
                                            edge3 = duffEdges[k];

                                            // if they form a triangle then edge2 must connect to edge1 somehow
                                            // so edge3 should fit between the end of edge2 and the start of edge 1
                                            if (edge3.Start == edge1.End)
                                            {
                                                if (edge3.End == edge2.Start)
                                                {
                                                    MeshFaces.Add(edge3.Start);
                                                    MeshFaces.Add(edge1.Start);
                                                    MeshFaces.Add(edge2.Start);

                                                    duffEdges.Remove(edge1);
                                                    duffEdges.Remove(edge2);
                                                    duffEdges.Remove(edge3);

                                                    res++;
                                                    again = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}