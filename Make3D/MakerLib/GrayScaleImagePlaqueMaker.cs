using Barnacle.Object3DLib;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class GrayScaleImagePlaqueMaker : MakerBase
    {
        private int imageHeight;
        private int imageWidth;
        private double plagueThickness;
        private string plaqueImagePath;
        private double plaqueLength;

        private int white = 255;

        public GrayScaleImagePlaqueMaker(double plagueThickness, string plaqueImagePath, double solidLength)
        {
            this.plagueThickness = plagueThickness;
            this.plaqueImagePath = plaqueImagePath;
            this.plaqueLength = solidLength;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
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

            for (int i = 0; i < Vertices.Count; i++)
            {
                Point3D p = Vertices[i];
                Vertices[i] = new Point3D(p.X * scaleX, p.Y /* * scaleY */ , p.Z * scaleX);
            }
        }

        private void EastEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int level = wrb.GetPixel(x, y).R;
                    if (level != white)
                    {
                        double h = GetPixelHeight(level);
                        int neighbourLevel = white;
                        if (x < imageWidth - 1)
                        {
                            neighbourLevel = wrb.GetPixel(x + 1, y).R;
                        }
                        double neighbourH = GetPixelHeight(neighbourLevel);

                        if (neighbourH < h)
                        {
                            int x1 = x + 1;
                            int v0 = AddVerticeOctTree(x1, h, y);
                            int v1 = AddVerticeOctTree(x1, h, y + 1);
                            int v2 = AddVerticeOctTree(x1, neighbourH, y);
                            int v3 = AddVerticeOctTree(x1, neighbourH, y + 1);

                            AddFace(v0, v1, v2);
                            AddFace(v1, v3, v2);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void FrontAndBack(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int level = wrb.GetPixel(x, y).R;
                    if (level != white)
                    {
                        double h = GetPixelHeight(level);

                        int x1 = x + 1;
                        int y1 = y + 1;
                        int v0 = AddVerticeOctTree(x, 0, y);
                        int v1 = AddVerticeOctTree(x1, 0, y);
                        int v2 = AddVerticeOctTree(x, 0, y1);
                        int v3 = AddVerticeOctTree(x1, 0, y1);

                        AddFace(v0, v1, v2);
                        AddFace(v1, v3, v2);

                        v0 = AddVerticeOctTree(x, h, y);
                        v1 = AddVerticeOctTree(x1, h, y);
                        v2 = AddVerticeOctTree(x, h, y1);
                        v3 = AddVerticeOctTree(x1, h, y1);

                        AddFace(v0, v2, v1);
                        AddFace(v1, v2, v3);
                    }
                    x++;
                }
                y++;
            }
        }

        private double GetPixelHeight(int level)
        {
            double height = ((double)(255 - level) / 255.0) * plagueThickness;
            return height;
        }

        private void NorthEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int level = wrb.GetPixel(x, y).R;
                    if (level != white)
                    {
                        double h = GetPixelHeight(level);
                        int neighbourLevel = white;
                        if (y > 0)
                        {
                            neighbourLevel = wrb.GetPixel(x, y - 1).R;
                        }
                        double neighbourH = GetPixelHeight(neighbourLevel);

                        if (neighbourH < h)
                        {
                            int x1 = x + 1;
                            int v0 = AddVerticeOctTree(x, h, y);
                            int v1 = AddVerticeOctTree(x1, h, y);
                            int v2 = AddVerticeOctTree(x, neighbourH, y);
                            int v3 = AddVerticeOctTree(x1, neighbourH, y);

                            AddFace(v0, v1, v2);
                            AddFace(v1, v3, v2);
                        }
                    }
                    x++;
                }
                y++;
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
            bwBitmapSource.DestinationFormat = PixelFormats.Gray8;
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

        private void SouthEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int level = wrb.GetPixel(x, y).R;
                    if (level != white)
                    {
                        double h = GetPixelHeight(level);
                        int neighbourLevel = white;
                        if (y < imageHeight - 1)
                        {
                            neighbourLevel = wrb.GetPixel(x, y + 1).R;
                        }
                        double neighbourH = GetPixelHeight(neighbourLevel);

                        if (neighbourH < h)
                        {
                            int x1 = x + 1;
                            int v0 = AddVerticeOctTree(x, h, y + 1);
                            int v1 = AddVerticeOctTree(x1, h, y + 1);
                            int v2 = AddVerticeOctTree(x, neighbourH, y + 1);
                            int v3 = AddVerticeOctTree(x1, neighbourH, y + 1);

                            AddFace(v0, v2, v1);
                            AddFace(v1, v2, v3);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void WestEdges(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    int level = wrb.GetPixel(x, y).R;
                    if (level != white)
                    {
                        double h = GetPixelHeight(level);
                        int neighbourLevel = white;
                        if (x > 0)
                        {
                            neighbourLevel = wrb.GetPixel(x - 1, y).R;
                        }
                        double neighbourH = GetPixelHeight(neighbourLevel);

                        if (neighbourH < h)
                        {
                            int y1 = y + 1;
                            int v0 = AddVerticeOctTree(x, h, y);
                            int v1 = AddVerticeOctTree(x, h, y + 1);
                            int v2 = AddVerticeOctTree(x, neighbourH, y);
                            int v3 = AddVerticeOctTree(x, neighbourH, y + 1);

                            AddFace(v0, v2, v1);
                            AddFace(v1, v2, v3);
                        }
                    }
                    x++;
                }
                y++;
            }
        }
    }
}