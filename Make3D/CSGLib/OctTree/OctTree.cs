using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CSGLib
{
    internal class OctTree
    {
        private const int MaxPointsPerNode = 500;
        private OctNode root;

        public OctTree(List<Vertex> pnts, Vertex minPnt, Vertex maxPnt, int maxTreeDepth)
        {
            root = new OctNode();
            minPnt.Position = new Vector3D(minPnt.Position .X- 10, minPnt.Position.Y - 10, minPnt.Position.Z- 10);
            maxPnt.Position = new Vector3D(maxPnt.Position.X + 10, maxPnt.Position.Y + 10, maxPnt.Position.Z + 10);

            OctNode.MaxTreeDepth = maxTreeDepth;
            OctNode.AllPoints = pnts;
            OctNode.MaxPointsPerNode = MaxPointsPerNode;
            List<int> indices = new List<int>();
            for (int i = 0; i < pnts.Count; i++)
            {
                indices.Add(i);
            }
            root.Create(indices, minPnt, maxPnt, 0);
            root.Split();
        }

        public OctNode FindNodeAround(Vertex pnt)
        {
            OctNode res = root?.FindNodeAround(pnt);

            return res;
        }

        internal void AddPoint(int index, Vertex position)
        {
            try
            {
                OctNode container = root.AddPoint(index, position);
                OctNode.AllPoints.Add(position);
                container?.Split();
            }
            catch (Exception ex)
            {
                
            }
        }
        public int PointPresent(Vertex pnt)
        {
            int res = -1;
            try
            {
                OctNode node = root?.FindNodeAround(pnt);
                if (node != null)
                {
                    foreach (int index in node.PointsInOctNode)
                    {
                        if (pnt.Equals(OctNode.AllPoints[index]))
                        {
                            res = index;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
    }
}
