﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace VisualSolutionExplorer
{
    public class ProjectFolder : IComparable<ProjectFolder>
    {
        public string SupportedFileExtension;
        private List<ProjectFile> _projectFiles = new List<ProjectFile>();
        private List<ProjectFolder> _projectFolders = null;
        private bool supportsFiles;
        private bool supportsSubFolders;
        public bool Export { get; set; }

        public ProjectFolder(string name)
        {
            this.FolderName = name;
            _projectFolders = new List<ProjectFolder>();
            _projectFiles = new List<ProjectFile>();
            SupportedFileExtension = "txt";
            Export = false;
        }

        public ProjectFolder()
        {
            this.FolderName = "";
            _projectFolders = new List<ProjectFolder>();
            _projectFiles = new List<ProjectFile>();
            SupportedFileExtension = "txt";

            Clean = false;
            Explorer = false;
            AutoLoad = false;
            Export = false;
        }

        public Boolean AutoLoad { get; set; }

        // should this folder be cleared when a clean command is issued
        public bool Clean { get; set; }

        // should this folder appear  on the project tree
        public bool Explorer { get; set; }

        public string FolderName { get; set; }

        public string FolderPath { get; set; }

        public string OldName { get; set; }

        public List<ProjectFile> ProjectFiles
        {
            get { return _projectFiles; }
        }

        public List<ProjectFolder> ProjectFolders
        {
            get { return _projectFolders; }
            set { _projectFolders = value; }
        }

        public bool SupportsFiles
        {
            get { return supportsFiles; }
            set { supportsFiles = value; }
        }

        public bool SupportsSubFolders
        {
            get { return supportsSubFolders; }
            set { supportsSubFolders = value; }
        }

        public int CompareTo(ProjectFolder comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;
            else
                return this.FolderName.CompareTo(comparePart.FolderName);
        }

        public string CreateNewFile()
        {
            string fileName = GetNextFileName("New File", SupportedFileExtension);
            ProjectFile fi = new ProjectFile(fileName);
            _projectFiles.Add(fi);
            _projectFiles.Sort();
            fi.FilePath = FolderPath + "\\" + fileName;
            return fi.FilePath;
        }

        public string CreateNewFolder()
        {
            string folderName = GetNextFolderName("New Folder");
            ProjectFolder fo = new ProjectFolder(folderName);
            fo.SupportsSubFolders = SupportsSubFolders;
            fo.SupportsFiles = SupportsFiles;
            fo.SupportedFileExtension = SupportedFileExtension;
            fo.Export = Export;
            _projectFolders.Add(fo);
            _projectFolders.Sort();
            fo.FolderPath = FolderPath + "\\" + folderName;
            return fo.FolderPath;
        }

        public void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("Name"))
                {
                    FolderName = ele.GetAttribute("Name");
                }

                if (ele.HasAttribute("AutoLoad"))
                {
                    AutoLoad = GetBoolean(ele, "AutoLoad");
                }
                SupportsSubFolders = GetBoolean(ele, "AddSubs");
                SupportsFiles = GetBoolean(ele, "AddFiles");
                Clean = GetBoolean(ele, "Clean");
                Explorer = GetBoolean(ele, "Explorer");
                Export = GetBoolean(ele, "Export");
                XmlNodeList fileNodes = ele.SelectNodes("File");
                foreach (XmlNode filn in fileNodes)
                {
                    ProjectFile pf = new ProjectFile();
                    pf.Load(doc, filn);
                    ProjectFiles.Add(pf);
                }
                XmlNodeList flds = ele.SelectNodes("Folder");
                foreach (XmlNode fl in flds)
                {
                    XmlElement fel = fl as XmlElement;
                    if (fel != null && fel.HasAttribute("Name"))
                    {
                        ProjectFolder nf = new ProjectFolder();
                        ProjectFolders.Add(nf);
                        nf.Load(doc, fel);
                    }
                }
            }
        }

        public void RepathSubFolders(String parentName)
        {
            FolderPath = parentName + "\\" + FolderName;
            SetSubPaths();
        }

        public void Save(XmlDocument solutionDoc, XmlElement root)
        {
            XmlElement el = solutionDoc.CreateElement("Folder");
            el.SetAttribute("Name", FolderName);
            el.SetAttribute("AddSubs", SupportsSubFolders.ToString());
            el.SetAttribute("AddFiles", SupportsFiles.ToString());
            el.SetAttribute("Explorer", Explorer.ToString());
            el.SetAttribute("Clean", Clean.ToString());
            if (AutoLoad)
            {
                el.SetAttribute("AutoLoad", AutoLoad.ToString());
            }
            if (Export)
            {
                el.SetAttribute("Export", Export.ToString());
            }
            root.AppendChild(el);
            foreach (ProjectFile pfi in _projectFiles)
            {
                pfi.Save(solutionDoc, el);
            }
            foreach (ProjectFolder fld in _projectFolders)
            {
                fld.Save(solutionDoc, root);
            }
        }

        internal void AddExistingFile(string ren)
        {
            if (!FileAlreadyPresent(ren))
            {
                ProjectFile pf = new ProjectFile();
                pf.FileName = ren;
                ProjectFiles.Add(pf);
                pf.FilePath = FolderPath + "\\" + pf.FileName;
            }
        }

        private bool FileAlreadyPresent(string ren)
        {
            bool found = false;
            foreach (ProjectFile pfi in ProjectFiles)
            {
                if (pfi.FilePath == ren)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        internal void GetRxportFiles(string ext, string baseFolder, List<string> filesToExport)
        {
            if (Export)
            {
                foreach (ProjectFile pf in ProjectFiles)
                {
                    if (System.IO.Path.GetExtension(pf.FileName) == ext)
                    {
                        filesToExport.Add(baseFolder + pf.FilePath);
                    }
                }
                foreach (ProjectFolder pfo in ProjectFolders)
                {
                    pfo.GetRxportFiles(ext, baseFolder, filesToExport);
                }
            }
        }

        internal void RecordOldName()
        {
            OldName = FolderName;
        }

        internal void Refresh(String baseFolder)
        {
            // in practice refresh just means update any autoloading folders
            // with the actual files on disk.
            if (AutoLoad)
            {
                _projectFiles.Clear();
                string[] fNames = Directory.GetFiles(baseFolder + FolderPath);
                foreach (String fn in fNames)
                {
                    string nm = System.IO.Path.GetFileName(fn);
                    AddExistingFile(nm);
                }
            }

            foreach (ProjectFolder pfo in _projectFolders)
            {
                pfo.Refresh(baseFolder);
            }
        }

        internal void RemoveFile(string p1)
        {
            List<ProjectFile> tmp = new List<ProjectFile>();
            for (int i = 0; i < _projectFiles.Count; i++)
            {
                if (_projectFiles[i].FileName != p1)
                {
                    tmp.Add(_projectFiles[i]);
                }
            }
            _projectFiles = tmp;
        }

        internal void UpdatePath()
        {
            String pth = System.IO.Path.GetDirectoryName(FolderPath);
            pth += "\\" + FolderName;
            FolderPath = pth;
            SetSubPaths();
        }

        private bool FileNameUsed(string name)
        {
            bool found = false;
            foreach (ProjectFile fi in _projectFiles)
            {
                if (System.IO.Path.GetFileName(fi.FileName) == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private bool FolderNameUsed(string name)
        {
            bool found = false;
            foreach (ProjectFolder fo in _projectFolders)
            {
                if (fo.FolderName == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private bool GetBoolean(XmlElement ele, string v)
        {
            bool res = false;
            if (ele.HasAttribute(v))
            {
                string s = ele.GetAttribute(v);
                res = Convert.ToBoolean(s);
            }
            return res;
        }

        private string GetNextFileName(string rootName, string ext)
        {
            string tmpName = rootName + "." + ext;
            int i = 2;
            while (FileNameUsed(tmpName))
            {
                tmpName = rootName + $"({i})." + ext;
                i++;
            }
            return tmpName;
        }

        private string GetNextFolderName(string rootName)
        {
            string tmpName = rootName;
            int i = 2;
            while (FolderNameUsed(tmpName))
            {
                tmpName = rootName + $"({i})";
                i++;
            }
            return tmpName;
        }

        private void SetSubPaths()
        {
            foreach (ProjectFolder fo in _projectFolders)
            {
                fo.RepathSubFolders(FolderPath);
            }

            foreach (ProjectFile fi in _projectFiles)
            {
                fi.FilePath = FolderPath + "\\" + fi.FileName;
            }
        }

        private void SortFileNames()
        {
            _projectFiles.Sort();
        }
    }
}