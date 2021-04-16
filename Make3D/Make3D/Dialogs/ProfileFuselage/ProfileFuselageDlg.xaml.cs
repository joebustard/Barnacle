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
        public List<LetterMarker> markers;
        private bool backBody;
        private bool dirty;
        private Int32Collection faces;
        private string filePath;
        private bool frontBody;
        private MeshGeometry3D mesh;
        private NoteWindow noteWindow;

        private int numberOfDivisions;

        private string sideViewFilename;
        private string topViewFilename;
        private Point3DCollection vertices;
        private bool wholeBody;

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
            numberOfDivisions = 80;
        }

        public bool BackBody
        {
            get
            {
                return backBody;
            }
            set
            {
                if (backBody != value)
                {
                    backBody = value;
                    NotifyPropertyChanged();
                    if (backBody)
                    {
                        RibManager.ModelType = 1;
                    }

                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public bool FrontBody
        {
            get
            {
                return frontBody;
            }
            set
            {
                if (frontBody != value)
                {
                    frontBody = value;
                    NotifyPropertyChanged();
                    if (frontBody)
                    {
                        RibManager.ModelType = 2;
                    }
                    GenerateSkin();
                    Redisplay();
                }
            }
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

        public int NumberOfDivisions
        {
            get
            {
                return numberOfDivisions;
            }
            set
            {
                if (numberOfDivisions != value)
                {
                    if ((value >= 10) && (value <= 200))
                    {
                        numberOfDivisions = value;
                        RibManager.NumberOfProfilePoints = numberOfDivisions;
                        GenerateSkin();
                        Redisplay();
                        NotifyPropertyChanged();
                    }
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

        public bool WholeBody
        {
            get
            {
                return wholeBody;
            }
            set
            {
                if (wholeBody != value)
                {
                    wholeBody = value;
                    if (wholeBody)
                    {
                        RibManager.ModelType = 0;
                    }
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public void MarkerMoved(string s, System.Drawing.Point p, bool finishedMove)
        {
            dirty = true;
            TopView.SetMarker(s, p.X);
            SideView.SetMarker(s, p.X);
            SortRibs();
            UpdateDisplay(finishedMove);
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
                                if (TopView.IsValid)
                                {
                                    TopView.UpdateLayout();
                                    UpdateLimits();
                                    dirty = true;
                                }
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
                                if (SideView.IsValid)
                                {
                                    SideView.UpdateLayout();
                                    UpdateLimits();
                                }
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
                                noteWindow.Owner = this;
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
            int nextY = 10;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position.X >= nextX)
                {
                    nextX = mk.Position.X + 10;
                }
                nextY = 40 - nextY;
            }
            CreateLetter(name, new System.Drawing.Point(nextX, nextY), rc);
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
            int nextX = 10;
            int nextY = 10;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position.X >= nextX)
                {
                    nextX = mk.Position.X + 10;
                }
                nextY = 40 - nextY;
            }
            CreateLetter(name, new System.Drawing.Point(nextX, nextY), rc);

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
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void Bottom_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeBottom();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void CopyLetter(string name)
        {
            RibManager.CopyARib(name);
        }

        private void CreateLetter(string v1, System.Drawing.Point v2, RibControl rib)
        {
            LetterMarker mk = new LetterMarker(v1, v2);
            mk.Rib = rib;
            markers.Add(mk);
        }

        private void Front_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeFront();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void GenerateSkin()
        {
            try
            {
                // clear out existing 3d model
                Faces.Clear();
                Vertices.Clear();

                // do we have enough data to construct the model
                if (RibManager.Ribs.Count > 1 && TopView.IsValid && SideView.IsValid)
                {
                    // there should be a marker and hence a dimension for every rib.
                    // If ther isn't then somethins wrong
                    if (RibManager.Ribs.Count != TopView.Dimensions.Count)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ribs {RibManager.Ribs.Count} TopView Dimensions {TopView.Dimensions.Count}");
                    }
                    else
                    {
                        // work out the range of faces we are going to do based upon whether we
                        // are doing the whole model or just fron or back
                        int facesPerRib = RibManager.Ribs[0].ProfilePoints.Count;

                        // assume its whole model
                        int start = 0;
                        int end = RibManager.Ribs[0].ProfilePoints.Count;

                        double x = TopView.GetXmm(markers[0].Position.X);
                        List<PointF> leftEdge = new List<PointF>();
                        double leftx = x;
                        List<PointF> rightEdge = new List<PointF>();
                        double rightx = x;
                        for (int i = 0; i < RibManager.Ribs.Count; i++)
                        {
                            System.Diagnostics.Debug.WriteLine($"Rib {i}");
                            x = TopView.GetXmm(markers[i].Position.X);
                            if (i == RibManager.Ribs.Count - 1)
                            {
                                rightx = x;
                            }

                            for (int proind = start; proind < end; proind++)
                            {
                                if (proind < RibManager.Ribs[i].ProfilePoints.Count)
                                {
                                    PointF pnt = RibManager.Ribs[i].ProfilePoints[proind];
                                    System.Diagnostics.Debug.WriteLine($"proind {proind} {pnt.X},{pnt.Y}");
                                    double v = pnt.X * TopView.Dimensions[i].Height / 2;
                                    double z = TopView.GetYmm(v + TopView.Dimensions[i].Mid.Y);

                                    v = pnt.Y * SideView.Dimensions[i].Height / 2;
                                    double y = -SideView.GetYmm((v + SideView.Dimensions[i].Mid.Y));

                                    AddVertice(x, y, z);
                                    if (i == 0)
                                    {
                                        leftEdge.Add(new PointF((float)y, (float)z));
                                    }
                                    if (i == RibManager.Ribs.Count - 1)
                                    {
                                        rightEdge.Add(new PointF((float)y, (float)z));
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"ERROR proind {proind} ProfilePoints Count {RibManager.Ribs[i].ProfilePoints.Count}");
                                }
                            }
                        }
                        facesPerRib = leftEdge.Count;

                        int first = 0;
                        int g = 0;
                        int h = 0;

                        System.Diagnostics.Debug.WriteLine("Starting faces");
                        for (int blk = 0; blk < RibManager.Ribs.Count - 1; blk++)
                        {
                            first = (blk * facesPerRib);

                            for (int index = 0; index < facesPerRib; index++)
                            {
                                g = first + index;
                                h = g + 1;
                                if (index == facesPerRib - 1)
                                {
                                    h = first;
                                }
                                Faces.Add(g);

                                Faces.Add(g + facesPerRib);
                                Faces.Add(h + facesPerRib);

                                Faces.Add(g);
                                Faces.Add(h + facesPerRib);
                                Faces.Add(h);
                            }

                            Faces.Add(first);
                            Faces.Add(first + facesPerRib);
                            Faces.Add(first + facesPerRib);

                            Faces.Add(first);
                            Faces.Add(first + facesPerRib);
                            Faces.Add(first);
                        }

                        TriangulatePerimiter(leftEdge, leftx, 0, 0, true);
                        TriangulatePerimiter(rightEdge, rightx, 0, 0, false);
                        CentreVertices();
                    }
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

        private async void LoadEditorParameters()
        {
            string s = EditorParameters.Get("Path");
            if (s != "")
            {
                filePath = s;
                noteWindow.Owner = this;
                noteWindow.Show();

                RibManager.ControlsEnabled = false;
                await Read(filePath);
                noteWindow.Hide();
                dirty = false;
                RibManager.ControlsEnabled = true;

                s = EditorParameters.Get("Model");
                if (s == "Whole")
                {
                    WholeBody = true;
                }
                else if (s == "Front")
                {
                    FrontBody = true;
                }
                else
                if (s == "Back")
                {
                    BackBody = true;
                }
                s = EditorParameters.Get("NumberOfDivisions");
                if (s != "")
                {
                    NumberOfDivisions = Convert.ToInt16(s);
                }
            }
        }

        private async Task<bool> LoadRib(XmlElement el, string pth, string nme, System.Drawing.Point position)
        {
            bool res = true;
            try
            {
                RibControl rc = new RibControl();
                rc.ImagePath = pth;
                rc.Header = nme;

                rc.FetchImage();
                if (rc.IsValid)
                {
                    rc.ClearSinglePixels();
                    rc.FindEdge();
                    rc.GenerateProfilePoints(0);
                    rc.SetImageSource();
                    rc.OnForceReload = RibManager.OnForceRibReload;
                    CreateLetter(nme, position, rc);
                    RibManager.Ribs.Add(rc);
                }
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
            int nextY = 10;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].Rib == after)
                {
                    if (i < markers.Count - 1)
                    {
                        nextX = markers[i].Position.X + (markers[i + 1].Position.X - markers[i].Position.X) / 2;
                    }
                    else
                    {
                        nextX = markers[i].Position.X + 10;
                    }
                }
                nextY = 40 - nextY;
            }
            if (nextX == 0 && markers.Count > 0)
            {
                nextX = markers[markers.Count - 1].Position.X + 10;
            }
            CreateLetter(name, new System.Drawing.Point(nextX, nextY), rc);
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
            TopView.UpdateDisplay();
            XmlElement sideNode = docNode.SelectSingleNode("Side") as XmlElement;
            SideView.ImageFilePath = sideNode.GetAttribute("Path");
            SideView.UpdateDisplay();

            RibManager.Ribs.Clear();
            Markers.Clear();
            XmlNodeList nodes = docNode.SelectNodes("Rib");
            int nextY = 10;
            foreach (XmlNode nd in nodes)
            {
                XmlElement el = nd as XmlElement;
                string pth = el.GetAttribute("Path");
                string nme = el.GetAttribute("Header");
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                noteWindow.Message = "Loading Rib " + nme;
                // noteWindow.Activate();
                noteWindow.Refresh();
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                System.Drawing.Point ribPos;
                int position = Convert.ToInt16(el.GetAttribute("Position"));
                ribPos = new System.Drawing.Point(position, nextY);
                Task ribber = LoadRib(el, pth, nme, ribPos);

                await ribber;
                nextY = 40 - nextY;
            }

            SortRibs();
            // need to update the top and side views BEFORE generating skin
            TopView.UpdateDisplay();
            SideView.UpdateDisplay();
            UpdateLimits();
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
                filePath = saveFileDialog.FileName;
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
            string md = "Whole";
            if (FrontBody)
            {
                md = "Front";
            }
            else
            {
                md = "Back";
            }
            EditorParameters.Set("Model", md);
            EditorParameters.Set("NumberOfDivisions", numberOfDivisions.ToString());
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
                        if (markers[i].Position.X > markers[i + 1].Position.X)
                        {
                            LetterMarker mk = markers[i];
                            markers[i] = markers[i + 1];
                            markers[i + 1] = mk;
                            swapped = true;
                        }
                    }
                } while (swapped);
                int nextY = 10;
                foreach (LetterMarker mk in markers)
                {
                    mk.Position = new System.Drawing.Point(mk.Position.X, nextY);
                    nextY = 40 - nextY;
                }
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
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void TriangulatePerimiter(List<PointF> points, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(xo, yo + t.Points[0].X, z + t.Points[0].Y);
                int c1 = AddVertice(xo, yo + t.Points[1].X, z + t.Points[1].Y);
                int c2 = AddVertice(xo, yo + t.Points[2].X, z + t.Points[2].Y);
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

        private void UpdateDisplay(bool regenSkin = true)
        {
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            if (regenSkin)
            {
                GenerateSkin();
            }
            Redisplay();
            NotifyPropertyChanged("CameraPos");
        }

        private void UpdateLimits()
        {
            if (TopView.IsValid && SideView.IsValid)
            {
                double l = TopView.LeftLimit;
                if (SideView.LeftLimit > l)
                {
                    l = SideView.LeftLimit;
                }

                double r = TopView.RightLimit;
                if (SideView.RightLimit < r)
                {
                    r = SideView.RightLimit;
                }

                TopView.LeftLimit = l;
                SideView.LeftLimit = l;
                TopView.RightLimit = r;
                SideView.RightLimit = r;
            }
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
                        ob.Write(doc, docNode, mk.Position.X, mk.Letter);
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