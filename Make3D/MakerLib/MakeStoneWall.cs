using Barnacle.Object3DLib;
using System;
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
        private int stoneSize;
        private double cutDepth;
        private Random rand;

        public StoneWallMaker(double wallLength, double wallHeight, double wallThickness, int stoneSize)
        {
            this.wallLength = wallLength;
            this.wallHeight = wallHeight;
            this.wallThickness = wallThickness;
            this.stoneSize = stoneSize;
            this.cutDepth = 1;
            rand = new Random();
        }

        /*
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
            byte reserved = (byte) (stones + 1);
            cells[0, 0] = reserved;
            cells[0, (int)(wallLength-1)] = reserved;
            while (stones > 0)
            {
                int rr = (int)(rand.NextDouble() * (double)wallHeight);
                int rc = (int)(rand.NextDouble() * (double)wallLength);

                if (cells[rr, rc] == 0)
                {
                    if (CheckGap(cells, rr, rc, 3, (byte)stones))
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
                int direction = (int)( rand.NextDouble() * 8);
                bool done = false;
                for (int dir = 0; dir < 8 && done == false; dir++)
                {
                    Point offset = GetOffset(direction);
                    Point p2 = new Point(p.X + offset.X, p.Y + offset.Y);
                    if (p2.X >= 0 && p2.X < wallLength && p2.Y >= 0 && p2.Y < wallHeight && cells[p2.Y, p2.X] == 0)
                    {
                        if (CheckGap(cells, p2.Y, p2.X, 2, cells[p.Y, p.X]))
                        {
                            cells[p2.Y, p2.X] = cells[p.Y, p.X];
                            cellQueue.Add(p2);
                            done = true;
                        }
                    }
                    direction = (direction + 1) % 8;
                }
                cellQueue.RemoveAt(0);
            }
            DumpCells(cells);

            for (int r = 0; r < wallHeight; r++)
            {
                for (int c = 0; c < wallLength; c++)
                {
                    double x = c;
                    double y = wallHeight - r;
                    double z = 0;

                    if (cells[r, c] != 0 && cells[r, c] != reserved)
                    {
                        z = 2;
                    }

                    if(cells[r, c] == 0 || cells[r, c] == reserved)
                    {
                        int v1 = AddVertice(x, y, 0);
                        int v2 = AddVertice(x + 1, y, 0);
                        int v3 = AddVertice(x, y - 1, 0);
                        int v4 = AddVertice(x + 1, y - 1, 0);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v2);

                    Faces.Add(v3);
                    Faces.Add(v4);
                    Faces.Add(v2);
                    }
                    else
                    {
                        int v1 = AddVertice(x, y, 2);
                        int v2 = AddVertice(x + 1, y, 2);
                        int v3 = AddVertice(x, y - 1, 2);
                        int v4 = AddVertice(x + 1, y - 1, 2);
                        int v5 = AddVertice(x + 0.5, y - 0.5, 2);

                        if ((c > 0 && cells[r, c - 1] == cells[r, c]) || (c == 0))
                    {
                            v1 = AddVertice(x, y, 2);

                            v3 = AddVertice(x, y - 1, 2);

                            v5 = AddVertice(x + 0.5, y - 0.5, 2);
                            Faces.Add(v1);
                            Faces.Add(v3);
                            Faces.Add(v5);
                        }
                        else
                        {
                            if ( r > 0)
                            {
                                if ((cells[r - 1, c] == cells[r, c]))
                        {
                                    v1 = AddVertice(x, y, 2);

                                    v3 = AddVertice(x, y - 1, 2);

                                    v5 = AddVertice(x + 0.5, y - 0.5, 2);
                                    Faces.Add(v1);
                                    Faces.Add(v3);
                                    Faces.Add(v5);
                                }
                                else
                            {
                                    v1 = AddVertice(x, y, 0);

                                    v3 = AddVertice(x, y - 1, 0);

                                    v5 = AddVertice(x + 0.5, y - 0.5, 0);
                                Faces.Add(v1);
                                Faces.Add(v3);
                                    Faces.Add(v5);
                                }
                            }
                        }

                        Faces.Add(v3);
                        Faces.Add(v4);
                                Faces.Add(v5);

                        Faces.Add(v4);
                        Faces.Add(v2);
                        Faces.Add(v5);

                        Faces.Add(v2);
                                Faces.Add(v1);
                        Faces.Add(v5);
                        if (r > 0 && c > 0)
                        {
                            if (cells[r, c - 1] == 0 || cells[r, c - 1] == reserved)
                            {
                                int v6 = AddVertice(x, y, 0);
                                int v7 = AddVertice(x, y-1, 0);
                            //    Faces.Add(v1);
                           //     Faces.Add(v3);
//Faces.Add(v6);

                        //        Faces.Add(v6);
//Faces.Add(v5);
//Faces.Add(v1);
                            }
                        }
                    }
                }
            }
        }
        */

        private enum stoneType
        {
            None,
            Used,
            Single,
            HDouble,
            HTriple,
            VDouble,
            VTriple,
            Quad
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            int l = (int)(wallLength / stoneSize) + 1;
            int h = (int)(wallHeight / stoneSize) + 1;

            stoneType[,] cells = new stoneType[h, l];
            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < l; c++)
                {
                    cells[r, c] = stoneType.None;
                }
            }

            int maxSpace = l * h;
            int spaceLeft = maxSpace;
            while (spaceLeft > 0)
            {
                int rr = (int)(rand.NextDouble() * (double)h);
                int rc = (int)(rand.NextDouble() * (double)l);
                if (cells[rr, rc] == stoneType.None)
                {
                    if (spaceLeft > 0.5 * maxSpace && rand.NextDouble() > 0.5)
                    {
                        if (CellClear(rr, rc + 1, cells, l, h) && CellClear(rr + 1, rc, cells, l, h) && CellClear(rr + 1, rc + 1, cells, l, h))
                        {
                            cells[rr, rc] = stoneType.Quad;
                            cells[rr, rc + 1] = stoneType.Used;
                            cells[rr + 1, rc] = stoneType.Used;
                            cells[rr + 1, rc + 1] = stoneType.Used;
                            spaceLeft -= 4;
                        }
                    }
                    else
                    {
                        if (spaceLeft > 0.25 * maxSpace)
                        {
                            if (CellClear(rr, rc + 1, cells, l, h) && CellClear(rr, rc + 2, cells, l, h))
                            {
                                cells[rr, rc] = stoneType.HTriple;
                                cells[rr, rc + 1] = stoneType.Used;
                                cells[rr, rc + 2] = stoneType.Used;
                                spaceLeft -= 3;
                            }
                            else
                            {
                                if (CellClear(rr + 1, rc, cells, l, h) && CellClear(rr + 2, rc, cells, l, h))
                                {
                                    cells[rr, rc] = stoneType.VTriple;
                                    cells[rr + 1, rc] = stoneType.Used;
                                    cells[rr + 2, rc] = stoneType.Used;
                                    spaceLeft -= 3;
                                }
                                else
                                {
                                    if (CellClear(rr, rc + 1, cells, l, h))
                                    {
                                        cells[rr, rc] = stoneType.HDouble;
                                        cells[rr, rc + 1] = stoneType.Used;

                                        spaceLeft -= 2;
                                    }
                                    else
                                    {
                                        if (CellClear(rr + 1, rc, cells, l, h))
                                        {
                                            cells[rr, rc] = stoneType.VDouble;
                                            cells[rr + 1, rc] = stoneType.Used;
                                            spaceLeft -= 2;
                                        }
                                        else
                                        {
                                            cells[rr, rc] = stoneType.Single;
                                            spaceLeft -= 1;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            cells[rr, rc] = stoneType.Single;
                            spaceLeft -= 1;
                        }
                    }
                }
            }
            /*
            cells[0, 0] = stoneType.Quad;
            cells[0, 2] = stoneType.Single;
            */
            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < l; c++)
                {
                    double x = c * stoneSize;
                    double y = r * stoneSize;
                    double sl = 0;
                    double sh = 0;
                    double dx;
                    double dy;

                    if (cells[r, c] == stoneType.Single)
                    {
                        sl = stoneSize;
                        sh = stoneSize;
                    }
                    else
                    if (cells[r, c] == stoneType.HDouble)
                    {
                        sl = 2 * stoneSize;
                        sh = stoneSize;
                    }
                    else
                    if (cells[r, c] == stoneType.VDouble)
                    {
                        sl = stoneSize;
                        sh = 2 * stoneSize;
                    }
                    else
                    if (cells[r, c] == stoneType.HTriple)
                    {
                        sl = 3 * stoneSize;
                        sh = stoneSize;
                    }
                    else if (cells[r, c] == stoneType.VTriple)
                    {
                        sl = stoneSize;
                        sh = 3 * stoneSize;
                    }
                    else if (cells[r, c] == stoneType.Quad)
                    {
                        sl = 2 * stoneSize;
                        sh = 2 * stoneSize;
                    }
                    else
                    {
                    }
                    if (sl != 0)
                    {
                        dx = -sl / 2;
                        dy = sh / 2;
                        double dz = rand.NextDouble();
                        SquirkleMaker mk = new SquirkleMaker(1, 1, 1, 1, sl, sh, wallThickness);
                        Point3DCollection onePnts = new Point3DCollection();
                        Int32Collection oneTris = new Int32Collection();
                        mk.Generate(onePnts, oneTris);

                        Point3DCollection backPnts = new Point3DCollection();
                        Int32Collection backTris = new Int32Collection();
                        Vector3DCollection vecs = new Vector3DCollection();
                        PrimitiveGenerator.GenerateCube(ref backPnts, ref backTris, ref vecs);

                        int vc = Vertices.Count;
                        foreach (Point3D p in onePnts)
                        {
                            Vertices.Add(new Point3D(p.X + x, p.Y + y + dy, p.Z + dz));
                        }
                        foreach (int index in oneTris)
                        {
                            Faces.Add(index + vc);
                        }

                        vc = Vertices.Count;
                        double backThickness = wallThickness - cutDepth;
                        foreach (Point3D p in backPnts)
                        {
                            Vertices.Add(new Point3D((p.X * sl) + x + (sl / 2), (sh * p.Y) + y + dy, (p.Z * backThickness) + backThickness / 2));
                        }
                        foreach (int index in backTris)
                        {
                            Faces.Add(index + vc);
                        }
                    }
                }
            }
        }

        private bool CellClear(int r, int c, stoneType[,] cells, int l, int h)
        {
            bool res = false;
            if (r >= 0 && r < h)
            {
                if (c >= 0 && c < l)
                {
                    if (cells[r, c] == stoneType.None)
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        private void DumpCells(byte[,] cells)
        {
            for (int r = 0; r < wallHeight; r++)
            {
                for (int c = 0; c < wallLength; c++)
                {
                    if (cells[r, c] == 0)
                    {
                        System.Diagnostics.Debug.Write('.');
                    }
                    else
                    {
                        System.Diagnostics.Debug.Write((char)(cells[r, c] + '@'));
                    }
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        private Point GetOffset(int direction)
        {
            switch (direction)
            {
#pragma warning disable CS0162 // Unreachable code detected
                case 0: return new Point(0, -1); break;

                case 1: return new Point(1, 0); break;
                case 2: return new Point(0, 1); break;
                case 3: return new Point(-1, 0); break;
                case 4: return new Point(-1, -1); break;
                case 5: return new Point(1, -1); break;
                case 6: return new Point(-1, 1); break;
                case 7: return new Point(1, 1); break;
                default: return new Point(0, 0); break;
#pragma warning restore CS0162 // Unreachable code detected
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