using Barnacle.Object3DLib;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ImagePlaqueMaker : MakerBase
    {
        private int imageHeight;
        private int imageWidth;
        private bool limitRunLengths;
        private int maxRunLength;
        private double plagueThickness;
        private string plaqueImagePath;
        private int runLengthLimit;
        private double plaqueLength;

        public ImagePlaqueMaker(double plagueThickness, string plaqueImagePath, bool limitRunLengths, int maxRunLength, double solidLength)
        {
            this.plagueThickness = plagueThickness;
            this.plaqueImagePath = plaqueImagePath;
            this.limitRunLengths = limitRunLengths;
            this.maxRunLength = maxRunLength;
            this.plaqueLength = solidLength;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (limitRunLengths)
            {
                if (maxRunLength > 1)
                {
                    runLengthLimit = maxRunLength;
                }
                else
                {
                    runLengthLimit = 1;
                }
            }
            else
            {
                runLengthLimit = int.MaxValue;
            }
            ProcessImage();

            // a quick function to calculate the scale of an object read in, into a unit sized object
            // centered on 0,0,0
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);

            double sizeX = max.X - min.X;
            double sizeY = max.Y - min.Y;
            double sizeZ = max.Z - min.Z;

            double scaleX = plaqueLength / sizeX;
            double scaleY = scaleX;
            if (scaleY * sizeY < 1)
            {
                scaleY = 1 / sizeY;
            }
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point3D p = Vertices[i];
                Vertices[i] = new Point3D(p.X * scaleX, p.Y * scaleY, p.Z * scaleX);
            }
        }

        private void EastEdges(WriteableBitmap wrb)
        {
            int x = 0;
            while (x < imageWidth)
            {
                int y = 0;
                while (y < imageHeight)
                {
                    int runLength = 0;
                    // find first black
                    while (y < imageHeight && wrb.GetPixel(x, y).R > 128)
                    {
                        y++;
                    }
                    if (y < imageHeight)
                    {
                        int start = y;
                        if (x == imageWidth - 1)
                        {
                            while (y < imageHeight && wrb.GetPixel(x, y).R < 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    y++;
                                }
                            }
                        }
                        else
                        {
                            while (y < imageHeight && wrb.GetPixel(x, y).R < 128 && wrb.GetPixel(x + 1, y).R > 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    y++;
                                }
                            }
                        }
                        if (runLength > 0)
                        {
                            double px = x + 1;
                            double py = start;
                            double ly = start + runLength;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(px, 0, ly);
                            int v2 = AddVerticeOctTree(px, plagueThickness, ly);
                            int v3 = AddVerticeOctTree(px, plagueThickness, py);
                            AddFace(v0, v2, v1);
                            AddFace(v0, v3, v2);
                        }
                    }
                    y++;
                }
                x++;
            }
        }

        private void ProcessImage()
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            BitmapImage plaqueBitmap = new BitmapImage();
            plaqueBitmap.BeginInit();
            plaqueBitmap.UriSource = new Uri(plaqueImagePath);
            plaqueBitmap.EndInit();

            // Create a FormatConvertedBitmap
            FormatConvertedBitmap bwBitmapSource = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            bwBitmapSource.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            bwBitmapSource.Source = plaqueBitmap;

            // Key of changing the bitmap format is DesitnationFormat property of BitmapSource.
            // It is a type of PixelFormat. FixelFormat has dozens of options to set
            // bitmap formatting.
            bwBitmapSource.DestinationFormat = PixelFormats.BlackWhite;
            bwBitmapSource.EndInit();

            imageWidth = (int)(Math.Ceiling(bwBitmapSource.Width)) + 2;
            imageHeight = (int)(Math.Ceiling(bwBitmapSource.Height)) + 2;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawImage(bwBitmapSource, new Rect(1, 1, bwBitmapSource.Width, bwBitmapSource.Height));

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
            CreateOctree(new Point3D(-1, -0.1, -1), new Point3D(imageWidth + 1, plagueThickness + 0.1, imageHeight + 1));
            NorthEdges(wrb);
            SouthEdges(wrb);
            WestEdges(wrb);
            EastEdges(wrb);
            FrontAndBack(wrb);
        }

        private void FrontAndBack(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int runLength = 0;
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        int start = x;

                        while (x < imageWidth && wrb.GetPixel(x, y).R < 128 && runLength < runLengthLimit)
                        {
                            runLength++;
                            if (runLength < runLengthLimit)
                            {
                                x++;
                            }
                        }

                        if (runLength > 0)
                        {
                            double px = start;
                            double py = y;
                            double lx = start + runLength;
                            double ly = y + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, 0, ly);
                            int v3 = AddVerticeOctTree(px, 0, ly);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);

                            v0 = AddVerticeOctTree(px, plagueThickness, py);
                            v1 = AddVerticeOctTree(lx, plagueThickness, py);
                            v2 = AddVerticeOctTree(lx, plagueThickness, ly);
                            v3 = AddVerticeOctTree(px, plagueThickness, ly);
                            AddFace(v0, v2, v1);
                            AddFace(v0, v3, v2);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void NorthEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int runLength = 0;
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        int start = x;
                        if (y == 0)
                        {
                            while (x < imageWidth && wrb.GetPixel(x, y).R < 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    x++;
                                }
                            }
                        }
                        else
                        {
                            while (x < imageWidth && wrb.GetPixel(x, y).R < 128 && wrb.GetPixel(x, y - 1).R > 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    x++;
                                }
                            }
                        }
                        if (runLength > 0)
                        {
                            double px = start;
                            double py = y;
                            double lx = start + runLength;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, plagueThickness, py);
                            int v3 = AddVerticeOctTree(px, plagueThickness, py);
                            AddFace(v0, v2, v1);
                            AddFace(v0, v3, v2);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void SouthEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int runLength = 0;
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        int start = x;
                        if (y == imageHeight - 1)
                        {
                            while (x < imageWidth && wrb.GetPixel(x, y).R < 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                x++;
                            }
                        }
                        else
                        {
                            while (x < imageWidth && wrb.GetPixel(x, y).R < 128 && wrb.GetPixel(x, y + 1).R > 128)
                            {
                                runLength++;
                                x++;
                            }
                        }
                        if (runLength > 0)
                        {
                            double px = start;
                            double py = y + 1;
                            double lx = start + runLength;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, plagueThickness, py);
                            int v3 = AddVerticeOctTree(px, plagueThickness, py);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void WestEdges(WriteableBitmap wrb)
        {
            int x = 0;
            while (x < imageWidth)
            {
                int y = 0;
                while (y < imageHeight)
                {
                    int runLength = 0;
                    // find first black
                    while (y < imageHeight && wrb.GetPixel(x, y).R > 128)
                    {
                        y++;
                    }
                    if (y < imageHeight)
                    {
                        int start = y;
                        if (x == 0)
                        {
                            while (y < imageHeight && wrb.GetPixel(x, y).R < 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    y++;
                                }
                            }
                        }
                        else
                        {
                            while (y < imageHeight && wrb.GetPixel(x, y).R < 128 && wrb.GetPixel(x - 1, y).R > 128 && runLength < runLengthLimit)
                            {
                                runLength++;
                                if (runLength < runLengthLimit)
                                {
                                    y++;
                                }
                            }
                        }
                        if (runLength > 0)
                        {
                            double px = x;
                            double py = start;
                            double ly = start + runLength;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(px, 0, ly);
                            int v2 = AddVerticeOctTree(px, plagueThickness, ly);
                            int v3 = AddVerticeOctTree(px, plagueThickness, py);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);
                        }
                    }
                    y++;
                }
                x++;
            }
        }
    }
}