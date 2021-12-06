using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace LibraryExplorer
{
    public class PartLibrary
    {
        public static string ProjectFilePath;
        public List<LibraryFolder> folders;
        private DateTime creationDate;

        public PartLibrary()
        {
            LibraryName = String.Empty;
            Description = String.Empty;
            BaseFolder = String.Empty;
            ProjectFilePath = String.Empty;
            LibraryFolders = new List<LibraryFolder>();
            FirstFile = "";
            creationDate = DateTime.Now;
        }

        public static String BaseFolder { get; set; }
        public String Description { get; set; }

        public string FirstFile
        {
            get;
            set;
        }

        public List<LibraryFolder> LibraryFolders
        {
            get
            {
                return folders;
            }
            set
            {
                if (value != folders)
                {
                    folders = value;
                }
            }
        }

        public String LibraryName { get; set; }

        public static string AbsPathToLibraryPath(string rf)
        {
            String folderRoot = System.IO.Path.GetDirectoryName(BaseFolder);
            if (rf.StartsWith(folderRoot))
            {
                rf = rf.Substring(folderRoot.Length);
            }
            return rf;
        }

        public static string LibraryPathToAbsPath(string rf)
        {
            string t = "\\" + System.IO.Path.GetFileName(BaseFolder);
            if (rf.StartsWith(t))
            {
                rf = rf.Substring(t.Length);
            }
            if (!rf.StartsWith("\\"))
            {
                rf = "\\" + rf;
            }
            rf = BaseFolder + rf;
            return rf;
        }

        public void AddFileToFolder(string folderPath, string fName)
        {
            folderPath = AbsPathToProjectPath(folderPath);
            foreach (ProjectFolder pf in LibraryFolders)
            {
                pf.AddFileToProject(folderPath, fName);
            }
        }

        public void CreateDefault()
        {
            LibraryName = "Project";

            // create a root folder entry with the project name
            LibraryFolder pfo = new LibraryFolder();
            pfo.FolderName = LibraryName;
            pfo.Clean = false;
            pfo.SupportsSubFolders = true;
            pfo.SupportsFiles = true;
            pfo.Export = true;
            pfo.CanBeRenamed = false;
            LibraryFolders.Add(pfo);
            LibraryFile pfi = new LibraryFile();
            pfi.FileName = "Untitled.txt";
            pfo.ProjectFiles.Add(pfi);
            pfo.RepathSubFolders("");
            FirstFile = pfi.FilePath;
            creationDate = DateTime.Now;
        }

        public void CreateNewFile()
        {
            // ask the root folder to create a new file
            if (LibraryFolders != null && LibraryFolders.Count > 0 && BaseFolder != "")
            {
                LibraryFolders[0].CreateNewFile();
            }
        }

        public string DefaultFileToOpen()
        {
            string res = "";
            foreach (LibraryFolder pfm in LibraryFolders[0].ProjectFolders)
            {
                res = pfm.DefaultFileToOpen();
                if (res != "")
                {
                    break;
                }
            }

            return res;
        }

        public void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("ProjectName"))
                {
                    LibraryName = ele.GetAttribute("ProjectName");
                }
                if (ele.HasAttribute("Open"))
                {
                    FirstFile = ele.GetAttribute("Open");
                }
                if (ele.HasAttribute("Created"))
                {
                    string d = ele.GetAttribute("Created");
                    DateTime.TryParse(d, out creationDate);
                }
            }
            LibraryFolder pfo = new LibraryFolder();
            pfo.FolderName = LibraryName;
            pfo.SupportsFiles = true;
            pfo.Clean = false;
            pfo.CanBeRenamed = false;
            pfo.SupportsSubFolders = true;
            pfo.Export = true;

            // CANT just use a single folder as the root, it has to be a list so treeview can work
            LibraryFolders.Add(pfo);

            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText.Trim();
            }
            /*
            XmlNode setNode = nd.SelectSingleNode("Settings");
            if (setNode != null)
            {
                ProjectSettings prj = new ProjectSettings();
                prj.Read(setNode);
                SharedProjectSettings = prj;
            }
            */
            XmlNodeList fileNodes = ele.SelectNodes("File");
            foreach (XmlNode filn in fileNodes)
            {
                LibraryFile pf = new LibraryFile();
                pf.Load(doc, filn);
                pf.Export = true;
                pfo.ProjectFiles.Add(pf);
            }

            XmlNodeList flds = ele.SelectNodes("Folder");
            foreach (XmlNode fl in flds)
            {
                XmlElement fel = fl as XmlElement;
                if (fel != null && fel.HasAttribute("Name"))
                {
                    LibraryFolder nf = new LibraryFolder();
                    pfo.LibraryFolders.Add(nf);
                    nf.Load(doc, fel);
                    if (FirstFile == "" && nf.ProjectFiles.Count > 0)
                    {
                        FirstFile = nf.ProjectFiles[0].FilePath;
                    }
                }
            }
            pfo.RepathSubFolders("");
        }

        public bool Open(string projectPath)
        {
            bool res = false;
            LibraryFolders.Clear();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            try
            {
                doc.Load(projectPath);
                XmlNode root = doc.SelectSingleNode("Project");
                Load(doc, root);
                res = true;
                BaseFolder = System.IO.Path.GetDirectoryName(projectPath);
                ProjectFilePath = projectPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return res;
        }

        public void Refresh()
        {
            if (LibraryFolders != null && LibraryFolders.Count > 0 && BaseFolder != "")
            {
                String folderRoot = System.IO.Path.GetDirectoryName(BaseFolder);
                LibraryFolders[0].Refresh(folderRoot);
                LibraryFolders[0].CheckTimeDependency(BaseFolder);
            }
        }

        public void Save()
        {
            if (ProjectFilePath != String.Empty)
            {
                SaveFile(ProjectFilePath);
            }
        }

        public void UpdateFolders()
        {
        }

        private void SaveFile(string solutionPath)
        {
            XmlDocument solutionDoc = new XmlDocument();
            solutionDoc.XmlResolver = null;
            XmlElement root = solutionDoc.CreateElement("Project");
            root.SetAttribute("ProjectName", LibraryName);
            if (FirstFile != String.Empty)
            {
                root.SetAttribute("Open", FirstFile);
            }
            //SharedProjectSettings.Write(solutionDoc, root);
            root.SetAttribute("Created", creationDate.ToString());
            solutionDoc.AppendChild(root);
            // The first project folder is a dummy one
            // Save its contents rather than it
            foreach (LIbraryFile pfi in LibraryFolders[0].ProjectFiles)
            {
                pfi.Save(solutionDoc, root);
            }
            foreach (LibraryFolder fld in LibraryFolders[0].ProjectFolders)
            {
                fld.Save(solutionDoc, root);
            }

            solutionDoc.Save(solutionPath);
        }
    }
}