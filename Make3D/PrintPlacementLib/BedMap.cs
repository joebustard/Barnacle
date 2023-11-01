using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintPlacementLib
{
    public class BedMap
    {
        public bool[,] map;
        private int numRows;
        private int numCols;

        public BedMap(int rows, int cols)
        {
            numRows = rows;
            numCols = cols;
            map = new bool[rows, cols];
        }

        internal void Set(int r, int c, bool v)
        {
            if (r >= 0 && r < numRows && c >= 0 && c < numCols)
            {
                map[r, c] = v;
            }
        }

        internal bool Get(int r, int c)
        {
            bool v = false;
            if (r >= 0 && r < numRows && c >= 0 && c < numCols)
            {
                v = map[r, c];
            }
            return v;
        }
    }
}