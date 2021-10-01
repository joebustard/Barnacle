using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.Dialogs.Figure
{
    /// <summary>
    /// Interaction logic for ModelAssignmentControl.xaml
    /// </summary>
    public partial class ModelAssignmentControl : UserControl, INotifyPropertyChanged
    {
        private List<String> availableFigureNames;
        private String boneName;
        private String figureName;

        private double hScale;

        private double lScale;

        private double wScale;

        public ModelAssignmentControl()
        {
            InitializeComponent();
            availableFigureNames = new List<string>();
            DataContext = this;
            hScale = 1.0;
            wScale = 1.0;
            lScale = 1.0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> AvailableFigureNames
        {
            get
            {
                return availableFigureNames;
            }
            set
            {
                if (availableFigureNames != value)
                {
                    availableFigureNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String BoneName
        {
            get
            {
                return boneName;
            }
            set
            {
                if (boneName != value)
                {
                    boneName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String FigureName
        {
            get
            {
                foreach (String s in availableFigureNames)
                {
                    if (String.Compare(s.ToLower(), figureName) == 0)
                    {
                        return s;
                    }
                }
                return figureName;
            }
            set
            {
                if (figureName != value)
                {
                    figureName = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("SelectedFigure", this);
                }
            }
        }

        public double HScale
        {
            get
            {
                return hScale;
            }
            set
            {
                if (hScale != value)
                {
                    if (value > 0)
                    {
                        hScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
                }
            }
        }

        public double LScale
        {
            get
            {
                return lScale;
            }
            set
            {
                if (lScale != value)
                {
                    if (value > 0)
                    {
                        lScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
                }
            }
        }

        public double WScale
        {
            get
            {
                return wScale;
            }
            set
            {
                if (wScale != value)
                {
                    if (value > 0)
                    {
                        wScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
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