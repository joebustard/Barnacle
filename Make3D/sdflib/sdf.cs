using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdflib
{
    public class Sdf : Isdf
    {
        private int rows;
        private int columns;
        private int planes;
        private double[,,] distances;

        public Sdf()
        {
            rows = 0;
            columns = 0;
            planes = 0;
            distances = null;
        }

        public void Clear()
        {
        }

        public double Get(int x, int y, int z)
        {
            double res = Double.NaN;
            res = distances[y, x, z];
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
            /*
            double c000 = Get((int)x, (int)y, (int)z);
            double c110 = Get((int)x + 1, (int)y, (int)z);
            double c010 = Get((int)x, (int)y + 1, (int)z);
            double c100 = Get((int)x + 1, (int)y + 1, (int)z);
            double c001 = Get((int)x, (int)y, (int)z + 1);
            double c111 = Get((int)x + 1, (int)y, (int)z + 1);
            double c011 = Get((int)x, (int)y + 1, (int)z + 1);
            double c101 = Get((int)x + 1, (int)y + 1, (int)z + 1);
            */

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

        public void Set(int x, int y, int z, double v)
        {
            distances[y, x, z] = v;
        }

        public void GetDimensions(ref int x, ref int y, ref int z)
        {
            x = columns;
            y = rows;
            z = planes;
        }

        public bool SetDimension(int x, int y, int z)
        {
            bool res = false;
            try
            {
                if (x > 0 && y > 0 && z > 0)
                {
                    columns = x;
                    rows = y;
                    planes = z;
                    distances = new double[rows, columns, planes];
                }
            }
            catch (Exception e)

            {
                res = false;
            }

            return res;
        }

        public void Perform(Isdf sdf, int x, int y, int z, int opType, double strength)
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
                case 0:
                case 1:
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

                                        if (opType == 0)
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
                case 2:
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
                                        double r = 1-(toold / maxD);
                                        //distances[y + iy, x + ix, z + iz] = distances[y + iy, x + ix, z + iz] * r; ;
                                    }
                                }
                            }


                        }
                    }
                    break;
            }
        }
        double sdf_smin(double a, double b, double k = 32)
        {
            double res = Math.Exp(-k * a) + Math.Exp(-k * b);
            return -Math.Log(Math.Max(0.0001, res)) / k;
        }
    }
}