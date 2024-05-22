using OctTreeLib;
using System.Threading;
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

            // oddly truncating vertice resolution may result in some faces ending up with illegal
            // indices where two points of a triangle are the same Throw these awy
            Int32Collection filtered = new Int32Collection();
            for (int i = 0; i < indices.Count; i += 3)
            {
                if (indices[i] != indices[i + 1] &&
                     indices[i + 1] != indices[i + 2] &&
                     indices[i + 2] != indices[i])
                {
                    filtered.Add(indices[i]);
                    filtered.Add(indices[i + 1]);
                    filtered.Add(indices[i + 2]);
                }
            }

            octTree = new OctTree(Vertices, minPnt, maxPnt, 200);

            for (int i = 0; i < filtered.Count; i++)
            {
                int f0 = filtered[i];
                Point3D p = points[f0];
                int nf0 = AddVertice(p);
                Faces.Add(nf0);
            }
        }

        public void RemoveDuplicateVerticesCancellable(Point3DCollection points, Int32Collection indices, CancellationToken token)
        {
            Faces = new Int32Collection();
            Vertices = new Point3DCollection();
            Point3D minPnt = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D maxPnt = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            MinMax(points, ref minPnt, ref maxPnt);

            // oddly truncating vertice resolution may result in some faces ending up with illegal
            // indices where two points of a triangle are the same Throw these awy
            Int32Collection filtered = new Int32Collection();
            for (int i = 0; i < indices.Count && !token.IsCancellationRequested; i += 3)
            {
                if (indices[i] != indices[i + 1] &&
                     indices[i + 1] != indices[i + 2] &&
                     indices[i + 2] != indices[i])
                {
                    filtered.Add(indices[i]);
                    filtered.Add(indices[i + 1]);
                    filtered.Add(indices[i + 2]);
                }
            }

            octTree = new OctTree(Vertices, minPnt, maxPnt, 200);

            for (int i = 0; i < filtered.Count && !token.IsCancellationRequested; i++)
            {
                int f0 = filtered[i];
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
                res = Vertices.Count;
                octTree.AddPoint(res, v);
            }
            return res;
        }
    }
}