using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.ViewModels
{
    class ObjectPropertiesViewModel : BaseViewModel, INotifyPropertyChanged
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
                if ( selectedObject != null )
                {
                    double v = GetDouble(value);
                    if( selectedObject.Position.X != v)
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
        public ObjectPropertiesViewModel()
        {
            selectedObject = null;

            NotificationManager.Subscribe("ObjectSelected", OnObjectSelected);
        }

        private void OnObjectSelected(object param)
        {
            selectedObject = param as Object3D;
            NotifyPropertyChanged("PositionX");
            NotifyPropertyChanged("PositionY");
            NotifyPropertyChanged("PositionZ");
        }
    }
}
