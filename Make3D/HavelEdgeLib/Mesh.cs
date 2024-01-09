using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HalfEdgeLib
{
    public class Mesh
    {
        public List<Vertex> Vertices;
        public List<Face> Faces;
        public List<HalfEdge> HalfEdges;

        public int HalfEdgeCount { get { return HalfEdges.Count; } }

        public Mesh()
        {
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
            HalfEdges = new List<HalfEdge>();
        }

        public Mesh(Point3DCollection verts, Int32Collection sourceFaces)
        {
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
            HalfEdges = new List<HalfEdge>();
            // create the right number of vertices and set their coordinates
            // We don't know their half edges yet
            foreach (var v in verts)
            {
                this.Vertices.Add(new Vertex(v));
            }
            // create the right number of faces.
            // We don't know anything else about them yet
            for (int i = 0; i < sourceFaces.Count / 3; i++)
            {
                int j = i * 3;
                HalfEdge he0 = new HalfEdge();
                he0.StartVertex = sourceFaces[j];
                he0.Face = this.Faces.Count;
                int heInd0 = HalfEdges.Count;
                HalfEdges.Add(he0);
                Vertices[he0.StartVertex].OutgoingHalfEdge = heInd0;

                HalfEdge he1 = new HalfEdge();
                he1.StartVertex = sourceFaces[j + 1];
                he1.Face = this.Faces.Count;
                int heInd1 = HalfEdges.Count;
                HalfEdges.Add(he1);
                this.Vertices[he1.StartVertex].OutgoingHalfEdge = heInd1;

                HalfEdge he2 = new HalfEdge();
                he2.StartVertex = sourceFaces[j + 2];
                he2.Face = this.Faces.Count;
                int heInd2 = HalfEdges.Count;
                HalfEdges.Add(he2);
                Vertices[he2.StartVertex].OutgoingHalfEdge = heInd2;

                he0.Next = heInd1;
                he1.Next = heInd2;
                he2.Next = heInd0;

                he0.Previous = heInd2;
                he1.Previous = heInd0;
                he2.Previous = heInd1;

                Face f = new Face(heInd0);
                this.Faces.Add(f);
            }

            // crude halfedge twinner
            for (int i = 0; i < HalfEdges.Count; i++)
            {
                if (HalfEdges[i].Twin == -1)
                {
                    int s0 = HalfEdges[i].StartVertex;
                    int e0 = HalfEdges[HalfEdges[i].Next].StartVertex;
                    bool more = true;
                    for (int j = 0; j < HalfEdges.Count && more; j++)
                    {
                        if (HalfEdges[j].Twin == -1 && j != i)
                        {
                            int s1 = HalfEdges[j].StartVertex;
                            int e1 = HalfEdges[HalfEdges[j].Next].StartVertex;
                            if (s0 == e1 && e0 == s1)
                            {
                                HalfEdges[i].Twin = j;
                                HalfEdges[j].Twin = i;
                                more = false;
                            }
                        }
                    }
                }
            }
        }

        private bool AllTwins()
        {
            bool broke = false;
            for (int i = 0; i < HalfEdges.Count; i++)
            {
                if (HalfEdges[i].Twin == -1)
                {
                    broke = true;
                    break;
                }
            }
            return !broke;
        }

        public void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public void DumpStats()
        {
            Log("==========");
            Log("Mesh Stats");
            Log($"Number of Vertices : {Vertices.Count}");
            Log($"Number of Faces    : {Faces.Count}");
            Log($"Number of HalfEdges: {HalfEdges.Count}");
            if (HalfEdges.Count % 2 == 1)
            {
                Log($"Number of HalfEdges is inconsistent");
            }
            if (!AllTwins())
            {
                Log($"Not all half edges are twinned");
            }
        }

        public void DumpHalfEdges()
        {
            Log("Half Edges");
            Log("index   Start   Face   Next   Previous  ");
            for (int i = 0; i < HalfEdges.Count; i++)
            {
                Log($"{i:D6} : {HalfEdges[i].StartVertex:D6}, {HalfEdges[i].Face:D6}, {HalfEdges[i].Next:D6}, {HalfEdges[i].Previous:D6}");
            }
        }

        public void DumpVertices()
        {
            Log("Vertices");
            for (int i = 0; i < Vertices.Count; i++)
            {
                int NumVEdges = GetHalfEdgesFromVertex(i).Count;
                Log($"{i:D6} : {Vertices[i].X}, {Vertices[i].Y}, {Vertices[i].Z},  he:{Vertices[i].OutgoingHalfEdge:D6} nn:{NumVEdges:D6}");
            }
        }

        private List<int> GetHalfEdgesFromVertex(int v)
        {
            List<int> res = new List<int>();
            if (v >= 0 && v < Vertices.Count())
            {
                int startEdge = Vertices[v].OutgoingHalfEdge;
                res.Add(startEdge);
                if (startEdge != -1)
                {
                    int edge = startEdge;

                    edge = HalfEdges[edge].Twin;
                    edge = HalfEdges[edge].Next;
                    bool more = true;
                    while (edge != startEdge && more)
                    {
                        if (!res.Contains(edge))
                        {
                            res.Add(edge);
                            edge = HalfEdges[edge].Twin;
                            edge = HalfEdges[edge].Next;
                        }
                        else
                        {
                            more = false;
                            Log("Edges from Vertex are inconsistent");
                        }
                    }
                }
            }
            return res;
        }

        public void Dump(string v)
        {
            Log(v);
            DumpStats();
            DumpVertices();
            DumpHalfEdges();
            DumpFaces();
        }

        private void DumpFaces()
        {
            Log("Faces");
            for (int i = 0; i < Faces.Count; i++)
            {
                List<int> verts = GetFaceVertices(i);
                string s = $"{i:D6} : ";
                foreach (int v in verts)
                {
                    s += $"{v:D6}, ";
                }
                Log(s);
            }
        }

        private List<int> GetFaceVertices(int f)
        {
            List<int> res = new List<int>();
            if (f >= 0 && f < Faces.Count())
            {
                int startEdge = Faces[f].FirstEdge;
                res.Add(HalfEdges[startEdge].StartVertex);
                int edge = startEdge;
                edge = HalfEdges[edge].Next;
                bool more = true;
                while (edge != startEdge && more)
                {
                    if (!res.Contains(HalfEdges[edge].StartVertex))
                    {
                        res.Add(HalfEdges[edge].StartVertex);
                        edge = HalfEdges[edge].Next;
                    }
                    else
                    {
                        more = false;
                        Log("Edges from Face are inconsistent");
                    }
                }
            }
            return res;
        }

        public void ToSoup(Point3DCollection vertices, Int32Collection faces)
        {
            foreach (Face pf in this.Faces)
            {
                int edge = pf.FirstEdge;
                faces.Add(this.HalfEdges[edge].StartVertex);

                edge = this.HalfEdges[edge].Next;
                faces.Add(this.HalfEdges[edge].StartVertex);

                edge = this.HalfEdges[edge].Next;
                faces.Add(this.HalfEdges[edge].StartVertex);
            }
        }

        public void SplitAllEdges()
        {
            List<int> neoVertices = new List<int>();
            List<int> neoHalfEdges = new List<int>();

            List<int> edgesToSplit = new List<int>();

            // get the edges that we will split
            // Doesn't matter what order they are in but we only include one of a pair
            Log("Edges to split");
            for (int edge = 0; edge < HalfEdges.Count; edge++)
            {
                int twin = HalfEdges[edge].Twin;
                if (!edgesToSplit.Contains(edge) && !edgesToSplit.Contains(twin))
                {
                    edgesToSplit.Add(edge);
                    Log($"{edge}");
                }
            }

            // create a new vertex at the mid point of the halfedge
            int s = 0;
            int e = 0;
            foreach (int edge in edgesToSplit)
            {
                GetEdgeVertices(edge, out s, out e);

                double mx = (Vertices[s].X + Vertices[e].X) / 2.0;
                double my = (Vertices[s].Y + Vertices[e].Y) / 2.0;
                double mz = (Vertices[s].Z + Vertices[e].Z) / 2.0;
                neoVertices.Add(Vertices.Count);
                Vertex ver = new Vertex(mx,my,mz,-1);
                Vertices.Add(ver);
            }
        }

        private void GetEdgeVertices(int edge, out int s, out int e)
        {
            s = -1;
            e = -1;
            if ( edge >= 0 && edge < HalfEdges.Count)
            {
                s = HalfEdges[edge].StartVertex;
                edge = HalfEdges[edge].Twin;
                e = HalfEdges[edge].StartVertex;
            }
        }
    }
}