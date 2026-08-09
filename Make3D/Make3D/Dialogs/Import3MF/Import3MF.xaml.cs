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

using Barnacle.Dialogs;
using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using CSGLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Import3MF.xaml
    /// </summary>
    public partial class Import3MF : Window, INotifyPropertyChanged
    {
        private bool closeEnabled;
        private List<ComponentRec> componentRecs;
        private Document document;
        private string filePath;
        private List<ObjectRec> objectRecs;
        private string resultsText;
        private bool startEnabled;

        public Import3MF(Document doc, string filePath)
        {
            InitializeComponent();
            DataContext = this;
            CloseEnabled = true;
            StartEnabled = false;
            document = doc;
            this.filePath = filePath;
            objectRecs = new List<ObjectRec>();
            componentRecs = new List<ComponentRec>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CloseEnabled
        {
            get
            {
                return closeEnabled;
            }

            set
            {
                if (closeEnabled != value)
                {
                    closeEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ResultsText
        {
            get
            {
                return resultsText;
            }

            set
            {
                if (resultsText != value)
                {
                    resultsText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool StartEnabled
        {
            get
            {
                return startEnabled;
            }

            set
            {
                if (startEnabled != value)
                {
                    startEnabled = value;
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

        private void AppendResults(string v)
        {
            ResultsText += v;
            ResultsText += "\n";
            ResultsBox.CaretIndex = ResultsBox.Text.Length;
            ResultsBox.ScrollToEnd();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void HandleComponentNode(XmlNode compNode)
        {
            ComponentRec compRec = new ComponentRec();
            compRec.LoadFromNode(compNode);
            componentRecs.Add(compRec);
        }

        private void HandleComponentsNode(XmlNode nd3)
        {
            foreach (XmlNode compNode in nd3.ChildNodes)
            {
                if (compNode.Name == "component")
                {
                    HandleComponentNode(compNode);
                }
            }
        }

        private void HandleMeshNode(XmlNode nd3, int id)
        {
            ObjectRec rec = new ObjectRec();
            rec.LoadFromNode(nd3);
            rec.Id = id;
            objectRecs.Add(rec);
        }

        private void Process3MFFile(string filepath)
        {
            try
            {
                List<string> fileList = ZipUtils.ListFilesInZip(filePath, "model");

                String tempFolderName = "";
                do
                {
                    string tp = System.IO.Path.GetTempFileName();
                    tempFolderName = System.IO.Path.GetTempPath() + System.IO.Path.GetFileNameWithoutExtension(tp);
                } while (Directory.Exists(tempFolderName));

                Directory.CreateDirectory(tempFolderName);
                Directory.CreateDirectory(tempFolderName + "\\3D");
                bool rootFound = false;
                string rootname = "";
                // find the 3dmodel.model
                foreach (string fn in fileList)
                {
                    if (fn.ToLower() == "3d/3dmodel.model")
                    {
                        // it does exist
                        rootname = tempFolderName + "\\" + fn;
                        ZipUtils.ExtractFileFromZip(filePath, fn, rootname);
                        rootFound = true;
                        break;
                    }
                }
                if (rootFound)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.Load(rootname);

                    XmlNode objectNodes = doc.ChildNodes[0];
                    if (objectNodes != null)
                    {
                        XmlNode modelNode = objectNodes.NextSibling;
                        if (modelNode != null)
                        {
                            foreach (XmlNode nd in modelNode.ChildNodes)
                            {
                                if (nd.Name.ToLower() == "resources")
                                {
                                    foreach (XmlNode nd2 in nd.ChildNodes)
                                    {
                                        if (nd2.Name == "object")
                                        {
                                            int id = -1;
                                            foreach (XmlAttribute attr in nd.Attributes)
                                            {
                                                if (attr.Name.ToLower() == "id")
                                                {
                                                    id = Convert.ToInt32(attr.Value);
                                                }
                                            }
                                            foreach (XmlNode nd3 in nd2.ChildNodes)
                                            {
                                                if (nd3.Name == "mesh")
                                                {
                                                    HandleMeshNode(nd3, id);
                                                }
                                                else
                                                {
                                                    if (nd3.Name == "components")
                                                    {
                                                        HandleComponentsNode(nd3);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        foreach (ComponentRec rec in componentRecs)
                        {
                            bool foundObject = false;
                            foreach (ObjectRec orec in objectRecs)
                            {
                                if (orec.Id == rec.SrcId)
                                {
                                    foundObject = true;
                                }
                            }

                            if (!foundObject && rec.ComponentPath != "")
                            {
                                ObjectRec nrec = new ObjectRec();
                                nrec.Id = rec.SrcId;
                                nrec.SrcPath = rec.ComponentPath;
                                objectRecs.Add(nrec);
                                string sfn = nrec.SrcPath;
                                if (sfn.StartsWith("/"))
                                {
                                    sfn = sfn.Substring(1);
                                }
                                string tfn = tempFolderName + "\\" + System.IO.Path.GetFileName(sfn);
                                if (ZipUtils.ExtractFileFromZip(filePath, sfn, tfn))
                                {
                                    nrec.LoadFromFile(tfn);
                                }
                            }
                        }
                    }
                }
                foreach (ObjectRec orec in objectRecs)
                {
                    if (orec.Vertices.Count > 0)
                    {
                        Object3D ob = new Object3D();
                        bool swapYZ = BaseViewModel.Project.SharedProjectSettings.ImportStlAxisSwap;
                        if (swapYZ)
                        {
                            foreach (Point3D p in orec.Vertices)
                            {
                                ob.RelativeObjectVertices.Add(new P3D(p.X, p.Z, p.Y));
                            }
                            ob.FlipInside();
                        }
                        else
                        {
                            foreach (Point3D p in orec.Vertices)
                            {
                                ob.RelativeObjectVertices.Add(new P3D(p.X, p.Y, p.Z));
                            }
                        }
                        foreach (int f in orec.Faces)
                        {
                            ob.TriangleIndices.Add(f);
                        }
                        ob.Color = BaseViewModel.Project.SharedProjectSettings.DefaultObjectColour;
                        ob.MoveOriginToCentroid();
                        ob.Name = $"Object_{document.Content.Count + 1}";
                        ob.MoveToFloor();
                        ob.MoveToCentre();
                        ob.Remesh();
                        document.Content.Add(ob);
                    }
                }
                // Get rid of the temp folder
                Directory.Delete(tempFolderName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process3MFFile();
        }
    }
}