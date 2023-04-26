using Barnacle.Models;
using MakerLib.TextureUtils;
using OctTreeLib;
using PolygonLibrary;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Devtest.xaml
    /// </summary>
    public partial class Platelet : BaseModellerDialog, INotifyPropertyChanged
    {
        private const byte bottomMask = 0x08;
        private const byte frontMask = 0x10;
        private const byte leftMask = 0x01;
        private const byte rightMask = 0x02;
        private const byte topMask = 0x04;
        private List<System.Windows.Point> displayPoints;
        private bool hollowShape;
        private bool largeTexture;
        private bool lastCentre;
        private bool loaded;
        private string loadedImageName;
        private OctTree octTree;
        private Point PathCentroid;
        private double plateWidth;
        private string selectedTexture;
        private Visibility showTextures;
        private Visibility showWidth;
        private bool smallTexture;
        private bool solidShape;
        private double textureDepth;
        private bool texturedShape;
        private SortedList<string, string> textureFiles;
        private int textureImageHeight;
        private int textureImageWidth;
        private TextureManager textureManager;
        private byte[,] textureMap;
        private bool tileTexture;
        private double wallWidth;
        private string warningText;
        private System.Drawing.Bitmap workingImage;

        private double xExtent;

        private double yExtent;

        public Platelet()
        {
            InitializeComponent();
            ToolName = "Platelet";
            DataContext = this;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;
            wallWidth = 2;
            textureDepth = 1;
            solidShape = true;
            hollowShape = false;
            texturedShape = false;
            largeTexture = true;
            tileTexture = true;
            showWidth = Visibility.Hidden;
            showTextures = Visibility.Hidden;
            ModelGroup = MyModelGroup;
            loaded = false;
            textureFiles = new SortedList<string, string>();
            loadedImageName = "";
            selectedTexture = "";
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureNames();
            textureManager.Mode = TextureManager.MapMode.ClippedTile;
        }

        public enum PixelColour
        {
            Unknown,
            Back,
            Front,
            Mixed
        }

        public bool HollowShape
        {
            get
            {
                return hollowShape;
            }
            set
            {
                if (hollowShape != value)
                {
                    hollowShape = value;
                    if (hollowShape == true)
                    {
                        ShowWidth = Visibility.Visible;
                        ShowTextures = Visibility.Hidden;
                    }
                    else
                    {
                        ShowWidth = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool LargeTexture
        {
            get
            {
                return largeTexture;
            }
            set
            {
                if (value != largeTexture)
                {
                    largeTexture = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double PlateWidth
        {
            get { return plateWidth; }
            set
            {
                if (plateWidth != value)
                {
                    if (value > 0 && value <= 500)
                    {
                        plateWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public string SelectedTexture
        {
            get { return selectedTexture; }
            set
            {
                if (value != selectedTexture)
                {
                    selectedTexture = value;
                    if (textureManager != null && !String.IsNullOrEmpty(selectedTexture))
                    {
                        textureManager.LoadTextureImage(selectedTexture);
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }
            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public Visibility ShowTextures
        {
            get
            {
                return showTextures;
            }
            set
            {
                if (showTextures != value)
                {
                    showTextures = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowWidth
        {
            get
            {
                return showWidth;
            }
            set
            {
                if (showWidth != value)
                {
                    showWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool SmallTexture
        {
            get
            {
                return smallTexture;
            }
            set
            {
                if (value != smallTexture)
                {
                    smallTexture = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool SolidShape
        {
            get
            {
                return solidShape;
            }
            set
            {
                if (solidShape != value)
                {
                    solidShape = value;
                    if (solidShape)
                    {
                        ShowWidth = Visibility.Hidden;
                        ShowTextures = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double TextureDepth
        {
            get
            {
                return textureDepth;
            }
            set
            {
                if (textureDepth != value)
                {
                    textureDepth = value;
                    if (textureDepth >= 0.1)
                    {
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool TexturedShape
        {
            get
            {
                return texturedShape;
            }
            set
            {
                if (texturedShape != value)
                {
                    texturedShape = value;
                    if (texturedShape == true)
                    {
                        ShowWidth = Visibility.Hidden;
                        ShowTextures = Visibility.Visible;
                    }
                    else
                    {
                        ShowTextures = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> TextureItems
        {
            get { return textureManager.TextureNames; }
        }

        public IEnumerable<String> TextureNames
        {
            get
            {
                return textureFiles.Keys;
            }
        }

        public bool TileTexture
        {
            get
            {
                return tileTexture;
            }
            set
            {
                if (tileTexture != value)
                {
                    tileTexture = value;
                    NotifyPropertyChanged();
                }
                UpdateDisplay();
            }
        }

        public double WallWidth
        {
            get { return wallWidth; }
            set
            {
                if (wallWidth != value)
                {
                    if (value > 0 && value <= 500)
                    {
                        wallWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public string WarningText
        {
            get
            {
                return warningText;
            }
            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int AddVerticeOctTree(double x, double y, double z)
        {
            int res = -1;
            if (octTree != null)
            {
                Point3D v = new Point3D(x, y, z);
                res = octTree.PointPresent(v);

                if (res == -1)
                {
                    res = Vertices.Count;
                    octTree.AddPoint(res, v);
                }
            }
            return res;
        }

        public double AreaOfTriangle(Point3DCollection vt, int p0, int p1, int p2)
        {
            Point pt0 = new Point(vt[p0].X, vt[p0].Y);
            Point pt1 = new Point(vt[p1].X, vt[p1].Y);
            Point pt2 = new Point(vt[p2].X, vt[p2].Y);

            double a = Distance(pt0, pt1);
            double b = Distance(pt1, pt2);
            double c = Distance(pt2, pt0);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public bool PointOnLineSegment(Point pt1, Point pt2, Point pt, double epsilon = 0.00001)
        {
            if (pt.X - Math.Max(pt1.X, pt2.X) > epsilon ||
                Math.Min(pt1.X, pt2.X) - pt.X > epsilon ||
                pt.Y - Math.Max(pt1.Y, pt2.Y) > epsilon ||
                Math.Min(pt1.Y, pt2.Y) - pt.Y > epsilon)
                return false;

            if (Math.Abs(pt2.X - pt1.X) < epsilon)
                return Math.Abs(pt1.X - pt.X) < epsilon || Math.Abs(pt2.X - pt.X) < epsilon;
            if (Math.Abs(pt2.Y - pt1.Y) < epsilon)
                return Math.Abs(pt1.Y - pt.Y) < epsilon || Math.Abs(pt2.Y - pt.Y) < epsilon;

            double x = pt1.X + (pt.Y - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
            double y = pt1.Y + (pt.X - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X);

            return Math.Abs(pt.X - x) < epsilon || Math.Abs(pt.Y - y) < epsilon;
        }

        protected Point Centroid(Point[] crns)
        {
            double x = 0;
            double y = 0;
            if (crns.GetLength(0) > 0)
            {
                foreach (Point p in crns)
                {
                    x += p.X;
                    y += p.Y;
                }
                x /= crns.GetLength(0);
                y /= crns.GetLength(0);
            }
            return new Point(x, y);
        }

        protected OctTree CreateOctree(Point3D minPoint, Point3D maxPoint)
        {
            octTree = new OctTree(Vertices, minPoint, maxPoint, 200);
            return octTree;
        }

        protected bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y
                || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        protected ConvexPolygon2D RectPoly(double px, double py, double sz)
        {
            System.Windows.Point[] rct = new System.Windows.Point[4];
            rct[0].X = px;
            rct[0].Y = py;

            rct[1].X = px;
            rct[1].Y = py + sz;

            rct[2].X = px + sz;
            rct[2].Y = py + sz;

            rct[3].X = px + sz;
            rct[3].Y = py;

            return new ConvexPolygon2D(rct);
        }

        protected void TriangulateSurface(Point[] points, double z, bool reverse = false)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (Point p in points)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, z);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, z);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, z);
                if (reverse)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        private void CalculateExtents(List<Point> tmp, out double lx, out double rx, out double ty, out double by)
        {
            lx = double.MaxValue;
            rx = double.MinValue;
            ty = double.MinValue;
            by = double.MaxValue;
            for (int i = 0; i < tmp.Count; i++)
            {
                lx = Math.Min(tmp[i].X, lx);
                rx = Math.Max(tmp[i].X, rx);
                by = Math.Min(tmp[i].Y, by);
                ty = Math.Max(tmp[i].Y, ty);
            }
            xExtent = rx - lx;
            yExtent = ty - by;
        }

        private void CloseEdge(double lx, double by, double rx, double ty, List<Point> tmp, double sz)
        {
            Extents ext = new Extents();
            int texturexX = 0;
            int texturexY = 0;
            for (double px = lx; px < rx; px += sz)
            {
                texturexY = 0;
                for (double py = by; py < ty; py += sz)
                {
                    TextureCell cell = textureManager.GetCell(texturexX, texturexY);
                    if (cell.Width > 0)
                    {
                        double z = (double)(cell.Width * textureDepth) / 255.0;
                        ext.Left = px;
                        ext.Bottom = py;
                        ext.Right = px + sz;
                        ext.Top = py + sz;
                        for (int i = 0; i < tmp.Count; i++)
                        {
                            int j = (i + 1) % tmp.Count;
                            Tuple<Point, Point> seg = CohenSutherland.CohenSutherlandLineClip(ext, tmp[i], tmp[j]);
                            if (seg != null)
                            {
                                // if (seg.Item1.X != seg.Item2.X || seg.Item1.Y != seg.Item2.Y)
                                {
                                    int v0 = AddVerticeOctTree(seg.Item1.X, seg.Item1.Y, 0);
                                    int v1 = AddVerticeOctTree(seg.Item2.X, seg.Item2.Y, 0);
                                    int v2 = AddVerticeOctTree(seg.Item2.X, seg.Item2.Y, textureDepth);
                                    int v3 = AddVerticeOctTree(seg.Item1.X, seg.Item1.Y, textureDepth);

                                    Faces.Add(v0);
                                    Faces.Add(v1);
                                    Faces.Add(v2);

                                    Faces.Add(v0);
                                    Faces.Add(v2);
                                    Faces.Add(v3);
                                }
                            }
                        }
                    }
                    texturexY++;
                }
                texturexX++;
            }
        }

        private void CreateReversedSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, -plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, -plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void CreateTheBack(List<Point> tmp)
        {
            // triangulate the basic polygon
            List<Triangle> tris;
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (System.Windows.Point p in tmp)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();

            tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, -plateWidth);
                int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, -plateWidth);
                int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, -plateWidth);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);
            }
        }

        private int FindVertexAtZero(List<Point> boundary, int sm)
        {
            Point testPoint = new Point(Vertices[sm].X, Vertices[sm].Y);
            if (PointOnBoundary(testPoint, boundary))
            {
                Point3D pAtZero = new Point3D(Vertices[sm].X, Vertices[sm].Y, 0.0);
                return AddVertice(pAtZero);
            }
            else
            {
                return -1;
            }
        }

        private void GenerateHollow()
        {
            List<System.Windows.Point> points = displayPoints;
            List<System.Drawing.PointF> outerPolygon = new List<System.Drawing.PointF>();
            List<System.Drawing.PointF> innerPolygon = new List<System.Drawing.PointF>();
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            double top = 0;
            double lx = double.MaxValue;
            double rx = double.MinValue;
            double ty = double.MinValue;
            double by = double.MaxValue;
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].X < lx)
                    {
                        lx = points[i].X;
                    }
                    if (points[i].X > rx)
                    {
                        rx = points[i].X;
                    }
                    if (points[i].Y > ty)
                    {
                        ty = points[i].Y;
                    }
                    if (points[i].Y < by)
                    {
                        by = points[i].Y;
                    }
                }

                // user may ahve chosen a wallwidth that means the inside walls overlap
                // DOnt allow that.
                double actualWallWidth = wallWidth;
                double d = ((rx - lx) / 2) - 0.1;
                if (d < actualWallWidth)
                {
                    actualWallWidth = d;
                }
                d = ((ty - by) / 2) - 0.1;
                if (d <= actualWallWidth)
                {
                    actualWallWidth = d;
                }

                top = ty;
                for (int i = 0; i < points.Count; i++)
                {
                    if (PathEditor.LocalImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        //tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                        tmp.Add(new System.Windows.Point(points[i].X, top - points[i].Y));
                    }
                    else
                    {
                        double x = PathEditor.ToMM(points[i].X);
                        double y = PathEditor.ToMM(top - points[i].Y);
                        tmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }
                Point[] clik = OrderAntiClockwise(tmp.ToArray());
                for (int i = 0; i < tmp.Count - 1; i++)
                {
                    outerPolygon.Add(new System.Drawing.PointF((float)tmp[i].X, (float)tmp[i].Y));
                }
                outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);

                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    innerPolygon.Add(new System.Drawing.PointF((float)outerPolygon[i].X, (float)outerPolygon[i].Y));
                }
                innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)actualWallWidth);

                tmp.Clear();
                for (int i = outerPolygon.Count - 1; i >= 0; i--)
                {
                    tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
                }

                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));                // generate side triangles so original points are already in list
                Point[] clk = OrderClockwise(tmp.ToArray());
                tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                tmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
                }
                // generate side triangles so original points are already in list               
                clk = OrderAntiClockwise(tmp.ToArray());
                tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    int j = i + 1;
                    if (j == outerPolygon.Count)
                    {
                        j = 0;
                    }
                    int c0 = AddVerticeOctTree(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                    int c1 = AddVerticeOctTree(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                    int c2 = AddVerticeOctTree(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                    int c3 = AddVerticeOctTree(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);

                    c0 = AddVerticeOctTree(outerPolygon[i].X, outerPolygon[i].Y, plateWidth);
                    c1 = AddVerticeOctTree(innerPolygon[i].X, innerPolygon[i].Y, plateWidth);
                    c2 = AddVerticeOctTree(innerPolygon[j].X, innerPolygon[j].Y, plateWidth);
                    c3 = AddVerticeOctTree(outerPolygon[j].X, outerPolygon[j].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }

                CentreVertices();
            }
        }

        private void GenerateShape()
        {
            if (loaded)
            {
                if (SolidShape)
                {
                    GenerateSolid();
                }
                else
                {
                    if (HollowShape)
                    {
                        GenerateHollow();
                    }
                    else
                    {
                        if (TexturedShape)
                        {
                            GenerateTexturedShape();
                        }
                    }
                }
            }
        }

        private void GenerateSolid()
        {
            ClearShape();
            if (displayPoints?.Count > 3)
            {
                List<System.Windows.Point> points = displayPoints;
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Y > top)
                    {
                        top = points[i].Y;
                    }
                }
                for (int i = 0; i < points.Count; i++)
                {
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));

                // generate side triangles so original points are already in list
                Point[] clk = OrderClockwise(tmp.ToArray());
                tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }
                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, plateWidth);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, plateWidth);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
                CentreVertices();
            }
        }

        private void GenerateTexturedShape()
        {
            if (!String.IsNullOrEmpty(selectedTexture))
            {
                ClearShape();
                PathCentroid = PathEditor.Centroid();
                if (displayPoints?.Count > 3)
                {
                    List<System.Windows.Point> points = displayPoints;

                    List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                    InvertVertical(points, tmp);
                    Point[] clk = OrderClockwise(tmp.ToArray());
                    tmp = clk.ToList();
                    double lx, rx, ty, by;
                    CalculateExtents(tmp, out lx, out rx, out ty, out by);

                    OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
              new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));

                    double sz = 0.5;
                    if (!largeTexture)
                    {
                        sz = 0.25;
                    }

                    // generate side triangles so original points are already in list
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        CreateReversedSideFace(tmp, i);
                    }

                    CreateTheBack(tmp);

                    ConvexPolygon2D boundary = new ConvexPolygon2D(tmp.ToArray());
                    ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
                    //   LoadTextureImage();

                    double off = sz / 2;
                    int texturexX = 0;
                    int texturexY = 0;
                    for (double px = lx; px < rx; px += sz)
                    {
                        texturexY = 0;
                        for (double py = by; py < ty; py += sz)
                        {
                            ConvexPolygon2D rect = RectPoly(px, py, sz);
                            ConvexPolygon2D interception = interceptor.GetIntersectionOfPolygons(rect, boundary);
                            if (interception.Corners.GetLength(0) > 2)
                            {
                                bool insidepixel = false;
                                // byte mask = GetTextureMask(px, py, sz, tileTexture);
                                byte mask = 0;
                                TextureCell cell = textureManager.GetCell(texturexX, texturexY);
                                if (interception.Corners.GetLength(0) == 4)
                                {
                                    insidepixel = IsItAnInsidePixel(interception.Corners, px, py, sz);
                                }
                                if (insidepixel)
                                {
                                    PolygonInside(tmp, sz, off, px, py, cell);
                                }
                                else
                                {
                                    PolygonOnEdge(tmp, sz, off, px, py, interception, cell);
                                }
                            }
                            texturexY++;
                        }
                        texturexX++;
                    }

                    CloseEdge(lx, by, rx, ty, tmp, sz);
                    CentreVertices();
                }
            }
        }

        private byte GetTextureMask(double px, double py, double sz, bool tile)
        {
            if (!tile)
            {
                if (sz == 0.5)
                {
                    double xr = (textureImageWidth / xExtent);
                    double yr = (textureImageHeight / yExtent);
                    px = px * xr;
                    py = py * yr;

                    if (px >= 0 && px < textureImageWidth && py >= 0 && py < textureImageHeight)
                    {
                        return textureMap[(int)px, (int)py];
                    }
                    return (byte)0;
                }
                if (sz == 0.25)
                {
                    double xr = 2 * (textureImageWidth / xExtent);
                    double yr = 2 * (textureImageHeight / yExtent);
                    px = px * xr;
                    py = py * yr;
                    px = px - (textureImageWidth / 2);
                    py = py - (textureImageHeight / 2);
                    if (px >= 0 && px < textureImageWidth && py >= 0 && py < textureImageHeight)
                    {
                        return textureMap[(int)px, (int)py];
                    }
                    return (byte)0;
                }
                return (byte)0;
            }
            else
            {
                px = px / sz;
                py = py / sz;
                px = px % textureImageWidth;
                py = py % textureImageHeight;
                return textureMap[(int)px, (int)py];
            }
        }

        /// <summary>
        /// Invert a list of points vertically
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tmp"></param>
        private void InvertVertical(List<Point> points, List<Point> tmp)
        {
            double top = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].Y > top)
                {
                    top = points[i].Y;
                }
            }
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (PathEditor.LocalImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                else

                {
                    double x = PathEditor.ToMM(points[i].X);
                    double y = PathEditor.ToMM(top - points[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }
        }

        private bool IsItAnInsidePixel(Point[] c, double px, double py, double sz)
        {
            bool result = false;
            if (c[0].X == px && c[0].Y == py)
            {
                if (c[1].X == px + sz && c[1].Y == py)
                {
                    if (c[2].X == px + sz && c[2].Y == py + sz)
                    {
                        if (c[3].X == px && c[3].Y == py + sz)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        private void LoadEditorParameters()
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }

            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            PlateWidth = EditorParameters.GetDouble("PlateWidth", 10);
            TextureDepth = EditorParameters.GetDouble("TextureDepth", 0.5);
            HollowShape = EditorParameters.GetBoolean("Hollow", false);
            TexturedShape = EditorParameters.GetBoolean("Textured", false);
            SolidShape = EditorParameters.GetBoolean("Solid", true);
            LargeTexture = EditorParameters.GetBoolean("LargeTexture", true);
            SmallTexture = EditorParameters.GetBoolean("SmallTexture", false);
            TileTexture = EditorParameters.GetBoolean("CentreTexture", true);
            SelectedTexture = EditorParameters.Get("SelectedTexture");
            textureManager.LoadTextureImage(selectedTexture);
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private bool PointInTriangle(Point pt, Point v1, Point v2, Point v3)
        {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private bool PointOnBoundary(Point testPoint, List<Point> boundary)
        {
            bool result = false;
            for (int i = 0; i < boundary.Count; i++)
            {
                int j = (i + 1) % boundary.Count;
                if (PointOnLineSegment(boundary[i], boundary[j], testPoint))
                {
                    return true;
                }
            }
            return result;
        }

        private void PolygonInside(List<Point> tmp, double sz, double off, double px, double py, TextureCell cell)
        {
            double z = 0;
            if (cell != null)
            {
                z = ((double)cell.Width * textureDepth) / 255.0;
                // main surface
                int v0 = AddVerticeOctTree(px, py, z);
                int v1 = AddVerticeOctTree(px + sz, py, z);
                int v2 = AddVerticeOctTree(px + sz, py + sz, z);
                int v3 = AddVerticeOctTree(px, py + sz, z);
                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);

                if (cell.WestWall > 0)
                {
                    double westSideDepth = ((double)(cell.Width - cell.WestWall) * textureDepth) / 255.0;

                    v0 = AddVerticeOctTree(px, py, z);
                    v1 = AddVerticeOctTree(px, py, westSideDepth);
                    v2 = AddVerticeOctTree(px, py + sz, westSideDepth);
                    v3 = AddVerticeOctTree(px, py + sz, z);
                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v1);

                    Faces.Add(v0);
                    Faces.Add(v3);
                    Faces.Add(v2);
                }

                if (cell.EastWall > 0)
                {
                    double eastSideDepth = ((double)(cell.Width - cell.EastWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px + sz, py, z);
                    v1 = AddVerticeOctTree(px + sz, py, eastSideDepth);
                    v2 = AddVerticeOctTree(px + sz, py + sz, eastSideDepth);
                    v3 = AddVerticeOctTree(px + sz, py + sz, z);
                    Faces.Add(v0);
                    Faces.Add(v1);
                    Faces.Add(v2);

                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v3);
                }

                if (cell.NorthWall > 0)
                {
                    double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px, py, z);
                    v1 = AddVerticeOctTree(px + sz, py, z);
                    v2 = AddVerticeOctTree(px + sz, py, sideDepth);
                    v3 = AddVerticeOctTree(px, py, sideDepth);

                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);
                }
                if (cell.SouthWall > 0)
                {
                    double sideDepth = ((double)(cell.Width - cell.SouthWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px, py + sz, z);
                    v1 = AddVerticeOctTree(px + sz, py + sz, z);
                    v2 = AddVerticeOctTree(px + sz, py + sz, sideDepth);
                    v3 = AddVerticeOctTree(px, py + sz, sideDepth);

                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }
            }
        }

        private void PolygonOnEdge(List<Point> tmp, double sz, double off, double px, double py, ConvexPolygon2D interception, TextureCell cell)
        {
            TriangulationPolygon ply;
            List<Triangle> tris;
            double z;
            if (interception.Corners.GetLength(0) == 3)
            {
                z = ((double)cell.Width * textureDepth) / 255.0;
                int v0 = AddVerticeOctTree(interception.Corners[0].X, interception.Corners[0].Y, z);
                int v1 = AddVerticeOctTree(interception.Corners[1].X, interception.Corners[1].Y, z);
                int v2 = AddVerticeOctTree(interception.Corners[2].X, interception.Corners[2].Y, z);

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);
                if (cell.Width > 0)
                {
                    if (cell.WestWall > 0)
                    {
                        if (IsPointInPolygon(new Point(px - off, py + off), tmp))
                        {
                            int s0 = AddVerticeOctTree(px, py, 0);
                            int s1 = AddVerticeOctTree(px, py, z);
                            int s2 = AddVerticeOctTree(px, py + sz, z);
                            int s3 = AddVerticeOctTree(px, py + sz, 0);
                            Faces.Add(s0);
                            Faces.Add(s2);
                            Faces.Add(s1);

                            Faces.Add(s0);
                            Faces.Add(s3);
                            Faces.Add(s2);
                        }
                    }
                }
                if (cell.EastWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + sz + off, py + off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px + sz, py, 0);
                        int s1 = AddVerticeOctTree(px + sz, py, z);
                        int s2 = AddVerticeOctTree(px + sz, py + sz, z);
                        int s3 = AddVerticeOctTree(px + sz, py + sz, 0);
                        Faces.Add(s0);
                        Faces.Add(s1);
                        Faces.Add(s2);

                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s3);
                    }
                }
                if (cell.SouthWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + off, py + sz + off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px, py + sz, 0);
                        int s1 = AddVerticeOctTree(px + sz, py + sz, 0);
                        int s2 = AddVerticeOctTree(px + sz, py + sz, z);
                        int s3 = AddVerticeOctTree(px, py + sz, z
                        );
                        Faces.Add(s0);
                        Faces.Add(s1);
                        Faces.Add(s2);

                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s3);
                    }
                }

                if (cell.NorthWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + off, py - off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px, py, 0);
                        int s1 = AddVerticeOctTree(px + sz, py, 0);
                        int s2 = AddVerticeOctTree(px + sz, py, z);
                        int s3 = AddVerticeOctTree(px, py, z);
                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s1);

                        Faces.Add(s0);
                        Faces.Add(s3);
                        Faces.Add(s2);
                    }
                }
            }
            else
            {
                z = ((double)cell.Width * textureDepth) / 255.0;
                /*
                if (mask > 15)
                {
                    z = textureDepth;
                }
                */

                ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pfr = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in interception.Corners)
                {
                    pfr.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pfr.ToArray();
                tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int v0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, z);
                    int v1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, z);
                    int v2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, z);

                    Faces.Add(v0);
                    Faces.Add(v1);
                    Faces.Add(v2);
                }
            }
        }

        private bool PtInTriangle(double px, double py, Point3DCollection vt, int p0, int p1, int p2)
        {
            bool res = PointInTriangle(new Point(px, py),
            new Point(vt[p0].X, vt[p0].Y),
            new Point(vt[p1].X, vt[p1].Y),
            new Point(vt[p2].X, vt[p2].Y));
            return res;
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("Path", PathEditor.PathString);
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("PlateWidth", PlateWidth.ToString());
            EditorParameters.Set("TextureDepth", TextureDepth.ToString());
            EditorParameters.Set("Hollow", HollowShape.ToString());
            EditorParameters.Set("Solid", SolidShape.ToString());
            EditorParameters.Set("Textured", TexturedShape.ToString());
            EditorParameters.Set("LargeTexture", LargeTexture.ToString());
            EditorParameters.Set("SmallTexture", SmallTexture.ToString());
            EditorParameters.Set("SelectedTexture", SelectedTexture);
            EditorParameters.Set("CentreTexture", TileTexture.ToString());
        }

        private List<System.Drawing.PointF> ShrinkPolygon(List<System.Drawing.PointF> ply, double offset)
        {
            List<System.Drawing.PointF> res = new List<System.Drawing.PointF>();
            float cX = 0.0F;
            float cY = 0.0F;
            foreach (System.Drawing.PointF pf in ply)
            {
                cX += pf.X;
                cY += pf.Y;
            }
            cX /= ply.Count;
            cY /= ply.Count;

            float dx;
            float dy;
            foreach (System.Drawing.PointF pf in ply)
            {
                dx = pf.X - cX;
                dy = pf.Y - cY;
                double dist = Math.Sqrt((dx * dx) + (dy * dy));
                double theta = Math.Atan2(dy, dx);
                if (dist > offset)
                {
                    dist -= offset;
                }
                System.Drawing.PointF np = new System.Drawing.PointF((float)(dist * Math.Cos(theta)) + cX, (float)(dist * Math.Sin(theta)) + cY);
                res.Add(np);
            }
            return res;
        }

        private double sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            //   LoadTextureNames();
            LoadEditorParameters();
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;
            GenerateShape();
            UpdateCameraPos();
            warningText = "";
            Redisplay();
        }
    }

    public class TextureTriangle
    {
        public bool inside;
        public int p0;
        public int p1;
        public int p2;

        public TextureTriangle(int c0, int c1, int c2)
        {
            p0 = c0;
            p1 = c1;
            p2 = c2;
        }
    }
}