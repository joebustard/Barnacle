using Make3D.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Make3D.ViewModels
{
    internal class ObjectPropertiesViewModel : BaseViewModel, INotifyPropertyChanged
    {
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
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(v, p.Y, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
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
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, v, p.Z);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
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
                        Point3D p = selectedObject.Position;
                        Point3D p2 = new Point3D(p.X, p.Y, v);
                        selectedObject.Position = p2;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

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
                    if (selectedObject.Scale.X != v)
                    {
                        Scale3D p = selectedObject.Scale;
                        Scale3D p2 = new Scale3D(v, p.Y, p.Z);
                        selectedObject.Scale = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
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
                    if (selectedObject.Scale.Y != v)
                    {
                        Scale3D p = selectedObject.Scale;
                        Scale3D p2 = new Scale3D(p.X, v, p.Z);
                        selectedObject.Scale = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
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
                    if (selectedObject.Scale.Z != v)
                    {
                        Scale3D p = selectedObject.Scale;
                        Scale3D p2 = new Scale3D(p.X, p.Y, v);
                        selectedObject.Scale = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

        public String RotationX
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Rotation.X.ToString("F3");
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
                    if (selectedObject.Rotation.X != v)
                    {
                        Point3D p = selectedObject.Rotation;
                        Point3D p2 = new Point3D(v, p.Y, p.Z);
                        selectedObject.Rotation = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

        public String RotationY
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Rotation.Y.ToString("F3");
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
                    if (selectedObject.Rotation.Y != v)
                    {
                        Point3D p = selectedObject.Rotation;
                        Point3D p2 = new Point3D(p.X, v, p.Z);
                        selectedObject.Rotation = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

        public String RotationZ
        {
            get
            {
                if (selectedObject != null)
                {
                    return selectedObject.Rotation.Z.ToString("F3");
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
                    if (selectedObject.Rotation.Z != v)
                    {
                        Point3D p = selectedObject.Rotation;
                        Point3D p2 = new Point3D(p.X, p.Y, v);
                        selectedObject.Rotation = p2;
                        selectedObject.Remesh();
                        NotifyPropertyChanged();
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

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

        private Object3D selectedObject;

        public ICommand MoveToFloorCommand { get; set; }
        public ICommand MoveToCentreCommand { get; set; }
        public ObjectPropertiesViewModel()
        {
            selectedObject = null;

            MoveToFloorCommand = new RelayCommand(OnMoveToFloor);
            MoveToCentreCommand = new RelayCommand(OnMoveToCentre);
            NotificationManager.Subscribe("ObjectSelected", OnObjectSelected);
        }

        private void OnMoveToFloor(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToFloor", selectedObject);
            }
        }

        private void OnMoveToCentre(object obj)
        {
            if (selectedObject != null)
            {
                NotificationManager.Notify("MoveObjectToCentre", selectedObject);
            }
        }
        private void OnObjectSelected(object param)
        {
            selectedObject = param as Object3D;
            NotifyPropertyChanged("PositionX");
            NotifyPropertyChanged("PositionY");
            NotifyPropertyChanged("PositionZ");

            NotifyPropertyChanged("ScaleX");
            NotifyPropertyChanged("ScaleY");
            NotifyPropertyChanged("ScaleZ");

            NotifyPropertyChanged("RotationX");
            NotifyPropertyChanged("RotationY");
            NotifyPropertyChanged("RotationZ");
        }
    }
}