using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OctTreeLib
{
    public class OctNode
    {
        // The whole list of All the points.
        public Point3DCollection AllPoints;

        // the subnodes of this node
        public OctNode[,,] Nodes;

        internal static int MaxPointsPerNode;

        internal static int MaxTreeDepth;

        public Point3D Centre { get; set; }

        public int Depth { get; set; }

        public Point3D High { get; set; }

        public Point3D Low { get; set; }

        // The indices of the vertices contained in AllPoints that are within the bounds
        // of this node
        public Int32Collection PointsInOctNode { get; set; }

        public void Create(Int32Collection pntsInThisNode, Point3D low, Point3D high, int depth, Point3DCollection allPoints)
        {
            Low = low;
            High = high;
            Depth = depth;
            PointsInOctNode = pntsInThisNode;
            Nodes = null;
            AllPoints = allPoints;
        }

        public void Split()
        {
            try
            {
                // if we havn't reached the max depth and there are lots of points
                // then make 8 sub nodes
                if (Depth < MaxTreeDepth && PointsInOctNode != null && PointsInOctNode.Count > MaxPointsPerNode)
                {
                    // Lots of points so subdivide this node into eight
                    double dx = (High.X - Low.X) / 2.0;
                    double dy = (High.Y - Low.Y) / 2.0;
                    double dz = (High.Z - Low.Z) / 2.0;
                    Nodes = new OctNode[2, 2, 2];
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
                                Nodes[i, j, k] = new OctNode();
                                Nodes[i, j, k].AllPoints = AllPoints;
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
                                    if (AllPoints[pntIndex].X >= Nodes[i, j, k].Low.X)
                                    {
                                        if (AllPoints[pntIndex].X < Nodes[i, j, k].High.X)
                                        {
                                            if (AllPoints[pntIndex].Y >= Nodes[i, j, k].Low.Y)
                                            {
                                                if (AllPoints[pntIndex].Y < Nodes[i, j, k].High.Y)
                                                {
                                                    if (AllPoints[pntIndex].Z >= Nodes[i, j, k].Low.Z)
                                                    {
                                                        if (AllPoints[pntIndex].Z < Nodes[i, j, k].High.Z)
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

                    if (Nodes != null)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    Nodes[i, j, k].Split();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Split() moving pnts" + ex.Message);
            }
        }

        internal OctNode AddPoint(int index, Point3D position)
        {
            OctNode targ = FindNodeAround(position);
            if (targ == null)
            {
                targ = this;
            }
            if (targ.PointsInOctNode == null)
            {
                targ.PointsInOctNode = new Int32Collection();
            }
            targ.PointsInOctNode.Add(index);
            return targ;
        }

        internal OctNode FindNodeAround(Point3D pnt)
        {
            OctNode res = null;
            if (pnt.X >= Low.X &&
                pnt.X < High.X &&
                pnt.Y >= Low.Y &&
                pnt.Y < High.Y &&
                pnt.Z >= Low.Z &&
                pnt.Z < High.Z)
            {
                // if we contain points then this probably as far down the tree as we can get
                if (PointsInOctNode != null && Nodes == null)
                {
                    res = this;
                }
                else
                {
                    if (Nodes != null)
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
            }

            return res;
        }
    }
}