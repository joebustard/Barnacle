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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection.Metadata.Ecma335;
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
        private string outputAssembly;
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

        private Dictionary<string, string> sourcePaths;

        public MainWindow()
        {
            InitializeComponent();

            AllScriptNames = new List<string>();
            AllScriptNames.Add("Assembler");
            AllScriptNames.Add("Assembler With Union");
            AllScriptNames.Add("Block Per Part");
            AllScriptNames.Add("Block Per Part With Union");
            DataContext = this;
            outputFileName = @"C:\tmp\scriptout.txt";
            modelObjects = new List<Object3D>();
            sourcePaths = new Dictionary<string, string>();
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

        public string OutputAssembly
        {
            get
            {
                return outputAssembly;
            }
            set
            {
                if (value != outputAssembly)
                {
                    outputAssembly = value;
                    NotifyPropertyChanged();
                }
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
                sourcePaths.Clear();
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
                        if (rotpnode != null)
                        {
                            robj.RX = Convert.ToDouble((rotpnode as XmlElement).GetAttribute("X"));
                            robj.RY = Convert.ToDouble((rotpnode as XmlElement).GetAttribute("Y"));
                            robj.RZ = Convert.ToDouble((rotpnode as XmlElement).GetAttribute("Z"));
                        }

                        XmlNode refnode = nd.SelectSingleNode("Reference");
                        if (refnode != null)
                        {
                            String fname = (refnode as XmlElement).GetAttribute("Path");
                            String part = (refnode as XmlElement).GetAttribute("SourceObject");

                            robj.Path = fname;
                            if (sourcePaths.Keys.Count == 0)
                            {
                                sourcePaths[fname] = "src";
                            }
                            else
                            {
                                if (!sourcePaths.ContainsKey(fname))
                                {
                                    sourcePaths[fname] = "src" + sourcePaths.Keys.Count.ToString();
                                }
                            }
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

        private void ExportAssembler(string outputFileName, bool withUnion = false)
        {
            string script = scriptSrc;
            string sources = "// sources\n";

            string positions = "";
            string solids = "    // Solids\n";
            string inserts = $"    // Insert the parts\n    string src = \"{modelFileName}\";\n";
            string union = "";
            string saveit = "";
            bool first = true;
            Dictionary<string, string> Xs = new Dictionary<string, string>();
            Dictionary<string, string> Ys = new Dictionary<string, string>();
            Dictionary<string, string> Zs = new Dictionary<string, string>();
            foreach (string k in sourcePaths.Keys)
            {
                sources += "string " + sourcePaths[k] + " = \"" + k + "\";\n";
            }

            foreach (Object3D obj in modelObjects)
            {
                string nx = obj.Name + "_X";
                string ny = obj.Name + "_Y";
                string nz = obj.Name + "_Z";
                positions += $"    // {obj.Name} \n";
                positions += $"    double {nx} = {LookUpValue(Xs, nx, obj.X)} ;\n";
                positions += $"    double {ny} = {LookUpValue(Ys, ny, obj.Y)} ;\n";
                positions += $"    double {nz} = {LookUpValue(Zs, nz, obj.Z)} ;\n";

                //positions += $"    double {ny} = {obj.Y:F4} ;\n";
                //positions += $"    double {nz} = {obj.Z:F4} ;\n";

                positions += $"    double {obj.Name}_RX = {obj.RX:F4} ;\n";

                positions += $"    double {obj.Name}_RY = {obj.RY:F4} ;\n";

                positions += $"    double {obj.Name}_RZ = {obj.RZ:F4} ;\n";
                solids += $"    Solid {obj.Name} ;\n";
                if (!String.IsNullOrEmpty(obj.Path))

                {
                    inserts += $"    {obj.Name} = InsertAndPlace({sourcePaths[obj.Path]},\"{obj.Part}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,Degrees({obj.Name}_RX),Degrees({obj.Name}_RY),Degrees({obj.Name}_RZ));\n";
                }
                else
                {
                    inserts += $"    {obj.Name} = InsertAndPlace(src,\"{sourcePaths[obj.Name]}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,Degrees({obj.Name}_RX),Degrees({obj.Name}_RY),Degrees({obj.Name}_RZ));\n";
                }
                if (withUnion)
                {
                    if (first)
                    {
                        union = "    // Joint all the parts together\n";
                        union += $"    Solid whole = Copy({obj.Name});\n";
                    }
                    else
                    {
                        union += $"    whole = ForceUnion(whole,{obj.Name});\n";
                    }
                    first = false;
                }
            }
            if (outputAssembly != "")
            {
                saveit = $"SaveSolids \"{outputAssembly}\";\n";
            }
            script = script.Replace("<SCRIPTBODY>", sources + positions + solids + inserts + union + saveit);
            File.WriteAllText(outputFileName, script);
            MessageBox.Show("Exported to " + outputFileName);
        }

        private void ExportBlockPerPart(string outputFileName, bool withUnion = false)
        {
            string script = scriptSrc;
            string body = "// sources\n";

            bool first = true;
            Dictionary<string, string> Xs = new Dictionary<string, string>();
            Dictionary<string, string> Ys = new Dictionary<string, string>();
            Dictionary<string, string> Zs = new Dictionary<string, string>();
            foreach (string k in sourcePaths.Keys)
            {
                body += "string " + sourcePaths[k] + " = \"" + k + "\";\n";
            }
            foreach (Object3D obj in modelObjects)
            {
                string nx = obj.Name + "_X";
                string ny = obj.Name + "_Y";
                string nz = obj.Name + "_Z";
                body += $"    // {obj.Name} \n";
                body += $"    Solid {obj.Name} ;\n";
                body += $"    double {nx} = {LookUpValue(Xs, nx, obj.X)} ;\n";
                body += $"    double {ny} = {LookUpValue(Ys, ny, obj.Y)} ;\n";
                body += $"    double {nz} = {LookUpValue(Zs, nz, obj.Z)} ;\n";

                //positions += $"    double {ny} = {obj.Y:F4} ;\n";
                //positions += $"    double {nz} = {obj.Z:F4} ;\n";

                body += $"    double {obj.Name}_RX = {obj.RX:F4} ;\n";

                body += $"    double {obj.Name}_RY = {obj.RY:F4} ;\n";

                body += $"    double {obj.Name}_RZ = {obj.RZ:F4} ;\n";

                if (!String.IsNullOrEmpty(obj.Path))

                {
                    body += $"    {obj.Name} = InsertAndPlace({sourcePaths[obj.Path]},\"{obj.Part}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,Degrees({obj.Name}_RX),Degrees({obj.Name}_RY),Degrees({obj.Name}_RZ));\n";
                }
                else
                {
                    body += $"    {obj.Name} = InsertAndPlace(src,\"{sourcePaths[obj.Name]}\",{obj.Name}_X,{obj.Name}_Y,{obj.Name}_Z,Degrees({obj.Name}_RX),Degrees({obj.Name}_RY),Degrees({obj.Name}_RZ));\n";
                }
                if (withUnion)
                {
                    if (first)
                    {
                        body += $"    Solid whole = Copy({obj.Name});\n";
                    }
                    else
                    {
                        body += $"    whole = ForceUnion(whole,{obj.Name});\n";
                    }
                    first = false;
                }
            }
            if (outputAssembly != "")
            {
                body += $"SaveSolids \"{outputAssembly}\";\n";
            }
            script = script.Replace("<SCRIPTBODY>", body);
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

                            case "assembler with union ":
                                {
                                    ExportAssembler(outputFileName, true);
                                }
                                break;

                            case "block per part":
                                {
                                    ExportBlockPerPart(outputFileName);
                                }
                                break;

                            case "block per part with union":
                                {
                                    ExportBlockPerPart(outputFileName, true);
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

        private string LookUpValue(Dictionary<string, string> xs, string nk, double x)
        {
            string res = x.ToString("F4");
            bool found = false;
            foreach (string k in xs.Keys)
            {
                if (xs[k] == res)
                {
                    res = k;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                xs[nk] = res;
            }
            return res;
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