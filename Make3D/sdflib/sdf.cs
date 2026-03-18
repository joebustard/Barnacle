using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MathsLib;

namespace sdflib
{
    public class Sdf : Isdf
    {
        public double[,,] distances;
        private int columns;
        private int planes;
        private int rows;

        public Sdf()
        {
            rows = 0;
            columns = 0;
            planes = 0;
            distances = null;
        }

        public enum OpType
        {
            Addition,
            Subtraction,
            Intersection,
            SmoothAddition,
            SmoothSubtraction,
            SmoothIntersection
        }

        public void Clear(double val)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    for (int p = 0; p < planes; p++)
                    {
                        distances[r, c, p] = val;
                    }
                }
            }
        }

        public double Get(int r, int c, int p)
        {
            double res = Double.NaN;
            if (r >= 0 && c >= 0 && p >= 0)
            {
                if (r < rows && c < columns && p < planes)
                {
                    res = distances[r, c, p];
                }
            }
            return res;
        }

        /// <summary>
        ///  interpolating version of get
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public double GetAt(double x, double y, double z)
        {
            double res = Double.NaN;
            double xd = x - Math.Floor(x);
            double yd = y - Math.Floor(y);
            double zd = z - Math.Floor(z);
            x = Math.Floor(x);
            y = Math.Floor(y);
            z = Math.Floor(z);

            double c000 = Get((int)x, (int)y, (int)z);
            double c110 = Get((int)x + 1, (int)y + 1, (int)z);
            double c010 = Get((int)x, (int)y + 1, (int)z);
            double c100 = Get((int)x + 1, (int)y, (int)z);
            double c001 = Get((int)x, (int)y, (int)z + 1);
            double c011 = Get((int)x, (int)y + 1, (int)z + 1);
            double c111 = Get((int)x + 1, (int)y + 1, (int)z + 1);
            double c101 = Get((int)x + 1, (int)y, (int)z + 1);
            double c00 = c000 * (1 - xd) + c100 * xd;
            double c01 = c001 * (1 - xd) + c101 * xd;
            double c10 = c010 * (1 - xd) + c110 * xd;
            double c11 = c011 * (1 - xd) + c111 * xd;

            double c0 = c00 * (1 - yd) + c10 * yd;
            double c1 = c01 * (1 - yd) + c11 * yd;

            res = c0 * (1 - zd) + c1 * zd;
            return res;
        }

        public void GetDimensions(ref int x, ref int y, ref int z)
        {
            x = columns;
            y = rows;
            z = planes;
        }

        public void MeshToSdfBruteForce(Point3DCollection points, Int32Collection faces)
        {
            Clear(Double.MaxValue);
            // move all the points to fit in a box that that is length+2,height+2,width+2
            // i.e there is a 1,1,1 boundary all around
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue); ;
            PointUtils.MinMax(points, ref min, ref max);
            double length = max.X - min.X;
            double height = max.Y - min.Y;
            double width = max.Z - min.Z;

            double dx = min.X - 1;
            double dy = min.Y - 1;
            double dz = min.Z - 1;
            Vector3D off = new Vector3D(dx, dy, dz);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += off;
            }
            length += 2;
            height += 2;
            width += 2;
            double cellLength = length / columns;
            double cellHeight = height / rows;
            double cellWidth = width / planes;
            // go through each triangle
            for (int i = 0; i < faces.Count; i += 3)
            {
                // get the vertices of the triangle
                Point3D tri0 = points[faces[i]];
                Point3D tri1 = points[faces[i + 1]];
                Point3D tri2 = points[faces[i + 2]];
                Vector3D v0 = new Vector3D(tri0.X, tri0.Y, tri0.Z);
                Vector3D v1 = new Vector3D(tri1.X, tri1.Y, tri1.Z);
                Vector3D v2 = new Vector3D(tri2.X, tri2.Y, tri2.Z);

                // get the bounds of the triangle
                double mintrix = Math.Min(Math.Min(tri0.X, tri1.X), tri2.X);
                double maxtrix = Math.Max(Math.Max(tri0.X, tri1.X), tri2.X);

                double mintriy = Math.Min(Math.Min(tri0.Y, tri1.Y), tri2.Y);
                double maxtriy = Math.Max(Math.Max(tri0.Y, tri1.Y), tri2.Y);

                double mintriz = Math.Min(Math.Min(tri0.Z, tri1.Z), tri2.Z);
                double maxtriz = Math.Max(Math.Max(tri0.Z, tri1.Z), tri2.Z);

                int mincol = (int)(Math.Floor(mintrix) / cellLength);
                if (mincol < 0)
                {
                    mincol = 0;
                }
                int maxcol = (int)(Math.Ceiling(maxtrix) / cellLength);
                if (maxcol > columns)
                {
                    maxcol = columns;
                }

                int minrow = (int)(Math.Floor(mintriy) / cellHeight);
                if (minrow < 0)
                {
                    minrow = 0;
                }
                int maxrow = (int)(Math.Ceiling(maxtriy) / cellHeight);
                if (maxrow > rows)
                {
                    maxrow = rows;
                }

                int minplane = (int)(Math.Floor(mintriz) / cellWidth);
                if (minplane < 0)
                {
                    minplane = 0;
                }
                int maxplane = (int)(Math.Ceiling(maxtriz) / cellWidth);
                if (maxplane > planes)
                {
                    maxplane = planes;
                }

                for (int r = minrow; r < maxrow; r++)
                {
                    for (int c = mincol; c < maxcol; c++)
                    {
                        for (int p = minplane; p < maxplane; p++)
                        {
                            double dist = CalcDistanceToTriangle(c * cellLength, r * cellHeight, p * cellWidth, v0, v1, v2);
                            if (Math.Abs(distances[r, c, p]) > Math.Abs(dist))
                            {
                                distances[r, c, p] = dist;
                            }
                        }
                    }
                }
            }
        }

        public double opSmoothUnion(double a, double b, double k)
        {
            k *= 4.0;
            double h = Math.Max(k - Math.Abs(a - b), 0.0);
            return Math.Min(a, b) - h * h * 0.25 / k;
        }

        public void Perform(Sdf sdf, OpType opType, double blend)
        {
            // create the appropriate delegate
            Func<int, int, int, double, double> act;
            // default to addition
            act = (r, c, p, b) => Math.Min(distances[r, c, p], sdf.distances[r, c, p]);
            switch (opType)
            {
                case OpType.Subtraction:
                    {
                        act = (r, c, p, b) => Math.Max(-distances[r, c, p], sdf.distances[r, c, p]);
                    }
                    break;

                case OpType.Intersection:
                    {
                        act = (r, c, p, b) => Math.Max(distances[r, c, p], sdf.distances[r, c, p]);
                    }
                    break;

                case OpType.SmoothAddition:
                    {
                        act = (r, c, p, b) => opSmoothUnion(distances[r, c, p], sdf.distances[r, c, p], b);
                    }
                    break;

                case OpType.SmoothSubtraction:
                    {
                        act = (r, c, p, b) => -opSmoothUnion(distances[r, c, p], -sdf.distances[r, c, p], b);
                    }
                    break;

                case OpType.SmoothIntersection:
                    {
                        act = (r, c, p, b) => -opSmoothUnion(-distances[r, c, p], -sdf.distances[r, c, p], b);
                    }
                    break;
            }
            // apply the operation to all the elements
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    for (int p = 0; p < planes; p++)
                    {
                        distances[r, c, p] = act(r, c, p, blend);
                    }
                }
            }
        }

        /// <summary>
        ///  Blended operations
        /// </summary>
        /// <param name="sdf"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="opType"></param>
        /// <param name="strength"></param>
        public void Perform(Isdf sdf, int x, int y, int z, OpType opType, double strength)
        {
            int nx = 0;
            int ny = 0;
            int nz = 0;
            sdf.GetDimensions(ref nx, ref ny, ref nz);
            int lx = -nx / 2;
            int hx = nx / 2;
            int ly = -ny / 2;
            int hy = ny / 2;
            int lz = -nz / 2;
            int hz = nz / 2;
            switch (opType)
            {
                case OpType.Addition:
                case OpType.Subtraction:
                    {
                        for (int ix = lx; ix < hx && (x + ix) < columns; ix++)
                        {
                            for (int iy = ly; iy < hy && (y + iy) < rows; iy++)
                            {
                                for (int iz = lz; iz < hz && (z + iz) < planes; iz++)
                                {
                                    if ((y + iy >= 0 && y + iy < rows) &&
                                        (x + ix >= 0 && x + ix < columns) &&
                                        (z + iz >= 0 && z + iz < planes))
                                    {
                                        if (opType == OpType.Addition)
                                        {
                                            distances[y + iy, x + ix, z + iz] = sdf_smin(distances[y + iy, x + ix, z + iz], sdf.Get(iy + ny / 2, ix + nx / 2, iz + nz / 2));
                                        }
                                        else
                                        {
                                            distances[y + iy, x + ix, z + iz] = Math.Max(distances[y + iy, x + ix, z + iz], -sdf.Get(iy + ny / 2, ix + nx / 2, iz + nz / 2));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case OpType.Intersection:
                    {
                        double maxD = Math.Sqrt(lx * lx + ly * ly + lz * lz);
                        for (int ix = lx; ix < hx && (x + ix) < columns; ix++)
                        {
                            for (int iy = ly; iy < hy && (y + iy) < rows; iy++)
                            {
                                for (int iz = lz; iz < hz && (z + iz) < planes; iz++)
                                {
                                    if ((y + iy >= 0 && y + iy < rows) &&
                                        (x + ix >= 0 && x + ix < columns) &&
                                        (z + iz >= 0 && z + iz < planes))
                                    {
                                        double toold = Math.Sqrt(ix * ix + iy * iy + iz * iz);
                                        double r = 1 - (toold / maxD);
                                        distances[y + iy, x + ix, z + iz] = distances[y + iy, x + ix, z + iz] * r;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void Set(int r, int c, int p, double v)
        {
            distances[r, c, p] = v;
        }

        public bool SetDimension(int r, int c, int p)
        {
            bool res = false;
            try
            {
                if (c > 0 && r > 0 && p > 0)
                {
                    columns = c;
                    rows = r;
                    planes = p;
                    distances = new double[rows, columns, planes];
                }
            }
            catch (Exception e)

            {
                res = false;
                LoggerLib.Logger.LogLine($"sdf::SetDimension() throw exception {e.Message}");
            }

            return res;
        }

        private double CalcDistanceToTriangle(double x, double y, double z, Vector3D v0, Vector3D v1, Vector3D v2)
        {
            double dist = TriangleDistance.GetDistance(new Vector3D(x, y, z), v0, v1, v2);

            return dist;
        }

        private double sdf_smin(double a, double b, double k = 32)
        {
            double res = Math.Exp(-k * a) + Math.Exp(-k * b);
            return -Math.Log(Math.Max(0.0001, res)) / k;
        }
    }
}