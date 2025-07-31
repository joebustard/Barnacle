// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ScriptMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<String> allscriptNames;
        private String modelFileName;

        private List<Object3D> modelObjects;
        private string outputFileName;

        private String scriptName;

        private string scriptSrc =
        @"
program ""Assembler script""
{
  Include ""libs\limplib.txt"" ;
<SCRIPTBODY>
}
";

        public MainWindow()
        {
            InitializeComponent();

            AllScriptNames = new List<string>();
            AllScriptNames.Add("Assembler");
            DataContext = this;
            outputFileName = @"C:\tmp\scriptout.txt";
            modelObjects = new List<Object3D>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<String> AllScriptNames
        {
            get
            {
                return allscriptNames;
            }
            set
            {
                allscriptNames = value;
                NotifyPropertyChanged();
            }
        }

        public String ModelFileName
        {
            get
            {
                return modelFileName;
            }
            set
            {
                modelFileName = value;
                NotifyPropertyChanged();
            }
        }

        public String ScriptName
        {
            get
            {
                return scriptName;
            }
            set
            {
                scriptName = value;
                NotifyPropertyChanged();
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void Read(string file)
        {
            try
            {
                modelObjects.Clear();
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(file);
                XmlNode docNode = doc.SelectSingleNode("Document");

                XmlNodeList nodes = docNode.ChildNodes;

                // get the file refs first
                foreach (XmlNode nd in nodes)
                {
                    string ndname = nd.Name.ToLower();

                    if (ndname == "obj" || ndname == "groupobj")
                    {
                        Object3D obj = new Object3D();
                        obj.Name = (nd as XmlElement).GetAttribute("Name");
                        XmlNode pnode = nd.SelectSingleNode("Position");
                        if (pnode != null)
                        {
                            obj.X = Convert.ToDouble((pnode as XmlElement).GetAttribute("X"));
                            obj.Y = Convert.ToDouble((pnode as XmlElement).GetAttribute("Y"));
                            obj.Z = Convert.ToDouble((pnode as XmlElement).GetAttribute("Z"));
                            modelObjects.Add(obj);
                        }
                    }
                    else if (ndname == "refobj" || ndname == "refgroupobj")
                    {
                        Object3D robj = new Object3D();
                        robj.Name = (nd as XmlElement).GetAttribute("Name");
                        XmlNode rpnode = nd.SelectSingleNode("Position");
                        if (rpnode != null)
                        {
                            robj.X = Convert.ToDouble((rpnode as XmlElement).GetAttribute("X"));
                            robj.Y = Convert.ToDouble((rpnode as XmlElement).GetAttribute("Y"));
                            robj.Z = Convert.ToDouble((rpnode as XmlElement).GetAttribute("Z"));
                        }

                        XmlNode rotpnode = nd.SelectSingleNode("Rotation");
                        if (rpnode != null)
                        {
                            robj.RX = Convert.ToDouble((rpnode as XmlElement).GetAttribute("X"));
                            robj.RY = Convert.ToDouble((rpnode as XmlElement).GetAttribute("Y"));
                            robj.RZ = Convert.ToDouble((rpnode as XmlElement).GetAttribute("Z"));
                        }

                        XmlNode refnode = nd.SelectSingleNode("Reference");
                        if (refnode != null)
                        {
                            String fname = (refnode as XmlElement).GetAttribute("Path");
                            String part = (refnode as XmlElement).GetAttribute("SourceObject");

                            robj.Path = fname;
                            robj.Part = part;
                        }

                        modelObjects.Add(robj);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BrowseClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                ModelFileName = dlg.FileName;
            }
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExportAssembler(string outputFileName)
        {
            string script = scriptSrc;
            string positions = "";
            string solids = "    // Solids\n";
            string inserts = $"    // Insert the parts\n    string src = \"{modelFileName}\";\n";
            string union = "    // Joint all the parts together\n";
            bool first = true;
            foreach (Object3D obj in modelObjects)
            {
                positions += $"    // {obj.Name} \n";
                positions += $"    double {obj.Name}_X = {obj.X:F4} ;\n";
                positions += $"    double {obj.Name}_Y = {obj.Y:F4} ;\n";
                positions += $"    double {obj.Name}_Z = {obj.Z:F4} ;\n";

                positions += $"    double {obj.Name}_RX = {obj.RX:F4} ;\n";

                positions += $"    double {obj.Name}_RY = {obj.RY:F4} ;\n";

                positions += $"    double {obj.Name}_RZ = {obj.RZ:F4} ;\n";
                solids += $"    Solid {obj.Name} ;\n";
                if (!String.IsNullOrEmpty(obj.Path))

                {
                    inserts += $"    {obj.Name} = InsertAndPlace(\"{obj.Path}\",\"{obj.Part}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,{obj.Name}_RX,{obj.Name}_RY,{obj.Name}_RZ);\n";
                }
                else
                {
                    inserts += $"    {obj.Name} = InsertAndPlace(src,\"{obj.Name}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,{obj.Name}_RX,{obj.Name}_RY,{obj.Name}_RZ);\n";
                }

                if (first)
                {
                    union += $"    Solid whole = Copy({obj.Name});\n";
                }
                else
                {
                    union += $"    whole = ForceUnion(whole,{obj.Name});\n";
                }
                first = false;
            }
            script = script.Replace("<SCRIPTBODY>", positions + solids + inserts + union);
            File.WriteAllText(outputFileName, script);
            MessageBox.Show("Exported to " + outputFileName);
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            if (ScriptName != "" && ModelFileName != "")
            {
                if (File.Exists(ModelFileName))
                {
                    Read(ModelFileName);
                    if (modelObjects.Count > 0)
                    {
                        switch (scriptName.ToLower())
                        {
                            case "assembler":
                                {
                                    ExportAssembler(outputFileName);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No such Model File");
                }
            }
        }

        public struct Object3D
        {
            public string Name;
            public string Part;
            public String Path;
            public double RX;
            public double RY;
            public double RZ;
            public double X;
            public double Y;
            public double Z;
        }
    }
}