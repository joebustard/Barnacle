using Barnacle.Models;
using Barnacle.Object3DLib;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CSPrimitiveMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private String primName;

        private string source =
@"
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void Generate!PRIM!(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
!POINTS!
        };

        int [] f =
        {
!FACES!
        };

           BuildPrimitive(pnts, indices, v, f);
    }
    }
}
";

        private String stlPath;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            STLPath = "";
            PrimName = "PRIM";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public String PrimName
        {
            get { return primName; }
            set
            {
                if (value != primName)
                {
                    primName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String STLPath
        {
            get { return stlPath; }
            set
            {
                if (stlPath != value)
                {
                    stlPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                STLPath = dlg.FileName;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (STLPath != "" && System.IO.Path.GetExtension(stlPath).ToLower() == ".stl")
            {
                try
                {
                    Document doc = new Document();
                    doc.ImportStl(stlPath, false);
                    if (doc.Content.Count == 1)
                    {
                        Object3D ob = doc.Content[0];
                        Bounds3D bnds = new Bounds3D();
                        foreach (P3D p in ob.RelativeObjectVertices)
                        {
                            bnds.Adjust(new System.Windows.Media.Media3D.Point3D(p.X, p.Y, p.Z));
                        }
                        double scaleX = bnds.Width;
                        double scaleY = bnds.Height;
                        double scaleZ = bnds.Depth;
                        if (scaleX > 0 && scaleY > 0 && scaleZ > 0)
                        {
                            ob.ScaleMesh(1.0 / scaleX, 1.0 / scaleY, 1.0 / scaleZ);
                            ExportCSharpe(ob);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ExportCSharpe(Object3D ob)
        {
            String target = System.IO.Path.ChangeExtension(stlPath, ".cs");
            string v = "";
            foreach (P3D p in ob.RelativeObjectVertices)
            {
                v += $"\t\t\t{p.X.ToString("F3")},{p.Y.ToString("F3")},{p.Z.ToString("F3")},\n";
            }
            string f = "";

            for (int i = 0; i < ob.TriangleIndices.Count; i += 3)
            {
                f += $"\t\t\t{ob.TriangleIndices[i]},{ob.TriangleIndices[i + 1]},{ob.TriangleIndices[i + 2]},\n";
            }
            string s = source;
            s = s.Replace("!POINTS!", v);
            s = s.Replace("!FACES!", f);
            s = s.Replace("!PRIM!", primName);
            System.IO.File.WriteAllText(target, s);
            MessageBox.Show("Wrote to " + target);
        }
    }
}