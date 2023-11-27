using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PrintPlacementLib
{
    public class Component
    {
        public int Columns;
        public int Rows;
        private double highOffX;
        private double highOffY;
        private Point3D lastHitPoint;
        private double lowOffX;
        private double lowOffY;
        private double tx;
        private double ty;
        public double Clearance { get; internal set; }
        public int Density { get; set; }
        public Point3D HighBound { get; set; }
        public Point3D LowBound { get; set; }
        public BedMap Map { get; set; }
        public Point3D OriginalPosition { get; set; }
        public Point3D Position { get; set; }
        public Object3D Shape { get; set; }
        public List<Rect> FaceBounds { get; set; }
        internal void ExpandMap()
        {
            int newRows = Rows + 2;
            int newColumns = Columns + 2;
            BedMap bm = new BedMap(newRows, newColumns);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Map.Get(r, c))
                    {
                        for (int rn = 0; rn < 3; rn++)
                        {
                            for (int cn = 0; cn < 3; cn++)
                            {
                                bm.Set(r + rn, c + cn, true);
                            }
                        }
                    }
                }
            }
            Rows = newRows;
            Columns = newColumns;
            Map = bm;
        }
        public void CalcFaceBounds()
        {
           FaceBounds = new List<Rect>();
            for (int i = 0; i < Shape.TriangleIndices.Count; i += 3)
            {
                Point low = new Point(int.MaxValue, int.MaxValue);
                Point high = new Point(int.MinValue, int.MinValue);
                double X = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i]].X;
                double Y = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i]].Y;
                double Z = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i]].Z;
                low.X = Math.Min(low.X, X);
                low.Y = Math.Min(low.Y, Z);

                high.X = Math.Max(high.X, X);
                high.Y = Math.Max(high.Y, Z);

                X = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 1]].X;
                Y = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 1]].Y;
                Z = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 1]].Z;
                low.X = Math.Min(low.X, X);
                low.Y = Math.Min(low.Y, Z);

                high.X = Math.Max(high.X, X);
                high.Y = Math.Max(high.Y, Z);

                X = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 2]].X;
                Y = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 2]].Y;
                Z = Shape.AbsoluteObjectVertices[Shape.TriangleIndices[i + 2]].Z;
                low.X = Math.Min(low.X, X);
                low.Y = Math.Min(low.Y, Z);

                high.X = Math.Max(high.X, X);
                high.Y = Math.Max(high.Y, Z);
                low.X -= 0.1;
                low.Y -= 0.1;
                high.X += 0.1;
                high.Y += 0.1;
                Rect bnd = new Rect(low, high);
                FaceBounds.Add(bnd);
            }
        }
        internal double Score(int row, int column, int maxRow, int maxCol, Bounds3D bedBounds)
        {
            double score = double.MaxValue;
            // if row,col is hte low bound will the high bound be on the bed
            if ((row + Rows < maxRow) &&
                 (column + Columns < maxCol))
            {
            /*
                Bounds3D scoreBounds = new Bounds3D(bedBounds);
                int lr = row - (int)(lowOffY / Clearance);
                int lc = column - (int)(lowOffX / Clearance);
                scoreBounds.Adjust(new Point3D(lc, 0, lr));

                int ur = row - (int)(highOffY / Clearance);
                int uc = column - (int)(highOffX / Clearance);
                scoreBounds.Adjust(new Point3D(uc, 0, ur));
                score = scoreBounds.Width * scoreBounds.Depth;
                */
                
                // convert the row and col back to a real position
                Point orig = new Point(column * Clearance, row * Clearance);

                Point p1 = new Point(orig.X - lowOffX, orig.Y - lowOffY);
                Point p2 = new Point(orig.X + highOffX, orig.Y + highOffY);

                double d1 = Math.Sqrt((tx - p1.X) * (tx - p1.X) + (ty - p1.Y) * (ty - p1.Y));

                double d2 = Math.Sqrt((tx - p2.X) * (tx - p2.X) + (ty - p2.Y) * (ty - p2.Y));
                score = Math.Max(d1, d2);

              //  score = Math.Sqrt((tx - orig.X) * (tx - orig.X) + (ty - orig.Y) * (ty - orig.Y));
                
            }
            return score;
        }

        internal void SetMap()
        {
            CalcFaceBounds();
            lowOffX = OriginalPosition.X - LowBound.X;
            lowOffY = OriginalPosition.Z - LowBound.Z;
            highOffX = HighBound.X - OriginalPosition.X;
            highOffY = HighBound.Z - OriginalPosition.Z;
            Density = 0;
            if (Shape != null && Clearance > 0)
            {
                //Columns = (int)(Math.Ceiling((HighBound.X - LowBound.X) / Clearance));
                //Rows = (int)(Math.Ceiling((HighBound.Z - LowBound.Z) / Clearance));

                Columns = (int)(Math.Ceiling(Shape.AbsoluteBounds.Width / Clearance));
                Rows = (int)(Math.Ceiling((Shape.AbsoluteBounds.Depth) / Clearance));
                if (Columns > 0 && Rows > 0)
                {
                    double rayY = LowBound.Y - 1;
                    Map = new BedMap(Rows, Columns);

                    // create a visual that we can ray trace
                    MeshGeometry3D model = Shape.Mesh;

                    double resolution = 0.25;
                    for (int row = 0; row < Rows; row++)
                    {
                        double z = (row * Clearance) + LowBound.Z;

                        for (int col = 0; col < Columns; col++)
                        {
                            Map.Set(row, col, false);
                            double x = (col * Clearance) + LowBound.X;

                            // x,y is the bottom corner of a cell of size Clearance x CLearance;
                            // go accross the cell in steps of resolution x resolution projecting a ray up wards
                            double x1 = x;

                            bool more = true;
                            while (x1 < x + Clearance && more)
                            {
                                double z1 = z;
                                while (z1 < z + Clearance && more)
                                {
                                    // if the ray hits anything then mark map as occupied and break
                                    // i.e. no need to process any more rays in this cell
                                    if (RayHit(model, x1, z1, rayY))
                                    {
                                        Map.Set(row, col, true);
                                        Density++;
                                        more = false;
                                    }
                                    z1 += resolution;
                                }
                                x1 += resolution;
                            }
                        }
                    }
                    // Map.Dump( Shape.Name);
                    Density = Rows * Columns; // test
                }
            }
        }

        internal void SetTarget(double x, double y)
        {
            tx = x;
            ty = y;
        }

        private bool RayHit(MeshGeometry3D model, double x, double z, double rayY)
        {
            bool res = false;
            int faceIndex = 0;
            double t = 0;
            Vector3D position = new Vector3D(x, rayY, z);
            Vector3D upwards = new Vector3D(0, 1, 0);
            int faceNum = 0;
            while (faceIndex < model.TriangleIndices.Count && res == false)
            {
                Rect r = FaceBounds[faceNum];
                if (x >= r.Left && x < r.Right && z <= r.Bottom && z >= r.Top)
                {
                    int f0 = model.TriangleIndices[faceIndex];
                    int f1 = model.TriangleIndices[faceIndex + 1];
                    int f2 = model.TriangleIndices[faceIndex + 2];
                    Vector3D v0 = new Vector3D(model.Positions[f0].X, model.Positions[f0].Y, model.Positions[f0].Z);
                    Vector3D v1 = new Vector3D(model.Positions[f1].X, model.Positions[f1].Y, model.Positions[f1].Z);
                    Vector3D v2 = new Vector3D(model.Positions[f2].X, model.Positions[f2].Y, model.Positions[f2].Z);
                    if (Utils.RayTriangleIntersect(position, upwards, v0, v1, v2, out t))
                    {
                        res = true;
                    }
                }
                faceIndex += 3;
                faceNum++;
            }
            return res;
        }
    }
}