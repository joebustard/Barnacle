using System.Collections.Generic;

namespace ManifoldLib
{
    public class Face
    {
        public List<Edge> Edges;
        public int NumberOfGoodSharedEdges { get; set; }
        public int NumberOfBadSharedEdges { get; set; }

        public Face(int p0, int p1, int p2)
        {
            Edges = new List<Edge>();
            Edge e1 = new Edge(p0, p1);
            Edge e2 = new Edge(p1, p2);
            Edge e3 = new Edge(p2, p0);
            Edges.Add(e1);
            Edges.Add(e2);
            Edges.Add(e3);
        }

        public void AddEdges(int p0, int p1, int p2)
        {
            Edge e1 = new Edge(p0, p1);
            Edge e2 = new Edge(p1, p2);
            Edge e3 = new Edge(p2, p0);
            Edges.Add(e1);
            Edges.Add(e2);
            Edges.Add(e3);
        }

        public int NumberOfCommonPoints(Face f)
        {
            int res = 0;
            foreach (Edge e in Edges)
            {
                foreach (Edge fe in f.Edges)
                {
                    if ((e.P0 == fe.P0) || (e.P0 == fe.P1))
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        public void CountSharedEdges(List<Face> others)
        {
            NumberOfGoodSharedEdges = 0;
            NumberOfBadSharedEdges = 0;
            foreach (Face f in others)
            {
                if (f != this)
                {
                    foreach (Edge fo in f.Edges)
                    {
                        foreach (Edge ft in Edges)
                        {
                            if (fo.EqualTo(ft, true))
                            {
                                NumberOfGoodSharedEdges++;
                            }
                            else
                            {
                                if (fo.EqualTo(ft, false))
                                {
                                    NumberOfBadSharedEdges++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}