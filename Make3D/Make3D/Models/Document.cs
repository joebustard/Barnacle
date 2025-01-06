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

using Barnacle.Object3DLib;
using FixLib;
using Object3DLib.OFF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using VisualSolutionExplorer;

namespace Barnacle.Models
{
    public class Document : INotifyPropertyChanged
    {
        public List<Object3D> Content;
        internal string ProjectFilter;
        private static int nextId;
        private string caption;
        private bool dirty;

        private Guid documentID;
        private List<String> referencedFiles;

        private int revision;

        public Document()
        {
            ModelScales.Initialise();
            Clear();
            NotificationManager.Subscribe("Document", "DocDirty", OnDocDirty);
            documentID = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static String NextName
        {
            get
            {
                nextId++;
                return "Object_" + nextId.ToString();
            }
        }

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Dirty
        {
            get
            {
                return dirty;
            }
            set
            {
                if (dirty != value)
                {
                    dirty = value;
                    caption = FileName;
                    if (dirty)
                    {
                        caption += "*";
                        NotifyPropertyChanged("Caption");
                    }
                }
            }
        }

        public Guid DocumentId
        {
            get
            {
                return documentID;
            }
        }

        public string Extension
        {
            get; set;
        }

        public string FileFilter
        {
            get; set;
        }

        public string FileName
        {
            get; set;
        }

        public string FilePath
        {
            get; set;
        }

        public Project ParentProject
        {
            get; set;
        }

        public ProjectSettings ProjectSettings
        {
            get; set;
        }

        public int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                revision = value;
            }
        }

