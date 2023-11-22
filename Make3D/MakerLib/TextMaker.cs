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
        private bool bold;
        private string fontName;
        private double textLength;

        private double holeOverlap = 10;
        private bool italic;
        private bool superSmooth;
        private string text;
        private double textThickness;

        public TextMaker(string t, string fn, double tl, double h, bool ss, bool bold, bool italic)
        {
            text = t;
            fontName = fn;
            textLength = tl;
            textThickness = h;
            superSmooth = ss;
            this.bold = bold;
            this.italic = italic;
        }

        private int imageWidth;
        private int imageHeight;

        public void GenerateText()
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            FontStyle fstyle = FontStyles.Normal;
            if (italic) fstyle = FontStyles.Italic;

            FontWeight fw = FontWeights.Normal;
            if (bold) fw = FontWeights.Bold;

            Typeface typeface = new Typeface(new System.Windows.Media.FontFamily(fontName), fstyle, fw, FontStretches.Normal);
            FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   typeface, 300, System.Windows.Media.Brushes.Black);
            // ft.SetFontStretch(FontStretches.Normal);

            System.Windows.Size sz = new System.Windows.Size(ft.Width, ft.Height);

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

        private bool Black(System.Windows.Media.Color c)
        {
            if (c.R < 50)
            {
                return true;
            }
            return false;
        }

        private bool White(System.Windows.Media.Color c)
        {
            if (c.R > 128)
            {
                return true;
            }
            return false;
        }

        private bool Gray(System.Windows.Media.Color c)
        {
            if (c.R > 50 && c.R < 128)
            {
                return true;
            }
            return false;
        }

        private void NorthGaps(WriteableBitmap wrb)
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
                        if (x + 1 < imageWidth && White(wrb.GetPixel(x, y - 1)) && Black((wrb.GetPixel(x + 1, y - 1))))
                        {
                            double px = x;
                            double py = y;
                            double lx = x + 1;
                            double ly = y - 1;
                            int v0 = AddVerticeOctTree(px, 0, py);
                            int v1 = AddVerticeOctTree(lx, 0, py);
                            int v2 = AddVerticeOctTree(lx, 0, ly);
                            int v3 = AddVerticeOctTree(px, textThickness, py);
                            int v4 = AddVerticeOctTree(lx, textThickness, py);
                            int v5 = AddVerticeOctTree(lx, textThickness, ly);
                            AddFace(v0, v2, v1);
                            AddFace(v3, v4, v5);
                            if (y > imageHeight / 2)
                            {
                                AddFace(v0, v2, v5);
                                AddFace(v0, v5, v3);
                            }
                            else
                            {
                               // AddFace(v0, v5, v2);
                               // AddFace(v0, v3, v5);
                            }
                            wrb.SetPixel(x, y - 1, System.Windows.Media.Color.FromArgb(255, 60, 60, 60));
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

        public string Generate(Point3DCollection pnts, Int32Collection faces)
        {
            string res = "";
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            GenerateText();
            /*
            PathGeometry p = TextHelper.PathFrom(text, "", true, fontName, fontsize, superSmooth, bold, italic);
            System.Diagnostics.Debug.WriteLine(p.ToString());
            res = p.ToString();
            string s;
            List<string> sfigures = new List<string>();
            List<TextPolygon> pfigures = new List<TextPolygon>();
            foreach (PathFigure pf in p.Figures)
            {
                var flatpf = pf.GetFlattenedPathFigure();
                s = flatpf.ToString();

                string[] parts = s.Split('M');
                foreach (string ap in parts)
                {
                    if (ap != "")
                    {
                        TextPolygon tp = new TextPolygon();
                        // get a list of pointfs defining the outter polygon
                        tp.SPath = "M" + ap;
                        List<System.Drawing.PointF> poutter = new List<System.Drawing.PointF>();
                        GetPathPoints(tp.SPath, tp.Points);
                        pfigures.Add(tp);
                    }
                }
            }
            HuntForHoles(pfigures);

            Object3D bodyShape = new Object3D();
            bodyShape.Position = new Point3D(0, 0, 0);
            Object3D holeShape = new Object3D();
            holeShape.Position = new Point3D(0, 0, 0);
            TriangulateFigureWallsBody(pfigures, bodyShape.AbsoluteObjectVertices, bodyShape.TriangleIndices);
            TriangulateFigureWallsHoles(pfigures, holeShape.AbsoluteObjectVertices, holeShape.TriangleIndices);

            //add front and back of body
            foreach (TextPolygon pf in pfigures)
            {
                bool reverse = true;
                for (float py = 0; py <= thickness; py += (float)thickness)
                {
                    EarClipping earClipping = new EarClipping();
                    List<Vector3m> rootPoints = new List<Vector3m>();

                    foreach (PointF rp in pf.Points)
                    {
                        rootPoints.Insert(0, new Vector3m(rp.X, py, rp.Y));
                    }

                    earClipping.SetPoints(rootPoints);

                    earClipping.Triangulate();
                    var surface = earClipping.Result;
                    for (int i = 0; i < surface.Count; i += 3)
                    {
                        int v1 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i].X, surface[i].Y, surface[i].Z);
                        int v2 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                        int v3 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);
                        if (reverse)
                        {
                            bodyShape.TriangleIndices.Add(v1);
                            bodyShape.TriangleIndices.Add(v3);
                            bodyShape.TriangleIndices.Add(v2);
                        }
                        else
                        {
                            bodyShape.TriangleIndices.Add(v1);
                            bodyShape.TriangleIndices.Add(v2);
                            bodyShape.TriangleIndices.Add(v3);
                        }
                    }
                    reverse = !reverse;
                }
            }

            bodyShape.AbsoluteToRelative();

            bool holesAdded = false;

            // Go through all the letter shapes
            foreach (TextPolygon pf in pfigures)
            {
                // does it have any holes
                if (pf.Holes.Count > 0)
                {
                    holesAdded = true;

                    for (float py = (float)-holeOverlap / 2; py <= (float)(thickness + holeOverlap / 2); py += (float)(thickness + holeOverlap))
                    {
                        EarClipping earClipping = new EarClipping();

                        for (int holeIndex = 0; holeIndex < pf.Holes.Count; holeIndex++)
                        {
                            TextPolygon hole = pf.Holes[holeIndex];

                            List<Vector3m> rootPoints = new List<Vector3m>();

                            // turns out that if there are more than one holes, their orientaion alternates
                            if (Math.Abs(py - (float)-holeOverlap / 2) < 0.0001)
                            {
                                foreach (PointF rp in hole.Points)
                                {
                                    rootPoints.Insert(0, new Vector3m(rp.X, py, rp.Y));
                                }
                            }
                            else
                            {
                                foreach (PointF rp in hole.Points)
                                {
                                    rootPoints.Add(new Vector3m(rp.X, py, rp.Y));
                                }
                            }

                            earClipping.SetPoints(rootPoints);

                            earClipping.Triangulate();
                            var surface = earClipping.Result;
                            for (int i = 0; i < surface.Count; i += 3)
                            {
                                int v1 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i].X, surface[i].Y, surface[i].Z);
                                int v2 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                                int v3 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);

                                holeShape.TriangleIndices.Add(v1);
                                holeShape.TriangleIndices.Add(v2);
                                holeShape.TriangleIndices.Add(v3);
                            }
                        }
                    }
                }
            }
            if (holesAdded)
            {
                holeShape.AbsoluteToRelative();
                Group3D merged = new Group3D();

                merged.LeftObject = bodyShape;
                merged.RightObject = holeShape;
                merged.Position = new Point3D(0, 0, 0);
                merged.PrimType = "groupdifference";

                //merged.PrimType = "groupunion";
                merged.Init();
                merged.Remesh();
                CopyShape(merged, pnts, faces);
            }
            else
            {
                CopyShape(bodyShape, pnts, faces);
            }
            */
            return res;
        }

        private void GetPathPoints(string txt, List<PointF> pnts)
        {
            float x = 0;
            float y = 0;

            pnts.Clear();
            txt = txt.Trim();
            while (txt.Length > 0)
            {
                if (txt.StartsWith("M"))
                {
                    txt = txt.Substring(1);
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
                else
                if (txt.StartsWith("L"))
                {
                    txt = txt.Substring(1);
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
                else if (txt.StartsWith(" "))
                {
                    txt = txt.Substring(1);
                }
                else if (txt.StartsWith("z") || txt.StartsWith("Z"))
                {
                    txt = txt.Substring(1);
                }
                else
                {
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
            }
        }

        private string GetTwoFloats(string txt, out float x, out float y)
        {
            x = 0;
            y = 0;
            txt = txt.Trim();
            string dummy = "";
            while (txt.Length > 0 && txt[0] != ' ' && txt[0] != 'L' && txt[0] != 'Z' && txt[0] != 'z')
            {
                dummy += txt[0];

                txt = txt.Substring(1);
            }
            string[] words = dummy.Split(',');
            if (words.GetLength(0) == 2)
            {
                try
                {
                    x = Convert.ToSingle(words[0]);
                    y = Convert.ToSingle(words[1]);
                }
                catch
                {
                }
            }
            return txt;
        }

        private void HuntForHoles(List<TextPolygon> pfigures)
        {
            bool rescan = false;
            while (!rescan)
            {
                rescan = true;

                for (int i = 0; i < pfigures.Count && rescan; i++)
                {
                    for (int j = 0; j < pfigures.Count && rescan; j++)
                    {
                        if (i != j)
                        {
                            if (pfigures[i].ContainsPoly(pfigures[j]))
                            {
                                pfigures[i].Holes.Add(pfigures[j]);
                                pfigures.RemoveAt(j);
                                rescan = false;
                            }
                        }
                    }
                }
            }
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

        private void TriangulateFigureWallsBody(List<TextPolygon> pfigures, Point3DCollection pnts, Int32Collection tris)
        {
            foreach (TextPolygon tp in pfigures)
            {
                MakeWall(tp.Points, textThickness, false, pnts, tris);
            }
        }

        private void TriangulateFigureWallsHoles(List<TextPolygon> pfigures, Point3DCollection pnts, Int32Collection tris)
        {
            foreach (TextPolygon tp in pfigures)
            {
                foreach (TextPolygon hole in tp.Holes)
                {
                    MakeWall(hole.Points, textThickness + holeOverlap, true, pnts, tris, holeOverlap / 2);
                }
            }
        }
    }
}