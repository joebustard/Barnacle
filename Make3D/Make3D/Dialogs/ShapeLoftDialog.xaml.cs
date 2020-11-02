using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapeLoftDialog.xaml
    /// </summary>
    public partial class ShapeLoftDialog : Window
    {
        public int degreeStep = 1;
        private List<Point> bottomPoints;
        private List<Point> bottomShell;
        private Int32Collection faces;
        private MeshGeometry3D mesh;
        private double sizeX;
        private double sizeY;
        private double sizeZ;
        private List<Point> topPoints;
        private List<Point> topShell;
        private Point3DCollection vertices;

        public ShapeLoftDialog()
        {
            InitializeComponent();
            topPoints = null;
            bottomPoints = null;
            vertices = new Point3DCollection();
            faces = new Int32Collection();
            bottomShell = new List<Point>();
            topShell = new List<Point>();
            sizeX = 10;
            sizeY = 10;
            sizeZ = 10;
        }

        public void Export(string filename, Point3DCollection vertices, Int32Collection TriangleIndices)
        {
            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            if (stream != null)
            {
                var writer = new BinaryWriter(stream);

                // write header
                var header = new byte[80]; // can be a garbage value
                writer.Write(header);
                uint totalTriangles = (uint)TriangleIndices.Count / 3;

                // write vertex count
                writer.Write(totalTriangles);

                // write triangles
                for (int face = 0; face < TriangleIndices.Count; face += 3)
                {
                    Point3D V1 = vertices[TriangleIndices[face]];
                    Point3D V2 = vertices[TriangleIndices[face + 1]];
                    Point3D V3 = vertices[TriangleIndices[face + 2]];
                    writer.Write((float)0);
                    writer.Write((float)0);
                    writer.Write((float)0);
                    //    System.Diagnostics.Debug.WriteLine($"n {normal.X},{normal.Y},{normal.Z}");
                    writer.Write((float)V1.X * sizeX / 2);
                    writer.Write((float)V1.Y * sizeY / 2);
                    writer.Write((float)V1.Z * sizeZ / 2);
                    //   System.Diagnostics.Debug.WriteLine($"V1 {triangle.V1.Position.X},{triangle.V1.Position.Y},{triangle.V1.Position.Z}");

                    writer.Write((float)V2.X * sizeX / 2);
                    writer.Write((float)V2.Y * sizeY / 2);
                    writer.Write((float)V2.Z * sizeZ / 2);
                    //    System.Diagnostics.Debug.WriteLine($"V2 {triangle.V2.Position.X},{triangle.V2.Position.Y},{triangle.V2.Position.Z}");

                    writer.Write((float)V3.X * sizeX / 2);
                    writer.Write((float)V3.Y * sizeY / 2);
                    writer.Write((float)V3.Z * sizeZ / 2);
                    //   System.Diagnostics.Debug.WriteLine($"V3 {triangle.V3.Position.X},{triangle.V3.Position.Y},{triangle.V3.Position.Z}");
                    writer.Write((ushort)0); // garbage value
                }

                writer.Flush();
                int size = 84 + (int)totalTriangles * 50;
                System.Diagnostics.Debug.WriteLine($"File should be {size} bytes");
            }
            stream.Close();
        }

        public Int32Collection GetFaces()
        {
            return faces;
        }

        public Point3DCollection GetVertices()
        {
            return vertices;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            //Export("C:\\tmp\\loft.stl", vertices, faces);
            DialogResult = true;
            Close();
        }

        private void OnBottomPointsChanged(List<Point> pnts)
        {
            bottomPoints = pnts;
            Redraw();
        }

        private void OnTopPointsChanged(List<Point> pnts)
        {
            topPoints = pnts;
            Redraw();
        }

        private void Redraw()
        {
            if (topPoints != null && bottomPoints != null)
            {
                bottomShell.Clear(); ;
                topShell.Clear();
                double dTheta = (degreeStep / 360.0) * Math.PI * 2;
                double dBottom = (Math.PI * 2) / (bottomPoints.Count);
                double dTop = (Math.PI * 2) / (topPoints.Count);
                for (double theta = 0; theta < Math.PI * 2; theta += dTheta)
                {
                    Point st = new Point(0, 0);
                    Point nd = new Point(0, 1); ;
                    int bottomIndex = (int)(theta / dBottom);
                    if (bottomIndex < bottomPoints.Count - 1)
                    {
                        st = bottomPoints[bottomIndex];
                        nd = bottomPoints[bottomIndex + 1];
                    }
                    else
                    {
                        st = bottomPoints[bottomIndex];
                        nd = bottomPoints[0];
                    }

                    double res = theta - (bottomIndex * dBottom);
                    double t = 0;
                    if (res > 0)
                    {
                        t = res / dBottom;
                    }
                    if (t < 0 || t > 1)
                    {
                    }

                    double px = st.X + (t * (nd.X - st.X));
                    double py = st.Y + (t * (nd.Y - st.Y));
                    bottomShell.Add(new Point(px, py));

                    int topIndex = (int)(theta / dTop);
                    if (topIndex < topPoints.Count - 1)
                    {
                        st = topPoints[topIndex];
                        nd = topPoints[topIndex + 1];
                    }
                    else
                    {
                        st = topPoints[topIndex];
                        nd = topPoints[0];
                    }

                    res = theta - (topIndex * dTop);
                    t = 0;
                    if (res > 0)
                    {
                        t = res / dTop;
                    }
                    if (t < 0 || t > 1)
                    {
                    }
                    px = st.X + (t * (nd.X - st.X));
                    py = st.Y + (t * (nd.Y - st.Y));
                    topShell.Add(new Point(px, py));
                }
                vertices.Clear();

                int numUp = 10;
                vertices.Add(new Point3D(0, 0, 0));
                for (int i = 0; i < bottomShell.Count; i++)
                {
                    double dt = 1.0 / numUp;

                    for (int j = 0; j <= numUp; j++)
                    {
                        double t = j * dt;

                        double lx1 = bottomShell[i].X + (t * (topShell[i].X - bottomShell[i].X));
                        double ly1 = bottomShell[i].Y + (t * (topShell[i].Y - bottomShell[i].Y));

                        Point3D v = new Point3D(lx1 * sizeX / 2, t * sizeY / 2, ly1 * sizeZ / 2);
                        vertices.Add(v);
                    }
                }
                vertices.Add(new Point3D(0, 1 * sizeY / 2, 0));
                int topPoint = vertices.Count - 1;
                faces.Clear();
                int offset = numUp + 1;
                int f = 1;
                for (int i = 0; i < bottomShell.Count - 1; i++)
                {
                    f = i * offset + 1;
                    faces.Add(0);
                    faces.Add(f);
                    faces.Add(f + offset);

                    for (int j = 0; j < numUp; j++)
                    {
                        faces.Add(f);
                        faces.Add(f + offset + 1);
                        faces.Add(f + offset);

                        faces.Add(f);

                        faces.Add(f + 1);
                        faces.Add(f + offset + 1);
                        f++;
                    }
                    faces.Add(f);
                    faces.Add(topPoint);
                    faces.Add(f + offset);
                }

                mesh = new MeshGeometry3D();
                mesh.Positions = vertices;
                mesh.TriangleIndices = faces;
                mesh.Normals = null;
                GeometryModel3D gm = new GeometryModel3D();
                gm.Geometry = mesh;

                DiffuseMaterial mt = new DiffuseMaterial();
                mt.Color = Colors.Pink;
                mt.Brush = new SolidColorBrush(Colors.Pink);
                gm.Material = mt;
                DiffuseMaterial mtb = new DiffuseMaterial();
                mtb.Color = Colors.CornflowerBlue;
                mtb.Brush = new SolidColorBrush(Colors.CornflowerBlue);
                gm.BackMaterial = mtb;
                MyModelGroup.Children.Clear();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void SizeX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeX = Convert.ToDouble(SizeX.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void SizeY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeY = Convert.ToDouble(SizeY.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void SizeZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeZ = Convert.ToDouble(SizeZ.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopShape.Header = "Top Shape";
            TopShape.OnPointsChanged += OnTopPointsChanged;

            BottomShape.Header = "Bottom Shape";
            BottomShape.OnPointsChanged += OnBottomPointsChanged;

            camMain.Position = new Point3D(-20, 10, -1);
            camMain.LookDirection = new Vector3D(-camMain.Position.X, -camMain.Position.Y, -camMain.Position.Z);
            MyModelGroup.Children.Clear();
            SizeX.Text = sizeX.ToString();
            SizeY.Text = sizeY.ToString();
            SizeZ.Text = sizeZ.ToString();
        }
    }
}