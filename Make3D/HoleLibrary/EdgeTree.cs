using Barnacle.Object3DLib;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace HoleLibrary
{
    public class EdgeTree
    {
        public EdgeTree()
        {
            BothLeft = new List<Edge>();
            BothRight = new List<Edge>();
            Mixed = new List<Edge>();
            CentrePoint = 0;
            Vertices = null;
        }

        public float CentrePoint { get; set; }
        public List<P3D> Vertices { get; set; }
        public List<Edge> BothLeft { get; set; }
        public List<Edge> BothRight { get; set; }
        public List<Edge> Mixed { get; set; }

        public void AddEdge(Edge ed)
        {
            if (Vertices != null)
            {
                if (Vertices[ed.Start].X <= CentrePoint && Vertices[ed.End].X <= CentrePoint)
                {
                    BothLeft.Add(ed);
                }
                else
                if (Vertices[ed.Start].X > CentrePoint && Vertices[ed.End].X > CentrePoint)
                {
                    BothRight.Add(ed);
                }
                else
                {
                    Mixed.Add(ed);
                }
            }
        }

        public Edge FindEdge(int start, int end, Face face)
        {
            Edge res = null;
            List<Edge> edgeList = null;
            if (Vertices != null)
            {
                if (Vertices[start].X <= CentrePoint && Vertices[end].X <= CentrePoint)
                {
                    edgeList = BothLeft;
                }
                else
                if (Vertices[start].X > CentrePoint && Vertices[end].X > CentrePoint)
                {
                    edgeList = BothRight;
                }
                else
                {
                    edgeList = Mixed;
                }

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
                    AddEdge(res);
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