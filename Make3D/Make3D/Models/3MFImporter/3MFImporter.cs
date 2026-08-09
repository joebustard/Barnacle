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
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Models._3MFImporter
{
    internal class _3MFImporter
    {
        private List<ComponentRec> componentRecs;
        private Document document;
        private string filePath;
        private List<ObjectRec> objectRecs;

        public _3MFImporter(Document document)
        {
            this.document = document;
            objectRecs = new List<ObjectRec>();
            componentRecs = new List<ComponentRec>();
        }

        public bool Process3MFFile(string filePath)
        {
            bool result = false;
            try
            {
                String tempFolderName = "";
                tempFolderName = CreateATempFolder();
                bool rootFound = false;
                string rootname = "";
                // find the 3dmodel.model. This either contains the actual mesh soup
                // or lists where its located of its in other files
                List<string> fileList = ZipUtils.ListFilesInZip(filePath, "model");
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

                // can't do anything unless we found it
                if (rootFound)
                {
                    // its an xml file, so load it
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.Load(rootname);
                    File.Delete(rootname);

                    // go down the hierarchy to the object(s)
                    // all a but cack handed because we dont want to dowload the
                    // xmlns which are probably online not local
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

                        // by now if the soup was in the first file then we've already loaded it
                        // if its in another file (or if there are meant to be multiple copies of it) it
                        // will be referenced in the component recs. If we have not loaded it already
                        // go get it.
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
                                    File.Delete(tfn);
                                }
                            }
                        }
                    }
                }

                // take all the soups we have stored in the objectrecs
                // Convert  to our internal Object3D format and add to the document.
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
                        result = true;
                    }
                }
                // Get rid of the temp folder
                Directory.Delete(tempFolderName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        private static string CreateATempFolder()
        {
            string tempFolderName;
            do
            {
                tempFolderName = System.IO.Path.GetTempPath() + "_" + System.Environment.TickCount.ToString();
            } while (Directory.Exists(tempFolderName));

            Directory.CreateDirectory(tempFolderName);
            Directory.CreateDirectory(tempFolderName + "\\3D");
            return tempFolderName;
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
    }
}