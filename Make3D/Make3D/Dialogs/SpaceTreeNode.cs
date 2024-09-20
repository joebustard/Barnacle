using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    internal class SpaceTreeNode
    {
        public enum NodeColour
        {
            Red,
            Black
        }

        private const double tolerance = 1E-7;
        private const double leftRightTolerance = 1E-4;

        public NodeColour Colour;

        private static SpaceTreeNode RotateLeft(SpaceTreeNode node)
        {
            SpaceTreeNode x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            x.Colour = node.Colour;
            node.Colour = NodeColour.Red;
            return x;
        }

        private static SpaceTreeNode RotateRight(SpaceTreeNode node)
        {
            SpaceTreeNode x = node.Left;
            node.Left = x.Right;
            x.Right = node;
            x.Colour = node.Colour;
            node.Colour = NodeColour.Red;
            return x;
        }

        private static void ColourFlip(SpaceTreeNode node)
        {
            node.Colour = NodeColour.Red;
            node.Left.Colour = NodeColour.Black;
            node.Right.Colour = NodeColour.Black;
        }

        private static bool IsRed(SpaceTreeNode node)
        {
            bool res = true;
            if (node != null && node.Colour == NodeColour.Black)
            {
                res = false;
            }
            return res;
        }

        public SpaceTreeNode(Point3D v)
        {
            Vertex = v;
        }

        public SpaceTreeNode Left
        {
            get; set;
        }

        public SpaceTreeNode Mid
        {
            get; set;
        }

        public SpaceTreeNode Right
        {
            get; set;
        }

        public Point3D Vertex
        {
            get; set;
        }

        public int Id { get; set; }

        public int Present(Point3D v)
        {
            int result = -1;

            double dx = v.X - Vertex.X;
            if (dx < -leftRightTolerance)
            {
                if (Left != null)
                {
                    result = Left.Present(v);
                }
            }
            else
            if (dx > leftRightTolerance)
            {
                if (Right != null)
                {
                    result = Right.Present(v);
                }
            }
            else
            {
                if (Math.Abs(v.X - Vertex.X) < tolerance && Math.Abs(v.Y - Vertex.Y) < tolerance && Math.Abs(v.Z - Vertex.Z) < tolerance)
                {
                    result = Id;
                }
                else
                {
                    if (Mid != null)
                    {
                        result = Mid.Present(v);
                    }
                }
            }

            return result;
        }

        public static SpaceTreeNode Create(Point3D v, int id)
        {
            SpaceTreeNode result = new SpaceTreeNode(v);
            result.Id = id;
            result.Colour = NodeColour.Red;
            return result;
        }

        public SpaceTreeNode Add(Point3D v, SpaceTreeNode node, int id)
        {
            SpaceTreeNode result = node;
            if (node == null)
            {
                result = new SpaceTreeNode(v);
                result.Id = id;
                result.Colour = NodeColour.Red;
            }
            else
            {
                double dx = v.X - node.Vertex.X;
                if (dx < -leftRightTolerance)
                {
                    node.Left = Add(v, node.Left, id);
                }
                else
                {
                    // right
                    if (dx > leftRightTolerance)
                    {
                        node.Right = Add(v, node.Right, id);
                    }
                    else
                    {
                        node.Mid = Add(v, node.Mid, id);
                    }
                }
            }
            if (node != null)
            {
                if (IsRed(node.Right) && !IsRed(node.Left))
                {
                    node = RotateLeft(node);
                }
                if (IsRed(node.Left) && !IsRed(node.Right?.Right))
                {
                    node = RotateRight(node);
                }

                if (IsRed(node.Left) && !IsRed(node.Right))
                {
                    ColourFlip(node);
                }
            }
            return result;
        }

        internal void AddToList(Point3DCollection tmp)
        {
            if (Left != null)
            {
                Left.AddToList(tmp);
            }
            tmp.Add(Vertex);

            if (Mid != null)
            {
                Mid.AddToList(tmp);
            }

            if (Right != null)
            {
                Right.AddToList(tmp);
            }
        }

        internal void Sort()
        {
            // the tree is arranged by x.
            // items with the same X should be in the mid but they might not have the same y or z
            // so sort the mids by y, then by z
            bool swapped = false;
            do
            {
                swapped = false;
                SpaceTreeNode t = this;
                while (t != null)
                {
                    if (t.Mid != null)
                    {
                        bool doit = false;
                        double dy = t.Vertex.Y - t.Mid.Vertex.Y;
                        if (dy > tolerance)
                        {
                            doit = true;
                        }
                        else
                        if ((Math.Abs(dy) < tolerance) &&
                             (t.Vertex.Z > t.Mid.Vertex.Z))
                        {
                            doit = true;
                        }
                        if (doit)
                        {
                            Point3D tmp = t.Vertex;
                            t.Vertex = t.Mid.Vertex;
                            t.Mid.Vertex = tmp;
                            swapped = true;
                        }
                    }
                    t = t.Mid;
                }
            } while (swapped);
            if (Left != null)
            {
                Left.Sort();
            }
            if (Right != null)
            {
                Right.Sort();
            }
        }
    }
}