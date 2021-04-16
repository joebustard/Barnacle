using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VisualSolutionExplorer
{
    public class Project
    {
        internal List<ProjectFolder> folders;

        public Project()
        {
            Name = String.Empty;
            Description = String.Empty;
            Folders = new List<ProjectFolder>();
        }

        internal String Description { get; set; }

        internal List<ProjectFolder> Folders
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

        internal String Name { get; set; }

        public void CreateDefault()
        {
            Name = "Project";
        }

        internal void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("Name"))
                {
                    Name = ele.GetAttribute("Name");
                }
            }
            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText.Trim();
            }
            XmlNodeList flds = ele.SelectNodes("Folder");
            foreach (XmlNode fl in flds)
            {
                XmlElement fel = fl as XmlElement;
                if (fel != null && fel.HasAttribute("Name"))
                {
                    String folderName = fel.GetAttribute("Name");
                    if (folderName != String.Empty)
                    {
                        ProjectFolder nf = new ProjectFolder();
                        nf.FolderName = folderName;
                        Folders.Add(nf);

                        XmlNodeList fileNodes = fel.SelectNodes("File");
                        foreach (XmlNode filn in fileNodes)
                        {
                            ProjectFile pf = new ProjectFile();
                            pf.Load(doc, filn);
                            nf.ProjectFiles.Add(pf);
                        }
                    }
                }
            }
        }
    }
}