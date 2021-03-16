using System;
using System.Collections.Generic;
using System.Xml;

namespace ProjectLib
{
    public class Project
    {
        internal String Name { get; set; }
        internal String Description { get; set; }
        internal List<ProjectFolder> folders;

        internal List<ProjectFolder> Folders
        {
            get { return folders; }
            set
            {
                if (value != folders)
                {
                    folders = value;
                }
            }
        }

        public Project()
        {
            Name = String.Empty;
            Description = String.Empty;
            Folders = new List<ProjectFolder>();
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
                        nf.Name = folderName;
                        Folders.Add(nf);

                        XmlNodeList fileNodes = fel.SelectNodes("File");
                        foreach (XmlNode filn in fileNodes)
                        {
                            ProjectFile pf = new ProjectFile();
                            pf.Load(doc, filn);
                            nf.Files.Add(pf);
                        }
                    }
                }
            }
        }
    }
}