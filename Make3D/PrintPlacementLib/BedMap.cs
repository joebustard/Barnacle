using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintPlacementLib
{
    internal class BedMap
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
    }
}