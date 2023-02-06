using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FixLib
{
    public class Fixer
    {
        private SpaceTreeNode spaceTreeRoot;
        public Int32Collection Faces { get; set; }
        public Point3DCollection Vertices { get; set; }
        public void RemoveDuplicateVertices(Point3DCollection points, Int32Collection indices)
        {
            spaceTreeRoot = null;
            Faces = new Int32Collection();
            Vertices = new Point3DCollection();

            for (int i = 0; i < indices.Count; i++)
            {
                int f0 = indices[i];
                Point3D p = points[f0];
                int nf0 = AddVertice(p);
                Faces.Add(nf0);
            }
        }

        private int AddVertice(Point3D v)
        {
            int res = -1;
            if (spaceTreeRoot != null)
            {
                res = spaceTreeRoot.Present(v);
            }

            if (res == -1)
            {
                Vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = Vertices.Count - 1;
                if (spaceTreeRoot == null)
                {
                    spaceTreeRoot = SpaceTreeNode.Create(v, res);
                }
                else
                {
                    spaceTreeRoot.Add(v, spaceTreeRoot, res);
                }
            }
            return res;
        }
    }
}
