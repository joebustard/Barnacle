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
        private Point3D lastHitPoint;

        public Object3D Shape { get; set; }
        public Point3D LowBound { get; set; }
        public Point3D HighBound { get; set; }
        public Point3D Position { get; set; }
        public Point3D OriginalPosition { get; set; }
        public BedMap Map { get; set; }
        public int Clearance { get; internal set; }

        internal void SetMap()
        {
            if (Shape != null && Clearance > 0)
            {
                int columns = (int)(Math.Ceiling((HighBound.X - LowBound.X) / Clearance));
                int rows = (int)(Math.Ceiling((HighBound.Z - LowBound.Z) / Clearance));
                if (columns > 0 && rows > 0)
                {
                    double rayY = LowBound.Y - 1;
                    Map = new BedMap(rows, columns);

                    // create a visual that we can ray trace
                    MeshGeometry3D model = Shape.Mesh;

                    double resolution = 0.25;
                    for (int row = 0; row < rows; row++)
                    {
                        double z = (row * Clearance) + LowBound.Z;

                        for (int col = 0; col < columns; col++)
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
                                        more = false;
                                    }
                                    z1 += resolution;
                                }
                                x1 += resolution;
                            }
                        }
                    }
                    Map.Dump();
                }
            }
        }

        private bool RayHit(MeshGeometry3D model, double x, double z, double rayY)
        {
            bool res = false;
            int faceIndex = 0;
            double t = 0;
            Vector3D position = new Vector3D(x, rayY, z);
            Vector3D upwards = new Vector3D(0, 1, 0);
            while (faceIndex < model.TriangleIndices.Count && res == false)
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
                faceIndex += 3;
            }
            return res;
        }
    }
}