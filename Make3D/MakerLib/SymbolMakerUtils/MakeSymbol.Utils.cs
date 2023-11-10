using Barnacle.Object3DLib;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class MakeSymbolUtils : ImageMakerUtils
    {
        private const double thickness = 5;

        public void GenerateSymbol(string v, string fontName)
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(v, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface(fontName), 200, System.Windows.Media.Brushes.Black);
            ft.SetFontStretch(FontStretches.Normal);
            Size sz = new Size(ft.Width, ft.Height);

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
            CreateOctree(new Point3D(-1, -0.1, -1), new Point3D(imageWidth + 1, 2 * thickness, imageHeight + 1));
            NorthEdges(wrb);
            SouthEdges(wrb);
            WestEdges(wrb);
            EastEdges(wrb);
            FrontAndBack(wrb);
            // a quick function to calculate the scale of an object read in, into a unit sized object
            // centered on 0,0,0
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);

            double sizeX = max.X - min.X;
            double sizeY = max.Y - min.Y;
            double sizeZ = max.Z - min.Z;

            double scaleX = 25.0 / sizeX;
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

            // GenerateField(wrb);
        }

        private void FrontAndBack(WriteableBitmap wrb)
        {
            int y = 0;
            while (y < imageHeight)
            {
                int x = 0;
                while (x < imageWidth)
                {
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        double px = x;
                        double py = y;
                        double lx = x + 1;
                        double ly = y + 1;
                        int v0 = AddVerticeOctTree(px, 0, py);
                        int v1 = AddVerticeOctTree(lx, 0, py);
                        int v2 = AddVerticeOctTree(lx, 0, ly);
                        int v3 = AddVerticeOctTree(px, 0, ly);
                        AddFace(v0, v1, v2);
                        AddFace(v0, v2, v3);

                        v0 = AddVerticeOctTree(px, thickness, py);
                        v1 = AddVerticeOctTree(lx, thickness, py);
                        v2 = AddVerticeOctTree(lx, thickness, ly);
                        v3 = AddVerticeOctTree(px, thickness, ly);
                        AddFace(v0, v2, v1);
                        AddFace(v0, v3, v2);
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
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        if (y == 0 || wrb.GetPixel(x, y - 1).R > 128)
                        {
                            double px = x;
                            double py = y;
                            double lx = x + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, thickness, py);
                            int v3 = AddVerticeOctTree(px, thickness, py);
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
                    // find first black
                    while (x < imageWidth && wrb.GetPixel(x, y).R > 128)
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        if (y == imageHeight - 1 || wrb.GetPixel(x, y + 1).R > 128)
                        {
                            double px = x;
                            double py = y + 1;
                            double lx = x + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, thickness, py);
                            int v3 = AddVerticeOctTree(px, thickness, py);
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
                    bool add = false;
                    // find first black
                    while (y < imageHeight && wrb.GetPixel(x, y).R > 128)
                    {
                        y++;
                    }
                    if (y < imageHeight)
                    {
                        if (x == 0 || wrb.GetPixel(x - 1, y).R > 128)
                        {
                            double px = x;
                            double py = y;
                            double ly = y + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(px, 0, ly);
                            int v2 = AddVerticeOctTree(px, thickness, ly);
                            int v3 = AddVerticeOctTree(px, thickness, py);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);
                        }
                    }
                    y++;
                }
                x++;
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
                    // find first black
                    while (y < imageHeight && wrb.GetPixel(x, y).R > 128)
                    {
                        y++;
                    }
                    if (y < imageHeight)
                    {
                        if (x == imageWidth - 1 || wrb.GetPixel(x + 1, y).R > 128)
                        {
                            double px = x + 1;
                            double py = y;
                            double ly = y + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(px, 0, ly);
                            int v2 = AddVerticeOctTree(px, thickness, ly);
                            int v3 = AddVerticeOctTree(px, thickness, py);
                            AddFace(v0, v2, v1);
                            AddFace(v0, v3, v2);
                        }
                    }
                    y++;
                }
                x++;
            }
        }

        /*
        internal void GenerateImage(string plaqueImagePath)
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            BitmapImage plaqueBitmap = new BitmapImage();
            plaqueBitmap.BeginInit();
            plaqueBitmap.UriSource = new Uri(plaqueImagePath);
            plaqueBitmap.EndInit();

            // Create a FormatConvertedBitmap
            FormatConvertedBitmap grayBitmapSource = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            grayBitmapSource.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            grayBitmapSource.Source = plaqueBitmap;

            // Key of changing the bitmap format is DesitnationFormat property of BitmapSource.
            // It is a type of PixelFormat. FixelFormat has dozens of options to set
            // bitmap formatting.
            grayBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            grayBitmapSource.EndInit();

            imageWidth = (int)(Math.Ceiling(grayBitmapSource.Width)) + 2;
            imageHeight = (int)(Math.Ceiling(grayBitmapSource.Height)) + 2;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawImage(grayBitmapSource, new Rect(1, 1, grayBitmapSource.Width, grayBitmapSource.Height));

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
        */
    }
}