        public static XmlElement FindExternalModel(string name, string path)
        {
            XmlElement res = null;
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.Load(path);
                    XmlNode docNode = doc.SelectSingleNode("Document");
                    XmlNodeList nodes = docNode.ChildNodes;
                    foreach (XmlNode nd in nodes)
                    {
                        if (nd.Name == "obj" || nd.Name == "groupobj")
                        {
                            XmlElement ele = nd as XmlElement;
                            if (ele.HasAttribute("Name"))
                            {
                                if (ele.GetAttribute("Name") == name)
                                {
                                    res = ele;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return res;
        }

        public String DuplicateName(string s)
        {
            string res;
            do
            {
                nextId++;
                res = s + "_" + nextId.ToString();
            }
            while (ContainsName(res));

            return res;
        }

        public void Empty()
        {
            if (Content != null)
            {
                foreach (Object3D ob in Content)
                {
                    ob.RelativeObjectVertices?.Clear();
                    ob.AbsoluteObjectVertices?.Clear();
                    ob.TriangleIndices?.Clear();
                }
                Content.Clear();
            }
            Content = null;
            referencedFiles?.Clear();
            referencedFiles = null;
            ProjectSettings = null;
        }

        public void ImportStl(string fileName, bool swapYZ)
        {
            STLExporter exp = new STLExporter();

            Object3D ob = new Object3D();
            Vector3DCollection normals = ob.Normals;
            List<P3D> pnts = ob.RelativeObjectVertices;
            Int32Collection tris = ob.TriangleIndices;

            bool wasBinary = exp.Import(fileName, ref normals, ref pnts, ref tris, swapYZ);

            ob.Color = ProjectSettings.DefaultObjectColour;
            ob.PrimType = "Mesh";
            ob.CalcScale();
            ob.RelativeToAbsolute();

            ob.Mesh.Positions = ob.AbsoluteObjectVertices;
            ob.Mesh.TriangleIndices = ob.TriangleIndices;
            ob.Mesh.Normals = ob.Normals;
            ob.Rotate(new Point3D(-90, 0, 0));
            ob.MoveToFloor();
            ob.Remesh();
            Content.Add(ob);
            ob.Name = "Object_" + Content.Count.ToString();
            Dirty = true;

            if (wasBinary)
            {
                RemoveDuplicateVertices(ob);
            }
        }

        public void Load(string fileName, bool reportMissing = true)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            if (ext == ".txt")
            {
                Read(fileName, true, reportMissing);

                FilePath = fileName;
                FileName = System.IO.Path.GetFileName(fileName);

                Dirty = false;
                Caption = FileName;
            }
            else
            {
                if (ext == ".bob")
                {
                    ReadBinary(fileName, true);
                }
            }
        }

        public void LoadGlobalSettings()
        {
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.SlicerPath != null && Properties.Settings.Default.SlicerPath != "")
            {
                ProjectSettings.SlicerPath = Properties.Settings.Default.SlicerPath;
            }
            if (Properties.Settings.Default.SDCardLabel != null)
            {
                ProjectSettings.SDCardName = Properties.Settings.Default.SDCardLabel;
            }

            if (Properties.Settings.Default.AutoSaveMinutes != null)
            {
                ProjectSettings.AutoSaveMinutes = Convert.ToInt32(Properties.Settings.Default.AutoSaveMinutes);
            }
            else
            {
                ProjectSettings.AutoSaveMinutes = 5;
            }

            ProjectSettings.AutoSaveOn = Properties.Settings.Default.AutoSaveOn;
        }

        public virtual void Read(string file, bool clearFirst, bool reportMissing = true)
        {
            try
            {
                if (clearFirst)
                {
                    Content.Clear();
                    referencedFiles.Clear();
                    GC.Collect();
                }
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(file);
                XmlNode docNode = doc.SelectSingleNode("Document");
                if (clearFirst)
                {
                    XmlElement docele = docNode as XmlElement;
                    if (docele != null)
                    {
                        GetInt(docele, "NextId", ref nextId, 0);
                        GetInt(docele, "Revision", ref revision, 1);
                        if (docele.HasAttribute("Guid"))
                        {
                            Guid.TryParse(docele.GetAttribute("Guid"), out documentID);
                        }
                    }
                }
                XmlNodeList nodes = docNode.ChildNodes;

                // get the file refs first
                foreach (XmlNode nd in nodes)
                {
                    string ndname = nd.Name.ToLower();
                    if (ndname == "fileref")
                    {
                        string fn = (nd as XmlElement).GetAttribute("Name");
                        if (ParentProject != null && !String.IsNullOrEmpty(fn))
                        {
                            fn = ParentProject.ProjectPathToAbsPath(fn);
                            referencedFiles.Add(fn);
                        }
                    }
                }
                foreach (string fn in referencedFiles)
                {
                    LoadReferencedFile(fn);
                }

                foreach (XmlNode nd in nodes)
                {
                    string ndname = nd.Name.ToLower();

                    if (ndname == "obj")
                    {
                        Object3D obj = new Object3D();
                        obj.Read(nd);

                        obj.SetMesh();
                        if (obj.PrimType != "Mesh")
                        {
                            obj = obj.ConvertToMesh();
                        }
                        if (!(double.IsNegativeInfinity(obj.Position.X)))
                        {
                            Content.Add(obj);
                        }
                    }
                    if (ndname == "refobj")
                    {
                        ReferenceObject3D obj = new ReferenceObject3D();
                        obj.Read(nd, reportMissing);

                        obj.SetMesh();
                        // so we should have already read the referenced files by now
                        // meaning there should already be a referenced object which matches.
                        // if there is then update its position to whatever this object says
                        bool found = false;
                        foreach (Object3D old in Content)
                        {
                            if (old is ReferenceObject3D)
                            {
                                if (old.Name == obj.Name && (old as ReferenceObject3D).Reference.Path == obj.Reference.Path)
                                {
                                    found = true;
                                    old.Position = obj.Position;
                                    old.Rotation = obj.Rotation;
                                    break;
                                }
                            }
                        }
                        if (!found)
                        {
                            // might have been converted into a group
                            ReferenceGroup3D grp = new ReferenceGroup3D();
                            grp.Read(nd, reportMissing);

                            grp.SetMesh();
                            foreach (Object3D old in Content)
                            {
                                if (old is ReferenceGroup3D)
                                {
                                    if (old.Name == obj.Name && (old as ReferenceGroup3D).Reference.Path == obj.Reference.Path)
                                    {
                                        found = true;
                                        old.Position = obj.Position;
                                        old.Rotation = obj.Rotation;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (ndname == "groupobj")
                    {
                        Group3D obj = new Group3D();
                        obj.Read(nd);

                        obj.SetMesh();
                        if (!(double.IsNegativeInfinity(obj.Position.X)))
                        {
                            Content.Add(obj);
                        }
                    }
                    if (ndname == "refgroupobj")
                    {
                        ReferenceGroup3D obj = new ReferenceGroup3D();
                        obj.Read(nd, reportMissing);

                        obj.SetMesh();

                        foreach (Object3D old in Content)
                        {
                            if (old is ReferenceGroup3D)
                            {
                                if (old.Name == obj.Name && (old as ReferenceGroup3D).Reference.Path == obj.Reference.Path)
                                {
                                    old.Position = obj.Position;
                                    old.Rotation = obj.Rotation;
                                    old.Remesh();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Save(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            if (ext == ".txt")
            {
                Write(fileName);
                FilePath = fileName;
                FileName = System.IO.Path.GetFileName(fileName);
                Caption = FileName;
                Dirty = false;
            }
            else
            {
                if (ext == ".bob")
                {
                    DateTime st = DateTime.Now;
                    WriteBinary(fileName);
                    DateTime end = DateTime.Now;
                    TimeSpan ts = end - st;
                    System.Diagnostics.Debug.WriteLine("bob = " + ts.TotalMilliseconds);
                }
            }
        }

        public void SaveGlobalSettings()
        {
            if (Properties.Settings.Default.SlicerPath != null && Properties.Settings.Default.SlicerPath != "")
            {
                ProjectSettings.SlicerPath = Properties.Settings.Default.SlicerPath;
            }
            // Properties.Settings.Default.SDCardLabel = ProjectSettings.SDCardName;
            Properties.Settings.Default.Save();
        }

        public virtual void Write(string file)
        {
            GC.Collect();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Document");
            docNode.SetAttribute("NextId", nextId.ToString());
            revision++;
            docNode.SetAttribute("Revision", revision.ToString());
            docNode.SetAttribute("Guid", documentID.ToString());

            foreach (String rf in referencedFiles)
            {
                string fn = ParentProject.AbsPathToProjectPath(rf);
                XmlElement fileRef = doc.CreateElement("FileRef");
                fileRef.SetAttribute("Name", fn);
                docNode.AppendChild(fileRef);
            }
            foreach (Object3D ob in Content)
            {
                ob.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(file);
        }

        public virtual void WriteBinary(string file)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                writer.Write(nextId);
                writer.Write(revision);
                writer.Write(documentID.ToString());
                writer.Write(referencedFiles.Count);
                foreach (String rf in referencedFiles)
                {
                    string fn = ParentProject.AbsPathToProjectPath(rf);
                    writer.Write(fn);
                }
                writer.Write(Content.Count);
                foreach (Object3D ob in Content)
                {
                    ob.WriteBinary(writer);
                }
            }
        }

        internal void AutoExport(string name, Bounds3D bnds)
        {
            double scalefactor = 1.0;
            if (ProjectSettings.BaseScale != ProjectSettings.ExportScale)
            {
                scalefactor = ModelScales.ConversionFactor(ProjectSettings.BaseScale, ProjectSettings.ExportScale);
            }

            List<Object3D> exportList = new List<Object3D>();
            foreach (Object3D ob in Content)
            {
                if (ob.Exportable)
                {
                    Object3D clone = ob.Clone();
                    if (scalefactor != 1.0)
                    {
                        clone.ScaleMesh(scalefactor, scalefactor, scalefactor);
                        clone.Position = new Point3D(clone.Position.X * scalefactor, clone.Position.Y * scalefactor, clone.Position.Z * scalefactor);
                    }
                    if (ProjectSettings.FloorAll)
                    {
                        clone.MoveToFloor();
                    }
                    exportList.Add(clone);
                }
            }

            STLExporter exp = new STLExporter();
            string pth = System.IO.Path.GetDirectoryName(name);
            if (!Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }

            exp.Export(name, exportList, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
        }

        internal void Clear()
        {
            FilePath = "";
            FileFilter = "Object Files (*.txt;*.bob)|*.txt;*.bob";
            ProjectFilter = "Project Files (*.bmf)|*.bmf";
            FileName = "Untitled";
            Extension = ".txt";
            Caption = "untitled";
            if (Content != null)
            {
                foreach (Object3D ob in Content)
                {
                    ob.RelativeObjectVertices.Clear();
                    ob.AbsoluteObjectVertices.Clear();
                    ob.TriangleIndices.Clear();
                }
                Content.Clear();
            }
            Content = new List<Object3D>();
            ProjectSettings = new ProjectSettings();
            LoadGlobalSettings();
            nextId = 0;
            revision = 0;
            Dirty = false;
            referencedFiles = new List<string>();
        }

        internal bool ContainsName(string name)
        {
            bool res = false;
            foreach (Object3D ob in Content)
            {
                if (ob.Name == name)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }

        internal void DeleteObject(Object3D o)
        {
            Content.Remove(o);
            Dirty = true;
        }

        internal string ExportAll(string v, Bounds3D bnds, string exportFolderPath)
        {
            string res = "";
            double scalefactor = 1.0;

            if (ProjectSettings.BaseScale != ProjectSettings.ExportScale)
            {
                scalefactor = ModelScales.ConversionFactor(ProjectSettings.BaseScale, ProjectSettings.ExportScale);
            }

            List<Object3D> exportList = new List<Object3D>();
            foreach (Object3D ob in Content)
            {
                if (ob.Exportable)
                {
                    Object3D clone = ob.Clone();
                    if (scalefactor != 1.0)
                    {
                        clone.ScaleMesh(scalefactor, scalefactor, scalefactor);
                        clone.Position = new Point3D(clone.Position.X * scalefactor, clone.Position.Y * scalefactor, clone.Position.Z * scalefactor);
                    }
                    if (ProjectSettings.FloorAll)
                    {
                        clone.MoveToFloor();
                        clone.Remesh();
                    }
                    exportList.Add(clone);
                }
            }
            if (v == "STL")
            {
                STLExporter exp = new STLExporter();

                if (FileName == "")
                {
                    MessageBox.Show("Save the document first. So there is an export name");
                }
                else
                {
                    String pth = exportFolderPath;
                    if (!Directory.Exists(pth))
                    {
                        Directory.CreateDirectory(pth);
                    }
                    string expName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
                    if (ProjectSettings.VersionExport)
                    {
                        expName += "_V_";
                        if (ProjectSettings.ClearPreviousVersionsOnExport)
                        {
                            string[] filesToDelete = System.IO.Directory.GetFiles(pth, expName + "*.stl");
                            foreach (string f in filesToDelete)
                            {
                                try
                                {
                                    System.IO.File.Delete(f);
                                }
                                catch (Exception ex)
                                {
                                    LoggerLib.Logger.LogLine(ex.Message);
                                }
                            }
                        }
                        expName += revision.ToString();
                    }
                    expName = expName + ".stl";
                    expName = System.IO.Path.Combine(pth, expName);
                    exp.Export(expName, exportList, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
                    res = expName;
                }
            }
            else
            if (v == "STLSLICE")
            {
                STLExporter exp = new STLExporter();

                String pth = exportFolderPath;
                if (!Directory.Exists(pth))
                {
                    Directory.CreateDirectory(pth);
                }
                string expName = System.IO.Path.GetFileNameWithoutExtension(FilePath);

                expName = expName + ".stl";
                expName = System.IO.Path.Combine(pth, expName);
                exp.Export(expName, exportList, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
                res = expName;
            }
            else if (v == "STLParts")
            {
                STLExporter exp = new STLExporter();

                if (FileName == "")
                {
                    MessageBox.Show("Save the document first. So there is an export name");
                }
                else
                {
                    String pth = exportFolderPath;

                    if (!Directory.Exists(pth))
                    {
                        Directory.CreateDirectory(pth);
                    }
                    foreach (Object3D ob in exportList)
                    {
                        string expName = System.IO.Path.Combine(pth, ob.Name + ".stl");
                        List<Object3D> tmp = new List<Object3D>();
                        tmp.Add(ob);
                        exp.Export(expName, exportList, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
                        res = expName;
                    }
                }
            }
            return res;
        }

        internal List<string> ExportAllPartsSeparately(Bounds3D bnds, string exportFolderPath)
        {
            List<string> allpaths = new List<string>();
            String pth = exportFolderPath;
            if (!Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }

            double scalefactor = 1.0;
            if (ProjectSettings.BaseScale != ProjectSettings.ExportScale)
            {
                scalefactor = ModelScales.ConversionFactor(ProjectSettings.BaseScale, ProjectSettings.ExportScale);
            }

            STLExporter exp = new STLExporter();

            List<Object3D> exportList = new List<Object3D>();
            foreach (Object3D ob in Content)
            {
                if (ob.Exportable)
                {
                    Object3D clone = ob.Clone();
                    if (scalefactor != 1.0)
                    {
                        clone.ScaleMesh(scalefactor, scalefactor, scalefactor);
                        clone.Position = new Point3D(clone.Position.X * scalefactor, clone.Position.Y * scalefactor, clone.Position.Z * scalefactor);
                    }
                    clone.MoveToCentre();
                    clone.MoveToFloor();
                    exportList.Add(clone);
                    string expName = System.IO.Path.Combine(pth, ob.Name + ".stl");
                    exp.Export(expName, exportList, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
                    exportList.Clear();
                    allpaths.Add(expName);
                }
            }

            return allpaths;
        }

        internal string ExportSelectedParts(string v, Bounds3D bnds, List<Object3D> parts, string exportFolderPath)
        {
            double scalefactor = 1.0;
            if (ProjectSettings.BaseScale != ProjectSettings.ExportScale)
            {
                scalefactor = ModelScales.ConversionFactor(ProjectSettings.BaseScale, ProjectSettings.ExportScale);
            }
            string res = "";
            if (v == "STL")
            {
                STLExporter exp = new STLExporter();

                List<Object3D> exportList = new List<Object3D>();
                foreach (Object3D ob in parts)
                {
                    if (ob.Exportable)
                    {
                        Object3D clone = ob.Clone();
                        if (scalefactor != 1.0)
                        {
                            clone.ScaleMesh(scalefactor, scalefactor, scalefactor);
                            clone.Position = new Point3D(clone.Position.X * scalefactor, clone.Position.Y * scalefactor, clone.Position.Z * scalefactor);
                        }
                        if (ProjectSettings.FloorAll)
                        {
                            clone.MoveToFloor();
                        }
                        exportList.Add(clone);
                    }
                }

                String pth = exportFolderPath;
                if (!Directory.Exists(pth))
                {
                    Directory.CreateDirectory(pth);
                }

                List<Object3D> oneOb = new List<Object3D>();
                foreach (Object3D ob in parts)
                {
                    if (ob.Exportable)
                    {
                        string expName = System.IO.Path.Combine(pth, ob.Name + ".stl");
                        oneOb.Clear();
                        oneOb.Add(ob);

                        exp.Export(expName, oneOb, ProjectSettings.ExportRotation, ProjectSettings.ExportAxisSwap, bnds);
                        if (res != "")
                        {
                            res += ", ";
                        }
                        res += expName;
                    }
                }
            }
            return res;
        }

        internal ObservableCollection<string> GetObjectNames()
        {
            ObservableCollection<string> res = new ObservableCollection<string>();
            foreach (Object3D ob in Content)
            {
                res.Add(ob.Name);
            }
            return res;
        }

        internal Object3D GroupToMesh(Group3D grp)
        {
            Object3D neo = grp.ConvertToMesh();

            Content.Remove(grp);
            neo.Remesh();

            Content.Add(neo);

            Dirty = true;
            return neo;
        }

        internal void ImportObjs(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    Object3D ob = new Object3D();
                    ob.Name = fileName;
                    Content.Add(ob);

                    string[] lines = File.ReadAllLines(fileName);
                    foreach (string s in lines)
                    {
                        string t = s.Trim();
                        if (t != "")
                        {
                            if (!t.StartsWith("#"))
                            {
                                if (t.StartsWith("o"))
                                {
                                    if (ob != null)
                                    {
                                        ob.PrimType = "Mesh";
                                        ob.CalcScale();
                                        ob.RelativeToAbsolute();
                                        ob.Mesh.Positions = ob.AbsoluteObjectVertices;
                                        ob.Mesh.TriangleIndices = ob.TriangleIndices;
                                        ob.Mesh.Normals = ob.Normals;
                                        ob.Rotate(new Point3D(-90, 0, 0));
                                        ob.MoveToFloor();
                                        ob.Remesh();
                                    }
                                    String name = t.Substring(2);
                                    ob = new Object3D();
                                    ob.Name = name;
                                    Content.Add(ob);
                                }
                                if (t.StartsWith("v "))
                                {
                                    t = t.Replace("  ", " ");
                                    string[] words = t.Split(' ');
                                    double x = Convert.ToDouble(words[1]);
                                    double y = Convert.ToDouble(words[2]);
                                    double z = Convert.ToDouble(words[3]);

                                    P3D p = new P3D(x, y, z);
                                    ob.RelativeObjectVertices.Add(p);
                                }

                                if (t.StartsWith("vn "))
                                {
                                    t = t.Replace("  ", " ");
                                    string[] words = t.Split(' ');
                                    double x = Convert.ToDouble(words[1]);
                                    double y = Convert.ToDouble(words[2]);
                                    double z = Convert.ToDouble(words[3]);
                                    Vector3D p = new Vector3D(x, y, z);
                                    ob.Normals.Add(p);
                                }
                                if (t.StartsWith("f "))
                                {
                                    t = t.Replace("  ", " ");
                                    string[] words = t.Split(' ');
                                    if (words.GetLength(0) == 4)
                                    {
                                        int ind = Convert.ToInt32(words[1]);
                                        ob.TriangleIndices.Add(ind - 1);
                                        ind = Convert.ToInt32(words[2]);
                                        ob.TriangleIndices.Add(ind - 1);
                                        ind = Convert.ToInt32(words[3]);
                                        ob.TriangleIndices.Add(ind - 1);
                                    }
                                    else
                                    {
                                        List<int> indices = new List<int>();

                                        double cx = 0;
                                        double cy = 0;
                                        double cz = 0;
                                        for (int j = 1; j < words.GetLength(0); j++)
                                        {
                                            int pi = Convert.ToInt32(words[j]);
                                            indices.Add(pi);
                                            cx += ob.RelativeObjectVertices[pi].X;
                                            cy += ob.RelativeObjectVertices[pi].Y;
                                            cz += ob.RelativeObjectVertices[pi].Z;
                                        }
                                        cx = cx / indices.Count;
                                        cy = cy / indices.Count;
                                        cz = cz / indices.Count;

                                        ob.RelativeObjectVertices.Add(new P3D(cx, cy, cz));
                                        int id = ob.RelativeObjectVertices.Count - 1;
                                        for (int j = 0; j < indices.Count - 1; j++)
                                        {
                                            ob.TriangleIndices.Add(indices[j]);
                                            ob.TriangleIndices.Add(indices[j + 1]);
                                            ob.TriangleIndices.Add(id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ob.PrimType = "Mesh";
                    ob.Remesh();
                    ob.MoveToFloor();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        internal void ImportOffs(string fileName)
        {
            try
            {
                Point3DCollection vertices = new Point3DCollection();
                Int32Collection faces = new Int32Collection();
                if (OFFFormat.ReadOffFile(fileName, vertices, faces))
                {
                    Object3D ob = new Object3D();
                    ob.Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    Content.Add(ob);
                    ob.AbsoluteObjectVertices = vertices;
                    ob.TriangleIndices = faces;
                    ob.AbsoluteToRelative();
                    ob.PrimType = "Mesh";
                    ob.CalcScale();
                    ob.RelativeToAbsolute();
                    ob.Mesh.Positions = ob.AbsoluteObjectVertices;
                    ob.Mesh.TriangleIndices = ob.TriangleIndices;
                    ob.Mesh.Normals = ob.Normals;

                    ob.MoveToFloor();
                    ob.Remesh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        internal void InsertFile(string fileName)
        {
            Read(fileName, false);

            Dirty = false;
        }

        internal void ReferenceFile(string fileName)
        {
            LoadReferencedFile(fileName);
            Dirty = true;
            if (!referencedFiles.Contains(fileName))
            {
                referencedFiles.Add(fileName);
            }
        }

        internal void RenameCurrent(string old, string renamed)
        {
            if (FilePath == old)
            {
                FilePath = renamed;
                FileName = System.IO.Path.GetFileName(renamed);
                Caption = FileName;
            }
        }

        internal void RenameFolder(string old, string renamed)
        {
            if (FilePath.StartsWith(old))
            {
                FilePath = FilePath.Replace(old, renamed);
            }
        }

        internal void ReplaceObjectsByGroup(Group3D grp)
        {
            Content.Remove(grp.LeftObject);
            Content.Remove(grp.RightObject);
            Content.Add(grp);
            Dirty = true;
        }

        internal void SplitGroup(Group3D grp)
        {
            Content.Remove(grp);
            grp.LeftObject.Remesh();
            Content.Add(grp.LeftObject);

            grp.RightObject.Remesh();
            Content.Add(grp.RightObject);

            grp.LeftObject.CalcScale(false);
            grp.RightObject.CalcScale(false);
            Dirty = true;
        }

        private void GetInt(XmlElement docele, string nm, ref int res, int v)
        {
            res = v;
            if (docele.HasAttribute(nm))
            {
                string s = docele.GetAttribute(nm);
                if (s != "")
                {
                    res = Convert.ToInt32(s);
                }
            }
        }

        private void LoadReferencedFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    DateTime timeStamp = File.GetLastWriteTime(fileName);
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.Load(fileName);
                    XmlNode docNode = doc.SelectSingleNode("Document");
                    XmlNodeList nodes = docNode.ChildNodes;
                    foreach (XmlNode nd in nodes)
                    {
                        if (nd.Name == "obj")
                        {
                            ReferenceObject3D obj = new ReferenceObject3D();

                            obj.Reference.Path = fileName;
                            obj.Reference.TimeStamp = timeStamp;
                            obj.BaseRead(nd);
                            obj.SetMesh();
                            if (!ReferencedObjectInContent(obj.Name, fileName) && !(double.IsNegativeInfinity(obj.Position.X)))
                            {
                                // only reference an object if its exportable/referenceable
                                if (obj.Exportable)
                                {
                                    Content.Add(obj);
                                }
                            }
                        }
                        if (nd.Name == "groupobj")
                        {
                            ReferenceGroup3D obj = new ReferenceGroup3D();

                            obj.Reference.Path = fileName;
                            obj.Reference.TimeStamp = timeStamp;
                            obj.BaseRead(nd);
                            obj.SetMesh();
                            if (!ReferencedObjectInContent(obj.Name, fileName) && !(double.IsNegativeInfinity(obj.Position.X)))
                            {
                                if (obj.Exportable)
                                {
                                    Content.Add(obj);
                                }
                            }

                            // Dispose of the subobjects that
                            // create the group.
                            // We can't do anything with them because
                            // you cant edit the referenced object so
                            // we might as well recover the memory
                            obj.LeftObject = null;
                            obj.RightObject = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnDocDirty(object param)
        {
            Dirty = true;
        }

        private void ReadBinary(string fileName, bool clearFirst)
        {
            if (clearFirst)
            {
                Content.Clear();
                referencedFiles.Clear();
            }
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                nextId = reader.ReadInt32();
                revision = reader.ReadInt32();
                string id = reader.ReadString();
                Guid.TryParse(id, out documentID);
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string fn = reader.ReadString();
                    fn = ParentProject.ProjectPathToAbsPath(fn);
                    referencedFiles.Add(fn);
                }

                foreach (string fn in referencedFiles)
                {
                    LoadReferencedFile(fn);
                }
                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    byte type = reader.ReadByte();
                    switch (type)
                    {
                        case 0:
                            {
                                Object3D ob = new Object3D();
                                ob.ReadBinary(reader);
                                ob.Remesh();
                                ob.SetMesh();
                                if (ob.PrimType != "Mesh")
                                {
                                    ob = ob.ConvertToMesh();
                                }
                                if (!(double.IsNegativeInfinity(ob.Position.X)))
                                {
                                    Content.Add(ob);
                                }
                            }
                            break;

                        case 1:
                            {
                                Group3D ob = new Group3D();
                                ob.ReadBinary(reader);
                                ob.Remesh();
                                ob.SetMesh();

                                if (!(double.IsNegativeInfinity(ob.Position.X)))
                                {
                                    Content.Add(ob);
                                }
                            }
                            break;

                        case 2:
                            {
                                ReferenceObject3D ob = new ReferenceObject3D();
                                ob.ReadBinary(reader);
                                ob.Remesh();
                                // so we should have already read the referenced files by now
                                // meaning there should already be a referenced object which matches.
                                // if there is then update its position to whatever this object says

                                foreach (Object3D old in Content)
                                {
                                    if (old is ReferenceObject3D)
                                    {
                                        if (old.Name == ob.Name && (old as ReferenceObject3D).Reference.Path == ob.Reference.Path)
                                        {
                                            old.Position = ob.Position;
                                            old.Rotation = ob.Rotation;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case 3:
                            {
                                ReferenceGroup3D ob = new ReferenceGroup3D();
                                ob.ReadBinary(reader);
                                ob.Remesh();
                                ob.SetMesh();

                                foreach (Object3D old in Content)
                                {
                                    if (old is ReferenceGroup3D)
                                    {
                                        if (old.Name == ob.Name && (old as ReferenceGroup3D).Reference.Path == ob.Reference.Path)
                                        {
                                            old.Position = ob.Position;
                                            old.Rotation = ob.Rotation;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private bool ReferencedObjectInContent(string name, string pth)
        {
            bool found = false;
            ExternalReference rf = null;
            foreach (Object3D obj in Content)
            {
                if (obj.Name == name)
                {
                    if (obj is ReferenceObject3D)
                    {
                        rf = (obj as ReferenceObject3D).Reference;
                        if (rf.Path == pth)
                        {
                            found = true;
                            break;
                        }
                    }
                    else
                    if (obj is ReferenceGroup3D)
                    {
                        rf = (obj as ReferenceGroup3D).Reference;
                        if (rf.Path == pth)
                        {
                            found = true;
                            break;
                        }
                    }
                }
            }
            return found;
        }

        private void RemoveDuplicateVertices(Object3D ob)
        {
            Fixer checker = new Fixer();
            Point3DCollection points = new Point3DCollection();
            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, points);
            checker.RemoveDuplicateVertices(points, ob.TriangleIndices);
            PointUtils.PointCollectionToP3D(checker.Vertices, ob.RelativeObjectVertices);
            ob.TriangleIndices = checker.Faces;
            ob.Remesh();
        }
    }
}