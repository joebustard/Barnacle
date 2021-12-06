using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibraryExplorer
{
    internal class LibraryFolder
: IComparable<LibraryFolder>
    {
        private List<LibraryFile> _libraryFiles = new List<LibraryFile>();
        private List<LibraryFolder> _libraryFolders = null;
        private bool supportsFiles;
        private bool supportsSubFolders;

        public LibraryFolder(string name)
        {
            this.FolderName = name;

            Init();
        }

        public LibraryFolder()
        {
            this.FolderName = "";
            Init();
        }

        public Boolean AutoLoad { get; set; }
        public bool CanBeRenamed { get; set; }

        // should this folder be cleared when a clean command is issued
        public bool Clean { get; set; }

        // should the files added here show "Edit"
        public bool EditFile { get; set; }

        // can this folder be opened in explorer
        public bool Explorer { get; set; }

        // should the contents of this folder be exported
        // when a library export is done
        public bool Export { get; set; }

        public string FileTemplate { get; set; }

        public string FolderName { get; set; }

        public string FolderPath { get; set; }

        public List<LibraryFile> LibraryFiles
        {
            get { return _libraryFiles; }
        }

        public List<LibraryFolder> LibraryFolders
        {
            get { return _libraryFolders; }
            set { _libraryFolders = value; }
        }

        public string OldName { get; set; }

        // should the files added here show "Run"
        public bool RunFile { get; set; }

        public string SupportedFileExtension { get; set; }

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

        public string TimeDependency { get; set; }

        public int CompareTo(LibraryFolder comparePart)
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
            ConfirmName dlg = new ConfirmName();
            dlg.FileName = fileName;
            dlg.FolderPath = PartLibrary.LibraryPathToAbsPath(FolderPath);
            dlg.Extension = SupportedFileExtension;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                LibraryFile fi = new LibraryFile(fileName);
                fi.EditFile = EditFile;
                fi.RunFile = RunFile;
                _libraryFiles.Add(fi);
                _libraryFiles.Sort();
                fi.FilePath = FolderPath + "\\" + fileName;

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
            LibraryFolder fo = new LibraryFolder(folderName);
            fo.SupportsSubFolders = SupportsSubFolders;
            fo.SupportsFiles = SupportsFiles;
            fo.SupportedFileExtension = SupportedFileExtension;
            fo.Export = Export;
            fo.EditFile = EditFile;
            fo.RunFile = RunFile;
            fo.FileTemplate = FileTemplate;
            fo.CanBeRenamed = true;
            _libraryFolders.Add(fo);
            _libraryFolders.Sort();
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
                SupportsFiles = GetBoolean(ele, "AddFiles");
                Clean = GetBoolean(ele, "Clean");
                Explorer = GetBoolean(ele, "Explorer");
                Export = GetBoolean(ele, "Export");
                RunFile = GetBoolean(ele, "Run");
                EditFile = GetBoolean(ele, "Edit");
                XmlNodeList fileNodes = ele.SelectNodes("File");
                foreach (XmlNode filn in fileNodes)
                {
                    LibraryFile pf = new LibraryFile();
                    pf.Load(doc, filn);
                    LibraryFiles.Add(pf);
                }
                XmlNodeList flds = ele.SelectNodes("Folder");
                foreach (XmlNode fl in flds)
                {
                    XmlElement fel = fl as XmlElement;
                    if (fel != null && fel.HasAttribute("Name"))
                    {
                        LibraryFolder nf = new LibraryFolder();
                        LibraryFolders.Add(nf);
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
            el.SetAttribute("Extension", SupportedFileExtension.ToString());
            el.SetAttribute("Template", FileTemplate.ToString());
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
            if (TimeDependency != "")
            {
                el.SetAttribute("TimeDependency", TimeDependency);
            }
            root.AppendChild(el);
            foreach (LibraryFile pfi in _libraryFiles)
            {
                pfi.Save(solutionDoc, el);
            }
            foreach (LibraryFolder fld in _libraryFolders)
            {
                fld.Save(solutionDoc, root);
            }
        }

        internal void AddExistingFile(string ren)
        {
            if (!FileAlreadyPresent(ren))
            {
                LibraryFile pf = new LibraryFile();
                pf.FileName = ren;
                pf.EditFile = EditFile;
                pf.Export = Export;
                pf.RunFile = RunFile;

                LibraryFiles.Add(pf);
                pf.FilePath = FolderPath + "\\" + pf.FileName;
            }
        }

        internal bool AddFileToLibrary(string folderPath, string fName)
        {
            bool found = false;
            if (folderPath == FolderPath || folderPath == FolderPath + "\\")
            {
                found = true;
                LibraryFile pfi = new LibraryFile();
                pfi.FileName = fName;
                _libraryFiles.Add(pfi);
                SetSubPaths();
            }
            else
            {
                foreach (LibraryFolder pfo in LibraryFolders)
                {
                    found = pfo.AddFileToLibrary(folderPath, fName);
                }
            }
            return found;
        }

        internal void CheckTimeDependency(String baseFolder)
        {
            if (TimeDependency != "")
            {
                string srcFolder = baseFolder + "\\" + TimeDependency;
                foreach (LibraryFile pfc in _libraryFiles)
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

            foreach (LibraryFolder pfo in _libraryFolders)
            {
                pfo.CheckTimeDependency(baseFolder);
            }
        }

        internal string CopyFile(string p)
        {
            string newName = GetValidCopyName(p);

            string fileName = System.IO.Path.GetFileName(newName);
            LibraryFile fi = new LibraryFile(fileName);
            _libraryFiles.Add(fi);
            _libraryFiles.Sort();
            fi.FilePath = FolderPath + "\\" + fileName;
            return newName;
        }

        internal string DefaultFileToOpen()
        {
            string res = "";
            foreach (LibraryFile s in LibraryFiles)
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

            List<LibraryFile> tmp = new List<LibraryFile>();
            for (int i = 0; i < _libraryFiles.Count; i++)
            {
                if (_libraryFiles[i].FilePath != p1)
                {
                    tmp.Add(_libraryFiles[i]);
                }
            }
            _libraryFiles = tmp;
            res = true;

            return res;
        }

        internal void GetRxportFiles(string ext, string baseFolder, List<string> filesToExport)
        {
            if (Export)
            {
                foreach (LibraryFile pf in LibraryFiles)
                {
                    if (System.IO.Path.GetExtension(pf.FileName) == ext)
                    {
                        filesToExport.Add(baseFolder + pf.FilePath);
                    }
                }
                foreach (LibraryFolder pfo in LibraryFolders)
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
                _libraryFiles.Clear();
                try
                {
                    string[] fNames = Directory.GetFiles(baseFolder + FolderPath);
                    foreach (String fn in fNames)
                    {
                        string nm = System.IO.Path.GetFileName(fn);
                        AddExistingFile(nm);
                    }
                }
                catch
                {
                }
            }

            foreach (LibraryFolder pfo in _libraryFolders)
            {
                pfo.Refresh(baseFolder);
            }
        }

        internal void RemoveFile(string p1)
        {
            List<LibraryFile> tmp = new List<LibraryFile>();
            for (int i = 0; i < _libraryFiles.Count; i++)
            {
                if (_libraryFiles[i].FileName != p1)
                {
                    tmp.Add(_libraryFiles[i]);
                }
            }
            _libraryFiles = tmp;
        }

        internal bool RemoveFileFromLibrary(string fPath)
        {
            bool found = false;
            for (int i = 0; i < _libraryFiles.Count; i++)
            {
                if (_libraryFiles[i].FilePath == fPath)
                {
                    found = true;
                    _libraryFiles.RemoveAt(i);
                }
            }
            if (!found)
            {
                foreach (LibraryFolder fld in _libraryFolders)
                {
                    found = fld.RemoveFileFromLibrary(fPath);
                    if (found)
                    {
                        break;
                    }
                }
            }
            return found;
        }

        internal void UpdatePath()
        {
            String pth = System.IO.Path.GetDirectoryName(FolderPath);
            pth += "\\" + FolderName;
            FolderPath = pth;
            SetSubPaths();
        }

        private bool FileAlreadyPresent(string ren)
        {
            bool found = false;
            foreach (LibraryFile pfi in LibraryFiles)
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
            foreach (LibraryFile fi in _libraryFiles)
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
            foreach (LibraryFolder fo in _libraryFolders)
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

                if (!File.Exists(PartLibrary.LibraryPathToAbsPath(cn)))
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
            _libraryFolders = new List<LibraryFolder>();
            _libraryFiles = new List<LibraryFile>();
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
        }

        private void SetSubPaths()
        {
            foreach (LibraryFolder fo in _libraryFolders)
            {
                fo.RepathSubFolders(FolderPath);
            }

            foreach (LibraryFile fi in _libraryFiles)
            {
                fi.FilePath = FolderPath + "\\" + fi.FileName;
            }
        }

        private void SortFileNames()
        {
            _libraryFiles.Sort();
        }
    }
}