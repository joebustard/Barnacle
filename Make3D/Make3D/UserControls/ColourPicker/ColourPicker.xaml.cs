using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for ColourPicker.xaml
    /// </summary>
    public partial class ColourPicker : UserControl, INotifyPropertyChanged
    {
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ColourPicker()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static AvailableColour FindAvailableColour(Color color)
        {
            if (AvailableColours == null)
            {
                SetAvailableColours();
            }
            AvailableColour res = null;
            foreach (AvailableColour cl in AvailableColours)
            {
                if (cl.Colour.A == color.A &&
                cl.Colour.R == color.R &&
                cl.Colour.G == color.G &&
                cl.Colour.B == color.B)
                {
                    res = cl;
                    break;
                }
            }
            return res;
        }

        private static List<AvailableColour> availableColours;

        public static void SetAvailableColours()
        {
            string[] ignore =
            {
        "AliceBlue",
        "Azure",
        "Beige",
        "Cornsilk",
        "Ivory",
        "GhostWhite",
        "LavenderBlush",
        "LightYellow",
        "Linen",
        "MintCream",
        "OldLace",
        "SeaShell",
        "Snow",
        "WhiteSmoke",
        "Transparent"
        };
            List<AvailableColour> cls = new List<AvailableColour>();
            Type colors = typeof(System.Drawing.Color);
            PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public |
                BindingFlags.Static);
            foreach (PropertyInfo info in colorInfo)
            {
                var result = Array.Find(ignore, element => element == info.Name);
                if (result == null || result == String.Empty)
                {
                    cls.Add(new AvailableColour(info.Name));
                }
            }
            AvailableColours = cls;
        }

        public static List<AvailableColour> AvailableColours
        {
            get { return availableColours; }
            set
            {
                if (availableColours != value)
                {
                    availableColours = value;
                }
            }
        }

        private AvailableColour selectedColour;

        public event PropertyChangedEventHandler PropertyChanged;

        public AvailableColour SelectedColour
        {
            get
            {
                return selectedColour;
            }
            set
            {
                if (selectedColour != value)
                {
                    selectedColour = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetAvailableColours();
            SelectedColour = FindAvailableColour(Colors.Black);
        }
    }
}