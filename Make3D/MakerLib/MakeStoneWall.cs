using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class StoneWallMaker : MakerBase
    {

        private double wallLength;
        private double wallHeight;
        private double wallThickness;
        private int numberOfStones;
        private Random rand;
        public StoneWallMaker(double wallLength, double wallHeight, double wallThickness, int numberOfStones)
        {
            this.wallLength = wallLength;
            this.wallHeight = wallHeight;
            this.wallThickness = wallThickness;
            this.numberOfStones = numberOfStones;
            rand = new Random();
        }
        private List<Point> cellQueue;
        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            cellQueue = new List<Point>();
            byte[,] cells = new byte[(int)wallHeight, (int)wallLength];
            for (int r = 0; r < wallHeight; r++)
            {
                for (int c = 0; c < wallLength; c++)
                {
                    cells[r, c] = 0;
                }
            }
            int stones = numberOfStones;
            while (stones > 0)
            {
                int rr = (int)(rand.NextDouble() * (double)wallHeight);
                int rc = (int)(rand.NextDouble() * (double)wallLength);

                if (cells[rr, rc] == 0)
                {
                    if (CheckGap(cells, rr, rc, 5, (byte)stones))
                    {
                        cells[rr, rc] = (byte)stones;
                        cellQueue.Add(new Point(rc, rr));

                        stones--;
                    }
                }
            }

            while (cellQueue.Count > 0)
            {
                Point p = cellQueue[0];
                int direction = (int)(rand.NextDouble() * 4.0);
                int startDirection = direction;
                bool done = false;
                while (!done)
                {
                    Point offset = GetOffset(direction);
                    Point p2 = new Point(p.X + offset.X, p.Y + offset.Y);
                    if (p2.X >= 0 && p2.X < wallLength && p2.Y >= 0 && p2.Y < wallHeight && cells[p2.Y,p2.X]==0)
                    {
                        if ( CheckGap(cells,p2.Y,p2.X,5,cells[p.Y,p.X]))
                        {
                            cells[p2.Y, p2.X] = cells[p.Y, p.X];
                            cellQueue.Add(p2);
                            done = true;
                        }
                        
                            

                    }
                    direction = (direction + 1) % 4;
                    if (direction == startDirection)
                    {
                        done = true;
                    }
                }
                cellQueue.RemoveAt(0);
            }
            DumpCells(cells);

        }

        private void DumpCells(byte[,] cells)
        {
            for ( int r = 0; r < wallHeight; r ++)
            {
                for (int c = 0; c < wallLength; c++)
                {
                    System.Diagnostics.Debug.Write(cells[r, c].ToString());
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        private Point GetOffset(int direction)
        {
            switch (direction)
            {
                case 0: return new Point(0, -1); break;
                case 1: return new Point(1, 0); break;
                case 2: return new Point(0, 1); break;
                case 3: return new Point(-1, 0); break;
                default: return new Point(0, 0); break;
            }
        }
        private bool CheckGap(byte[,] cells, int r, int c, int v, byte ignore)
        {
            int tlx = c - v;
            int tly = r - v;
            int brx = c + v;
            int bry = r + v;
            bool result = true;
            for (int i = tly; i <= bry && result == true; i++)
            {
                for (int j = tlx; j <= brx && result == true; j++)
                {
                    if (j >= 0 && j < wallLength && i >= 0 && i < wallHeight)
                    {
                        if (cells[i, j] != 0 && cells[i, j] != ignore)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
