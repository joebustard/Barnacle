using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Make3D.DialogsDialog.xaml
    /// </summary>
    public partial class LinearLoftDialog : Window

    {
        private List<Point> line;
        private int numDivisions = 36;
        private Point3DCollection pnts;
        private Int32Collection tris;
        private MeshGeometry3D mesh;

        public Point3DCollection GetVertices()
        {
            return pnts;
        }

        public Int32Collection GetFaces()
        {
            return tris;
        }

        public LinearLoftDialog()
        {
            InitializeComponent();

            pnts = new Point3DCollection();

            tris = new Int32Collection();
        }

        private void GenerateShape()
        {
            if (line != null)
            {
                int numSpokes = numDivisions;
                double deltaTheta = 360.0 / numSpokes; // in degrees
                Point3D[,] vertices = new Point3D[numSpokes, line.Count];
                for (int i = 0; i < line.Count; i++)
                {
                    vertices[0, i] = new Point3D(line[i].X*20, line[i].Y * 20, 0);
                }

                for (int i = 1; i < numSpokes; i++)
                {
                    double theta = i * deltaTheta;
                    double rad = Math.PI * theta / 180.0;
                    for (int j = 0; j < line.Count; j++)
                    {
                        double x = vertices[0, j].X * Math.Cos(rad) ;
                        double z = vertices[0, j].X * Math.Sin(rad)  ;
                        vertices[i, j] = new Point3D(x, vertices[0, j].Y, z);
                    }
                }
                pnts = new Point3DCollection();
                pnts.Add(new Point3D(0, 0, 0));
                for (int i = 0; i < numSpokes; i++)
                {
                    for (int j = 0; j < line.Count; j++)
                    {
                        pnts.Add(vertices[i, j]);
                    }
                }
                int topPoint = pnts.Count;
                pnts.Add(new Point3D(0, 20, 0));
                tris = new Int32Collection();
                int spOff;
                int spOff2;
                for (int i = 0; i < numSpokes; i++)
                {
                    spOff = i * line.Count + 1;
                    spOff2 = (i + 1) * line.Count + 1;
                    if ( i == numSpokes-1)
                    {
                        spOff2 = 1;
                    }
                    // base
                    tris.Add(0);
                    tris.Add(spOff);
                    tris.Add(spOff2);

                    for (int j = 0; j < line.Count - 1; j++)
                    {
                        tris.Add(spOff + j);
                        tris.Add(spOff2 + j + 1);
                        tris.Add(spOff2 + j);

                        tris.Add(spOff + j);
                        tris.Add(spOff + j + 1);
                        tris.Add(spOff2 + j + 1);
                    }

                    // Top

                    tris.Add(spOff + line.Count - 1);
                    tris.Add(topPoint);
                    tris.Add(spOff2 + line.Count - 1);
                }
                
                spOff = (numSpokes - 1) * line.Count + 1;
                spOff2 = 1;
                // base
                tris.Add(0);
                tris.Add(spOff);
tris.Add(spOff2);
                /*

                
                for (int j = 0; j < line.Count - 1; j++)
                {
                    tris.Add(spOff + j);
                    tris.Add(spOff2 + j + 1);
                    tris.Add(spOff2+ j);

                   // tris.Add(spOff + j);
                  //  tris.Add(spOff + j + 1);
                    // tris.Add(spOff2 + j + 1);
                }
                
                // Top

               tris.Add(spOff + line.Count - 1);
               tris.Add(topPoint);
               tris.Add(spOff2 + line.Count - 1);
               */
            }
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
                    writer.Write((float)V1.X);
                    writer.Write((float)V1.Y);
                    writer.Write((float)V1.Z);
                    //   System.Diagnostics.Debug.WriteLine($"V1 {triangle.V1.Position.X},{triangle.V1.Position.Y},{triangle.V1.Position.Z}");

                    writer.Write((float)V2.X);
                    writer.Write((float)V2.Y);
                    writer.Write((float)V2.Z);
                    //    System.Diagnostics.Debug.WriteLine($"V2 {triangle.V2.Position.X},{triangle.V2.Position.Y},{triangle.V2.Position.Z}");

                    writer.Write((float)V3.X);
                    writer.Write((float)V3.Y);
                    writer.Write((float)V3.Z);
                    //   System.Diagnostics.Debug.WriteLine($"V3 {triangle.V3.Position.X},{triangle.V3.Position.Y},{triangle.V3.Position.Z}");
                    writer.Write((ushort)0); // garbage value
                }

                writer.Flush();
                int size = 84 + (int)totalTriangles * 50;
                System.Diagnostics.Debug.WriteLine($"File should be {size} bytes");
            }
            stream.Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sl = sender as Slider;
            if (sl.Tag != null)
            {
                string s = sl.Tag.ToString();
                double y = Convert.ToDouble(s);
                for (int i = 0; i < line.Count; i++)
                {
                    if (line[i].Y == y)
                    {
                        line[i] = new Point(sl.Value, y);
                        break;
                    }
                }
                RedrawLine();
                GenerateShape();
                RedrawShape();
            }
        }

        private void RedrawShape()
        {
            if (line != null)
            {
                mesh = new MeshGeometry3D();
                mesh.Positions = pnts;
                mesh.TriangleIndices = tris;
                mesh.Normals = null;
                GeometryModel3D gm = new GeometryModel3D();
                gm.Geometry = mesh;

                DiffuseMaterial mt = new DiffuseMaterial();
                mt.Color = Colors.Pink;
                mt.Brush = new SolidColorBrush(Colors.CornflowerBlue);
                gm.Material = mt;
                DiffuseMaterial mtb = new DiffuseMaterial();
                mtb.Color = Colors.CornflowerBlue;
                mtb.Brush = new SolidColorBrush(Colors.Black);
                gm.BackMaterial = mtb;
                MyModelGroup.Children.Clear();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void RedrawLine()
        {
            LineCanvas.Children.Clear();
            double x1;
            double y1;
            double x2;
            double y2;
            for (int i = 0; i < line.Count - 1; i++)
            {
                x1 = line[i].X * LineCanvas.ActualWidth;
                y1 = (1 - line[i].Y) * LineCanvas.ActualHeight;

                x2 = line[i + 1].X * LineCanvas.ActualWidth;
                y2 = (1 - line[i + 1].Y) * LineCanvas.ActualHeight;
                Line ln = new Line();
                ln.X1 = x1;
                ln.Y1 = y1;
                ln.X2 = x2;
                ln.Y2 = y2;
                ln.Stroke = new SolidColorBrush(Colors.Black);
                LineCanvas.Children.Add(ln);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            camMain.Position = new Point3D(1, 1, 60);
            camMain.LookDirection = new Vector3D(-1, -1, -60);
            MyModelGroup.Children.Clear();
            line = new List<Point>();
            line.Add(new Point(0.5, 0));
            line.Add(new Point(0.5, 0.1));
            line.Add(new Point(0.5, 0.2));
            line.Add(new Point(0.5, 0.3));
            line.Add(new Point(0.5, 0.4));
            line.Add(new Point(0.5, 0.5));
            line.Add(new Point(0.5, 0.6));
            line.Add(new Point(0.5, 0.7));
            line.Add(new Point(0.5, 0.8));
            line.Add(new Point(0.5, 0.9));
            line.Add(new Point(0.5, 1));
            RedrawLine();
        }

        private void HDivSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            numDivisions = (int)e.NewValue;
            GenerateShape();
            RedrawShape();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}