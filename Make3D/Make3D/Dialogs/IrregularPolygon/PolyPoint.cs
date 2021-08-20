using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Dialogs
{
    public class PolyPoint : INotifyPropertyChanged
    {
        private int id;

        private double x;

        private double y;

        public PolyPoint(double x, double y)
        {
            Id = -1;
            X = x;
            Y = y;
            Selected = false;
        }

        public PolyPoint(System.Windows.Point p)
        {
            Id = -1;
            X = p.X;
            Y = p.Y;
            Selected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                if (value != id)
                {
                    id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public System.Windows.Point Point
        {
            get { return new System.Windows.Point(X, Y); }
        }

        public bool Selected { get; set; }

        public double X
        {
            get
            {
                return x;
            }
            set
            {
                if (value != x)
                {
                    x = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                if (value != y)
                {
                    y = value;
                    NotifyPropertyChanged();
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
    }
}