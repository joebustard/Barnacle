using Barnacle.Object3DLib;
using EarClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using FontStyle = System.Windows.FontStyle;

namespace MakerLib
{
    public class TextMaker : MakerBase
    {
        private int alignment;
        private bool bold;
        private string fontName;

        private int imageHeight;
        private int imageWidth;
        private bool italic;
        private bool superSmooth;
        private string text;
        private double textLength;
        private double textThickness;

        public TextMaker(string t, string fn, double tl, double h, bool ss, bool bold, bool italic, int alignment = 0)
        {
            text = t;
            fontName = fn;
            textLength = tl;
            textThickness = h;
            superSmooth = ss;
            this.bold = bold;
            this.italic = italic;
            this.alignment = alignment;
        }

        public string Generate(Point3DCollection pnts, Int32Collection faces)
        {
            string res = "";
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            GenerateText();
            return res;
        }

        public void GenerateText()
        {
            BitmapImage bitmap;

            FontStyle fstyle = FontStyles.Normal;
            if (italic) fstyle = FontStyles.Italic;

            FontWeight fw = FontWeights.Normal;
            if (bold) fw = FontWeights.Bold;

            Typeface typeface = new Typeface(new System.Windows.Media.FontFamily(fontName), fstyle, fw, FontStretches.Normal);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   typeface, 300, System.Windows.Media.Brushes.Black);

            ft.TextAlignment = TextAlignment.Left;

            System.Windows.Size sz = new System.Windows.Size(ft.Width, ft.Height);
            imageWidth = (int)(Math.Ceiling(sz.Width)) + 20;
            imageHeight = (int)(Math.Ceiling(sz.Height)) + 20;
            System.Windows.Point origin = new System.Windows.Point(10, 10);
            if (alignment == 1)
            {
                ft.TextAlignment = TextAlignment.Center;
                origin = new System.Windows.Point(imageWidth / 2, 10);
            }
            if (alignment == 2)
            {
                ft.TextAlignment = TextAlignment.Right;
                origin = new System.Windows.Point(imageWidth - 10, 10);
            }
            ft.LineHeight = 250;
            Logger.Log($"LineHeight {ft.LineHeight}");
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawText(ft, origin);

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
            CreateOctree(new Point3D(-1, -0.1, -1), new Point3D(imageWidth + 1, 2 * textThickness, imageHeight + 1));

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

            double scaleX = textLength / sizeX;
            double scaleY = textThickness / sizeY;
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point3D p = Vertices[i];
                Vertices[i] = new Point3D(p.X * scaleX, p.Y * scaleY, p.Z * scaleX);
            }
        }

        private bool Black(System.Windows.Media.Color c)
        {
            if (c.R < 50)
            {
                return true;
            }
            return false;
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
                            int v2 = AddVerticeOctTree(px, textThickness, ly);
                            int v3 = AddVerticeOctTree(px, textThickness, py);
                            AddFace(v0, v2, v1);
                            AddFace(v0, v3, v2);
                        }
                    }
                    y++;
                }
                x++;
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

                        v0 = AddVerticeOctTree(px, textThickness, py);
                        v1 = AddVerticeOctTree(lx, textThickness, py);
                        v2 = AddVerticeOctTree(lx, textThickness, ly);
                        v3 = AddVerticeOctTree(px, textThickness, ly);
                        AddFace(v0, v2, v1);
                        AddFace(v0, v3, v2);
                    }
                    x++;
                }
                y++;
            }
        }

        private bool Gray(System.Windows.Media.Color c)
        {
            if (c.R > 50 && c.R < 128)
            {
                return true;
            }
            return false;
        }

        private void MakeWall(List<PointF> points, double thickness, bool invert, double offset = 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                int p0 = AddVertice(points[i].X, -offset, points[i].Y);
                int p1 = AddVertice(points[i].X, thickness - offset, points[i].Y);
                int p2 = AddVertice(points[j].X, thickness - offset, points[j].Y);
                int p3 = AddVertice(points[j].X, -offset, points[j].Y);

                Faces.Add(p0);
                Faces.Add(p1);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p3);
            }
        }

        private void MakeWall(List<PointF> points, double thickness, bool invert, Point3DCollection pnts, Int32Collection tris, double offset = 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                int p0 = AddVertice(pnts, points[i].X, -offset, points[i].Y);
                int p1 = AddVertice(pnts, points[i].X, thickness - offset, points[i].Y);
                int p2 = AddVertice(pnts, points[j].X, thickness - offset, points[j].Y);
                int p3 = AddVertice(pnts, points[j].X, -offset, points[j].Y);

                if (!invert)
                {
                    tris.Add(p0);
                    tris.Add(p1);
                    tris.Add(p2);

                    tris.Add(p0);
                    tris.Add(p2);
                    tris.Add(p3);
                }
                else
                {
                    tris.Add(p0);
                    tris.Add(p2);
                    tris.Add(p1);

                    tris.Add(p0);
                    tris.Add(p3);
                    tris.Add(p2);
                }
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
                    while (x < imageWidth && White(wrb.GetPixel(x, y)))
                    {
                        x++;
                    }
                    if (x < imageWidth)
                    {
                        if (y == 0 || White(wrb.GetPixel(x, y - 1)))
                        {
                            double px = x;
                            double py = y;
                            double lx = x + 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, textThickness, py);
                            int v3 = AddVerticeOctTree(px, textThickness, py);
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
                            int v2 = AddVerticeOctTree(lx, textThickness, py);
                            int v3 = AddVerticeOctTree(px, textThickness, py);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);
                        }
                    }
                    x++;
                }
                y++;
            }
        }

        private void TriangulateFigureWalls(List<TextPolygon> pfigures)
        {
            foreach (TextPolygon tp in pfigures)
            {
                MakeWall(tp.Points, textThickness, false);
                foreach (TextPolygon hole in tp.Holes)
                {
                    MakeWall(hole.Points, textThickness, true);
                }
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
                            int v2 = AddVerticeOctTree(px, textThickness, ly);
                            int v3 = AddVerticeOctTree(px, textThickness, py);
                            AddFace(v0, v1, v2);
                            AddFace(v0, v2, v3);
                        }
                    }
                    y++;
                }
                x++;
            }
        }

        private bool White(System.Windows.Media.Color c)
        {
            if (c.R > 128)
            {
                return true;
            }
            return false;
        }
    }
}