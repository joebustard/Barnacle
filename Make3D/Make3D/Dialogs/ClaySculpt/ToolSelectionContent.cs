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

        internal void InitialFaceSelection(int v0)
        {
            PlanktonVertex pv0 = null;
            if (v0 >= 0 && v0 < pmesh.Vertices.Count)
            {
                SearchVertexQueue.Add(v0);
            }
        }

        internal void SelectedInRange(Point3D centre, double radius)
        {
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
                                while (he.NextHalfedge != startHe)
                                {
                                    SearchVertexQueue.Add(he.StartVertex);
                                    he = pmesh.Halfedges[he.NextHalfedge];
                                }
                                he = pmesh.Halfedges[he.NextHalfedge];
                                startHe = he.Twin;
                            } while (startHe != origin);
                        }
                    }
                }

                DumpSelectedVertices();

                Int32Collection searchFaces = new Int32Collection();
                foreach (int vid in SelectedVertices)
                {
                    // find all the faces that this point belongs too
                    PlanktonVertex vx = pmesh.Vertices[vid];
                    int origin = vx.OutgoingHalfedge;
                    int startHe = origin;
                    do
                    {
                        PlanktonHalfedge he = pmesh.Halfedges[startHe];
                        if (!searchFaces.Contains(he.Face))
                        {
                            searchFaces.Add(he.Face);
                        }

                        startHe = he.Twin;
                        startHe = pmesh.Halfedges[startHe].NextHalfedge;
                    } while (startHe != origin);
                }
            }
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

        private double Distance(Point3D c, PlanktonVertex vx)
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