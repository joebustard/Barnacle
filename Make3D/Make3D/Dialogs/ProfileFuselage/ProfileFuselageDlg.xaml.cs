using Make3D.Models;
using Microsoft.Win32;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class ProfileFuselageDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool wholeBody;

        public bool WholeBody
        {
            get { return wholeBody; }
            set
            {
                if (wholeBody != value)
                {
                    wholeBody = value;
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        private bool frontBody;

        public bool FrontBody
        {
            get { return frontBody; }
            set
            {
                if (frontBody != value)
                {
                    frontBody = value;
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        private bool backBody;

        public bool BackBody
        {
            get { return backBody; }
            set
            {
                if (backBody != value)
                {
                    backBody = value;
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public List<LetterMarker> markers;

        private bool dirty;

        private Int32Collection faces;

        private string filePath;

        private MeshGeometry3D mesh;

        private NoteWindow noteWindow;

        //  private PolarCamera polarCamera;
        private System.Windows.Point oldMousePos;

        private string sideViewFilename;

        private string topViewFilename;

        private Point3DCollection vertices;

        private double zoomLevel;

        public ProfileFuselageDlg()
        {
            InitializeComponent();
            ToolName = "ProfileFuselage";
            DataContext = this;
            faces = new Int32Collection();
            mesh = new MeshGeometry3D();
            vertices = new Point3DCollection();
            WholeBody = true;
            zoomLevel = 1;
            dirty = false;
            filePath = "";
            noteWindow = new NoteWindow();
        }

        public List<LetterMarker> Markers
        {
            get
            {
                return markers;
            }
            set
            {
                if (markers != value)
                {
                    markers = value;
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

        public void MarkerMoved(string s, int x)
        {
            dirty = true;
            TopView.SetMarker(s, x);
            SideView.SetMarker(s, x);
            SortRibs();
            UpdateDisplay();
        }

        public async void OnCommand(string com)
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            switch (com)
            {
                case "LoadTop":
                    {
                        opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                topViewFilename = opDlg.FileName;
                                TopView.ImageFilePath = topViewFilename;
                                TopView.UpdateLayout();
                                dirty = true;
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;

                case "LoadSide":
                    {
                        opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                sideViewFilename = opDlg.FileName;
                                SideView.ImageFilePath = sideViewFilename;
                                SideView.UpdateLayout();
                                dirty = true;
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;

                case "SaveProject":
                    {
                        SaveProject();
                    }
                    break;

                case "LoadProject":
                    {
                        opDlg.Filter = "Fusalage spar files (*.spr) | *.spr";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                filePath = opDlg.FileName;
                                noteWindow.Show();
                                RibManager.ControlsEnabled = false;
                                await Read(filePath);
                                noteWindow.Hide();
                                dirty = false;
                                RibManager.ControlsEnabled = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                RibManager.ControlsEnabled = true;
                                noteWindow.Hide();
                            }
                        }
                    }
                    break;
            }
        }

        public void OnRibAdded(string name, RibControl rc)
        {
            int nextX = 0;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position >= nextX)
                {
                    nextX = mk.Position + 10;
                }
            }
            CreateLetter(name, nextX, rc);
            TopView.AddRib(name);
            SideView.AddRib(name);
            dirty = true;
        }

        public void OnRibDeleted(RibControl rc)
        {
            LetterMarker target = null;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Rib == rc)
                {
                    target = mk;
                    break;
                }
            }
            if (target != null)
            {
                markers.Remove(target);
            }
            TopView.DeleteMarker(rc);
            SideView.DeleteMarker(rc);
            dirty = true;
        }

        public void OnRibInserted(string name, RibControl rc)
        {
            int nextX = 0;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position >= nextX)
                {
                    nextX = mk.Position + 10;
                }
            }
            CreateLetter(name, nextX, rc);

            TopView.AddRib(name);
            SideView.AddRib(name);
            UpdateDisplay();
            dirty = true;
        }

        public void PinMoved(int x)
        {
            TopView.PinPos = x;
            SideView.PinPos = x;
            dirty = true;
        }

        protected override void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            noteWindow.Close();
            Close();
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (dirty)
            {
                MessageBoxResult res = MessageBox.Show("Profile has changed. Do you want to save it?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    SaveProject();
                }
            }
            SaveEditorParmeters();
            DialogResult = true;
            noteWindow.Close();
            Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeBack();
            Camera.Distance = 2 * Bounds.Depth;
            UpdateCameraPos();
        }

        private void Bottom_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeBottom();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private void CopyLetter(string name)
        {
            RibManager.CopyARib(name);
        }

        private void CreateLetter(string v1, int v2, RibControl rib)
        {
            LetterMarker mk = new LetterMarker(v1, v2);
            mk.Rib = rib;
            markers.Add(mk);
        }

        private void Front_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeFront();
            Camera.Distance = 2 * Bounds.Depth;
            UpdateCameraPos();
        }

        private void GenerateSkin()
        {
            try
            {
                Faces.Clear();
                Vertices.Clear();
                if (RibManager.Ribs.Count > 1 && TopView.IsValid && SideView.IsValid)
                {
                    int facesPerRib = RibManager.Ribs[0].ProfilePoints.Count;
                    int start = 0;
                    int end = RibManager.Ribs[0].ProfilePoints.Count;
                    if (backBody)
                    {
                        end = end / 2;
                        facesPerRib = facesPerRib / 2;
                    }
                    if (frontBody)
                    {
                        end = RibManager.Ribs[0].ProfilePoints.Count;
                        start = end / 2;
                        facesPerRib = facesPerRib / 2;
                    }
                    double x = TopView.GetXmm(markers[0].Position);
                    List<PointF> leftEdge = new List<PointF>();
                    double leftx = x;
                    List<PointF> rightEdge = new List<PointF>();
                    double rightx = x;
                    for (int i = 0; i < RibManager.Ribs.Count; i++)
                    {
                        x = TopView.GetXmm(markers[i].Position);
                        if (i == RibManager.Ribs.Count - 1)
                        {
                            rightx = x;
                        }

                        for (int proind = start; proind < end; proind++)
                        {
                            PointF pnt = RibManager.Ribs[i].ProfilePoints[proind];
                            //double z = TopView.GetYmm(pnt.X * TopView.Dimensions[i].Height / 2);
                            // double y = SideView.GetYmm((pnt.Y * SideView.Dimensions[i].Height / 2));
                            double v = pnt.X * TopView.Dimensions[i].Height / 2;
                            double z = TopView.GetYmm(v + TopView.Dimensions[i].Mid.Y);

                            v = pnt.Y * SideView.Dimensions[i].Height / 2;
                            double y = SideView.GetYmm((v + SideView.Dimensions[i].Mid.Y));
                            // y += SideView.GetYmm(SideView.Dimensions[i].Mid.Y);
                            AddVertice(x, z, y);
                            if (i == 0)
                            {
                                leftEdge.Add(new PointF((float)y, (float)z));
                            }
                            if (i == RibManager.Ribs.Count - 1)
                            {
                                rightEdge.Add(new PointF((float)y, (float)z));
                            }
                        }
                    }

                    // int right = Vertices.Count;
                    // AddVertice(x, -SideView.GetYmm(SideView.Dimensions[SideView.Dimensions.Count - 1].Mid.Y), 0);
                    int f = 0;
                    int first = f;

                    for (int blk = 0; blk < RibManager.Ribs.Count - 1; blk++)
                    {
                        f = (blk * facesPerRib);
                        first = f;
                        for (int index = 0; index < facesPerRib - 1; index++)
                        {
                            Faces.Add(f);

                            Faces.Add(f + facesPerRib);
                            Faces.Add(f + facesPerRib + 1);

                            Faces.Add(f);
                            Faces.Add(f + facesPerRib + 1);
                            Faces.Add(f + 1);

                            f++;
                        }

                        Faces.Add(f);
                        Faces.Add(f + facesPerRib);
                        Faces.Add(first + facesPerRib);

                        Faces.Add(f);
                        Faces.Add(first + facesPerRib);
                        Faces.Add(first);
                    }

                    TriangulatePerimiter(leftEdge, leftx, 0, 0, false);
                    TriangulatePerimiter(rightEdge, rightx, 0, 0, true);
                    CentreVertices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GenerateSkin");
                throw ex;
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeLeft();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private void LoadEditorParameters()
        {
            string s = EditorParameters.Get("Path");
            if (s != "")
            {
                filePath = s;
                Read(filePath);
            }
        }

        private async Task<bool> LoadRib(XmlElement el, string pth, string nme, int position)
        {
            bool res = true;
            try
            {
                RibControl rc = new RibControl();
                rc.ImagePath = pth;
                rc.Header = nme;

                XmlNode pnts = el.SelectSingleNode("EdgeDUMMY");
                if (pnts != null)
                {
                    rc.FetchImage();
                    rc.ProfilePoints.Clear();
                    double len = Convert.ToDouble((pnts as XmlElement).GetAttribute("EdgeLength"));
                    rc.EdgeLength = len;
                    XmlNodeList ndl = pnts.SelectNodes("V");
                    foreach (XmlNode pn in ndl)
                    {
                        XmlElement pe = pn as XmlElement;
                        float x = (float)Convert.ToDouble(pe.GetAttribute("X"));
                        float y = (float)Convert.ToDouble(pe.GetAttribute("Y"));
                        PointF f = new PointF(x, y);
                        rc.ProfilePoints.Add(f);
                    }
                }
                else
                {
                    rc.FetchImage();
                    rc.ClearSinglePixels();
                    rc.FindEdge();
                    rc.SetImageSource();
                }
                CreateLetter(nme, position, rc);
                RibManager.Ribs.Add(rc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadRib");
            }

            return res;
        }

        private void OnRibInserted(string name, RibControl rc, RibControl after)
        {
            int nextX = 0;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].Rib == after)
                {
                    if (i < markers.Count - 1)
                    {
                        nextX = markers[i].Position + (markers[i + 1].Position - markers[i].Position) / 2;
                    }
                    else
                    {
                        nextX = markers[i].Position + 10;
                    }
                }
            }
            if (nextX == 0 && markers.Count > 0)
            {
                nextX = markers[markers.Count - 1].Position + 10;
            }
            CreateLetter(name, nextX, rc);
            TopView.AddRib(name);
            SideView.AddRib(name);
            UpdateDisplay();
            dirty = true;
        }

        private void OnRibsRenamed(List<RibManager.NameRec> newNames)
        {
            foreach (LetterMarker mk in markers)
            {
                foreach (RibManager.NameRec rc in newNames)
                {
                    if (mk.Letter == rc.originalName)
                    {
                        mk.Letter = rc.newName;
                        break;
                    }
                }
            }
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            dirty = true;
        }

        private async Task Read(string fileName)
        {
            this.Cursor = Cursors.Wait;

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode docNode = doc.SelectSingleNode("Spars");
            XmlElement ele = docNode as XmlElement;
            string s = ele.GetAttribute("NextLetter");
            RibManager.NextNameLetter = s[0];
            s = ele.GetAttribute("NextNumber");
            RibManager.NextNameNumber = Convert.ToInt32(s);
            XmlElement topNode = docNode.SelectSingleNode("Top") as XmlElement;
            TopView.ImageFilePath = topNode.GetAttribute("Path");
            XmlElement sideNode = docNode.SelectSingleNode("Side") as XmlElement;
            SideView.ImageFilePath = sideNode.GetAttribute("Path");
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            RibManager.Ribs.Clear();
            Markers.Clear();
            XmlNodeList nodes = docNode.SelectNodes("Rib");

            foreach (XmlNode nd in nodes)
            {
                XmlElement el = nd as XmlElement;
                string pth = el.GetAttribute("Path");
                string nme = el.GetAttribute("Header");
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                noteWindow.Message = "Loading Rib " + nme;
                noteWindow.Activate();
                noteWindow.Refresh();
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                int position = Convert.ToInt16(el.GetAttribute("Position"));

                Task ribber = LoadRib(el, pth, nme, position);
                await ribber;
            }

            SortRibs();
            // need to update the top and side views BEFORE generating skin
            TopView.UpdateDisplay();
            SideView.UpdateDisplay();
            GenerateSkin();
            Redisplay();
            noteWindow.Hide();
            this.Activate();
            this.Cursor = Cursors.Arrow;
        }

        private void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeRight();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Fusalage spar files (*.spr) | *.spr";
            if (saveFileDialog.ShowDialog() == true)
            {
                Write(saveFileDialog.FileName);
                dirty = false;
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            if (filePath != "")
            {
                EditorParameters.Set("Path", filePath);
            }
        }

        private void SaveProject()
        {
            if (filePath == "")
            {
                SaveAs();
            }
            else
            {
                Write(filePath);
                dirty = false;
            }
        }

        private void SortRibs()
        {
            try
            {
                bool swapped = false;
                do
                {
                    swapped = false;
                    for (int i = 0; i < markers.Count - 1; i++)
                    {
                        if (markers[i].Position > markers[i + 1].Position)
                        {
                            LetterMarker mk = markers[i];
                            markers[i] = markers[i + 1];
                            markers[i + 1] = mk;
                            swapped = true;
                        }
                    }
                } while (swapped);
                SideView.Markers = markers;
                TopView.Markers = markers;
                ObservableCollection<RibControl> ribs = new ObservableCollection<RibControl>();
                foreach (LetterMarker mk in markers)
                {
                    ribs.Add(mk.Rib);
                }
                RibManager.Ribs = ribs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SortRibs");
            }
        }

        private void Top_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeTop();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private void TriangulatePerimiter(List<PointF> points, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(xo, yo + t.Points[0].Y, z + t.Points[0].X);
                int c1 = AddVertice(xo, yo + t.Points[1].Y, z + t.Points[1].X);
                int c2 = AddVertice(xo, yo + t.Points[2].Y, z + t.Points[2].X);
                if (invert)
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

        private void UpdateDisplay()
        {
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            GenerateSkin();
            Redisplay();
            NotifyPropertyChanged("CameraPos");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            markers = new List<LetterMarker>();
            TopView.SetHeader("Top View");
            // TopView.ImageFilePath = "C:\\tmp\\109top.png";
            TopView.OnPinMoved = PinMoved;
            TopView.OnMarkerMoved = MarkerMoved;
            TopView.OnCopyLetter = CopyLetter;
            TopView.Markers = markers;
            SideView.SetHeader("Side View");
            // SideView.ImageFilePath = "C:\\tmp\\109side.png";
            SideView.OnPinMoved = PinMoved;
            SideView.OnMarkerMoved = MarkerMoved;
            SideView.Markers = markers;
            SideView.OnCopyLetter = CopyLetter;
            RibManager.OnRibAdded = OnRibAdded;
            RibManager.OnRibInserted = OnRibInserted;
            RibManager.OnCommandHandler = OnCommand;
            RibManager.OnRibsRenamed = OnRibsRenamed;
            RibManager.OnRibDeleted = OnRibDeleted;
            UpdateCameraPos();
            LoadEditorParameters();
            MyModelGroup.Children.Clear();

            Redisplay();
        }

        private void Write(string f)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement docNode = doc.CreateElement("Spars");
            docNode.SetAttribute("NextLetter", RibManager.NextNameLetter.ToString());
            docNode.SetAttribute("NextNumber", RibManager.NextNameNumber.ToString());
            XmlElement topNode = doc.CreateElement("Top");
            topNode.SetAttribute("Path", TopView.ImageFilePath);
            docNode.AppendChild(topNode);
            XmlElement sideNode = doc.CreateElement("Side");
            sideNode.SetAttribute("Path", SideView.ImageFilePath);
            docNode.AppendChild(sideNode);
            foreach (RibControl ob in RibManager.Ribs)
            {
                foreach (LetterMarker mk in markers)
                {
                    if (mk.Letter == ob.Header)
                    {
                        ob.Write(doc, docNode, mk.Position, mk.Letter);
                        break;
                    }
                }
            }
            doc.AppendChild(docNode);
            doc.Save(f);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel *= 1.1;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel *= 0.9;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel = 1;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }
    }
}