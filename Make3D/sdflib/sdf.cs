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

        public void Union(Isdf sdf, int x, int y, int z)
        {
            int nx = 0;
            int ny = 0;
            int nz = 0;
            sdf.GetDimensions(ref nx, ref ny, ref nz);
            for (int ix = 0; ix < nx && (x + ix) < columns; ix++)
            {
                for (int iy = 0; iy < ny && (y + iy) < rows; iy++)
                {
                    for (int iz = 0; iz < nz && (z + iz) < planes; iz++)
                    {
                        distances[y + iy, x + ix, z + iz] = Math.Min(distances[y + iy, x + ix, z + iz], sdf.Get(iy, ix, iz));
                    }
                }
            }
        }

        public void Difference(Isdf sdf, int x, int y, int z)
        {
            int nx = 0;
            int ny = 0;
            int nz = 0;
            sdf.GetDimensions(ref nx, ref ny, ref nz);
            for (int ix = 0; ix < nx && (x + ix) < columns; ix++)
            {
                for (int iy = 0; iy < ny && (y + iy) < rows; iy++)
                {
                    for (int iz = 0; iz < nz && (z + iz) < planes; iz++)
                    {
                        distances[y + iy, x + ix, z + iz] = Math.Max(-distances[y + iy, x + ix, z + iz], sdf.Get(iy, ix, iz));
                    }
                }
            }
        }
    }
}