using Plankton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.ClaySculpt
{
    public class ToolSelectionContent
    {
        public List<FaceStatus> FaceStates;
        public Int32Collection SelectedVertices;
        private PlanktonMesh pmesh;
        public Int32Collection SearchVertexQueue;

        public ToolSelectionContent(PlanktonMesh pmesh)
        {
            FaceStates = new List<FaceStatus>();
            SelectedVertices = new Int32Collection();
            SearchVertexQueue = new Int32Collection();
            this.pmesh = pmesh;
        }

        public void Clear()
        {
            FaceStates.Clear();
            SelectedVertices.Clear();
            SearchVertexQueue.Clear();
        }

        internal PlanktonVertex GetVertex(int vid)
        {
            PlanktonVertex vx = pmesh.Vertices[vid];

            return vx;
        }

        internal PlanktonXYZ GetVertexNormal(int vid)
        {
            PlanktonXYZ vx = pmesh.GetVertexNormal(vid);

            return vx;
        }

        internal void InitialVertexSelection(int v0)
        {
            if (v0 >= 0 && v0 < pmesh.Vertices.Count && !SelectedVertices.Contains(v0))
            {
                SearchVertexQueue.Add(v0);
            }
        }

        internal void MoveVertex(PlanktonXYZ offset, int vid)
        {
            pmesh.Vertices[vid].X += offset.X;
            pmesh.Vertices[vid].Y += offset.Y;
            pmesh.Vertices[vid].Z += offset.Z;
        }

        private Point3D hitCentre;
        private double searchRadius;

        internal void SelectedInRange(Point3D centre, double radius)
        {
            hitCentre = centre;
            searchRadius = radius;
            System.Diagnostics.Debug.WriteLine($"Tool at {centre.X},{centre.Y},{centre.Z}, radius={radius}");
            if (SearchVertexQueue != null)
            {
                while (SearchVertexQueue.Count > 0)
                {
                    int vid = SearchVertexQueue[0];
                    SearchVertexQueue.RemoveAt(0);
                    if (!SelectedVertices.Contains(vid))
                    {
                        PlanktonVertex vx = pmesh.Vertices[vid];
                        if (Distance(centre, vx) < radius)
                        {
                            SelectedVertices.Add(vid);

                            int origin = vx.OutgoingHalfedge;
                            int startHe = origin;
                            do
                            {
                                PlanktonHalfedge he = pmesh.Halfedges[startHe];
                                SearchVertexQueue.Add(he.StartVertex);
                                he = pmesh.Halfedges[he.NextHalfedge];
                                SearchVertexQueue.Add(he.StartVertex);
                                he = pmesh.Halfedges[he.NextHalfedge];
                                SearchVertexQueue.Add(he.StartVertex);
                                he = pmesh.Halfedges[he.NextHalfedge];
                                he = pmesh.Halfedges[he.Twin];

                                startHe = he.NextHalfedge;
                            } while (startHe != origin);
                        }
                    }
                }

                DumpSelectedVertices();




            }
        }

        private void DumpSearchFaces(Int32Collection fces)
        {
            System.Diagnostics.Debug.WriteLine("SearchFaces");
            foreach (int f in fces)
            {
                System.Diagnostics.Debug.Write($"{f},");
            }
            System.Diagnostics.Debug.WriteLine("Search");
        }



        public void SubdivideSelectedFaces()
        {
            foreach (int vindex in SelectedVertices)
            {
                // get a list of the halfedges from this point
                
                List<int> el = GetListOfEdgesFromPoint(vindex);
               //List<int> el= pmesh.Halfedges.GetVertexCirculator(firstHalfEdge).ToList();

                foreach (int outbound in el)
                {
                    int face = pmesh.Halfedges[outbound].Face;
                    
                    // el is a list of the first edges, the next edge on from each of these is
                    // the face opposite the point
                    // int oppositeEdge = pmesh.Halfedges[outbound].NextHalfedge;
                    //int lastEdge = pmesh.Halfedges[oppositeEdge].NextHalfedge;
                    // pmesh.Halfedges.SplitEdge(oppositeEdge);
                   
                }
                break; // just do one for now
            }
        }

        public List<int> GetListOfEdgesFromPoint(int vindex)
        {
            List<int> res = new List<int>();
            int firstHalfEdge = pmesh.Vertices[vindex].OutgoingHalfedge;
            int cur = firstHalfEdge;
            int count = 0;
            do
            {
                if ( !res.Contains(cur))
                {
                    res.Add(cur);
                }
                cur = pmesh.Halfedges[cur].NextHalfedge;
                cur = pmesh.Halfedges[cur].NextHalfedge;
                cur = pmesh.Halfedges[cur].Twin;
                count++;
            } while (count < 1000 && cur != firstHalfEdge);
            return res;
        }

        private int CountPointsOfFace(int faceId, Int32Collection verticeList)
        {
            int res = 0;
            PlanktonFace face = pmesh.Faces[faceId];
            int startHe = face.FirstHalfedge;

            PlanktonHalfedge he = pmesh.Halfedges[startHe];
            if (verticeList.Contains(he.StartVertex))
            {
                res++;
            }
            he = pmesh.Halfedges[he.NextHalfedge];

            if (verticeList.Contains(he.StartVertex))
            {
                res++;
            }
            he = pmesh.Halfedges[he.NextHalfedge];

            if (verticeList.Contains(he.StartVertex))
            {
                res++;
            }
            return res;
        }

        private void DumpFaceList(Int32Collection faces)
        {
            System.Diagnostics.Debug.WriteLine("Faces");
            foreach (int f in faces)

            {
                System.Diagnostics.Debug.Write($"{f},");
            }
            System.Diagnostics.Debug.WriteLine("");
        }

        private void DumpSelectedVertices()
        {
            System.Diagnostics.Debug.WriteLine("SelectedVertices");
            foreach (int v in SelectedVertices)
            {
                System.Diagnostics.Debug.Write($"{v}=");
                System.Diagnostics.Debug.Write($"{pmesh.Vertices[v].X},");
                System.Diagnostics.Debug.Write($"{pmesh.Vertices[v].Y},");
                System.Diagnostics.Debug.WriteLine($"{pmesh.Vertices[v].Z}");
            }
        }

        public double Distance(Point3D c, PlanktonVertex vx)
        {
            double res = Math.Sqrt(
                (c.X - (double)vx.X) * (c.X - (double)vx.X) +
                (c.Y - (double)vx.Y) * (c.Y - (double)vx.Y) +
                (c.Z - (double)vx.Z) * (c.Z - (double)vx.Z)
                );

            return res;
        }
    }
}


