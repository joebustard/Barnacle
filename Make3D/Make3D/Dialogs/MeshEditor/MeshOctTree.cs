using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.MeshEditor
{
    internal class MeshOctTree
    {
        private const int MaxPointsPerNode = 1000;
        private MeshOctNode root;

        public MeshOctTree(List<MeshVertex> pnts, Point3D minPnt, Point3D maxPnt, int maxTreeDepth)
        {
            root = new MeshOctNode();
            minPnt += new Vector3D(-1, -1, -1);
            maxPnt += new Vector3D(1, 1, 1);

            MeshOctNode.MaxTreeDepth = maxTreeDepth;
            MeshOctNode.AllPoints = pnts;
            MeshOctNode.MaxPointsPerNode = MaxPointsPerNode;
            Int32Collection indices = new Int32Collection(pnts.Count);
            for (int i = 0; i < pnts.Count; i++)
            {
                indices.Add(i);
            }
            root.Create(indices, minPnt, maxPnt, 0);
            root.Split();
        }

        public MeshOctNode FindNodeAround(Point3D pnt)
        {
            MeshOctNode res = root?.FindNodeAround(pnt);

            return res;
        }

        internal void AddPoint(int index, Point3D position)
        {
            root.AddPoint(index, position);
        }

        internal void FindPointsInRadius(double radius, Point3D pos, Int32Collection pointsInRadius, List<double> distances)
        {
            List<MeshOctNode> relevantBoxes = new List<MeshOctNode>();
            pointsInRadius.Clear();
            distances.Clear();
            FindRelevantBoxes(pos, radius, relevantBoxes);
            if (relevantBoxes.Count > 0)
            {
                double radrad = radius * radius;
                foreach (MeshOctNode nd in relevantBoxes)
                {
                    foreach (int ip in nd.PointsInOctNode)
                    {
                        MeshVertex mv = MeshOctNode.AllPoints[ip];

                        double dd = ((mv.Position.X - pos.X) * (mv.Position.X - pos.X)) +
                        ((mv.Position.Y - pos.Y) * (mv.Position.Y - pos.Y)) +
                        ((mv.Position.Z - pos.Z) * (mv.Position.Z - pos.Z));
                        if (dd < radrad)
                        {
                            pointsInRadius.Add(ip);
                            distances.Add(Math.Sqrt(dd));
                        }
                    }
                }
            }
        }

        private void FindRelevantBoxes(Point3D pos, double radius, List<MeshOctNode> relevantBoxes)
        {
            // for now just add the box that the search point is in
            MeshOctNode bx;
            double x;
            double y;
            double z;
            for (x = pos.X - radius; x <= pos.X + radius; x += radius)
            {
                for (y = pos.Y - radius; y <= pos.Y + radius; y += radius)
                {
                    for (z = pos.Z - radius; z <= pos.Z + radius; z += radius)
                    {
                        bx = FindNodeAround(new Point3D(x, y, z));
                        if (bx != null && !relevantBoxes.Contains(bx))
                        {
                            relevantBoxes.Add(bx);
                        }
                    }
                }
            }
        }
        public int PointPresent(Point3D pnt)
        {
            int res = -1;
            try
            {
                MeshOctNode node = root?.FindNodeAround(pnt);
                if (node != null)
                {
                    foreach (int index in node.PointsInOctNode)
                    {
                        if (PointUtils.equals(MeshOctNode.AllPoints[index].Position, pnt.X, pnt.Y, pnt.Z))
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
    }
}