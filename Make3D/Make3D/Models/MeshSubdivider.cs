using Plankton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class MeshSubdivider
    {
        private PlanktonMesh pMesh;

        public MeshSubdivider(Point3DCollection tmp, Int32Collection triangleIndices)
        {
            pMesh = new PlanktonMesh(tmp, triangleIndices);
        }

        public void Subdivide(Point3DCollection v, Int32Collection tri)
        {
            v.Clear();
            tri.Clear();
            int edgeCount = pMesh.Halfedges.Count;
            List<int> faces = new List<int>();

            foreach (PlanktonFace f in pMesh.Faces)
            {
                faces.Add(f.FirstHalfedge);
            }
            foreach (int i in faces)
            {
                pMesh.Halfedges.TriangleSplitEdge(i);
            }
            if (pMesh != null)
            {
                pMesh.ToSoup(v, tri);
            }
        }
    }
}