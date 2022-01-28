using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Barnacle.Object3DLib;

namespace Barnacle.Models
{
    internal class OctTree
    {
        private const int MaxPointsPerNode = 1000;
        private OctNode root;

        public OctTree(Point3DCollection pnts, Point3D minPnt, Point3D maxPnt, int maxTreeDepth)
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

        internal void AddPoint(int index, Point3D position)
        {
            root.AddPoint(index, position);
        }
    }
}