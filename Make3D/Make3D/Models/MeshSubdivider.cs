using HalfEdgeLib;
using LoggerLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class MeshSubdivider
    {
        private Mesh pMesh;

        public MeshSubdivider(Point3DCollection tmp, Int32Collection triangleIndices)
        {
            Logger.LogDateTime("Starting Mesh Subdivision");
            pMesh = new Mesh(tmp, triangleIndices);
            //  pMesh.Dump("Before");
        }

        public void Subdivide(Point3DCollection v, Int32Collection tri)
        {
            v.Clear();
            tri.Clear();

            pMesh.SplitAllEdges();

            if (pMesh != null)
            {
                pMesh.ToSoup(v, tri);
            }
        }
    }
}