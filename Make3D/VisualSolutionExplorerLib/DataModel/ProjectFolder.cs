﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using VisualSolutionExplorerLib.Dialogs;

namespace VisualSolutionExplorer
{
    public class ProjectFolder : IComparable<ProjectFolder>
    {
        public bool canAddToLibrary;
        private List<ProjectFile> _projectFiles = new List<ProjectFile>();

        private List<ProjectFolder> _projectFolders = null;
        private bool isInLibrary;
        private bool supportsFiles;
        private bool supportsSubFolders;

        public ProjectFolder(string name)
        {
            this.FolderName = name;

            Init();
        }

        public ProjectFolder()
        {
            this.FolderName = "";
            Init();
        }

        public Boolean AutoLoad
        {
            get; set;
        }

        public bool CanAddToLibrary
        {
            get
            {
                return canAddToLibrary;
            }

            internal set
            {
                canAddToLibrary = value;

                /*
                foreach (ProjectFile pfi in ProjectFiles)
                {
                    pfi.IsLibraryFile = canAddToLibrary;
                }
                */
                foreach (ProjectFolder pf in ProjectFolders)
                {
                    pf.CanAddToLibrary = canAddToLibrary;
                }
            }
        }

        public bool CanBeRenamed
        {
            get; set;
        }

        // should this folder be cleared when a clean command is issued
        public bool Clean
        {
            get; set;
        }

        // should the files added here show "Edit"
        public bool EditFile
        {
            get; set;
        }

        // can this folder be opened in explorer
        public bool Explorer
        {
            get; set;
        }

        // should the contents of this folder be exported when a project export is done
        public bool Export
        {
            get; set;
        }

        public string FileTemplate
        {
            get; set;
        }

        public string FolderName
        {
            get; set;
        }

        public string FolderPath
        {
            get; set;
        }

        public bool IsInLibrary
        {
            get
            {
                return isInLibrary;
            }
            set
            {
                isInLibrary = value;
            }
        }

        public string OldName
        {
            get; set;
        }

        public Project ParentProject
        {
            get; set;
        }

        public List<ProjectFile> ProjectFiles
        {
            get
            {
                return _projectFiles;
            }
        }

        public List<ProjectFolder> ProjectFolders
        {
            get
            {
                return _projectFolders;
            }
            set
            {
                _projectFolders = value;
            }
        }

        // should the files added here show "Run"
        public bool RunFile
        {
            get; set;
        }

        public string SupportedFileExtension
        {
            get; set;
        }

        public bool SupportsFiles
        {
            get
            {
                return supportsFiles;
            }
            set
            {
                supportsFiles = value;
            }
        }

        public bool SupportsSubFolders
        {
            get
            {
                return supportsSubFolders;
            }
            set
            {
                supportsSubFolders = value;
            }
        }

        public string TimeDependency
        {
            get; set;
        }

        public ProjectFolderViewModel Vm
        {
            get; internal set;
        }

        public int CompareTo(ProjectFolder comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;
            else
                return this.FolderName.CompareTo(comparePart.FolderName);
        }

        public string CreateNamedFolder(string folderName)
        {
            ProjectFolder fo = new ProjectFolder(folderName);
            fo.SupportsSubFolders = SupportsSubFolders;
            fo.SupportsFiles = SupportsFiles;
            fo.SupportedFileExtension = SupportedFileExtension;
            fo.Export = Export;
            fo.EditFile = EditFile;
            fo.RunFile = RunFile;
            fo.FileTemplate = FileTemplate;
            fo.CanBeRenamed = true;
            fo.AutoLoad = AutoLoad;
            fo.Explorer = Explorer;
            fo.IsInLibrary = IsInLibrary;
            fo.ParentProject = this.ParentProject;
            _projectFolders.Add(fo);
            _projectFolders.Sort();
            fo.FolderPath = FolderPath + System.IO.Path.DirectorySeparatorChar + folderName;
            return fo.FolderPath;
        }

        public string CreateNewFile()
        {
            string fileName = GetNextFileName("New File", SupportedFileExtension);
            ConfirmName dlg = new ConfirmName();
            dlg.FileName = fileName;
            dlg.FolderPath = ParentProject.ProjectPathToAbsPath(FolderPath);
            dlg.Extension = SupportedFileExtension;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                ProjectFile fi = new ProjectFile(fileName);
                fi.EditFile = EditFile;
                fi.RunFile = RunFile;
                _projectFiles.Add(fi);
                _projectFiles.Sort();
                fi.FilePath = FolderPath + System.IO.Path.DirectorySeparatorChar + fileName;

                return fi.FilePath;
            }
            else
            {
                return "";
            }
        }

