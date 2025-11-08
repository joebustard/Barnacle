using Barnacle.Object3DLib;
using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class StoneWallMaker : MakerBase
    {
        private double cutDepth;
        private Random rand;
        private int stoneSize;
        private double wallHeight;
        private double wallLength;
        private double wallThickness;

        public StoneWallMaker(double wallLength, double wallHeight, double wallThickness, int stoneSize)
        {
            this.wallLength = wallLength;
            this.wallHeight = wallHeight;
            this.wallThickness = wallThickness;
            this.stoneSize = stoneSize;
            this.cutDepth = 1;
            rand = new Random();
        }

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
                        // should be marked as used
                        stoneType st = cells[r, c];
                        if (st != stoneType.Used)
                        {
                            Logger.Log($"Unexpected stone type {st.ToString()}");
                        }
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

                        double mx = double.MaxValue;
                        double my = double.MaxValue;
                        double mz = double.MaxValue;
                        foreach (Point3D p in onePnts)
                        {
                            mx = Math.Min(mx, p.X);
                            my = Math.Min(my, p.Y);
                            mz = Math.Min(mz, p.Z);
                        }
                        int vc = Vertices.Count;
                        foreach (Point3D p in onePnts)
                        {
                            Vertices.Add(new Point3D(p.X + x - mx, p.Y + y - my, p.Z + dz - mz));
                        }
                        foreach (int index in oneTris)
                        {
                            Faces.Add(index + vc);
                        }

                        Point3DCollection backPnts = new Point3DCollection();
                        Int32Collection backTris = new Int32Collection();
                        Vector3DCollection vecs = new Vector3DCollection();
                        PrimitiveGenerator.GenerateCube(ref backPnts, ref backTris, ref vecs);

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
    }
}