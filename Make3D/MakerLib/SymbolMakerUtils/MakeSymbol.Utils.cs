using asdflibrary;
using MakerLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MakerLib
{
    public class MakeSymbolUtils : MakerBase
    {
        private const float diagonalDist = 0.70710678118F;
        private const float diagonalDistNeigh = 2 * diagonalDist;
        private const float horizontalDist = 0.5F;
        private const float horizontalDistNeigh = 2 * horizontalDist;
        private const float vdiagDist = 0.886F;
        private const float vdiagDistNeigh = 2 * vdiagDist;
        private int imageHeight = 75;
        private int imageWidth = 75;

        private void AddQueue(List<QueueItem> queue, QueueItem qe)
        {
            if (queue != null)
            {
                // nothing there yet so just add as first one
                if (queue.Count == 0)
                {
                    queue.Add(qe);
                }
                else
                {
                    // should it go at the front
                    if (qe.dist <= queue[0].dist)
                    {
                        queue.Insert(0, qe);
                    }
                    else
                    {
                        // should it just go at end
                        if (qe.dist >= queue[queue.Count - 1].dist)
                        {
                            queue.Add(qe);
                        }
                        else
                        {
                            // binary split to find best location
                            bool found = false;
                            int sIndex = 0;
                            int eIndex = queue.Count - 1;

                            while (!found)
                            {
                                int eMid = sIndex + (eIndex - sIndex) / 2;
                                if (eIndex - sIndex <= 1)
                                {
                                    queue.Insert(sIndex + 1, qe);

                                    found = true;
                                }
                                else
                                {
                                    if (queue[eMid].dist >= qe.dist)
                                    {
                                        eIndex = eMid;
                                    }
                                    else
                                    {
                                        sIndex = eMid;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool Black(int px, int py, int pz, Mock3DBitmap wrb)
        {
            if (wrb.GetPixel(px, py, pz).R < 128)
            {
                return true;
            }
            return false;
        }

        private void CheckNeighbour(Mock3DBitmap wrb, List<QueueItem> queue, int dx, int dy, int dz, QueueItem item, float dist, bool[,,] visited, bool inside = false)
        {
            float nearpdx = item.dx;
            float nearpdy = item.dy;
            float nearpdz = item.dz;
            bool add = false;
            if (inside)
            {
                if (valid(item.px + dx, item.py + dy, item.pz + dz) && Black(item.px + dx, item.py + dy, item.pz + dz, wrb) && !visited[item.px + dx, item.py + dy, item.pz + dz])
                {
                    add = true;
                }
            }
            else
            {
                if (valid(item.px + dx, item.py + dy, item.pz + dz) && White(item.px + dx, item.py + dy, item.pz + dz, wrb) && !visited[item.px + dx, item.py + dy, item.pz + dz])
                {
                    add = true;
                }
            }
            if (add)
            {
                QueueItem qe = new QueueItem();
                qe.px = item.px + dx;
                qe.py = item.py + dy;
                qe.pz = item.pz + dz;
                qe.dx = dx + nearpdx;
                qe.dy = (1 * dy) + nearpdy;
                qe.dz = dz + nearpdz;
                qe.dist = (float)Math.Sqrt((qe.dx * qe.dx) + (qe.dy * qe.dy) + (qe.dz * qe.dz));
                AddQueue(queue, qe);
            }
        }

        private void CheckSeed(Mock3DBitmap wrb, List<QueueItem> queue, int dx, int dy, int dz, int px, int py, int pz, float dist, bool inside = false)
        {
            bool add = false;
            if (inside)
            {
                if (valid(px + dx, py + dy, py + dz) && White(px + dx, py + dy, pz + dz, wrb))
                {
                    add = true;
                }
            }
            else
            {
                if (valid(px + dx, py + dy, py + dz) && Black(px + dx, py + dy, pz + dz, wrb))
                {
                    add = true;
                }
            }
            if (add)
            {
                QueueItem qe = new QueueItem();
                qe.px = px + dx;
                qe.py = py + dy;
                qe.pz = pz + dz;
                qe.dx = dx;
                qe.dy = dy;
                qe.dz = dz;
                qe.dist = dist;
                AddQueue(queue, qe);
            }
        }

        private BitmapImage bitmap;
        private double frontXSize;
        private double frontYSize;
        private double frontZSize;

        public void GenerateSymbol(string v, string fontName)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(v, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface(fontName), 150, System.Windows.Media.Brushes.Black);
            ft.SetFontStretch(FontStretches.Normal);
            Size sz = new Size(ft.Width, ft.Height);

            frontXSize = 0.25;
            frontZSize = 0.25;
            frontYSize = 0.5;
            imageWidth = (int)(Math.Ceiling(sz.Width)) + 20;
            imageHeight = (int)(Math.Ceiling(sz.Height)) + 20;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawText(ft, new System.Windows.Point(10, 10));

            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Default);
            bmp.Render(drawingVisual);
            bitmap = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            WriteableBitmap wrb = new WriteableBitmap(bmp);
            GenerateField(wrb);
        }

        private void GenerateField(WriteableBitmap wrb)
        {
            bool[,,] visited;
            visited = new bool[imageWidth, numberOfLayers, imageHeight];
            DistVector[,,] distVector = new DistVector[imageWidth, numberOfLayers * 2, imageHeight];
            Mock3DBitmap mock3D = new Mock3DBitmap(imageWidth, numberOfLayers, imageHeight);
            mock3D.SetLayerImage(wrb, 2);

            List<QueueItem> queue = new List<QueueItem>();

            // pass one, outside
            for (int px = 0; px < imageWidth; px++)
            {
                for (int py = 0; py < numberOfLayers; py++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        visited[px, py, pz] = false;

                        if (White(px, py, pz, mock3D))
                        {
                            GenerateSeeds(mock3D, queue, px, py, pz, false);
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                QueueItem qe = queue[0];
                queue.RemoveAt(0);
                if (valid(qe.px, qe.py, qe.pz) && visited[qe.px, qe.py, qe.pz] == false)
                {
                    visited[qe.px, qe.py, qe.pz] = true;
                    distVector[qe.px, qe.py, qe.pz].dx = qe.dx;
                    distVector[qe.px, qe.py, qe.pz].dy = qe.dy;
                    distVector[qe.px, qe.py, qe.pz].dz = qe.dz;
                    distVector[qe.px, qe.py, qe.pz].dv = qe.dist;
                    distVector[qe.px, qe.py, qe.pz].inside = false;
                    CheckAllNeighbours(mock3D, queue, qe, visited, false);
                }
            }

            //pass 2, inside
            for (int px = 0; px < imageWidth; px++)
            {
                for (int py = 0; py < numberOfLayers; py++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        visited[px, py, pz] = false;
                        if (Black(px, py, pz, mock3D))
                        {
                            GenerateSeeds(mock3D, queue, px, py, pz, true);
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                QueueItem qe = queue[0];
                queue.RemoveAt(0);
                if (valid(qe.px, qe.py, qe.pz) && visited[qe.px, qe.py, qe.pz] == false)
                {
                    visited[qe.px, qe.py, qe.pz] = true;
                    distVector[qe.px, qe.py, qe.pz].dx = qe.dx;
                    distVector[qe.px, qe.py, qe.pz].dy = qe.dy;
                    distVector[qe.px, qe.py, qe.pz].dz = qe.dz;
                    distVector[qe.px, qe.py, qe.pz].inside = true;
                    distVector[qe.px, qe.py, qe.pz].dv = qe.dist;
                    CheckAllNeighbours(mock3D, queue, qe, visited, true);
                }
            }

            for (int py = 0; py < numberOfLayers; py++)
            {
                for (int px = 0; px < imageWidth; px++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        if (distVector[px, py, pz].inside)
                        {
                            distVector[px, py, pz].dv = -distVector[px, py, pz].dv;
                        }
                        distVector[px, 2 * numberOfLayers - py - 1, pz] = distVector[px, py, pz];
                    }
                }
            }
            //  DumpLayerImages(wrb, distVector);
            CubeMarcher cm = new CubeMarcher();
            GridCell gc = new GridCell();
            List<Triangle> triangles = new List<Triangle>();
            for (int py = 0; py < 2 * numberOfLayers - 1; py++)
            {
                int ly = py;
                int hy = (py + 1);
                for (int px = 0; px < imageWidth - 1; px++)
                {
                    int hx = px + 1;

                    for (int pz = 0; pz < imageHeight - 1; pz++)
                    {
                        int hz = pz + 1;
                        gc.p[0] = new XYZ(px, ly, pz);
                        gc.p[1] = new XYZ(hx, ly, pz);
                        gc.p[2] = new XYZ(hx, ly, hz);
                        gc.p[3] = new XYZ(px, ly, hz);
                        gc.p[4] = new XYZ(px, hy, pz);
                        gc.p[5] = new XYZ(hx, hy, pz);
                        gc.p[6] = new XYZ(hx, hy, hz);
                        gc.p[7] = new XYZ(px, hy, hz);

                        gc.val[0] = distVector[px, py, pz].dv;
                        gc.val[1] = distVector[hx, py, pz].dv;
                        gc.val[2] = distVector[hx, py, hz].dv;
                        gc.val[3] = distVector[px, py, hz].dv;
                        gc.val[4] = distVector[px, py + 1, pz].dv;
                        gc.val[5] = distVector[hx, py + 1, pz].dv;
                        gc.val[6] = distVector[hx, py + 1, hz].dv;
                        gc.val[7] = distVector[px, py + 1, hz].dv;
                        triangles.Clear();

                        cm.Polygonise(gc, 0, triangles);

                        foreach (Triangle t in triangles)
                        {
                            int p0 = AddVertice(t.p[0].x * frontXSize, (t.p[0].y * frontYSize), t.p[0].z * frontZSize);
                            int p1 = AddVertice(t.p[1].x * frontXSize, (t.p[1].y * frontYSize), t.p[1].z * frontZSize);
                            int p2 = AddVertice(t.p[2].x * frontXSize, (t.p[2].y * frontYSize), t.p[2].z * frontZSize);

                            Faces.Add(p0);
                            Faces.Add(p1);
                            Faces.Add(p2);
                        }
                    }
                }
            }
        }

        private void DumpLayerImages(WriteableBitmap wrb, DistVector[,,] distVector)
        {
            float minDist = float.MaxValue;
            float maxDist = float.MinValue;

            for (int px = 0; px < imageWidth; px++)
            {
                for (int py = 0; py < numberOfLayers; py++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        if (distVector[px, py, pz].inside)
                        {
                            distVector[px, py, pz].dv = -distVector[px, py, pz].dv;
                        }
                        if (distVector[px, py, pz].dv < minDist)
                        {
                            minDist = distVector[px, py, pz].dv;
                        }
                        if (distVector[px, py, pz].dv > maxDist)
                        {
                            maxDist = distVector[px, py, pz].dv;
                        }
                    }
                }
            }
            byte r;
            float range = maxDist - minDist;
            for (int y = 0; y < numberOfLayers; y++)
            {
                for (int px = 0; px < imageWidth; px++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        float v;
                        v = (distVector[px, y, pz].dv - minDist);
                        v = v / range;
                        r = (byte)(255.0 * v);
                        r = (byte)(255 - r);
                        wrb.SetPixel(px, pz, r, r, r);
                    }
                }
                using (FileStream stream5 = new FileStream(@"C:\tmp\sdf" + y.ToString() + ".png", FileMode.OpenOrCreate))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(wrb));
                    encoder5.Save(stream5);
                }
            }
        }

        private void CheckAllNeighbours(Mock3DBitmap mock3D, List<QueueItem> queue, QueueItem qe, bool[,,] visited, bool inside)
        {
            CheckNeighbour(mock3D, queue, -1, -1, -1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, -1, -1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, -1, -1, 1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, -1, 1, qe, vdiagDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, -1, 0, -1, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 0, -1, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, -1, 0, 1, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 0, 1, qe, diagonalDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, -1, 1, -1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 1, -1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, -1, 1, 1, qe, vdiagDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 1, 1, qe, vdiagDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, -1, -1, 0, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, -1, 0, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, -1, -1, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, -1, 1, qe, diagonalDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, -1, 0, 0, qe, horizontalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 0, 0, qe, horizontalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, 0, -1, qe, horizontalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, 0, 1, qe, horizontalDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, -1, 1, 0, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 1, 1, 0, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, 1, -1, qe, diagonalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, 1, 1, qe, diagonalDistNeigh, visited, inside);

            CheckNeighbour(mock3D, queue, 0, -1, 0, qe, horizontalDistNeigh, visited, inside);
            CheckNeighbour(mock3D, queue, 0, 1, 0, qe, horizontalDistNeigh, visited, inside);
        }

        private void GenerateSeeds(Mock3DBitmap mock3D, List<QueueItem> queue, int px, int py, int pz, bool inside)
        {
            // diagonals
            CheckSeed(mock3D, queue, -1, -1, -1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, 1, -1, -1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, -1, -1, 1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, 1, -1, 1, px, py, pz, vdiagDist, inside);

            //
            CheckSeed(mock3D, queue, -1, 0, -1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 1, 0, -1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, -1, 0, 1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 1, 0, 1, px, py, pz, diagonalDist, inside);
            //
            CheckSeed(mock3D, queue, -1, 1, -1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, 1, 1, -1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, -1, 1, 1, px, py, pz, vdiagDist, inside);
            CheckSeed(mock3D, queue, 1, 1, 1, px, py, pz, vdiagDist, inside);

            // orthogonals
            CheckSeed(mock3D, queue, -1, -1, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 1, -1, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, -1, -1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, -1, 1, px, py, pz, diagonalDist, inside);

            CheckSeed(mock3D, queue, -1, 0, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 1, 0, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, 0, -1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, 0, 1, px, py, pz, diagonalDist, inside);

            CheckSeed(mock3D, queue, -1, 1, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 1, 1, 0, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, 1, -1, px, py, pz, diagonalDist, inside);
            CheckSeed(mock3D, queue, 0, 1, 1, px, py, pz, diagonalDist, inside);

            CheckSeed(mock3D, queue, 0, -1, 0, px, py, pz, horizontalDist, inside);
            CheckSeed(mock3D, queue, 0, 1, 0, px, py, pz, horizontalDist, inside);
        }

        private const int numberOfLayers = 3;

        private bool valid(int px, int py, int pz)
        {
            if (px >= 0 && px < imageWidth && py >= 0 && py < numberOfLayers && pz >= 0 && pz < imageHeight)
            {
                return true;
            }
            return false;
        }

        private bool White(int px, int py, int pz, Mock3DBitmap wrb)
        {
            if (wrb.GetPixel(px, py, pz).R > 128)
            {
                return true;
            }
            return false;
        }

        private struct DistVector
        {
            public float dv;
            public float dx;
            public float dy;
            public float dz;
            public bool inside;
        }

        private struct QueueItem
        {
            public float dist;
            public float dx;
            public float dy;
            public float dz;
            public int px;
            public int py;
            public int pz;
        }
    }
}