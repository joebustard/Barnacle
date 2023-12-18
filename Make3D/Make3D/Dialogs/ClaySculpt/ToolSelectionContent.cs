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