        public string CreateNewFolder()
        {
            string folderName = GetNextFolderName("New Folder");
            String folderPath = CreateNamedFolder(folderName);
            return folderPath;
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
                if (ele.HasAttribute("TimeDependency"))
                {
                    TimeDependency = ele.GetAttribute("TimeDependency");
                }
                if (ele.HasAttribute("Extension"))
                {
                    SupportedFileExtension = ele.GetAttribute("Extension");
                }
                if (ele.HasAttribute("Template"))
                {
                    FileTemplate = ele.GetAttribute("Template");
                }
                SupportsSubFolders = GetBoolean(ele, "AddSubs");
                CanBeRenamed = GetBoolean(ele, "CanBeRenamed");
                SupportsFiles = GetBoolean(ele, "AddFiles");
                Clean = GetBoolean(ele, "Clean");
                Explorer = GetBoolean(ele, "Explorer");
                Export = GetBoolean(ele, "Export");
                RunFile = GetBoolean(ele, "Run");
                EditFile = GetBoolean(ele, "Edit");
                IsInLibrary = GetBoolean(ele, "IsInLibrary");
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
                        nf.ParentProject = ParentProject;
                        ProjectFolders.Add(nf);
                        nf.Load(doc, fel);
                    }
                }
            }
        }

        public void RepathSubFolders(String parentName)
        {
            FolderPath = parentName + System.IO.Path.DirectorySeparatorChar + FolderName;
            SetSubPaths();
        }

        public void Save(XmlDocument solutionDoc, XmlElement root)
        {
            XmlElement el = solutionDoc.CreateElement("Folder");
            el.SetAttribute("Name", FolderName);
            if (SupportsSubFolders)
            {
                el.SetAttribute("AddSubs", "True");
            }
            if (SupportsFiles)
            {
                el.SetAttribute("AddFiles", "True");
            }
            if (Explorer)
            {
                el.SetAttribute("Explorer", Explorer.ToString());
            }
            if (Clean)
            {
                el.SetAttribute("Clean", Clean.ToString());
            }
            if (SupportedFileExtension != "")
            {
                el.SetAttribute("Extension", SupportedFileExtension.ToString());
            }
            if (FileTemplate != "")
            {
                el.SetAttribute("Template", FileTemplate);
            }
            if (AutoLoad)
            {
                el.SetAttribute("AutoLoad", AutoLoad.ToString());
            }
            if (Export)
            {
                el.SetAttribute("Export", Export.ToString());
            }
            if (RunFile)
            {
                el.SetAttribute("Run", RunFile.ToString());
            }
            if (EditFile)
            {
                el.SetAttribute("Edit", EditFile.ToString());
            }
            if (IsInLibrary)
            {
                el.SetAttribute("IsInLibrary", IsInLibrary.ToString());
            }
            if (CanBeRenamed)
            {
                el.SetAttribute("CanBeRenamed", CanBeRenamed.ToString());
            }
            if (TimeDependency != "")
            {
                el.SetAttribute("TimeDependency", TimeDependency);
            }
            root.AppendChild(el);
            foreach (ProjectFile pfi in _projectFiles)
            {
                pfi.Save(solutionDoc, el);
            }
            foreach (ProjectFolder fld in _projectFolders)
            {
                fld.Save(solutionDoc, el);
            }
        }

        internal void AddExistingFile(string ren)
        {
            if (!FileAlreadyPresent(ren))
            {
                ProjectFile pf = new ProjectFile();
                pf.FileName = ren;
                pf.EditFile = EditFile;
                pf.Export = Export;
                pf.RunFile = RunFile;
                pf.IsLibraryFile = IsInLibrary;
                ProjectFiles.Add(pf);
                pf.FilePath = FolderPath + System.IO.Path.DirectorySeparatorChar + pf.FileName;
            }
        }

        internal bool AddFileToProject(string folderPath, string fName, bool allowDuplicates = true)
        {
            bool found = false;
            if (folderPath == FolderPath || folderPath == FolderPath + System.IO.Path.DirectorySeparatorChar)
            {
                found = true;
                bool add = true;
                if (allowDuplicates == false)
                {
                    foreach (ProjectFile oldFile in _projectFiles)
                    {
                        if (oldFile.FileName.ToLower() == fName.ToLower())
                        {
                            add = false;
                            break;
                        }
                    }
                }
                if (add)
                {
                    ProjectFile pfi = new ProjectFile();
                    pfi.IsLibraryFile = IsInLibrary;
                    pfi.FileName = fName;
                    _projectFiles.Add(pfi);
                    SetSubPaths();
                }
            }
            else
            {
                foreach (ProjectFolder pfo in ProjectFolders)
                {
                    found = pfo.AddFileToProject(folderPath, fName, allowDuplicates);
                }
            }
            return found;
        }

        internal void CheckTimeDependency(String baseFolder)
        {
            if (TimeDependency != "")
            {
                string srcFolder = baseFolder + System.IO.Path.DirectorySeparatorChar + TimeDependency;
                foreach (ProjectFile pfc in _projectFiles)
                {
                    pfc.OutOfDate = false;
                    // does this file have an equivalent in the source folder.
                    string targetPath = System.IO.Path.GetDirectoryName(baseFolder) + pfc.FilePath;
                    string targetName = System.IO.Path.GetFileNameWithoutExtension(targetPath);
                    // Is the source newer that the target?
                    string[] srcNames = Directory.GetFiles(srcFolder, targetName + ".*");
                    if (srcNames.GetLength(0) == 1)
                    {
                        DateTime srcTime = File.GetLastWriteTime(srcNames[0]);
                        DateTime trgTime = File.GetLastWriteTime(targetPath);

                        if (srcTime > trgTime)
                        {
                            // mark the target a out of date
                            pfc.OutOfDate = true;
                        }
                    }
                }
            }

            foreach (ProjectFolder pfo in _projectFolders)
            {
                pfo.CheckTimeDependency(baseFolder);
            }
        }

        internal string CopyFile(string p)
        {
            string newName = GetValidCopyName(p);

            string fileName = System.IO.Path.GetFileName(newName);
            ProjectFile fi = new ProjectFile(fileName);
            _projectFiles.Add(fi);
            _projectFiles.Sort();
            fi.FilePath = FolderPath + System.IO.Path.DirectorySeparatorChar + fileName;
            return newName;
        }

        internal string DefaultFileToOpen()
        {
            string res = "";
            foreach (ProjectFile s in ProjectFiles)
            {
                if (System.IO.Path.GetExtension(s.FilePath) == "txt")
                {
                    res = s.FilePath;
                    break;
                }
            }
            return res;
        }

        internal bool DeleteFile(string p1)
        {
            bool res = false;

            List<ProjectFile> tmp = new List<ProjectFile>();
            for (int i = 0; i < _projectFiles.Count; i++)
            {
                if (_projectFiles[i].FilePath != p1)
                {
                    tmp.Add(_projectFiles[i]);
                }
            }
            _projectFiles = tmp;
            res = true;

            return res;
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

        internal void MarkAsLibrary()
        {
            foreach (ProjectFile pfi in ProjectFiles)
            {
                pfi.MarkAsLibrary();
            }
            foreach (ProjectFolder pfo in ProjectFolders)
            {
                pfo.MarkAsLibrary();
            }
            SupportsSubFolders = true;
            SupportsFiles = false;
            Clean = false;
            Explorer = false;
            Export = false;
            RunFile = false;
            EditFile = false;
            CanBeRenamed = true;
            isInLibrary = true;
        }

        internal void RecordOldName()
        {
            OldName = FolderName;
        }

        internal void Refresh(String baseFolder)
        {
            // in practice refresh just means update any autoloading folders with the actual files
            // on disk.
            if (AutoLoad)
            {
                _projectFiles.Clear();
                try
                {
                    string[] fNames = Directory.GetFiles(baseFolder + FolderPath);
                    foreach (String fn in fNames)
                    {
                        string nm = System.IO.Path.GetFileName(fn);
                        AddExistingFile(nm);
                    }
                }
                catch (Exception ex)
                {
                    LoggerLib.Logger.LogLine(ex.Message);
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

        internal bool RemoveFileFromProject(string fPath)
        {
            bool found = false;
            for (int i = 0; i < _projectFiles.Count; i++)
            {
                if (_projectFiles[i].FilePath == fPath)
                {
                    found = true;
                    _projectFiles.RemoveAt(i);
                }
            }
            if (!found)
            {
                foreach (ProjectFolder fld in _projectFolders)
                {
                    found = fld.RemoveFileFromProject(fPath);
                    if (found)
                    {
                        break;
                    }
                }
            }
            return found;
        }

        internal void UpdateMenu()
        {
            if (Vm != null)
            {
                Vm.UpdateMenu();
            }
            foreach (ProjectFolder pf in ProjectFolders)
            {
                pf.UpdateMenu();
            }
        }

        internal void UpdatePath()
        {
            String pth = System.IO.Path.GetDirectoryName(FolderPath);
            pth += System.IO.Path.DirectorySeparatorChar + FolderName;
            FolderPath = pth;
            SetSubPaths();
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

        private string GetValidCopyName(string p)
        {
            String noext = System.IO.Path.GetFileNameWithoutExtension(p);
            String ext = System.IO.Path.GetExtension(p);

            string res = "";
            int i = 1;
            bool found = false;
            while (!found)
            {
                string cn = noext + " (" + i.ToString() + ")" + ext;

                if (!File.Exists(ParentProject.ProjectPathToAbsPath(cn)))
                {
                    found = true;
                    res = cn;
                }
                i++;
            }

            return res;
        }

        private void Init()
        {
            _projectFolders = new List<ProjectFolder>();
            _projectFiles = new List<ProjectFile>();
            SupportedFileExtension = "txt";
            FileTemplate = "";
            Export = false;
            Clean = false;
            Explorer = false;
            AutoLoad = false;
            CanBeRenamed = true;
            TimeDependency = "";
            RunFile = false;
            EditFile = false;
            isInLibrary = false;
        }

        private void SetSubPaths()
        {
            foreach (ProjectFolder fo in _projectFolders)
            {
                fo.RepathSubFolders(FolderPath);
            }

            foreach (ProjectFile fi in _projectFiles)
            {
                fi.FilePath = FolderPath + System.IO.Path.DirectorySeparatorChar + fi.FileName;
            }
        }

        private void SortFileNames()
        {
            _projectFiles.Sort();
        }
    }
}