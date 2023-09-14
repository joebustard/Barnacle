using asdflibrary;
using Lerp.LerpLib;
using OctTreeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ImageMakerUtils : MakerBase
    {
        protected const float diagonalDist = 0.70710678118F;
        protected const float diagonalDistNeigh = 2 * diagonalDist;
        protected const float horizontalDist = 0.5F;
        protected const float horizontalDistNeigh = 2 * horizontalDist;
        protected const float vdiagDist = 0.886F;
        protected const float vdiagDistNeigh = 2 * vdiagDist;
        protected int imageHeight = 75;
        protected int imageWidth = 75;

        protected void AddQueue(List<QueueItem> queue, QueueItem qe)
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

        protected void CheckNeighbour(Mock3DBitmap wrb, List<QueueItem> queue, int dx, int dy, int dz, QueueItem item, float dist, bool[,,] visited, bool inside = false)
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

        protected void CheckSeed(Mock3DBitmap wrb, List<QueueItem> queue, int dx, int dy, int dz, int px, int py, int pz, float dist, bool inside = false)
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

        protected bool Black(int px, int py, int pz, Mock3DBitmap wrb)
        {
            if (wrb.GetPixel(px, py, pz).R < 128)
            {
                return true;
            }
            return false;
        }

        protected void GenerateField(WriteableBitmap wrb)
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
                    }
                }
            }

            QuickMap(mock3D, distVector, visited, imageWidth, imageHeight, 2);

            for (int px = 0; px < imageWidth; px++)
            {
                for (int py = 0; py < numberOfLayers; py++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        if (visited[px, py, pz] == false)
                        {
                            if (White(px, py, pz, mock3D))
                            {
                                GenerateSeeds(mock3D, queue, px, py, pz, false);
                            }
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
                    }
                }
            }
            QuickMap(mock3D, distVector, visited, imageWidth, imageHeight, 2);
            for (int px = 0; px < imageWidth; px++)
            {
                for (int py = 0; py < numberOfLayers; py++)
                {
                    for (int pz = 0; pz < imageHeight; pz++)
                    {
                        if (visited[px, py, pz] == false)
                        {
                            if (Black(px, py, pz, mock3D))
                            {
                                GenerateSeeds(mock3D, queue, px, py, pz, true);
                            }
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
            DumpLayerImages(wrb, distVector);
            DateTime start = DateTime.Now;
            List<Triangle> triangles = new List<Triangle>();
            triangles.Clear();
            CubeMarcher cm = new CubeMarcher();
            GridCell gc;

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
                        LerpBox lb = new LerpBox();
                        lb.SetVoxel(0, 0, 0, px, py, pz, distVector[px, py, pz].dv);
                        lb.SetVoxel(1, 0, 0, hx, py, pz, distVector[hx, py, pz].dv);
                        lb.SetVoxel(1, 0, 1, hx, py, hz, distVector[hx, py, hz].dv);
                        lb.SetVoxel(0, 0, 1, px, py, hz, distVector[px, py, hz].dv);

                        lb.SetVoxel(0, 1, 0, px, hy, pz, distVector[px, hy, pz].dv);
                        lb.SetVoxel(1, 1, 0, hx, hy, pz, distVector[hx, hy, pz].dv);
                        lb.SetVoxel(1, 1, 1, hx, hy, hz, distVector[hx, hy, hz].dv);
                        lb.SetVoxel(0, 1, 1, px, hy, hz, distVector[px, hy, hz].dv);

                        if (lb.CornerMask != 0 && lb.CornerMask != 255)
                        {
                            List<LerpBox> divs = lb.Subdivide();
                            foreach (LerpBox subBox in divs)
                            {
                                subBox.SetMask();
                                List<LerpBox> divs2 = subBox.Subdivide();
                                foreach (LerpBox subBox2 in divs2)
                                {
                                    subBox2.SetMask();
                                if (subBox2.CornerMask != 0 && subBox2.CornerMask != 255)
                                    {
                                        gc = subBox2.ToGridCell();

                                        cm.Polygonise(gc, 0, triangles);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            DateTime end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"Lerp time {end - start}");
            CreateOctree(new Point3D(-1, -1, -1), new Point3D(imageWidth, 7, imageHeight));
            foreach (Triangle t in triangles)
            {
                int p0 = AddVerticeOctTree(t.p[0].x, t.p[0].y, t.p[0].z);
                int p1 = AddVerticeOctTree(t.p[1].x, t.p[1].y, t.p[1].z);
                int p2 = AddVerticeOctTree(t.p[2].x, t.p[2].y, t.p[2].z);

                Faces.Add(p0);
                Faces.Add(p1);
                Faces.Add(p2);
            }
        }

        private int minDim = 100;
        private int cellSize = 10;

        private void QuickMap(Mock3DBitmap mock3D, DistVector[,,] distVector, bool[,,] visited, int imageWidth, int imageHeight, int layer)
        {
            if (imageWidth > minDim && imageHeight > minDim)
            {
                byte empty = 0;
                byte white = 1;
                byte black = 2;
                int mapWidth = (imageWidth / cellSize) + 1;
                int mapHeight = (imageHeight / cellSize) + 1;
                byte[,] map = new byte[mapWidth, mapHeight];
                for (int i = 0; i < mapWidth; i++)
                {
                    for (int j = 0; j < mapHeight; j++)
                    {
                        map[i, j] = empty;
                    }
                }

                for (int i = 0; i < mapWidth; i++)
                {
                    for (int j = 0; j < mapHeight; j++)
                    {
                        int tlx = (i * cellSize) - 1;
                        int tly = (j * cellSize) - 1;
                        int brx = tlx + cellSize + 2;
                        int bry = tly + cellSize + 2;
                        bool done = false;
                        for (int x = tlx; x <= brx && !done; x++)
                        {
                            for (int y = tly; y <= bry && !done; y++)
                            {
                                for ( int z = 0; z< numberOfLayers ; z++)
                                //int z = layer;
                                {
                                    if (White(x, z, y, mock3D))
                                    {
                                        map[i, j] = (byte)(map[i, j] | white);
                                    }
                                    else
                                    {
                                        if (Black(x, z, y, mock3D))
                                        {
                                            map[i, j] = (byte)(map[i, j] | black);
                                        }
                                    }
                                    if (map[i, j] == 3)
                                    {
                                        done = true;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < mapWidth; i++)
                {
                    for (int j = 0; j < mapHeight; j++)
                    {
                        int tlx = (i * cellSize);
                        int tly = (j * cellSize);
                        int brx = tlx + cellSize;
                        int bry = tly + cellSize;

                        for (int x = tlx; x <= brx; x++)
                        {
                            for (int y = tly; y <= bry; y++)
                            {
                                if (x < imageWidth && y < imageHeight)
                                {
                                    for (int z = 0; z < numberOfLayers ; z++)
                                  
                                    {
                                        if (map[i, j] == white)
                                        {
                                            visited[x, z, y] = true;
                                            
                                                        distVector[x, z, y] = new DistVector();
                                                        distVector[x, z, y].dx = 1000;
                                                        distVector[x, z, y].dy = 1000;
                                                        distVector[x, z, y].dz = 1000;
                                                        distVector[x, z, y].inside = false;
                                                        distVector[x, z, y].dv = 100000;
                                                  
                                        }
                                        else if (map[i, j] == black)
                                        {
                                            visited[x, z, y] = true;
                                            distVector[x, z, y] = new DistVector();
                                            distVector[x, z, y].dx = -1000;
                                            distVector[x, z, y].dy = -1000;
                                            distVector[x, z, y].dz = -1000;
                                            distVector[x, z, y].inside = true;
                                            distVector[x, z, y].dv = -100000;
                                        }
                                    }
                                }
                            }
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

        protected const int numberOfLayers = 3;

        protected bool valid(int px, int py, int pz)
        {
            if (px >= 0 && px < imageWidth && py >= 0 && py < numberOfLayers && pz >= 0 && pz < imageHeight)
            {
                return true;
            }
            return false;
        }

        protected bool White(int px, int py, int pz, Mock3DBitmap wrb)
        {
            if (wrb.GetPixel(px, py, pz).R > 128)
            {
                return true;
            }
            return false;
        }

        protected struct DistVector
        {
            public float dv;
            public float dx;
            public float dy;
            public float dz;
            public bool inside;
        }

        protected struct QueueItem
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