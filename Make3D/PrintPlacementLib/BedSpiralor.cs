using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PrintPlacementLib
{
    public class BedSpiralor
    {
        private bool[,] map;
        private int numRows;
        private int numCols;

        private enum Direction
        {
            North,
            East,
            South,
            West
        }

        private Direction RightOf(Direction d)
        {
            int i = (int)d;
            i = (i + 1) % 4;
            Direction res = (Direction)i;
            return res;
        }

        private Direction currentDir;
        private MapPoint pos;
        public int numberVisited;
        public int MaxVisited;

        public class MapPoint
        {
            public int Row;
            public int Column;

            public MapPoint(int r, int c)

            {
                this.Row = r;
                this.Column = c;
            }
        }

        public BedSpiralor(int rows, int cols)
        {
            numRows = rows;
            numCols = cols;
            map = new bool[rows, cols];
            currentDir = Direction.North;
            pos = new MapPoint(numRows / 2, numCols / 2);
            MaxVisited = numRows * numCols;
        }

        public MapPoint GetNextPos()
        {
            MapPoint res = new MapPoint(-1, -1);
            if (pos.Row >= 0 && pos.Row < numRows && pos.Column >= 0 && pos.Column < numCols && numberVisited < MaxVisited)
            {
                numberVisited++;
                res.Row = pos.Row;
                res.Column = pos.Column;
                map[(int)pos.Row, (int)pos.Column] = true;
                MovePos();
            }
            else
            {
                numberVisited++;
            }
            return res;
        }

        private void MovePos()
        {
            MapPoint rightPoint = GetPointToRight();
            if (map[(int)rightPoint.Row, (int)rightPoint.Column] == false)
            {
                currentDir = RightOf(currentDir);
            }
            MoveForward();
        }

        private MapPoint GetPointToRight()
        {
            MapPoint res = new MapPoint(-1, -1);
            switch (currentDir)
            {
                case Direction.North:
                    {
                        res = new MapPoint(pos.Row, pos.Column + 1);
                    }
                    break;

                case Direction.South:
                    {
                        res = new MapPoint(pos.Row, pos.Column - 1);
                    }
                    break;

                case Direction.East:
                    {
                        res = new MapPoint(pos.Row + 1, pos.Column);
                    }
                    break;

                case Direction.West:
                    {
                        res = new MapPoint(pos.Row - 1, pos.Column);
                    }
                    break;
            }
            return res;
        }

        private void MoveForward()
        {
            switch (currentDir)
            {
                case Direction.North:
                    {
                        pos.Row = pos.Row - 1;
                    }
                    break;

                case Direction.South:
                    {
                        pos.Row = pos.Row + 1;
                    }
                    break;

                case Direction.East:
                    {
                        pos.Column = pos.Column + 1;
                    }
                    break;

                case Direction.West:
                    {
                        pos.Column = pos.Column - 1;
                    }
                    break;
            }
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("======");
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (map[i, j])
                    {
                        System.Diagnostics.Debug.Write("+");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Write(".");
                    }
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }
    }
}