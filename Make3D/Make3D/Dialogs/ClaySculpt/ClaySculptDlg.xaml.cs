using asdflibrary;
using MakerLib;
using sdflib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ClaySculpt.xaml
    /// </summary>
    public partial class ClaySculptDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxtoolsSize = 10;
        private const double maxtoolStrength = 10;
        private const double mintoolsSize = 1;
        private const double mintoolStrength = 1;
        private GeometryModel3D clayModel;
        private bool claySelected;
        private bool symetric;
        private bool loaded;
        private int maxX = 132;
        private int maxY = 132;
        private int maxZ = 132;
        private Isdf sdf;
        private Isdf tool;
        private bool toolInverse;
        private string toolShape;
        private ObservableCollection<String> toolShapeItems;
        private double toolsSize;
        private double toolStrength;

        public bool Symetric
        {
            get { return symetric; }
            set
            {
                if (symetric = value)
                {
                    symetric = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SymetricToolTip
        {
            get
            {
                return "When a change is made to one side of the model, apply it to the other side automaticaly";
            }
        }

        private string warningText;

        public ClaySculptDlg()
        {
            InitializeComponent();
            ToolName = "ClaySculpt";
            DataContext = this;
            meshColour = Colors.Brown;
            ModelGroup = MyModelGroup;
            loaded = false;
            sdf = new Sdf();
            sdf.SetDimension(maxX, maxY, maxZ);
            SetSphere(sdf, maxX / 2, maxY / 2, maxZ / 2, maxX / 8);
            tool = new Sdf();
            tool.SetDimension(11, 11, 11);
            SetSphere(tool, 5, 5, 5, 4);
            op = 0;
            symetric = true;
            ShowFloor = false;
            ToolShapeItems = new ObservableCollection<string>();
            ToolShapeItems.Add("Inflate");
            ToolShapeItems.Add("Smooth");
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

        public bool ToolInverse
        {
            get
            {
                return toolInverse;
            }
            set
            {
                if (toolInverse != value)
                {
                    toolInverse = value;
                    if (toolInverse)
                    {
                        op = 1;
                    }
                    else
                    {
                        op = 0;
                    }
                    NotifyPropertyChanged();
                    // UpdateDisplay();
                }
            }
        }

        public String ToolInverseToolTip
        {
            get
            {
                return $"Invert the behaviour of the tool.";
            }
        }

        public string ToolShape
        {
            get { return toolShape; }
            set
            {
                if (toolShape != value)
                {
                    toolShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> ToolShapeItems
        {
            get { return toolShapeItems; }
            set
            {
                if (toolShapeItems != value)
                {
                    toolShapeItems = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String toolShapeToolTip
        {
            get { return "ToolShape Text"; }
        }

        public double ToolsSize
        {
            get
            {
                return toolsSize;
            }
            set
            {
                if (toolsSize != value)
                {
                    if (value >= mintoolsSize && value <= maxtoolsSize)
                    {
                        toolsSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ToolsSizeToolTip
        {
            get
            {
                return $"ToolsSize must be in the range {mintoolsSize} to {maxtoolsSize}";
            }
        }

        public double ToolStrength
        {
            get
            {
                return toolStrength;
            }
            set
            {
                if (toolStrength != value)
                {
                    if (value >= mintoolStrength && value <= maxtoolStrength)
                    {
                        toolStrength = value;
                        strength = toolStrength / 10.0;
                        if (tool != null)
                        {
                            SetSphere(tool, 5, 5, 5, 4 * strength);
                        }
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        public String ToolStrengthToolTip
        {
            get
            {
                return $"ToolStrength must be in the range {mintoolStrength} to {maxtoolStrength}";
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

        protected override void Redisplay()
        {
            if (ModelGroup != null)
            {
                ModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    ModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                clayModel = GetModel();
                ModelGroup.Children.Add(clayModel);
            }
        }

        protected override void Viewport_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    lastHitModel = null;

                    oldMousePos = e.GetPosition(vp);
                    HitTest(vp, oldMousePos);
                    if (lastHitModel == clayModel)
                    {
                        claySelected = true;
                    }

                    if (floor.Matches(lastHitModel) || grid.Matches(lastHitModel))
                    {
                        claySelected = false;
                    }
                }
                else
                    if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    oldMousePos = e.GetPosition(vp);
                }
            }
        }

        protected override void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                Point pn = e.GetPosition(vp);
                HitTest(vp, pn);
                double dx = pn.X - oldMousePos.X;
                double dy = pn.Y - oldMousePos.Y;
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    if (claySelected)
                    {
                        if (dx < 1 || dx < 1)
                        {
                            if (lastHitPoint != null)
                            {
                                sdf.Perform(tool, (int)(lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                                if (symetric)
                                {
                                    sdf.Perform(tool, (int)(-lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                                }
                            }
                        }
                        else
                        {
                            for (double t = 0; t <= 1; t += 0.1)

                            {
                                double px = oldMousePos.X + t * dx;
                                double py = oldMousePos.Y + t * dy;
                                HitTest(vp, new Point(px, py));
                                if (lastHitPoint != null)
                                {
                                    sdf.Perform(tool, (int)(lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                                    if (symetric)
                                    {
                                        sdf.Perform(tool, (int)(-lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Camera.Move(dx, -dy);
                        UpdateCameraPos();
                    }
                    oldMousePos = pn;
                    e.Handled = true;
                }
                else
                if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    Camera.Move(dx, -dy);
                    UpdateCameraPos();
                    oldMousePos = pn;
                    e.Handled = true;
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            CubeMarcher cm = new CubeMarcher();
            GridCell gc = new GridCell();
            List<Triangle> triangles = new List<Triangle>();

            double dd = 0.5;
            double cx = maxX / 2;
            double cy = maxY / 2;
            double cz = maxZ / 2;
            for (double x = 0; x < maxX - 1; x += dd)
            {
                for (double y = 0; y < maxY - 1; y += dd)
                {
                    for (double z = 0; z < maxZ - 1; z += dd)
                    {
                        if (x + dd < maxX - 1 && y + dd < maxY - 1 && z + dd < maxZ - 1)
                        {
                            gc.p[0] = new XYZ(x, y, z);

                            gc.p[1] = new XYZ(x + dd, y, z);
                            gc.p[2] = new XYZ(x + dd, y, z + dd);
                            gc.p[3] = new XYZ(x, y, z + dd);
                            gc.p[4] = new XYZ(x, y + dd, z);
                            gc.p[5] = new XYZ(x + dd, y + dd, z);
                            gc.p[6] = new XYZ(x + dd, y + dd, z + dd);
                            gc.p[7] = new XYZ(x, y + dd, z + dd);

                            for (int i = 0; i < 8; i++)
                            {
                                gc.val[i] = sdf.GetAt(gc.p[i].x, gc.p[i].y, gc.p[i].z);
                            }
                            triangles.Clear();

                            cm.Polygonise(gc, 0, triangles);

                            foreach (Triangle t in triangles)
                            {
                                int p0 = AddVertice(t.p[0].x - cx, t.p[0].y - cy, t.p[0].z - cz);
                                int p1 = AddVertice(t.p[1].x - cx, t.p[1].y - cy, t.p[1].z - cz);
                                int p2 = AddVertice(t.p[2].x - cx, t.p[2].y - cy, t.p[2].z - cz);

                                Faces.Add(p0);
                                //  Faces.Add(p2);
                                // Faces.Add(p1);

                                Faces.Add(p1);
                                Faces.Add(p2);
                            }
                        }
                    }
                }
            }
            //  CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            ToolShape = EditorParameters.Get("ToolShape");

            ToolsSize = EditorParameters.GetDouble("ToolsSize", 30);

            ToolStrength = EditorParameters.GetDouble("ToolStrength", 5);

            ToolInverse = EditorParameters.GetBoolean("ToolInverse", false);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("ToolShape", ToolShape);
            EditorParameters.Set("ToolsSize", ToolsSize.ToString());
            EditorParameters.Set("ToolStrength", ToolStrength.ToString());
            EditorParameters.Set("ToolInverse", ToolInverse.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            ToolShape = "Inflate";
            ToolsSize = 30;
            ToolStrength = 5;
            ToolInverse = false;

            loaded = true;
        }

        private void SetSphere(Isdf sdf, int cx, int cy, int cz, double radius)
        {
            int l = 0;
            int h = 0;
            int w = 0;
            sdf.GetDimensions(ref l, ref h, ref w);
            for (int x = 0; x < l; x++)
            {
                double dx = x - cx;

                for (int y = 0; y < h; y++)
                {
                    double dy = y - cy;
                    for (int z = 0; z < w; z++)
                    {
                        double dz = z - cz;
                        double v = (dx * dx) + (dy * dy) + (dz * dz);
                        double d = Math.Sqrt(v) - (double)radius;
                        sdf.Set(x, y, z, d);
                    }
                }
            }
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private int op;
        private double strength;

        private void Viewport_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (claySelected)
            {
                if (lastHitPoint != null && e.LeftButton == System.Windows.Input.MouseButtonState.Released)
                {
                    sdf.Perform(tool, (int)(lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                    if (symetric)
                    {
                        sdf.Perform(tool, (int)(-lastHitPoint.X) + maxX / 2, maxY / 2 + (int)(lastHitPoint.Y), (int)(lastHitPoint.Z) + maxZ / 2, op, strength);
                    }
                    // sdf.Union(tool, maxX / 2 + 1, maxY / 2 + 1, maxZ / 2 + 1);
                    UpdateDisplay();
                }
                claySelected = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();
            meshColour = Colors.Chocolate;
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            ToolShape = ToolShapeItems[0];
            loaded = true;
            UpdateDisplay();
        }
    }
}