using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace VisualSolutionExplorer
{
    public class Project
    {
        public static string ProjectFilePath;
        public List<ProjectFolder> folders;
        private DateTime creationDate;

        public Project()
        {
            ProjectName = String.Empty;
            Description = String.Empty;
            BaseFolder = String.Empty;
            ProjectFilePath = String.Empty;
            ProjectFolders = new List<ProjectFolder>();
            FirstFile = "";
            creationDate = DateTime.Now;
            SharedProjectSettings = new ProjectSettings();
        }

        public static String BaseFolder { get; set; }
        public String Description { get; set; }

        public string FirstFile
        {
            get;
            set;
        }

        public List<ProjectFolder> ProjectFolders
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

        public String ProjectName { get; set; }
        public ProjectSettings SharedProjectSettings { get; set; }

        public static string AbsPathToProjectPath(string rf)
        {
            String folderRoot = System.IO.Path.GetDirectoryName(BaseFolder);
            if (rf.StartsWith(folderRoot))
            {
                rf = rf.Substring(folderRoot.Length);
            }
            return rf;
        }

        public static string ProjectPathToAbsPath(string rf)
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
            foreach (ProjectFolder pf in ProjectFolders)
            {
                pf.AddFileToProject(folderPath, fName);
            }
        }

        public void CreateDefault()
        {
            ProjectName = "Project";

            // create a root folder entry with the project name
            ProjectFolder pfo = new ProjectFolder();
            pfo.FolderName = ProjectName;
            pfo.Clean = false;
            pfo.SupportsSubFolders = true;
            pfo.SupportsFiles = true;
            pfo.Export = true;
            pfo.CanBeRenamed = false;
            ProjectFolders.Add(pfo);
            ProjectFile pfi = new ProjectFile();
            pfi.FileName = "Untitled.txt";
            pfo.ProjectFiles.Add(pfi);
            pfo.RepathSubFolders("");
            FirstFile = pfi.FilePath;
            creationDate = DateTime.Now;
        }

        public void CreateNewFile()
        {
            // ask the root folder to create a new file
            if (ProjectFolders != null && ProjectFolders.Count > 0 && BaseFolder != "")
            {
                ProjectFolders[0].CreateNewFile();
            }
        }

        public string DefaultFileToOpen()
        {
            string res = "";
            foreach (ProjectFolder pfm in ProjectFolders[0].ProjectFolders)
            {
                res = pfm.DefaultFileToOpen();
                if (res != "")
                {
                    break;
                }
            }

            return res;
        }

        public string[] GetExportFiles(string v)
        {
            List<String> filesToExport = new List<string>();
            String fln = System.IO.Path.GetDirectoryName(BaseFolder);
            foreach (ProjectFolder pfo in ProjectFolders)
            {
                pfo.GetRxportFiles(v, fln, filesToExport);
            }

            return filesToExport.ToArray();
        }

        public void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("ProjectName"))
                {
                    ProjectName = ele.GetAttribute("ProjectName");
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
            ProjectFolder pfo = new ProjectFolder();
            pfo.FolderName = ProjectName;
            pfo.SupportsFiles = true;
            pfo.Clean = false;
            pfo.CanBeRenamed = false;
            pfo.SupportsSubFolders = true;
            pfo.Export = true;

            // CANT just use a single folder as the root, it has to be a list so treeview can work
            ProjectFolders.Add(pfo);

            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText.Trim();
            }
            XmlNode setNode = nd.SelectSingleNode("Settings");
            if (setNode != null)
            {
                ProjectSettings prj = new ProjectSettings();
                prj.Read(setNode);
                SharedProjectSettings = prj;
            }
            XmlNodeList fileNodes = ele.SelectNodes("File");
            foreach (XmlNode filn in fileNodes)
            {
                ProjectFile pf = new ProjectFile();
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
                    ProjectFolder nf = new ProjectFolder();
                    pfo.ProjectFolders.Add(nf);
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
            ProjectFolders.Clear();
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
            if (ProjectFolders != null && ProjectFolders.Count > 0 && BaseFolder != "")
            {
                String folderRoot = System.IO.Path.GetDirectoryName(BaseFolder);
                ProjectFolders[0].Refresh(folderRoot);
                ProjectFolders[0].CheckTimeDependency(BaseFolder);
            }
        }

        public void RemoveFileFromFolder(string fName)
        {
            fName = AbsPathToProjectPath(fName);
            foreach (ProjectFolder pf in ProjectFolders)
            {
                pf.RemoveFileFromProject(fName);
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
            root.SetAttribute("ProjectName", ProjectName);
            if (FirstFile != String.Empty)
            {
                root.SetAttribute("Open", FirstFile);
            }
            SharedProjectSettings.Write(solutionDoc, root);
            root.SetAttribute("Created", creationDate.ToString());
            solutionDoc.AppendChild(root);
            // The first project folder is a dummy one
            // Save its contents rather than it
            foreach (ProjectFile pfi in ProjectFolders[0].ProjectFiles)
            {
                pfi.Save(solutionDoc, root);
            }
            foreach (ProjectFolder fld in ProjectFolders[0].ProjectFolders)
            {
                fld.Save(solutionDoc, root);
            }

            solutionDoc.Save(solutionPath);
        }
    }
}