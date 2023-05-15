using System;
using System.Collections.Generic;

namespace CSGLib
{
    internal class OctNode
    {
        // The whole list of All the points.
        // This list is shared by all the nodes
        public static List<Vertex> AllPoints;

        // the subnodes of this node
        public OctNode[,,] Nodes;

        internal static int MaxPointsPerNode;

        internal static int MaxTreeDepth;

        public Vertex Centre { get; set; }

        public int Depth { get; set; }

        public Vertex High { get; set; }

        public Vertex Low { get; set; }

        // The indices of the vertices contained in AllPoints that are within the bounds
        // of this node
        public List<int> PointsInOctNode { get; set; }

        public void Create(List<int> pntsInThisNode, Vertex low, Vertex high, int depth)
        {
            Low = low;
            High = high;
            Depth = depth;
            PointsInOctNode = pntsInThisNode;
            Nodes = null;
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
                    double dx = (High.Position.X - Low.Position.X) / 2.0;
                    double dy = (High.Position.Y - Low.Position.Y) / 2.0;
                    double dz = (High.Position.Z - Low.Position.Z) / 2.0;
                    Nodes = new OctNode[2, 2, 2];
                    List<int>[,,] pntsInNode = new List<int>[2, 2, 2];
                    for (int i = 0; i < 2; i++)
                    {
                        double lowX = Low.Position.X + (i * dx);
                        double highX = lowX + dx;

                        for (int j = 0; j < 2; j++)
                        {
                            double lowY = Low.Position.Y + (j * dy);
                            double highY = lowY + dy;

                            for (int k = 0; k < 2; k++)
                            {
                                double lowZ = Low.Position.Z + (k * dz);
                                double highZ = lowZ + dz;
                                Nodes[i, j, k] = new OctNode();
                                Nodes[i, j, k].Depth = Depth + 1;
                                Nodes[i, j, k].PointsInOctNode = new List<int>();
                                Nodes[i, j, k].Low = new Vertex(lowX, lowY, lowZ);
                                Nodes[i, j, k].High = new Vertex(highX, highY, highZ);
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
                                    if (AllPoints[pntIndex].Position.X >= Nodes[i, j, k].Low.Position.X)
                                    {
                                        if (AllPoints[pntIndex].Position.X < Nodes[i, j, k].High.Position.X)
                                        {
                                            if (AllPoints[pntIndex].Position.Y >= Nodes[i, j, k].Low.Position.Y)
                                            {
                                                if (AllPoints[pntIndex].Position.Y < Nodes[i, j, k].High.Position.Y)
                                                {
                                                    if (AllPoints[pntIndex].Position.Z >= Nodes[i, j, k].Low.Position.Z)
                                                    {
                                                        if (AllPoints[pntIndex].Position.Z < Nodes[i, j, k].High.Position.Z)
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
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        internal OctNode AddPoint(int index, Vertex position)
        {
            OctNode targ = FindNodeAround(position);
            if (targ == null)
            {
                targ = this;
            }
            if (targ.PointsInOctNode == null)
            {
                targ.PointsInOctNode = new List<int>();
            }
            targ.PointsInOctNode.Add(index);
            return targ;
        }

        internal OctNode FindNodeAround(Vertex pnt)
        {
            OctNode res = null;
            if (pnt.Position.X >= Low.Position.X &&
                pnt.Position.X < High.Position.X &&
                pnt.Position.Y >= Low.Position.Y &&
                pnt.Position.Y < High.Position.Y &&
                pnt.Position.Z >= Low.Position.Z &&
                pnt.Position.Z < High.Position.Z)
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