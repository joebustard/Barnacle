using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace HoleLibrary
{
    public class EdgeTree
    {
        public EdgeTree()
        {
            CentrePoint = new P3D(0, 0, 0);
            Vertices = null;
            Edgebucket = new List<Edge>[3, 3, 3];
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    for (int d = 0; d < 3; d++)
                    {
                        Edgebucket[c, r, d] = new List<Edge>();
                    }
                }
            }
        }

        public List<Edge>[,,] Edgebucket;

        public P3D CentrePoint { get; set; }
        public List<P3D> Vertices { get; set; }

        public void AddEdge(Edge ed)
        {
            int r;
            int c;
            int d;
            ClassifyEdge(ed.Start, ed.End, out r, out c, out d);

            if (Vertices != null)
            {
                Edgebucket[c, r, d].Add(ed);
            }
        }

        private void ClassifyEdge(int start, int end, out int r, out int c, out int d)
        {
            r = 1;
            c = 1;
            d = 1;

            if (Vertices[start].X <= CentrePoint.X && Vertices[end].X <= CentrePoint.X)
            {
                c = 0;
            }
            else
            if (Vertices[start].X > CentrePoint.X && Vertices[end].X > CentrePoint.X)
            {
                c = 2;
            }

            if (Vertices[start].Y <= CentrePoint.Y && Vertices[end].Y <= CentrePoint.Y)
            {
                r = 0;
            }
            else
            if (Vertices[start].Y > CentrePoint.Y && Vertices[end].Y > CentrePoint.Y)
            {
                r = 2;
            }

            if (Vertices[start].Z <= CentrePoint.Z && Vertices[end].Z <= CentrePoint.Z)
            {
                d = 0;
            }
            else
            if (Vertices[start].Z > CentrePoint.Z && Vertices[end].Z > CentrePoint.Z)
            {
                d = 2;
            }
        }

        public Edge FindEdge(int start, int end, Face face)
        {
            int r;
            int c;
            int d;
            Edge res = null;
            List<Edge> edgeList = null;
            if (Vertices != null)
            {
                ClassifyEdge(start, end, out r, out c, out d);
                edgeList = Edgebucket[c, r, d];
                // dummy for now
                foreach (Edge e in edgeList)
                {
                    if (e.EdgeMatch(start, end))
                    {
                        res = e;
                        break;
                    }
                }
                if (res == null)
                {
                    res = new Edge(start, end, face);
                    //  AddEdge(res);
                    edgeList.Add(res);
                }
                else
                {
                    res.Face2 = face;
                }
            }
            return res;
        }
    }
}