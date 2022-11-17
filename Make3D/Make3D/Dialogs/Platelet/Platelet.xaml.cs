using Barnacle.Models;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using PolygonLibrary;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Devtest.xaml
    /// </summary>
    public partial class Platelet : BaseModellerDialog, INotifyPropertyChanged
    {
        private List<System.Windows.Point> displayPoints;
        private bool hollowShape;
        private bool loaded;
        private string loadedImageName;
        private double plateWidth;
        private string selectedTexture;
        private Visibility showTextures;
        private Visibility showWidth;
        private bool solidShape;
        private double textureDepth;
        private bool texturedShape;
        private Dictionary<string, string> textureFiles;
        private double wallWidth;
        private string warningText;
        private int textureImageWidth;
        private int textureImageHeight;
        private System.Drawing.Bitmap workingImage;

        public Platelet()
        {
            InitializeComponent();
            ToolName = "Platelet";
            DataContext = this;
            // ModelGroup = MyModelGroup;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            wallWidth = 2;
            textureDepth = 1;
            solidShape = true;
            hollowShape = false;
            texturedShape = false;
            showWidth = Visibility.Hidden;
            showTextures = Visibility.Hidden;
            ModelGroup = MyModelGroup;
            loaded = false;
            textureFiles = new Dictionary<string, string>();
            loadedImageName = "";
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

                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        public IEnumerable<String> TextureNames
        {
            get
            {
                return textureFiles.Keys;
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
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

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, -plateWidth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, -plateWidth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
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

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, plateWidth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, plateWidth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
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
                // generate side triangles so original points are already in list
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
                    int c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                    int c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                    int c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                    int c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);

                    c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, plateWidth);
                    c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, plateWidth);
                    c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, plateWidth);
                    c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, plateWidth);
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

                // generate side triangles so original points are already in list
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
                    int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVertice(t.Points[0].X, t.Points[0].Y, plateWidth);
                    c1 = AddVertice(t.Points[1].X, t.Points[1].Y, plateWidth);
                    c2 = AddVertice(t.Points[2].X, t.Points[2].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
                CentreVertices();
            }
        }

        private enum PixelColour
        {
            Unknown,
            Back,
            Front,
            Mixed
        }

        private double minTriSize = 0.2;

        internal List<TextureTriangle> triangleQueue;
        internal List<TextureTriangle> backgroundTriangles;
        internal List<TextureTriangle> frontTriangles;

        private void GenerateTexturedShape()
        {
            if (!String.IsNullOrEmpty(selectedTexture))
            {
                ClearShape();
                if (displayPoints?.Count > 3)
                {
                    List<System.Windows.Point> points = displayPoints;

                    List<System.Windows.Point> tmp = new List<System.Windows.Point>();
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
                    double lx = double.MaxValue;
                    double rx = double.MinValue;
                    double ty = double.MinValue;
                    double by = double.MaxValue;
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        lx = Math.Min(tmp[i].X, lx);
                        rx = Math.Max(tmp[i].X, rx);
                        by = Math.Min(tmp[i].Y, by);
                        ty = Math.Max(tmp[i].Y, ty);
                    }
                    double sz = 1;
                    double xspan = (rx - lx) / sz;
                    double yspan = (ty - by) / sz;
                    // generate side triangles so original points are already in list
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        CreateReversedSideFace(tmp, i);
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
                        int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, -plateWidth);
                        int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, -plateWidth);
                        int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, -plateWidth);
                        Faces.Add(c0);
                        Faces.Add(c2);
                        Faces.Add(c1);
                    }

                    ConvexPolygon2D boundary = new ConvexPolygon2D(tmp.ToArray());
                    ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
                    LoadTextureImage();
                    PlateletSurface surface = new PlateletSurface();
                    double z;
                    for (double px = lx; px < rx; px += sz)
                    {
                        for (double py = by; py < ty; py += sz)
                        {
                            ConvexPolygon2D rect = RectPoly(px, py, sz);
                            ConvexPolygon2D interception = interceptor.GetIntersectionOfPolygons(rect, boundary);
                            if (interception.Corners.GetLength(0) > 2)
                            {
                                if (interception.Corners.GetLength(0) == 4)
                                {
                                    // assume its a rect
                                    // surface.AddFaces(px, py, sz, sz);
                                    byte mask = GetTextureMask((int)px, (int)py);
                                    if (mask > 15)
                                    {
                                        z = textureDepth;
                                    }
                                    else
                                    {
                                        z = 0;
                                        if ((byte)(mask & leftMask) != 0)
                                        {
                                            int s0 = AddVertice(px, py, 0);
                                            int s1 = AddVertice(px, py, textureDepth);
                                            int s2 = AddVertice(px, py + sz, textureDepth);
                                            int s3 = AddVertice(px, py + sz, z);
                                            Faces.Add(s0);
                                            Faces.Add(s2);
                                            Faces.Add(s1);

                                            Faces.Add(s0);
                                            Faces.Add(s3);
                                            Faces.Add(s2);
                                        }

                                        if ((byte)(mask & rightMask) != 0)
                                        {
                                            int s0 = AddVertice(px + sz, py, 0);
                                            int s1 = AddVertice(px + sz, py, textureDepth);
                                            int s2 = AddVertice(px + sz, py + sz, textureDepth);
                                            int s3 = AddVertice(px + sz, py + sz, 0);
                                            Faces.Add(s0);
                                            Faces.Add(s1);
                                            Faces.Add(s2);

                                            Faces.Add(s0);
                                            Faces.Add(s2);
                                            Faces.Add(s3);
                                        }

                                        if ((byte)(mask & topMask) != 0)
                                        {
                                            int s0 = AddVertice(px, py + sz, 0);
                                            int s1 = AddVertice(px + sz, py + sz, 0);
                                            int s2 = AddVertice(px + sz, py + sz, textureDepth);
                                            int s3 = AddVertice(px, py + sz, textureDepth);
                                            Faces.Add(s0);
                                            Faces.Add(s2);
                                            Faces.Add(s1);

                                            Faces.Add(s0);
                                            Faces.Add(s3);
                                            Faces.Add(s2);
                                        }

                                        if ((byte)(mask & bottomMask) != 0)
                                        {
                                            int s0 = AddVertice(px, py, 0);
                                            int s1 = AddVertice(px + sz, py, 0);
                                            int s2 = AddVertice(px + sz, py, textureDepth);
                                            int s3 = AddVertice(px, py, textureDepth);
                                            Faces.Add(s0);
                                            Faces.Add(s1);
                                            Faces.Add(s2);

                                            Faces.Add(s0);
                                            Faces.Add(s2);
                                            Faces.Add(s3);
                                        }
                                    }
                                    int v0 = AddVertice(px, py, z);
                                    int v1 = AddVertice(px + sz, py, z);
                                    int v2 = AddVertice(px + sz, py + sz, z);
                                    int v3 = AddVertice(px, py + sz, z);
                                    Faces.Add(v0);
                                    Faces.Add(v1);
                                    Faces.Add(v2);

                                    Faces.Add(v0);
                                    Faces.Add(v2);
                                    Faces.Add(v3);
                                }
                                else
                                {
                                    ply = new TriangulationPolygon();
                                    List<System.Drawing.PointF> pfr = new List<System.Drawing.PointF>();
                                    foreach (System.Windows.Point p in interception.Corners)
                                    {
                                        pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                                    }
                                    tris = ply.Triangulate();
                                    foreach (Triangle t in tris)
                                    {
                                        int v0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0);
                                        int v1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0);
                                        int v2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0);

                                        Faces.Add(v0);
                                        Faces.Add(v1);
                                        Faces.Add(v2);
                                    }
                                }
                            }
                        }
                    }
                    //surface.ClassifyFaces(workingImage);

                    CentreVertices();
                }
            }
        }

        private byte GetTextureMask(int px, int py)
        {
            px = px % textureImageWidth;
            py = py % textureImageHeight;
            return textureMap[px, py];
        }

        private int Midpoint(Point3DCollection vt, int p0, int p1)
        {
            Point3D mid = new Point3D(vt[p0].X + (vt[p1].X - vt[p0].X) / 2.0,
                                        vt[p0].Y + (vt[p1].Y - vt[p0].Y) / 2.0,
                                        vt[p0].Z + (vt[p1].Z - vt[p0].Z) / 2.0);
            return AddPoint(vt, mid);
        }

        private int CentroidPoint(Point3DCollection vt, TextureTriangle tt)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            x = vt[tt.p0].X + vt[tt.p1].X + vt[tt.p2].X;
            y = vt[tt.p0].Y + vt[tt.p1].Y + vt[tt.p2].Y;
            z = vt[tt.p0].Z + vt[tt.p1].Z + vt[tt.p2].Z;
            Point3D mid = new Point3D(x / 3.0, y / 3.0, z / 3.0);
            return AddPoint(vt, mid);
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

        private PixelColour ClassifyTriangle(Point3DCollection vt, TextureTriangle tt, double lx, double by)
        {
            bool first = true;
            PixelColour firstPixel = PixelColour.Unknown;
            Rect bnds = GetTriBounds(vt, tt.p0, tt.p1, tt.p2);

            for (double px = bnds.Left; px <= bnds.Right; px++)
            {
                for (double py = bnds.Top; py <= bnds.Bottom; py++)
                {
                    if (PtInTriangle(px, py, vt, tt.p0, tt.p1, tt.p2))
                    {
                        PixelColour pixel = GetTexturePixel(px, py);
                        if (first)
                        {
                            firstPixel = pixel;
                            first = false;
                        }
                        else
                        {
                            if (pixel != firstPixel)
                            {
                                return PixelColour.Mixed;  // mixed
                            }
                        }
                    }
                }
            }
            return firstPixel;
        }

        private double sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
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

        private bool PtInTriangle(double px, double py, Point3DCollection vt, int p0, int p1, int p2)
        {
            bool res = PointInTriangle(new Point(px, py),
            new Point(vt[p0].X, vt[p0].Y),
            new Point(vt[p1].X, vt[p1].Y),
            new Point(vt[p2].X, vt[p2].Y));
            return res;
        }

        private Rect GetTriBounds(Point3DCollection vt, int p0, int p1, int p2)
        {
            double lx = double.MaxValue;
            double ly = double.MaxValue;
            double rx = double.MinValue;
            double ry = double.MinValue;
            lx = Math.Min(vt[p0].X, vt[p1].X);
            lx = Math.Min(lx, vt[p2].X);

            ly = Math.Min(vt[p0].Y, vt[p1].Y);
            ly = Math.Min(ly, vt[p2].Y);

            rx = Math.Max(vt[p0].X, vt[p1].X);
            rx = Math.Max(rx, vt[p2].X);

            ry = Math.Max(vt[p0].Y, vt[p1].Y);
            ry = Math.Max(ry, vt[p2].Y);

            Rect res = new Rect(lx, ly, rx - lx, ry - ly);
            return res;
        }

        private PixelColour GetTexturePixel(double cx, double cy)
        {
            double px = cx % textureImageWidth;
            double py = cy % textureImageHeight;
            if ((px >= 0 && px < textureImageWidth) && (py >= 0 && py < textureImageHeight))
            {
                System.Drawing.Color col = workingImage.GetPixel((int)px, (int)py);

                if (col.R < 128)
                {
                    return PixelColour.Front;
                }
            }
            return PixelColour.Back;
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
            HollowShape = EditorParameters.GetBoolean("Hollow", false);
            SolidShape = !HollowShape;
        }

        private byte[,] textureMap;
        private const byte leftMask = 0x01;
        private const byte rightMask = 0x02;
        private const byte topMask = 0x04;
        private const byte bottomMask = 0x08;
        private const byte frontMask = 0x10;

        private void LoadTextureImage()
        {
            if (selectedTexture != "")
            {
                if (selectedTexture != loadedImageName)
                {
                    string imagePath = textureFiles[selectedTexture];
                    if (File.Exists(imagePath))
                    {
                        workingImage = new System.Drawing.Bitmap(imagePath);
                        loadedImageName = selectedTexture;
                        textureImageWidth = workingImage.Width;
                        textureImageHeight = workingImage.Height;
                        textureMap = new byte[workingImage.Width, workingImage.Height];
                        for (int x = 0; x < workingImage.Width; x++)
                        {
                            for (int y = 0; y < workingImage.Height; y++)
                            {
                                byte mask = 0;
                                System.Drawing.Color col = workingImage.GetPixel(x, y);

                                if (col.R < 128)
                                {
                                    mask = frontMask;
                                }
                                else
                                {
                                    // its the back, do we need to raise the sides
                                    if (x > 0)
                                    {
                                        col = workingImage.GetPixel(x - 1, y);
                                        if (col.R < 128)
                                        {
                                            mask = (byte)(mask | leftMask);
                                        }
                                    }
                                    if (x < workingImage.Width - 1)
                                    {
                                        col = workingImage.GetPixel(x + 1, y);
                                        if (col.R < 128)
                                        {
                                            mask = (byte)(mask | rightMask);
                                        }
                                    }

                                    if (y > 0)
                                    {
                                        col = workingImage.GetPixel(x, y - 1);
                                        if (col.R < 128)
                                        {
                                            mask = (byte)(mask | bottomMask);
                                        }
                                    }
                                    if (y < workingImage.Height - 1)
                                    {
                                        col = workingImage.GetPixel(x, y + 1);
                                        if (col.R < 128)
                                        {
                                            mask = (byte)(mask | topMask);
                                        }
                                    }
                                }
                                textureMap[x, y] = mask;
                            }
                        }
                    }
                }
            }
        }

        private void LoadTextureNames()
        {
            try
            {
                String appFolder = AppDomain.CurrentDomain.BaseDirectory + "Data\\Textures";
                string[] txtFiles = Directory.GetFiles(appFolder, "*.png");
                foreach (string s in txtFiles)
                {
                    string fName = System.IO.Path.GetFileNameWithoutExtension(s);
                    textureFiles[fName] = s;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("Path", PathEditor.PathString);
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("PlateWidth", PlateWidth.ToString());
            EditorParameters.Set("Hollow", HollowShape.ToString());
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

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            LoadTextureNames();
            LoadEditorParameters();
            loaded = true;
            GenerateShape();
            UpdateCameraPos();
            // MyModelGroup.Children.Clear();
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