using System;
using System.Collections.Generic;

namespace ManifoldLib
{
    public class VertexTreeNode
    {
        private const double tolerance = 1E-6;

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

        public void Add(Vertex v)
        {
            double dx = v.Pos.X - Vertex.Pos.X;
            if (dx < -tolerance)
            {
                // left
                if (Left == null)
                {
                    Left = new VertexTreeNode(v);
                }
                else
                {
                    Left.Add(v);
                }
            }
            else
            {
                // right
                if (dx > tolerance)
                {
                    if (Right == null)
                    {
                        Right = new VertexTreeNode(v);
                    }
                    else
                    {
                        Right.Add(v);
                    }
                }
                else
                {
                    if (Mid == null)
                    {
                        Mid = new VertexTreeNode(v);
                    }
                    else
                    {
                        Mid.Add(v);
                    }
                }
            }
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