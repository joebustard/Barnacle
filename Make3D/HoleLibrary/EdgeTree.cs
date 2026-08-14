using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace HoleLibrary
{
    public class EdgeTree
    {
        public List<Edge>[] Edgebucket;
        private int bucketLim = 1500;
        private int numBuckets = 300;

        public EdgeTree(int count)
        {
            if (count > numBuckets * 10)
            {
                numBuckets = count / numBuckets;
                if (numBuckets > bucketLim)
                {
                    numBuckets = bucketLim;
                }
            }
            Edgebucket = new List<Edge>[numBuckets];
            for (int r = 0; r < numBuckets; r++)
            {
                Edgebucket[r] = new List<Edge>();
            }
        }

        public List<P3D> Vertices
        {
            get; set;
        }

        public void AddEdge(Edge ed)
        {
            int r;
            ClassifyEdge(ed.Start, ed.End, out r);

            if (Vertices != null)
            {
                Edgebucket[r].Add(ed);
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
                ClassifyEdge(start, end, out r);
                edgeList = Edgebucket[r];
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
                    edgeList.Add(res);
                }
                else
                {
                    res.Face2 = face;
                }
            }
            return res;
        }

        private void ClassifyEdge(int start, int end, out int r)
        {
            r = (start + end) % numBuckets;

            /*
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
            */
        }
    }
}