using System;
using System.Windows.Media.Media3D;

namespace ManifoldLib
{
    public class Vertex
    {
        public Point3D Pos { get; set; }
        public int DuplicateOf { get; set; }
        public int OriginalNumber { get; set; }
        public int NewNumber { get; set; }
        public int FaceReferencs { get; set; }

        public Vertex()
        {
            DuplicateOf = -1;
            FaceReferencs = 0;
        }

        internal void Dump()
        {
            System.Diagnostics.Debug.WriteLine($" {Pos.X},{Pos.Y},{Pos.Z}, original {OriginalNumber} Dup {DuplicateOf} new number {NewNumber}");
        }

        private const double tol = 0.000001;

        internal bool GreaterOrEqual(Vertex v)
        {
            bool res = false;

            if (Pos.X - v.Pos.X > tol)
            {
                res = true;
            }
            else
            {
                if (Math.Abs(Pos.X - v.Pos.X) < tol)
                {
                    if (Pos.Y - v.Pos.Y > tol)
                    {
                        res = true;
                    }
                    else
                    {
                        if (Math.Abs(Pos.Y - v.Pos.Y) < tol)
                        {
                            if (Math.Abs(Pos.Z - v.Pos.Z) < tol)
                            {
                                res = true;
                            }
                            else
                            if (Pos.Z - v.Pos.Z > tol)
                            {
                                res = true;
                            }
                        }
                    }
                }
            }

            return res;
        }
    }
}