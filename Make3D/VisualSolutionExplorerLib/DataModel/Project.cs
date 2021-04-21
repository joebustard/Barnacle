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

        public Project()
        {
            ProjectName = String.Empty;
            Description = String.Empty;
            BaseFolder = String.Empty;
            ProjectFilePath = String.Empty;
            ProjectFolders = new List<ProjectFolder>();
            FirstFile = "";
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
            ProjectFolders.Add(pfo);
            ProjectFile pfi = new ProjectFile();
            pfi.FileName = "Untitled.txt";
            pfo.ProjectFiles.Add(pfi);
            pfo.RepathSubFolders("");
            FirstFile = pfi.FilePath;
        }

        public void CreateNewFile()
        {
            // ask the root folder to create a new file
            if (ProjectFolders != null && ProjectFolders.Count > 0 && BaseFolder != "")
            {
                ProjectFolders[0].CreateNewFile();
            }
        }

        public string[] GetRxportFiles(string v)
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
            }
            ProjectFolder pfo = new ProjectFolder();
            pfo.FolderName = ProjectName;
            pfo.SupportsFiles = true;
            pfo.Clean = false;
            pfo.SupportsSubFolders = true;
            pfo.Export = true;

            // CANT just use a single folder as the root, it has to be a list so treeview can work
            ProjectFolders.Add(pfo);

            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText.Trim();
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
            }
        }

        public void Save()
        {
            if (ProjectFilePath != String.Empty)
            {
                SaveFile(ProjectFilePath);
            }
        }

        private void SaveFile(string solutionPath)
        {
            XmlDocument solutionDoc = new XmlDocument();
            XmlElement root = solutionDoc.CreateElement("Project");
            root.SetAttribute("ProjectName", ProjectName);
            if (FirstFile != String.Empty)
            {
                root.SetAttribute("Open", FirstFile);
            }
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