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
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for GridSettings.xaml
    /// </summary>
    public partial class GridSettingsDlg : Window, INotifyPropertyChanged
    {
        public GridSettingsDlg()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            PolarRadius = 10;
            Polar10Checked = true;
            Rect10Checked = true;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();

        }
        public event PropertyChangedEventHandler PropertyChanged;

        private bool polar5Checked;
        public bool Polar5Checked
        {
            get { return polar5Checked; }
            set
            {
                if (value != polar5Checked)
                {
                    polar5Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private bool polar10Checked;
        public bool Polar10Checked
        {
            get { return polar10Checked; }
            set
            {
                if (value != polar10Checked)
                {
                    polar10Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool polar15Checked;
        public bool Polar15Checked
        {
            get { return polar15Checked; }
            set
            {
                if (value != polar15Checked)
                {
                    polar15Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool polar20Checked;
        public bool Polar20Checked
        {
            get { return polar20Checked; }
            set
            {
                if (value != polar20Checked)
                {
                    polar20Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool rect5Checked;
        public bool Rect5Checked
        {
            get { return rect5Checked; }
            set
            {
                if (value != rect5Checked)
                {
                    rect5Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool rect10Checked;
        public bool Rect10Checked
        {
            get { return rect10Checked; }
            set
            {
                if (value != rect10Checked)
                {
                    rect10Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool rect15Checked;
        public bool Rect15Checked
        {
            get { return rect15Checked; }
            set
            {
                if (value != rect15Checked)
                {
                    rect15Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool rect20Checked;
        public bool Rect20Checked
        {
            get { return rect20Checked; }
            set
            {
                if (value != rect20Checked)
                {
                    rect20Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private double polarRadius;
        public double PolarRadius
        {
            get { return polarRadius; }
            set
            {
                if (value != polarRadius)
                {
                    polarRadius = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rect10Checked = true;
            PolarRadius = 10;
            Polar10Checked = true;
        }
    }
}
