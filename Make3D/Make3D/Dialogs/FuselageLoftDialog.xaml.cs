using Make3D.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for FuselageLoftDialog.xaml
    /// </summary>
    public partial class FuselageLoftDialog : Window, INotifyPropertyChanged
    {
        public double bezierStep = 0.05;

        //private ObservableCollection<FuselageBulkhead> bulkHeads;
        private ObservableCollection<BulkheadControl> bulkHeads;

        private Int32Collection faces;
        private MeshGeometry3D mesh;
        private Point3DCollection vertices;
        private PolarCamera polarCamera;
        private Point oldMousePos;
        private string filter = "Fuselage Files (*.fsl)|*.fsl";

        public FuselageLoftDialog()
        {
            InitializeComponent();
            DataContext = this;
            vertices = new Point3DCollection();
            faces = new Int32Collection();
            BulkHeads = new ObservableCollection<BulkheadControl>();
            polarCamera = new PolarCamera(100);
            DataContext = this;
            selectedBulkhead = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<BulkheadControl> BulkHeads
        {
            get { return bulkHeads; }
            set
            {
                if (bulkHeads != value)
                {
                    bulkHeads = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Int32Collection GetFaces()
        {
            return faces;
        }

        public Point3DCollection GetVertices()
        {
            return vertices;
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PrependBulkhead_Click(object sender, RoutedEventArgs e)
        {
            foreach (BulkheadControl c in bulkHeads)
            {
                c.IdNumber++;
                c.BulkHead.OffsetX += 5;
            }

            FuselageBulkhead fb = new FuselageBulkhead();
            fb.Depth = 40;
            fb.Height = 40;
            fb.OffsetX = 0;

            BulkheadControl bhc = new BulkheadControl();
            bhc.BulkHead = fb;
            bhc.OnPerformAction += PerformAction;
            bhc.IdNumber = 1;
            bulkHeads.Insert(0, bhc);
            bhc.CopyToNextButton.Visibility = Visibility.Visible;

            foreach (BulkheadControl c in bulkHeads)
            {
                c.Redraw();
            }
            NotifyPropertyChanged("BulkHeads");
        }

        private void InsertBulkhead_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBulkhead != -1)
            {
                // if they click insert while on the last one
                if (selectedBulkhead == bulkHeads.Count)
                {
                    AddBulkhead_Click(sender, e);
                }
                else
                {
                    double distance = bulkHeads[selectedBulkhead].GetDistance() - bulkHeads[selectedBulkhead - 1].GetDistance();
                    FuselageBulkhead fb = new FuselageBulkhead();
                    fb.Depth = 40;
                    fb.Height = 40;
                    fb.OffsetX = bulkHeads[selectedBulkhead - 1].GetDistance() + distance / 2;

                    BulkheadControl bhc = new BulkheadControl();
                    bhc.BulkHead = fb;
                    bhc.OnPerformAction += PerformAction;
                    bhc.IdNumber = bulkHeads.Count + 1;
                    bulkHeads.Insert(selectedBulkhead, bhc);
                    bhc.CopyToNextButton.Visibility = Visibility.Visible;

                    for (int i = 0; i < bulkHeads.Count; i++)
                    {
                        bulkHeads[i].IdNumber = i + 1;
                        bulkHeads[i].Redraw();
                    }
                    NotifyPropertyChanged("BulkHeads");
                }
            }
        }

        private void AddBulkhead_Click(object sender, RoutedEventArgs e)
        {
            double distance = bulkHeads[bulkHeads.Count - 1].GetDistance() + 5;
            FuselageBulkhead fb = new FuselageBulkhead();
            fb.Depth = 40;
            fb.Height = 40;
            fb.OffsetX = distance;
            bulkHeads[bulkHeads.Count - 1].CopyToNextButton.Visibility = Visibility.Visible;
            BulkheadControl bhc = new BulkheadControl();
            bhc.BulkHead = fb;
            bhc.OnPerformAction += PerformAction;
            bhc.IdNumber = bulkHeads.Count + 1;
            bulkHeads.Add(bhc);
            bhc.CopyToNextButton.Visibility = Visibility.Hidden;
            NotifyPropertyChanged("BulkHeads");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CopyOn(int i)
        {
            i = i - 1;
            if (i >= 0 && i < bulkHeads.Count - 1)
            {
                List<Point> pnts = bulkHeads[i].Points;
                List<Point> ctrls = bulkHeads[i].ControlPoints;
                List<double> dists = bulkHeads[i].PointDistance;

                bulkHeads[i + 1].Set(pnts, ctrls, dists);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PerformAction(int id, string cmd)
        {
            if (cmd == "CopyToNext")
            {
                CopyOn(id);
            }
            else if (cmd == "Refresh")
            {
                if (bulkHeads.Count > 1)
                {
                    Redraw();
                }
            }
            else if (cmd == "BulkheadFocused")
            {
                selectedBulkhead = id;
            }
        }

        private void Redraw()
        {
            if (bulkHeads != null && bulkHeads.Count > 0 && bulkHeads[bulkHeads.Count - 1].BzLines[0] != null)
            {
                vertices.Clear();
                double radius = 5;
                // create a place to store all the points from the bulkheads
                List<Point>[] blkPoints = new List<Point>[bulkHeads.Count];
                for (int i = 0; i < bulkHeads.Count; i++)
                {
                    blkPoints[i] = new List<Point>();
                }

                double x = 0;
                int facesPerBulkhead = 0;
                vertices.Add(new Point3D(0, 0, 0));
                // calculate the points for each bulk head
                for (int blk = 0; blk < bulkHeads.Count; blk++)
                {
                    facesPerBulkhead = 0;
                    x = bulkHeads[blk].BulkHead.OffsetX;

                    for (int line = 0; line < bulkHeads[blk].BzLines.GetLength(0); line++)
                    {
                        double lim = 1;
                        if (line == 3)
                        {
                            lim += bezierStep;
                        }
                        for (double t = 0; t < lim; t += bezierStep)
                        {
                            Point p = bulkHeads[blk].BzLines[line].GetCoord(t);
                            blkPoints[blk].Add(p);
                            vertices.Add(new Point3D(x, -p.Y * radius, p.X * radius));
                            facesPerBulkhead++;
                        }
                    }
                }

                int right = vertices.Count;
                vertices.Add(new Point3D(x, 0, 0));
                int f = 0;
                int first = f;
                for (int blk = 0; blk < bulkHeads.Count - 1; blk++)
                {
                    f = (blk * facesPerBulkhead) + 1;
                    first = f;
                    for (int index = 0; index < facesPerBulkhead - 1; index++)
                    {
                        faces.Add(f);
                        faces.Add(f + facesPerBulkhead + 1);
                        faces.Add(f + facesPerBulkhead);

                        faces.Add(f);
                        faces.Add(f + 1);
                        faces.Add(f + facesPerBulkhead + 1);

                        f++;
                    }

                    faces.Add(f);
                    faces.Add(first + facesPerBulkhead);
                    faces.Add(f + facesPerBulkhead);

                    faces.Add(f);
                    faces.Add(first);
                    faces.Add(first + facesPerBulkhead);
                }

                for (int i = 0; i < facesPerBulkhead - 1; i++)
                {
                    faces.Add(0);
                    faces.Add(i + 1);
                    faces.Add(i);
                }

                faces.Add(0);
                faces.Add(1);
                faces.Add(facesPerBulkhead - 1);

                f = first + facesPerBulkhead;
                for (int i = 0; i < facesPerBulkhead - 1; i++)
                {
                    faces.Add(right);
                    faces.Add(f);
                    faces.Add(f + 1);

                    f++;
                }
                CentreVertices();
                SetMesh();
            }
        }

        private void CentreVertices()
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(vertices, ref min, ref max);

            double scaleX = max.X - min.X;

            double scaleY = max.Y - min.Y;

            double scaleZ = max.Z - min.Z;

            double midx = min.X + (scaleX / 2);
            double midy = min.Y + (scaleY / 2);
            double midz = min.Z + (scaleZ / 2);
            Vector3D offset = new Vector3D(-midx, -midy, -midz);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] += offset;
            }
        }

        private void SetMesh()
        {
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FuselageBulkhead fb = new FuselageBulkhead();
            fb.Depth = 20;
            fb.Height = 20;
            fb.OffsetX = 0;
            BulkheadControl bhc = new BulkheadControl();
            bhc.BulkHead = fb;
            bhc.IdNumber = 1;
            bhc.CopyToNextButton.Visibility = Visibility.Visible;
            bhc.OnPerformAction += PerformAction;
            bulkHeads.Add(bhc);
            bhc.Redraw();
            fb = new FuselageBulkhead();
            fb.Depth = 20;
            fb.Height = 20;
            fb.OffsetX = 5;
            bhc = new BulkheadControl();
            bhc.BulkHead = fb;
            bhc.CopyToNextButton.Visibility = Visibility.Hidden;
            bhc.OnPerformAction += PerformAction;
            bulkHeads.Add(bhc);
            bhc.IdNumber = 2;
            bhc.Redraw();
            NotifyPropertyChanged("BulkHeads");

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            DataContext = this;
        }

        private void Viewport3D1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point pn = e.GetPosition(viewport3D1);
                double dx = pn.X - oldMousePos.X;
                double dy = pn.Y - oldMousePos.Y;
                polarCamera.Move(dx, -dy);
                UpdateCameraPos();
                oldMousePos = pn;
            }
        }

        private void Viewport3D1_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                oldMousePos = e.GetPosition(viewport3D1);
            }
        }

        private void Viewport3D1_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double diff = Math.Sign(e.Delta) * 1;
            polarCamera.Zoom(diff);
            UpdateCameraPos();
        }

        private void UpdateCameraPos()
        {
            lookDirection.X = -polarCamera.CameraPos.X;
            lookDirection.Y = -polarCamera.CameraPos.Y;
            lookDirection.Z = -polarCamera.CameraPos.Z;
            lookDirection.Normalize();
            camMain.Position = new Point3D(polarCamera.CameraPos.X, polarCamera.CameraPos.Y, polarCamera.CameraPos.Z);
            camMain.LookDirection = new Vector3D(lookDirection.X, lookDirection.Y, lookDirection.Z);
        }

        public Point3D CameraPos
        {
            get { return polarCamera.CameraPos; }
            set
            {
                NotifyPropertyChanged();
            }
        }

        private Vector3D lookDirection;
        public Vector3D LookDirection
        {
            get { return lookDirection; }
            set
            {
                if (lookDirection != value)
                {
                    lookDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private int selectedBulkhead;

 

        private void LoadBulkhead_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = filter;
            if (dlg.ShowDialog() == true)
            {
                string f = dlg.FileName;
                Read(f);
            }
            this.InvalidateVisual();
            foreach (BulkheadControl bk in bulkHeads)
            {
                bk.InvalidateVisual();
            }
        }

        private void Read(string f)
        {
            try
            {
                ObservableCollection<BulkheadControl> tmp = new ObservableCollection<BulkheadControl>();
                XmlDocument doc = new XmlDocument();
                doc.Load(f);

                XmlNode docNode = doc.SelectSingleNode("Fuselage");
                XmlNodeList nodeList = docNode.SelectNodes("Bulkhead");
                foreach (XmlNode nd in nodeList)
                {
                    BulkheadControl bhc = new BulkheadControl();
                    bhc.Read(doc, nd);
                    bhc.OnPerformAction += PerformAction;
                    tmp.Add(bhc);
                }
                BulkHeads = tmp;
                NotifyPropertyChanged("BulkHeads");
                Redraw();
            }
            catch
            {
            }
        }

        private void SaveBulkhead_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = filter;
            if (dlg.ShowDialog() == true)
            {
                string f = dlg.FileName;
                Write(f);
            }
        }

        private void Write(string f)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement docNode = doc.CreateElement("Fuselage");
            foreach (BulkheadControl ob in bulkHeads)
            {
                ob.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(f);
        }

        private void TopBulkhead_Click(object sender, RoutedEventArgs e)
        {
            foreach (BulkheadControl ob in bulkHeads)
            {
                ob.MoveToTop();
            }
            NotifyPropertyChanged("BulkHeads");
            Redraw();
        }
    }
}