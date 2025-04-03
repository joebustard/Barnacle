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
        private Visibility showTriangle;
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
            ShowTriangle = Visibility.Collapsed;
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
                                ShowTriangle = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                            }
                            break;

                        case "sphere":
                            {
                                ShowSize = Visibility.Collapsed;
                                ShowRadius = Visibility.Visible;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Collapsed;
                                ShowTriangle = Visibility.Collapsed;
                            }
                            break;

                        case "torus":
                            {
                                ShowSize = Visibility.Collapsed;
                                ShowRadius = Visibility.Visible;
                                ShowThickness = Visibility.Visible;
                                ShowRot = Visibility.Visible;
                                ShowTriangle = Visibility.Collapsed;
                            }
                            break;

                        case "cylinder":
                            {
                                ShowSize = Visibility.Visible;
                                ShowRadius = Visibility.Collapsed;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                                ShowTriangle = Visibility.Collapsed;
                            }
                            break;

                        case "triangle":
                            {
                                ShowSize = Visibility.Collapsed;
                                ShowRadius = Visibility.Collapsed;
                                ShowThickness = Visibility.Collapsed;
                                ShowRot = Visibility.Visible;
                                ShowTriangle = Visibility.Visible;
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

        public Visibility ShowTriangle
        {
            get
            {
                return showTriangle;
            }
            set
            {
                if (showTriangle != value)
                {
                    showTriangle = value;
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

        private void PostChanges()
        {
            if (OnStepsChanged != null)
            {
                OnStepsChanged();
            }
        }
    }
}