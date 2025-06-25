using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace TemplateLib
{
    public class ProjectTemplateFolder
    {
        public List<ProjectTemplateFile> files;

        public List<ProjectTemplateFolder> folders;

        public ProjectTemplateFolder()
        {
            Files = new List<ProjectTemplateFile>();
            Folders = new List<ProjectTemplateFolder>();
            Attributes = new Dictionary<string, string>();
        }

        public ProjectTemplateFolder(VisualSolutionExplorer.ProjectFolder folder, string projectName)
        {
            Files = new List<ProjectTemplateFile>();
            Attributes = new Dictionary<string, string>();
            Folders = new List<ProjectTemplateFolder>();
            Name = folder.FolderName;
            if (Name.StartsWith(projectName))
            {
                Name = Name.Substring(projectName.Length);
                if (Name == "")
                {
                    Name = ".";
                }
            }
            Attributes["Name"] = Name;
            Attributes["AddSubs"] = folder.SupportsSubFolders.ToString();
            Attributes["AddFiles"] = folder.SupportsFiles.ToString();
            Attributes["AutoLoad"] = folder.AutoLoad.ToString();
            Attributes["Clean"] = folder.Clean.ToString();
            Attributes["Explorer"] = folder.Explorer.ToString();
            Attributes["Export"] = folder.Export.ToString();
            if (folder.SupportedFileExtension != "")
            {
                Attributes["Extension"] = folder.SupportedFileExtension.ToString();
            }
            if (folder.FileTemplate != "")
            {
                Attributes["Template"] = folder.FileTemplate;
            }
            if (folder.EditFile)
            {
                Attributes["Edit"] = folder.EditFile.ToString();
            }
            if (folder.RunFile)
            {
                Attributes["Run"] = folder.RunFile.ToString();
            }
            Attributes["TimeDependency"] = folder.TimeDependency;
            if (!folder.AutoLoad)
            {
                foreach (VisualSolutionExplorer.ProjectFile file in folder.ProjectFiles)
                {
                    ProjectTemplateFile ptf = new ProjectTemplateFile();
                    ptf.Name = file.FileName;
                    ptf.Attributes["Name"] = file.FileName;
                    if (file.Source == "")
                    {
                        string ext = Path.GetExtension(file.FileName);
                        switch (ext)
                        {
                            case ".txt":
                                {
                                    ptf.Attributes["Source"] = @"templates/blankmodel1_35.txt";
                                }
                                break;
                        }
                    }
                    else
                    {
                        ptf.Source = file.Source;
                    }

                    Files.Add(ptf);
                }
            }
            // do the sub folders
            foreach (VisualSolutionExplorer.ProjectFolder pfld in folder.ProjectFolders)
            {
                ProjectTemplateFolder ptf = new ProjectTemplateFolder(pfld, projectName);
                folders.Add(ptf);
            }
        }

        public Dictionary<string, string> Attributes
        {
            get; set;
        }

        public List<ProjectTemplateFile> Files
        {
            get
            {
                return files;
            }
            set
            {
                if (value != files)
                {
                    files = value;
                }
            }
        }

        public global::VisualSolutionExplorer.ProjectFolder Folder
        {
            get;
        }

        public List<ProjectTemplateFolder> Folders
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

        public String Name
        {
            get; set;
        }

        public List<TemplateSubstitution> Substitutions
        {
            get; set;
        }

        public void CreateFiles(string path)
        {
            foreach (ProjectTemplateFile pf in Files)
            {
                string trg = System.IO.Path.Combine(path, pf.Name);
                if (pf.Source != String.Empty)
                {
                    string src = pf.Source;
                    // if it looks like an absolute path leave it alone
                    if (!System.IO.Path.IsPathRooted(src))
                    {
                        src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src);
                    }
                    if (File.Exists(src) && !File.Exists(trg))
                    {
                        File.Copy(src, trg, true);
                    }
                }
                else
                {
                    if (!File.Exists(trg))
                    {
                        File.Create(trg);
                    }
                }
            }
        }

        public void CreateSolutionEntry(XmlDocument solutionDoc, XmlElement fldEl)
        {
            foreach (string s in Attributes.Keys)
            {
                string v = Attributes[s];
                fldEl.SetAttribute(s, v);
            }

            foreach (ProjectTemplateFile fi in Files)
            {
                XmlElement fil = solutionDoc.CreateElement("File");
                foreach (string s in fi.Attributes.Keys)
                {
                    if (s != "Source")
                    {
                        string v = fi.Attributes[s];
                        fil.SetAttribute(s, v);
                    }
                }

                fldEl.AppendChild(fil);
            }

            foreach (ProjectTemplateFolder fol in Folders)
            {
                XmlElement nl = solutionDoc.CreateElement("Folder");
                fol.CreateSolutionEntry(solutionDoc, nl);
                fldEl.AppendChild(nl);
            }
        }

        public void SaveAsTemplate(XmlElement foldernode, XmlDocument doc)
        {
            bool appendToRoot = false;
            if (Attributes.ContainsKey("Name"))
            {
                String n = Attributes["Name"];
                if (n == ".")
                {
                    appendToRoot = true;
                }
            }
            foreach (string s in Attributes.Keys)
            {
                string v = Attributes[s];
                foldernode.SetAttribute(s, v);
            }

            foreach (ProjectTemplateFile fi in Files)
            {
                XmlElement fil = doc.CreateElement("File");
                foreach (string s in fi.Attributes.Keys)
                {
                    string v = fi.Attributes[s];
                    fil.SetAttribute(s, v);
                    foldernode.AppendChild(fil);
                }
            }
            foreach (ProjectTemplateFolder nf in Folders)
            {
                XmlElement fn = doc.CreateElement("Folder");
                foldernode.AppendChild(fn);
                nf.SaveAsTemplate(fn, doc);
            }
        }

        internal void CreateFilesAndFolders(string pth, List<TemplateSubstitution> substitutions)
        {
            Substitutions = substitutions;
            string p = pth;
            if (Name != ".")
            {
                p = System.IO.Path.Combine(pth, Name);
            }
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            CreateFiles(p);
            foreach (ProjectTemplateFolder projectTemplateFolder in folders)
            {
                projectTemplateFolder.CreateFilesAndFolders(p, substitutions);
            }
        }

        internal void Load(XmlDocument doc, XmlElement fel)
        {
            if (fel != null && fel.HasAttribute("Name"))
            {
                String folderName = fel.GetAttribute("Name");
                if (folderName != String.Empty)
                {
                    Name = folderName;

                    // add ALL attributes to the folders attribute dictionary
                    XmlAttributeCollection atrs = fel.Attributes;
                    foreach (XmlAttribute a in atrs)
                    {
                        Attributes[a.Name] = a.Value;
                    }

                    XmlNodeList fileNodes = fel.SelectNodes("File");
                    foreach (XmlNode filn in fileNodes)
                    {
                        ProjectTemplateFile pf = new ProjectTemplateFile();
                        pf.Load(doc, filn);
                        Files.Add(pf);
                    }

                    XmlNodeList flds = fel.SelectNodes("Folder");
                    foreach (XmlNode fl in flds)
                    {
                        XmlElement subfel = fl as XmlElement;
                        ProjectTemplateFolder nf = new ProjectTemplateFolder();
                        nf.Load(doc, subfel);
                        Folders.Add(nf);
                    }
                }
            }
        }
    }
}