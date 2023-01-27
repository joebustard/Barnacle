using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HoleLibrary
{
    public class HoleFinder
    {
        private List<P3D> points;
        private List<Face> faces;
        private List<Edge> edges;

        public HoleFinder(List<P3D> meshPoints, Int32Collection meshFaces)
        {
            points = meshPoints;
            edges = new List<Edge>();
            faces = new List<Face>();
            for (int i = 0; i < meshFaces.Count; i += 3)
            {
                Face nf = new Face(meshFaces[i],
                    meshFaces[i + 1],
                    meshFaces[i + 2],
                    edges);
                faces.Add(nf);
            }

        }
        public void FindHoles()
        {
            List<Edge> duffEdges = new List<Edge>();
            foreach (Edge e in edges)
            {
                if (e.Face2 == null)
                {
                    duffEdges.Add(e);
                    Debug($"Edge {e.Start} to {e.End}");
                }
            }
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
                if ( closed)
                {
                    FillHole(holePoints);
                }
                more = (duffEdges.Count >= 3);
            }

        }

        private void FillHole(List<int> holePoints)
        {
            
        }

        private void Debug(string v)
        {
            System.Diagnostics.Debug.WriteLine(v);
        }
    }
}