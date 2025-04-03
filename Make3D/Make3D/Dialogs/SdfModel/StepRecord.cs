using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public class StepRecord : INotifyPropertyChanged
    {
        public StepsChanged OnStepsChanged;
        private double blend;
        private Int32Collection faces;
        private Color meshColour;
        private string opType;
        private Point3D position;
        private string primitiveType;
        private List<String> primTypes;
        private double radius;
        private double rotX;
        private double rotY;
        private double rotZ;
        private bool selected;
        private Visibility showBlend;
        private Visibility showRadius;
        private Visibility showRot;
        private Visibility showSize;
        private Visibility showThickness;
        private Vector3D size;
        private double sizeX;
        private double sizeY;
        private double sizeZ;
        private double thickness;

        private Point3DCollection vertices;

        public StepRecord()
        {
            Position = new Point3D(0, 0, 0);
            SizeX = 20;
            SizeY = 20;
            SizeZ = 20;
            RotX = 0;
            RotY = 0;
            RotY = 0;
            Blend = 3;
            Thickness = 3;
            Radius = 10;
            ShowSize = Visibility.Visible;
            ShowRot = Visibility.Visible;
            primTypes = new List<string>();
            primTypes.Add("Box");
            primTypes.Add("Cylinder");
            primTypes.Add("Sphere");
            primTypes.Add("Torus");
            primTypes.Add("Triangle");
            OpTypes = new ObservableCollection<string>();
            OpTypes.Add("Union");
            OpTypes.Add("Subtraction");
            OpTypes.Add("Intersection");
            OpTypes.Add("Xor");
            OpTypes.Add("Smooth Union");
            OpTypes.Add("Smooth Subtraction");
            OpTypes.Add("Smooth Intersection");
            OnStepsChanged = null;
            vertices = new Point3DCollection();
            faces = new Int32Collection();
            meshColour = Color.FromArgb(200, 255, 0, 0);
            Selected = false;
        }

        public delegate void StepsChanged();

        public event PropertyChangedEventHandler PropertyChanged;

        public double Blend
        {
            get
            {
                return blend;
            }
            set
            {
                if (value != blend)
                {
                    blend = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public int Id
        {
            get;
            set;
        }

        public String OpType
        {
            get
            {
                return opType;
            }
            set
            {
                if (opType != value)
                {
                    opType = value;
                    if (Id == 0)
                    {
                        opType = "";
                    }
                    if (opType.Contains("Smooth"))
                    {
                        ShowBlend = Visibility.Visible;
                    }
                    else
                    {
                        ShowBlend = Visibility.Collapsed;
                    }
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public ObservableCollection<String> OpTypes
        {
            get; set;
        }

        public Point3D Position
        {
            get
            {
                return position;
            }
            set
            {
                if (position != value)
                {
                    position = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String PrimitiveType
        {
            get
            {
                return primitiveType;
            }
            set
            {
                if (primitiveType != value)
                {
                    primitiveType = value;
                    switch (primitiveType.ToLower())
                    {
                        case "box":
                            {
                                ShowSize = Visibility.Visible;
                                ShowRadius = Visibility.Collapsed;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                            }
                            break;

                        case "sphere":
                            {
                                ShowSize = Visibility.Collapsed;
                                ShowRadius = Visibility.Visible;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Collapsed;
                            }
                            break;

                        case "torus":
                            {
                                ShowSize = Visibility.Collapsed;
                                ShowRadius = Visibility.Visible;
                                ShowThickness = Visibility.Visible;
                                ShowRot = Visibility.Visible;
                            }
                            break;

                        case "cylinder":
                            {
                                ShowSize = Visibility.Visible;
                                ShowRadius = Visibility.Collapsed;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                            }
                            break;

                        case "Triangle":
                            {
                                ShowSize = Visibility.Visible;
                                ShowRadius = Visibility.Collapsed;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                            }
                            break;
                    }

                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public List<String> PrimTypes
        {
            get
            {
                return primTypes;
            }
            set
            {
                if (primTypes != value)
                {
                    primTypes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value != radius)
                {
                    radius = value;
                    sizeX = radius * 2;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double RotX
        {
            get
            {
                return rotX;
            }
            set
            {
                if (value != rotX)
                {
                    rotX = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double RotY
        {
            get
            {
                return rotY;
            }
            set
            {
                if (value != rotY)
                {
                    rotY = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double RotZ
        {
            get
            {
                return rotZ;
            }
            set
            {
                if (value != rotZ)
                {
                    rotZ = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    if (selected)
                    {
                        GenerateModel();
                    }
                    else
                    {
                        vertices.Clear();
                        faces.Clear();
                    }
                }
            }
        }

        public Visibility ShowBlend
        {
            get
            {
                return showBlend;
            }
            set
            {
                if (showBlend != value)
                {
                    showBlend = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowRadius
        {
            get
            {
                return showRadius;
            }
            set
            {
                if (showRadius != value)
                {
                    showRadius = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowRot
        {
            get
            {
                return showRot;
            }
            set
            {
                if (showRot != value)
                {
                    showRot = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowSize
        {
            get
            {
                return showSize;
            }
            set
            {
                if (showSize != value)
                {
                    showSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowThickness
        {
            get
            {
                return showThickness;
            }
            set
            {
                if (showThickness != value)
                {
                    showThickness = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double SizeX
        {
            get
            {
                return sizeX;
            }
            set
            {
                if (value != sizeX)
                {
                    sizeX = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double SizeY
        {
            get
            {
                return sizeY;
            }
            set
            {
                if (value != sizeY)
                {
                    sizeY = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double SizeZ
        {
            get
            {
                return sizeZ;
            }
            set
            {
                if (value != sizeZ)
                {
                    sizeZ = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (value != thickness)
                {
                    thickness = value;
                    NotifyPropertyChanged();
                    PostChanges();
                }
            }
        }

        public GeometryModel3D GenerateModel()
        {
            GeometryModel3D res = null;
            bool flip = false;
            vertices.Clear();
            faces.Clear();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            switch (primitiveType.ToLower())
            {
                case "box":
                    {
                        Object3DLib.PrimitiveGenerator.GenerateCube(ref points, ref faces, ref normals);
                    }
                    break;

                case "sphere":
                    {
                        Object3DLib.PrimitiveGenerator.GenerateSphere(ref points, ref faces, ref normals);
                    }
                    break;

                case "torus":
                    {
                        Object3DLib.PrimitiveGenerator.GenerateTorus(ref points, ref faces, ref normals);
                        flip = true;
                    }
                    break;

                case "cylinder":
                    {
                        Object3DLib.PrimitiveGenerator.GenerateCylinder(ref points, ref faces, ref normals);
                    }
                    break;

                case "triangle":
                    {
                        Object3DLib.PrimitiveGenerator.GenerateRoof(ref points, ref faces, ref normals);
                        flip = true;
                    }
                    break;
            }
            foreach (Point3D p in points)
            {
                Point3D np;
                if (flip)
                {
                    np = new Point3D((p.X * sizeX * 0.8) + Position.X, (p.Z * sizeY * 1.1) + Position.Y + SizeX / 2, (p.Y * sizeX * 1.1) + Position.Z);
                }
                else
                {
                    np = new Point3D((p.X * sizeX * 1.001) + Position.X, (p.Y * sizeY * 1.001) + Position.Y, (p.Z * sizeZ * 1.001) + Position.Z);
                }
                vertices.Add(np);
            }
            res = GetModel();
            return res;
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            string s = PrimitiveType + ",";
            s += Position.X.ToString() + ",";
            s += Position.Y.ToString() + ",";
            s += Position.Z.ToString() + ",";
            s += sizeX.ToString() + ",";
            s += sizeY.ToString() + ",";
            s += sizeZ.ToString() + ",";
            s += rotX.ToString() + ",";
            s += rotY.ToString() + ",";
            s += rotZ.ToString() + ",";
            s += Thickness.ToString();
            if (OpType != null)
            {
                s += "," + OpType + ",";
            }
            else
            {
                s += ",null,";
            }
            s += Blend.ToString();
            return s;
        }

        internal void Move(int v2, int v3, int v4)
        {
            Position += new Vector3D(v2, v3, v4);
        }

        protected virtual GeometryModel3D GetModel()
        {
            MeshGeometry3D mesh = null;

            GeometryModel3D gm = null;

            if (Selected && vertices != null && vertices.Count >= 3 && faces != null && faces.Count >= 3)
            {
                mesh = new MeshGeometry3D();
                mesh.Positions = vertices;
                mesh.TriangleIndices = faces;
                mesh.Normals = null;
                gm = new GeometryModel3D();
                gm.Geometry = mesh;

                DiffuseMaterial mt = new DiffuseMaterial();
                mt.Color = meshColour;
                mt.Brush = new SolidColorBrush(meshColour);
                gm.Material = mt;
                DiffuseMaterial mtb = new DiffuseMaterial();
                mtb.Color = Colors.CornflowerBlue;
                mtb.Brush = new SolidColorBrush(Colors.Green);
                gm.BackMaterial = mtb;
            }
            return gm;
        }

        private void PostChanges()
        {
            if (OnStepsChanged != null)
            {
                OnStepsChanged();
            }
        }
    }
}