using OctTreeLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FixLib
{
    public class Fixer
    {
        private OctTree octTree;

        public Int32Collection Faces { get; set; }
        public Point3DCollection Vertices { get; set; }

        public void MinMax(Point3DCollection pnts, ref Point3D min, ref Point3D max)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;

            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;
            foreach (Point3D pd in pnts)
            {
                if (pd.X < minX)
                {
                    minX = pd.X;
                }
                if (pd.Y < minY)
                {
                    minY = pd.Y;
                }
                if (pd.Z < minZ)
                {
                    minZ = pd.Z;
                }

                if (pd.X > maxX)
                {
                    maxX = pd.X;
                }

                if (pd.Y > maxY)
                {
                    maxY = pd.Y;
                }

                if (pd.Z > maxZ)
                {
                    maxZ = pd.Z;
                }
            }
            min = new Point3D(minX, minY, minZ);
            max = new Point3D(maxX, maxY, maxZ);
        }

        public void RemoveDuplicateVertices(Point3DCollection points, Int32Collection indices)
        {
            Faces = new Int32Collection();
            Vertices = new Point3DCollection();
            Point3D minPnt = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D maxPnt = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            MinMax(points, ref minPnt, ref maxPnt);

            octTree = new OctTree(Vertices, minPnt, maxPnt, 200);

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
            res = octTree.PointPresent(v);

            if (res == -1)
            {
                //Vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = Vertices.Count;
                octTree.AddPoint(res, v);
            }
            return res;
        }
    }
}