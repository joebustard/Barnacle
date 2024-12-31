using Barnacle.Models;
using Barnacle.Object3DLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CSPrimitiveMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string buttonCode =
@"
 <Button
                Command=""{Binding AddCommand}""
                CommandParameter=""!PrimName!""
                IsEnabled=""{Binding EditingActive}""
                Style=""{StaticResource PrimitiveImageButton}""
                ToolTip=""Add a !PrimDesc!"">
                <Image Source = ""/Barnacle;component/Images/Buttons/CrossBlock.png"" />
            </Button>
";

        private const string buttonDescMarker = "!PrimDesc!";
        private const string buttonNameMarker = "!PrimName!";
        private List<String> colourNames;
        private string nextPrimitiveMarker = @"//Next_Primitive_Table_Entry";
        private string primitiveDesc;
        private String primName;
        private string selectedColour;

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
            colourNames = GetAvailableColours();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> ColourNames
        {
            get
            {
                return colourNames;
            }
        }

        public string PrimitiveDesc
        {
            get
            {
                return primitiveDesc;
            }

            set
            {
                if (value != primitiveDesc)
                {
                    primitiveDesc = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String PrimName
        {
            get
            {
                return primName;
            }
            set
            {
                if (value != primName)
                {
                    primName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SelectedColour
        {
            get
            {
                return selectedColour;
            }
            set
            {
                if (value != selectedColour)
                {
                    selectedColour = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String STLPath
        {
            get
            {
                return stlPath;
            }
            set
            {
                if (stlPath != value)
                {
                    stlPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public static List<String> GetAvailableColours()
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
            List<String> cls = new List<String>();
            Type colors = typeof(System.Drawing.Color);
            PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public |
                BindingFlags.Static);
            foreach (PropertyInfo info in colorInfo)
            {
                var result = Array.Find(ignore, element => element == info.Name);
                if (result == null || result == String.Empty)
                {
                    cls.Add(info.Name);
                }
            }
            return cls;
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
            else
            {
                MessageBox.Show("Failed to load STL");
            }
        }

        private void ExportCSharpe(Object3D ob)
        {
            string local = AppDomain.CurrentDomain.BaseDirectory;
            local += "..\\..\\..\\Object3DLib\\ExtraPrimitives\\";
            String target = local + System.IO.Path.GetFileNameWithoutExtension(stlPath) + ".cs";

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

            String object3dpath = AppDomain.CurrentDomain.BaseDirectory;
            object3dpath += "..\\..\\..\\Object3DLib\\object3d.cs";
            string primLine = @"             new PrimTableEntry( """ + primName.ToLower() + @""", PrimitiveGenerator.Generate" + PrimName + ",Colors." + selectedColour + "),";
            try
            {
                File.Copy(object3dpath, object3dpath + ".bak", true);
                string[] lines = File.ReadAllLines(object3dpath);
                StreamWriter fout = new StreamWriter(object3dpath);
                if (fout != null)
                {
                    foreach (string l in lines)
                    {
                        if (l.Trim() == nextPrimitiveMarker)
                        {
                            fout.WriteLine(primLine);
                        }
                        fout.WriteLine(l);
                    }
                    fout.Close();
                }
            }
            catch
            {
            }

            try
            {
                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                projectPath += "..\\..\\..\\Object3DLib\\object3dlib.csproj";
                File.Copy(projectPath, projectPath + ".bak", true);
                string[] lines = File.ReadAllLines(projectPath);
                StreamWriter fout = new StreamWriter(projectPath);
                if (fout != null)
                {
                    bool foundFirst = false;
                    int i = 0;
                    while (i < lines.GetLength(0) && !foundFirst)
                    {
                        fout.WriteLine(lines[i]);
                        if (lines[i].Contains("ExtraPrimitives"))
                        {
                            foundFirst = true;
                        }
                        i++;
                    }
                    while (i < lines.GetLength(0) && lines[i].Contains("ExtraPrimitives"))
                    {
                        fout.WriteLine(lines[i]);
                        i++;
                    }
                    fout.WriteLine(@"  <Compile Include=""ExtraPrimitives\" + System.IO.Path.GetFileNameWithoutExtension(stlPath) + @".cs"" />");
                    while (i < lines.GetLength(0))
                    {
                        fout.WriteLine(lines[i]);
                        i++;
                    }

                    fout.Close();
                }
            }
            catch
            {
            }

            try
            {
                string newButtonCode = buttonCode.Replace(buttonNameMarker, primName).Replace(buttonDescMarker, primitiveDesc);
                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                projectPath += "..\\..\\..\\Make3D\\Views\\objectpalette.xaml";
                File.Copy(projectPath, projectPath + ".bak", true);
                string[] lines = File.ReadAllLines(projectPath);
                StreamWriter fout = new StreamWriter(projectPath);
                if (fout != null)
                {
                    int i = 0;
                    while (i < lines.GetLength(0))
                    {
                        if (lines[i].Contains("</UniformGrid>"))
                        {
                            fout.WriteLine(newButtonCode);
                        }
                        fout.WriteLine(lines[i]);
                        i++;
                    }
                    fout.Close();
                }
            }
            catch
            {
            }
            MessageBox.Show("Wrote to " + target);
        }
    }
}