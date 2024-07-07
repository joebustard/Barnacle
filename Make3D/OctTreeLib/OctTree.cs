using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OctTreeLib
{
    public class OctTree
    {
        private const int MaxPointsPerNode = 500;
        private OctNode root;

        public OctTree(Point3DCollection pnts, Point3D minPnt, Point3D maxPnt, int maxTreeDepth = 200)
        {
            root = new OctNode();
            minPnt += new Vector3D(-10, -10, -10);
            maxPnt += new Vector3D(10, 10, 10);

            OctNode.MaxTreeDepth = maxTreeDepth;
            OctNode.AllPoints = pnts;
            OctNode.MaxPointsPerNode = MaxPointsPerNode;
            Int32Collection indices = new Int32Collection(pnts.Count);
            for (int i = 0; i < pnts.Count; i++)
            {
                indices.Add(i);
            }
            root.Create(indices, minPnt, maxPnt, 0);
            root.Split();
        }

        public OctNode FindNodeAround(Point3D pnt)
        {
            OctNode res = root?.FindNodeAround(pnt);

            return res;
        }

        public void AddPoint(int index, Point3D position)
        {
            try
            {
                OctNode container = root.AddPoint(index, position);
                OctNode.AllPoints.Add(position);
                container?.Split();
            }
            catch (Exception ex)
            {
                MessageBox.Show("AddPoint() " + ex.Message);
            }
        }

        public int PointPresent(Point3D pnt)
        {
            int res = -1;
            try
            {
                OctNode node = root?.FindNodeAround(pnt);
                if (node != null)
                {
                    foreach (int index in node.PointsInOctNode)
                    {
                        if (Equals(OctNode.AllPoints[index], pnt.X, pnt.Y, pnt.Z))
                        {
                            res = index;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PointPresent() " + ex.Message);
            }
            return res;
        }

        private bool Equals(Point3D p, double x, double y, double z)
        {
            return equals(p.X, x) && equals(p.Y, y) && equals(p.Z, z);
        }

        public static bool equals(double v1, double v2)
        {
            if (Math.Abs(v1 - v2) < 0.00000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}