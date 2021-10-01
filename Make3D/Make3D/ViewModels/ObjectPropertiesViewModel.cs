using Barnacle.Models;
using Barnacle.Object3DLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal class ObjectPropertiesViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private bool canScale;

        private bool exportable;
        private Color objectColour;

        private string objectName;

        private double percentScale;

        private double rotationX;

        private double rotationY;

        private double rotationZ;

        private Object3D selectedObject;

        public ObjectPropertiesViewModel()
        {
            selectedObject = null;
            RotateXCommand = new RelayCommand(OnRotateX);
            RotateYCommand = new RelayCommand(OnRotateY);
            RotateZCommand = new RelayCommand(OnRotateZ);
            SetRotationCommand = new RelayCommand(OnSetRotation);
            ScaleByPercentCommand = new RelayCommand(OnScaleByPercent);
            MoveToFloorCommand = new RelayCommand(OnMoveToFloor);
            MoveToCentreCommand = new RelayCommand(OnMoveToCentre);
            MoveToZeroCommand = new RelayCommand(OnMoveToZero);
            NudgeCommand = new RelayCommand(OnNudge);
            NotificationManager.Subscribe("ObjectSelected", OnObjectSelected);
            NotificationManager.Subscribe("ScaleUpdated", OnScaleUpdated);
            NotificationManager.Subscribe("PositionUpdated", OnPositionUpdated);
            rotationX = 90;
            rotationY = 90;
            rotationZ = 90;
            PercentScale = 1;
            CanScale = false;
        }

        public bool CanScale
        {
            get
            {
                return canScale;
            }
            set
            {
                if (value != canScale)
                {
                    canScale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Exportable
        {
            get
            {
                return exportable;
            }
            set
            {
                if (exportable != value)
                {
                    exportable = value;
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        selectedObject.Exportable = exportable;
                        Document.Dirty = true;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand MoveToCentreCommand { get; set; }
        public ICommand MoveToFloorCommand { get; set; }
        public ICommand MoveToZeroCommand { get; set; }
        public ICommand NudgeCommand { get; set; }

        public Color ObjectColour
        {
            get
            {
                return objectColour;
            }
            set
            {
                if (objectColour != value)
                {
                    objectColour = value;
                    NotifyPropertyChanged();
                    if (selectedObject != null)
                    {
                        CheckPoint();
                        selectedObject.Color = objectColour;
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String ObjectName
        {
            get
            {
                return objectName;
            }
            set
            {
                if (objectName != value)
                {
                    objectName = value;
                    NotifyPropertyChanged();
                    if (selectedObject != null)
                    {
                        if (selectedObject.Name != value)
                        {
                            CheckPoint();
                            selectedObject.Name = value;
                            Document.Dirty = true;
                            NotificationManager.Notify("ObjectNamesChanged", null);
                        }
                    }
                }
            }
        }

        public double PercentScale
        {
            get
            {
                return percentScale;
            }
            set
            {
                if (percentScale != value)
                {
                    percentScale = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public String PositionX
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.X.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.X != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(v, p.Y, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String PositionY
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.Y.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.Y != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, v, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String PositionZ
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Position.Z.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    if (selectedObject.Position.Z != v)
                    {
                        CheckPoint();
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, p.Y, v);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public ICommand RotateXCommand { get; set; }
        public ICommand RotateYCommand { get; set; }
        public ICommand RotateZCommand { get; set; }

        public double RotationX
        {
            get
            {
                return rotationX;
            }

            set
            {
                if (rotationX != value)
                {
                    rotationX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double RotationY
        {
            get
            {
                return rotationY;
            }

            set
            {
                if (rotationY != value)
                {
                    rotationY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double RotationZ
        {
            get
            {
                return rotationZ;
            }

            set
            {
                if (rotationZ != value)
                {
                    rotationZ = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ScaleByPercentCommand { get; set; }

        public String ScaleX
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.X.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.X != v && p.X != 0 && v != 0)
                    {
                        CheckPoint();
                        Scale3D p2 = new Scale3D(v, p.Y, p.Z);
                        double d = v / p.X;
                        selectedObject.Scale = p2;
                        selectedObject.ScaleMesh(d, 1.0, 1.0);
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String ScaleY
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.Y.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.Y != v && p.Y != 0 && v != 0)
                    {
                        CheckPoint();
                        Scale3D p2 = new Scale3D(p.X, v, p.Z);
                        double d = v / p.Y;
                        selectedObject.Scale = p2;
                        selectedObject.ScaleMesh(1.0, d, 1.0);
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public String ScaleZ
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Scale.Z.ToString("F3");
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (selectedObject != null)
                {
                    double v = GetDouble(value);
                    Scale3D p = selectedObject.Scale;
                    if (p.Z != v && p.Z != 0 && v != 0)
                    {
                        CheckPoint();
                        Scale3D p2 = new Scale3D(p.X, p.Y, v);
                        double d = v / p.Z;
                        selectedObject.Scale = p2;
                        selectedObject.ScaleMesh(1.0, 1.0, d);
                        selectedObject.Remesh();
                        NotifyPropertyChanged();

                        NotificationManager.Notify("ScaleRefresh", selectedObject);
                        NotificationManager.Notify("RefreshAdorners", null);
                        Document.Dirty = true;
                    }
                }
            }
        }

        public ICommand SetRotationCommand { get; set; }

        private double GetDouble(string value)
        {
            double res = 0.0;
            try
            {
                res = Convert.ToDouble(value);
            }
            catch
            {
            }
            return res;
        }

        private void OnMoveToCentre(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToCentre", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnMoveToFloor(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToFloor", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnMoveToZero(object obj)
        {
            if (selectedObject.Position.X != 0.0 || selectedObject.Position.Y != 0.0 || selectedObject.Position.Z != 0.0)
            {
                CheckPoint();

                Point3D p2 = new Point3D(0, 0, 0);
                selectedObject.Position = p2;
                NotifyPropertyChanged();
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
            }
        }

        private void OnNudge(object obj)
        {
            Point3D d = new Point3D(0, 0, 0);
            string s = obj as string;
            if (s != null)
            {
                switch (s)
                {
                    case "X+":
                        {
                            d.X = 1;
                        }
                        break;

                    case "X-":
                        {
                            d.X = -1;
                        }
                        break;

                    case "Y+":
                        {
                            d.Y = 1;
                        }
                        break;

                    case "Y-":
                        {
                            d.Y = -1;
                        }
                        break;

                    case "Z+":
                        {
                            d.Z = 1;
                        }
                        break;

                    case "Z-":
                        {
                            d.Z = -1;
                        }
                        break;
                }
                CheckPoint();
                Point3D p = selectedObject.Position;
                Point3D p2 = new Point3D(p.X + d.X, p.Y + d.Y, p.Z + d.Z);
                selectedObject.Position = p2;
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
            }
        }

        private void OnObjectSelected(object param)
        {
            selectedObject = param as Object3D;
            if (selectedObject == null)
            {
                //objectColour = new SolidColorBrush(Colors.Transparent);
                objectColour = Colors.White;
                objectName = "";
                exportable = false;
            }
            else
            {
                objectColour = selectedObject.Color;
                objectName = selectedObject.Name;
                CanScale = selectedObject.IsSizable();
                exportable = selectedObject.Exportable;
            }
            NotifyPropertyChanged("PositionX");
            NotifyPropertyChanged("PositionY");
            NotifyPropertyChanged("PositionZ");

            NotifyPropertyChanged("ScaleX");
            NotifyPropertyChanged("ScaleY");
            NotifyPropertyChanged("ScaleZ");

            NotifyPropertyChanged("RotationX");
            NotifyPropertyChanged("RotationY");
            NotifyPropertyChanged("RotationZ");
            NotifyPropertyChanged("ObjectColour");
            NotifyPropertyChanged("ObjectName");
            NotifyPropertyChanged("Exportable");
        }

        private void OnPositionUpdated(object param)
        {
            Object3D o = param as Object3D;
            if (o != null && o == selectedObject)
            {
                NotifyPropertyChanged("PositionX");
                NotifyPropertyChanged("PositionY");
                NotifyPropertyChanged("PositionZ");

                NotifyPropertyChanged("ScaleX");
                NotifyPropertyChanged("ScaleY");
                NotifyPropertyChanged("ScaleZ");
            }
        }

        private void OnRotateX(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(rotationX, 0, 0);
            }
            else
            {
                p2 = new Point3D(-rotationX, 0, 0);
            }
            RotateSelected(p2);
        }

        private void OnRotateY(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(0, rotationY, 0);
            }
            else
            {
                p2 = new Point3D(0, -rotationY, 0);
            }
            RotateSelected(p2);
        }

        private void OnRotateZ(object obj)
        {
            Point3D p2;
            if (obj.ToString() == "+")
            {
                p2 = new Point3D(0, 0, rotationZ);
            }
            else
            {
                p2 = new Point3D(0, 0, -rotationZ);
            }
            RotateSelected(p2);
        }

        private void OnScaleByPercent(object obj)
        {
            if (selectedObject != null)
            {
                double v = 100.0;
                if (obj.ToString() == "+")
                {
                    v += percentScale;
                }
                else
                {
                    v -= percentScale;
                }
                if (v > 0)
                {
                    v = ScaleBy(v);
                }
                else
                {
                    MessageBox.Show("Scale Invalid. Operation ignored", "Warning");
                }
            }
        }

        private void OnScaleUpdated(object param)
        {
            NotifyPropertyChanged("ScaleX");
            NotifyPropertyChanged("ScaleY");
            NotifyPropertyChanged("ScaleZ");
        }

        private void OnSetRotation(object obj)
        {
            string v = obj.ToString();
            double d = Convert.ToDouble(v);
            rotationX = d;
            rotationY = d;
            rotationZ = d;
            NotifyPropertyChanged("RotationX");
            NotifyPropertyChanged("RotationY");
            NotifyPropertyChanged("RotationZ");
        }

        private void RotateSelected(Point3D p2)
        {
            if (selectedObject != null)
            {
                CheckPoint();
                selectedObject.Rotate(p2);
                selectedObject.Remesh();

                NotifyPropertyChanged();
                NotificationManager.Notify("ScaleRefresh", selectedObject);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
            }
        }

        private double ScaleBy(double v)
        {
            v = v / 100.0;
            CheckPoint();

            selectedObject.ScaleMesh(v, v, v);
            selectedObject.Remesh();
            selectedObject.CalcScale(false);
            NotifyPropertyChanged();

            NotificationManager.Notify("ScaleRefresh", selectedObject);
            NotificationManager.Notify("RefreshAdorners", null);
            OnScaleUpdated(null);
            Document.Dirty = true;
            return v;
        }
    }
}