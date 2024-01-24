using LoggerLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HalfEdgeLib
{
    public class Mesh
    {
        public List<Face> Faces;
        public List<HalfEdge> HalfEdges;
        public List<Vertex> Vertices;

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

        public int HalfEdgeCount { get { return HalfEdges.Count; } }

        public void AddFacePoint(int e, Int32Collection faces)
        {
            if (e < 0 || e >= HalfEdges.Count())
            {
                Logger.LogLine($"Edge {e} is not valid");
            }
            else if (this.HalfEdges[e].StartVertex < 0 || this.HalfEdges[e].StartVertex >= Vertices.Count)
            {
                Logger.LogLine($"Vertex {this.HalfEdges[e].StartVertex} is not valid");
            }
            else
            {
                faces.Add(this.HalfEdges[e].StartVertex);
            }
        }

        public void Dump(string v)
        {
            Logger.LogLine(v);
            DumpStats();
            DumpVertices();
            DumpHalfEdges();
            DumpFaces();
        }

        public void DumpHalfEdges()
        {
            Logger.LogLine("Half Edges");
            Logger.LogLine("index     Next   Previous  Twin    Start   Face   ");
            for (int i = 0; i < HalfEdges.Count; i++)
            {
                Logger.LogLine($"{i:D6} :{HalfEdges[i].Next:D6}, {HalfEdges[i].Previous:D6},{HalfEdges[i].Twin:D6}, {HalfEdges[i].StartVertex:D6}, {HalfEdges[i].Face:D6} {CheckHalfLinks(i)} {CheckTwins(i)}");
            }
        }

        public void DumpStats()
        {
            Logger.LogLine("==========");
            Logger.LogLine("Mesh Stats");
            Logger.LogLine($"Number of Vertices : {Vertices.Count}");
            Logger.LogLine($"Number of Faces    : {Faces.Count}");
            Logger.LogLine($"Number of HalfEdges: {HalfEdges.Count}");
            if (HalfEdges.Count % 2 == 1)
            {
                Logger.LogLine($"Number of HalfEdges is inconsistent");
            }
            if (!AllTwins())
            {
                Logger.LogLine($"Not all half edges are twinned");
            }
        }

        public void DumpVertices()
        {
            Logger.LogLine("Vertices");
            for (int i = 0; i < Vertices.Count; i++)
            {
                int NumVEdges = GetHalfEdgesFromVertex(i).Count;
                Logger.Log($"{i:D6} :");
                LogVertex(i);
                Logger.LogLine($" he:{Vertices[i].OutgoingHalfEdge:D6} nn:{NumVEdges:D6}");
            }
        }

        /// <summary>
        ///  Sbbdivide all the edges
        /// </summary>
        public void SplitAllEdges()
        {
            List<int> neoVertices = new List<int>();
            List<int> neoHalfEdges = new List<int>();

            List<int> edgesToSplit = new List<int>();

            // get the edges that we will split
            // Doesn't matter what order they are in but we only include one of a pair
            //Logger.LogLine("Edges to split");
            for (int edge = 0; edge < HalfEdges.Count; edge++)
            {
                int twin = HalfEdges[edge].Twin;
                if (!edgesToSplit.Contains(edge) && !edgesToSplit.Contains(twin))
                {
                    edgesToSplit.Add(edge);
                    //    Logger.LogLine($"{edge}");
                }
            }

            int lastOldVertex = Vertices.Count - 1;
            int midPointIndex = 0;
            List<int> allNewEdges = new List<int>();
            List<int> oneNewEdges = new List<int>();
            foreach (int edge in edgesToSplit)
            {
                midPointIndex = SplitSingleEdge(edge, oneNewEdges);
                Dump($"After Splitting {edge}");
                neoVertices.Add(midPointIndex);
                allNewEdges.AddRange(oneNewEdges);
            }
            int s = -1;
            int e = -1;

            List<int> edgesToFlip = new List<int>();
            foreach (int edge in allNewEdges)
            {
                int twin = HalfEdges[edge].Twin;
                if (!edgesToFlip.Contains(edge) && !edgesToFlip.Contains(twin))
                {
                    GetEdgeVertices(edge, out s, out e);
                    if ((s <= lastOldVertex && e > lastOldVertex) ||
                          (e <= lastOldVertex && s > lastOldVertex))
                    {
                        edgesToFlip.Add(edge);
                        Logger.LogLine($"To Flip: {edge}");
                    }
                }
            }

            int count = -1;
            foreach (int edge in edgesToFlip)
            {
                FlipTriangle(edge);
                Dump($"After FLipping edge {edge}");
                count++;
                if (count == 1)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Convert the data structure back to a basic soup as used by WPF
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faces"></param>
        public void ToSoup(Point3DCollection vertices, Int32Collection faces)
        {
            foreach (Vertex v in Vertices)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
            }
            foreach (Face pf in this.Faces)
            {
                int edge = pf.FirstEdge;
                if (edge != -1)
                {
                    AddFacePoint(edge, faces);
                    edge = this.HalfEdges[edge].Next;
                    AddFacePoint(edge, faces);
                    edge = this.HalfEdges[edge].Next;
                    AddFacePoint(edge, faces);
                }
            }
        }

        /// <summary>
        /// Add anew halfedge emanating from the given vertex
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private int AddHalfEdge(int v)
        {
            int res = HalfEdges.Count;
            HalfEdge he = new HalfEdge();
            HalfEdges.Add(he);
            he.StartVertex = v;
            return res;
        }

        /// <summary>
        /// Check if all the halfedges are marked as having a twin
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Check that a halfedge is actually part of a triangle
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string CheckHalfLinks(int i)
        {
            string res = "";
            int n = HalfEdges[i].Next;
            n = HalfEdges[n].Next;
            n = HalfEdges[n].Next;
            if (i != n)
            {
                res = " broke";
            }
            return res;
        }

        /// <summary>
        /// Check if the link between halfEdge i's and its twin are correct
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string CheckTwins(int i)
        {
            string res = "";
            // if i has a twin, then the twins twin must be i
            int twin = HalfEdges[i].Twin;
            if (HalfEdges[twin].Twin != i)
            {
                res = " twin mismatch";
            }
            return res;
        }

        /// <summary>
        /// Create a new face record. Mark the first halfedge
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <param name="e3"></param>
        private void CreateFace(int e1, int e2, int e3)
        {
            Face fe1 = new Face(e1);
            int fc = Faces.Count;
            Faces.Add(fe1);
            HalfEdges[e1].Face = fc;
            HalfEdges[e2].Face = fc;
            HalfEdges[e3].Face = fc;
            Logger.LogLine($"new face {fc}: Edges {e1}, {e2}, {e3}");
        }

        /// <summary>
        /// Dumo the details of all faces to the log
        /// </summary>
        private void DumpFaces()
        {
            Logger.LogLine("Faces");
            for (int i = 0; i < Faces.Count; i++)
            {
                if (Faces[i].FirstEdge != -1)
                {
                    List<int> verts = GetFaceVertices(i);
                    string s = $"{i:D6} verts : ";
                    foreach (int v in verts)
                    {
                        s += $"{v:D6}, ";
                    }

                    Logger.LogLine(s);
                }
            }
        }

        /// <summary>
        /// Return the start and end vertices of the requested half edge
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void GetEdgeVertices(int edge, out int s, out int e)
        {
            s = -1;
            e = -1;
            if (edge >= 0 && edge < HalfEdges.Count)
            {
                s = HalfEdges[edge].StartVertex;
                edge = HalfEdges[edge].Twin;
                e = HalfEdges[edge].StartVertex;
            }
        }

        /// <summary>
        /// Get a list of the vertices associated with a face
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
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
                        Logger.LogLine("Edges from Face are inconsistent");
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get all the halfedges eminating from the requested vertex
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
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
                            Logger.LogLine("Edges from Vertex are inconsistent");
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Link three half edges as a triangle
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        private void LinkEdges(int e0, int e1, int e2)
        {
            HalfEdges[e0].Next = e1;
            HalfEdges[e1].Next = e2;
            HalfEdges[e2].Next = e0;
            HalfEdges[e2].Previous = e1;
            HalfEdges[e1].Previous = e0;
            HalfEdges[e0].Previous = e2;
        }

        /// <summary>
        /// Add details of a vertex to the log
        /// </summary>
        /// <param name="i"></param>
        private void LogVertex(int i)
        {
            Logger.Log($" {Vertices[i].X}, {Vertices[i].Y}, {Vertices[i].Z},  ");
        }

        /// <summary>
        /// Throw away all face records and generate them again from scratch
        /// </summary>
        private void Reface()
        {
            Int32Collection used = new Int32Collection();
            Faces.Clear();
            for (int i = 0; i < HalfEdges.Count; i++)
            {
                if (!used.Contains(i))
                {
                    used.Add(i);
                    Face fc = new Face(i);
                    int faceN = Faces.Count;
                    HalfEdges[i].Face = faceN;
                    Faces.Add(fc);
                    int n = HalfEdges[i].Next;
                    HalfEdges[n].Face = faceN;
                    used.Add(n);
                    n = HalfEdges[n].Next;
                    HalfEdges[n].Face = faceN;
                    used.Add(n);
                }
            }
        }

        /// <summary>
        /// Splits the given halfedge ( and twin), effectively converting two
        /// triangles into 4
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="neoVertices"></param>
        private int SplitSingleEdge(int edge, List<int> newHalfEdges)
        {
            Logger.LogLine($"Splitting halfedge {edge}");

            newHalfEdges.Clear();
            // note existing edge ids
            int e0 = edge;
            int e1 = HalfEdges[e0].Next;
            int e2 = HalfEdges[e1].Next;

            int t0 = HalfEdges[edge].Twin;
            int t1 = HalfEdges[t0].Next;
            int t2 = HalfEdges[t1].Next;

            // note existing vertex ids
            int v0 = HalfEdges[e0].StartVertex;
            int v1 = HalfEdges[e1].StartVertex;
            int v2 = HalfEdges[e2].StartVertex;
            int v3 = HalfEdges[t2].StartVertex;
            Logger.LogLine($"existing  halfedge ring  {e0},{e1},{e2} vertex ids  {v0},{v1},{v2}");

            Logger.LogLine($"existing  halfedge twin ring  {t0},{t1},{t2} twin vertex ids  {v1},{v0},{v3}");

            // note original faces
            int fe0 = HalfEdges[e0].Face;
            int ft0 = HalfEdges[t0].Face;
            Logger.LogLine($"existing  faces faces  {fe0},{ft0}");

            // create a new vertex at the mid point of the halfedge
            int vertexStart = 0;
            int vertexEnd = 0;
            GetEdgeVertices(edge, out vertexStart, out vertexEnd);
            Logger.Log($"edge to split runs from vertex  {vertexStart},{vertexEnd} thats ");
            LogVertex(vertexStart);
            Logger.Log(" to ");
            LogVertex(vertexEnd);
            Logger.LogLine("");
            double mx = (Vertices[vertexStart].X + Vertices[vertexEnd].X) / 2.0;
            double my = (Vertices[vertexStart].Y + Vertices[vertexEnd].Y) / 2.0;
            double mz = (Vertices[vertexStart].Z + Vertices[vertexEnd].Z) / 2.0;
            int v4 = Vertices.Count;

            Vertex ver = new Vertex(mx, my, mz, -1);
            Vertices.Add(ver);
            Logger.LogLine($"new midpoint id={v4} at {ver.X},{ver.Y},{ver.Z}");

            int ne0 = AddHalfEdge(v4);
            int ne1 = AddHalfEdge(v4);
            int ne2 = AddHalfEdge(v2);
            Logger.LogLine($"Left side ne0={ne0},ne1={ne1},ne2={ne2}");
            newHalfEdges.Add(ne0);
            newHalfEdges.Add(ne1);
            newHalfEdges.Add(ne2);

            int nt0 = AddHalfEdge(v4);
            int nt1 = AddHalfEdge(v3);
            int nt2 = AddHalfEdge(v4);
            newHalfEdges.Add(nt0);
            newHalfEdges.Add(ne1);
            newHalfEdges.Add(ne2);
            Logger.LogLine($"right side nt0={nt0},nt1={nt1},nt2={nt2}");

            LinkEdges(e0, ne0, e2);
            LinkEdges(ne1, e1, ne2);
            LinkEdges(t0, nt2, t2);
            LinkEdges(nt0, t1, nt1);

            TwinUp(e0, nt0);
            TwinUp(nt1, nt2);
            TwinUp(t0, ne1);
            TwinUp(ne0, ne2);

            // mark exsing faces as dead
            Faces[fe0].FirstEdge = -1;
            Faces[ft0].FirstEdge = -1;

            CreateFace(e0, ne0, e2);
            CreateFace(t0, nt2, t2);
            CreateFace(ne1, ne2, e1);
            CreateFace(nt0, nt1, t1);

            // attach the new mid point to one of the new half edges

            ver.OutgoingHalfEdge = ne0;

            return v4;
        }

        /// <summary>
        /// Mark two halfedges as twins
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void TwinUp(int i, int j)
        {
            Logger.LogLine($"Twin up {i},{j}");
            HalfEdges[i].Twin = j;
            HalfEdges[j].Twin = i;
        }

        public void FlipTriangle(int e0)
        {
            // gather data
            int e1 = HalfEdges[e0].Next;
            int e2 = HalfEdges[e1].Next;

            int t0 = HalfEdges[e0].Twin;
            int t1 = HalfEdges[t0].Next;
            int t2 = HalfEdges[t1].Next;

            int face0 = HalfEdges[e0].Face;
            int face1 = HalfEdges[t0].Face;

            int v0 = HalfEdges[e0].StartVertex;
            int v1 = HalfEdges[e1].StartVertex;
            int v2 = HalfEdges[e2].StartVertex;
            int v3 = HalfEdges[t2].StartVertex;
            Logger.LogLine($" Flip edges {e0},{e1},{e2}");
            Logger.LogLine($" vertices {v0},{v1},{v2}");
            Logger.LogLine($" Flip twin edges {t0},{t1},{t2}");
            Logger.LogLine($" vertices {v1},{v0},{v3}");

            // all ready have e0 and t0 twins, gather the rest
            int e1t = HalfEdges[e1].Twin;
            int e2t = HalfEdges[e2].Twin;
            int t1t = HalfEdges[t1].Twin;
            int t2t = HalfEdges[t2].Twin;
            Logger.LogLine($" Outside twins e1t={e1t}, e2t = {e2t}, t1t={t1t}, t2t={t2t}");

            // now swap things around
            HalfEdges[e0].StartVertex = v2;
            HalfEdges[e1].StartVertex = v3;
            HalfEdges[e2].StartVertex = v1;
            Logger.LogLine($" After Flip {v2},{v3},{v1}");
            HalfEdges[t0].StartVertex = v3;
            HalfEdges[t1].StartVertex = v2;
            HalfEdges[t2].StartVertex = v0;
            Logger.LogLine($" After Flip {v3},{v2},{v0}");

            TwinUp(e1, t2t);
            TwinUp(e2, e1t);
            TwinUp(t1, e2t);
            TwinUp(t2, t1t);

            Vertices[v0].OutgoingHalfEdge = t2;
            Vertices[v1].OutgoingHalfEdge = e2;
            Vertices[v2].OutgoingHalfEdge = e0;
            Vertices[v3].OutgoingHalfEdge = t0;
        }
    }
}