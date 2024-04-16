using System;
using System.Collections.Generic;

namespace ManifoldLib
{
    public class VertexTreeNode
    {
        public enum NodeColour
        {
            Red,
            Black
        }

        private const double tolerance = 1E-7;

        public NodeColour Colour;

        private static VertexTreeNode RotateLeft(VertexTreeNode node)
        {
            VertexTreeNode x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            x.Colour = node.Colour;
            node.Colour = NodeColour.Red;
            return x;
        }

        private static VertexTreeNode RotateRight(VertexTreeNode node)
        {
            VertexTreeNode x = node.Left;
            node.Left = x.Right;
            x.Right = node;
            x.Colour = node.Colour;
            node.Colour = NodeColour.Red;
            return x;
        }

        private static void ColourFlip(VertexTreeNode node)
        {
            node.Colour = NodeColour.Red;
            node.Left.Colour = NodeColour.Black;
            node.Right.Colour = NodeColour.Black;
        }

        private static bool IsRed(VertexTreeNode node)
        {
            bool res = true;
            if (node != null && node.Colour == NodeColour.Black)
            {
                res = false;
            }
            return res;
        }

        public VertexTreeNode(Vertex v)
        {
            Vertex = v;
        }

        public VertexTreeNode Left
        {
            get; set;
        }

        public VertexTreeNode Mid
        {
            get; set;
        }

        public VertexTreeNode Right
        {
            get; set;
        }

        public Vertex Vertex
        {
            get; set;
        }

        public static VertexTreeNode Add(Vertex v, VertexTreeNode node)
        {
            VertexTreeNode result = node;
            if (node == null)
            {
                result = new VertexTreeNode(v);
                result.Colour = NodeColour.Red;
            }
            else
            {
                double dx = v.Pos.X - node.Vertex.Pos.X;
                if (dx < -tolerance)
                {
                    node.Left = Add(v, node.Left);
                }
                else
                {
                    // right
                    if (dx > tolerance)
                    {
                        node.Right = Add(v, node.Right);
                    }
                    else
                    {
                        node.Mid = Add(v, node.Mid);
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

        private void BalanceLeft(VertexTreeNode parent)
        {
        }

        private void BalanceRight(VertexTreeNode parent)
        {
        }

        internal void AddToList(List<Vertex> tmp)
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
                VertexTreeNode t = this;
                while (t != null)
                {
                    if (t.Mid != null)
                    {
                        bool doit = false;
                        double dy = t.Vertex.Pos.Y - t.Mid.Vertex.Pos.Y;
                        if (dy > tolerance)
                        {
                            doit = true;
                        }
                        else
                        if ((Math.Abs(dy) < tolerance) &&
                             (t.Vertex.Pos.Z > t.Mid.Vertex.Pos.Z))
                        {
                            doit = true;
                        }
                        if (doit)
                        {
                            Vertex tmp = t.Vertex;
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