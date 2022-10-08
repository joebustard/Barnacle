using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.MeshEditor
{
    internal class MeshOctNode
    {
        // The whole list of All the points.
        // This list is shared by all the nodes
        public static List<MeshVertex> AllPoints;

        // the subnodes of this node
        public MeshOctNode[,,] Nodes;

        internal static int MaxPointsPerNode;

        internal static int MaxTreeDepth;

        public Point3D Centre { get; set; }

        public int Depth { get; set; }

        public Point3D High { get; set; }

        public Point3D Low { get; set; }

        // The indices of the vertices contained in AllPoints that are within the bounds
        // of this node
        public Int32Collection PointsInOctNode { get; set; }

        public void Create(Int32Collection pntsInThisNode, Point3D low, Point3D high, int depth)
        {
            Low = low;
            High = high;
            Depth = depth;
            PointsInOctNode = pntsInThisNode;
            Nodes = null;
        }

        public void Split()
        {
            // if we havn't reached the max depth and there are lots of points
            // then make 8 sub nodes
            if (Depth < MaxTreeDepth && PointsInOctNode.Count > MaxPointsPerNode)
            {
                // Lots of points so subdivide this node into eight
                double dx = (High.X - Low.X) / 2.0;
                double dy = (High.Y - Low.Y) / 2.0;
                double dz = (High.Z - Low.Z) / 2.0;
                Nodes = new MeshOctNode[2, 2, 2];
                Int32Collection[,,] pntsInNode = new Int32Collection[2, 2, 2];
                for (int i = 0; i < 2; i++)
                {
                    double lowX = Low.X + (i * dx);
                    double highX = lowX + dx;

                    for (int j = 0; j < 2; j++)
                    {
                        double lowY = Low.Y + (j * dy);
                        double highY = lowY + dy;

                        for (int k = 0; k < 2; k++)
                        {
                            double lowZ = Low.Z + (k * dz);
                            double highZ = lowZ + dz;
                            Nodes[i, j, k] = new MeshOctNode();
                            Nodes[i, j, k].Depth = Depth + 1;
                            Nodes[i, j, k].PointsInOctNode = new Int32Collection();
                            Nodes[i, j, k].Low = new Point3D(lowX, lowY, lowZ);
                            Nodes[i, j, k].High = new Point3D(highX, highY, highZ);
                        }
                    }
                }

                foreach (int pntIndex in PointsInOctNode)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (AllPoints[pntIndex].Position.X >= Nodes[i, j, k].Low.X)
                                {
                                    if (AllPoints[pntIndex].Position.X < Nodes[i, j, k].High.X)
                                    {
                                        if (AllPoints[pntIndex].Position.Y >= Nodes[i, j, k].Low.Y)
                                        {
                                            if (AllPoints[pntIndex].Position.Y < Nodes[i, j, k].High.Y)
                                            {
                                                if (AllPoints[pntIndex].Position.Z >= Nodes[i, j, k].Low.Z)
                                                {
                                                    if (AllPoints[pntIndex].Position.Z < Nodes[i, j, k].High.Z)
                                                    {
                                                        Nodes[i, j, k].PointsInOctNode.Add(pntIndex);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                PointsInOctNode.Clear();
                PointsInOctNode = null;
            }
        }

        internal void AddPoint(int index, Point3D position)
        {
            MeshOctNode targ = FindNodeAround(position);
            if (targ != null)
            {
                targ.PointsInOctNode.Add(index);
            }
        }

        internal MeshOctNode FindNodeAround(Point3D pnt)
        {
            MeshOctNode res = null;
            if (pnt.X >= Low.X &&
                pnt.X < High.X &&
                pnt.Y >= Low.Y &&
                pnt.Y < High.Y &&
                pnt.Z >= Low.Z &&
                pnt.Z < High.Z)
            {
                // if we contain points then this probably as far down the tree as we can get
                if (PointsInOctNode != null && PointsInOctNode.Count > 0)
                {
                    res = this;
                }
                else
                {
                    for (int i = 0; i < 2 && res == null; i++)
                    {
                        for (int j = 0; j < 2 && res == null; j++)
                        {
                            for (int k = 0; k < 2 && res == null; k++)
                            {
                                res = Nodes[i, j, k].FindNodeAround(pnt);
                            }
                        }
                    }
                }
            }

            return res;
        }
    }
}