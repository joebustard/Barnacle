using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoleLibrary
{
    public class Face
    {
        public Edge[] Edges { get; set; }

        public Face()
        {
            Edges = new Edge[3];
        }

        public Face(int v0, int v1, int v2, List<Edge> edges)
        {
            Edges = new Edge[3];
            Edges[0] = FindEdge(v0, v1, this, edges);
            Edges[1] = FindEdge(v1, v2, this, edges);
            Edges[2] = FindEdge(v2, v0, this, edges);
        }

        private Edge FindEdge(int v0, int v1, Face face, List<Edge> edges)
        {
            Edge res = null;
            // dummy for now
            foreach (Edge e in edges)
            {
                if (e.EdgeMatch(v0, v1))
                {
                    res = e;
                    break;
                }
            }
            if (res == null)
            {
                res = new Edge(v0, v1, face);
                edges.Add(res);
            }
            else
            {
                res.Face2 = this;
            }
            return res;
        }
    }
}