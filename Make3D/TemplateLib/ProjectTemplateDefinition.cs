using FileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplateDefinition
    {
        public List<ProjectTemplateFolder> folders;
        private List<TemplateSubstitution> substitutions;

        public ProjectTemplateDefinition()
        {
            Name = String.Empty;
            Description = String.Empty;
            InitialFile = String.Empty;
            Folders = new List<ProjectTemplateFolder>();
            substitutions = new List<TemplateSubstitution>();
        }

        public String Description
        {
            get; set;
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

        public String InitialFile
        {
            get; set;
        }

        public String Name
        {
            get; set;
        }

        public List<TemplateSubstitution> Substitutions
        {
            get
            {
                return substitutions;
            }
            set
            {
                if (substitutions != value)
                {
                    substitutions = value;
                }
            }
        }

        public void Load(XmlDocument doc, XmlNode nd)
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
            XmlNode inif = nd.SelectSingleNode("InitialFile");
            if (inif != null)
            {
                InitialFile = inif.InnerText.Trim();
            }
            XmlNodeList flds = ele.SelectNodes("Folder");
            foreach (XmlNode fl in flds)
            {
                XmlElement fel = fl as XmlElement;
                ProjectTemplateFolder nf = new ProjectTemplateFolder();
                nf.Load(doc, fel);
                Folders.Add(nf);
            }

            XmlNodeList subs = ele.SelectNodes("Replace");
            foreach (XmlNode fl in subs)
            {
                XmlElement fel = fl as XmlElement;
                if (fel != null && fel.HasAttribute("Original"))
                {
                    String original = fel.GetAttribute("Original");
                    if (fel.HasAttribute("Replacement"))
                    {
                        string rep = fel.GetAttribute("Replacement");

                        if (original != String.Empty && rep != String.Empty)
                        {
                            TemplateSubstitution nf = new TemplateSubstitution();
                            nf.Original = original;
                            nf.Replacement = rep;
                            Substitutions.Add(nf);
                        }
                    }
                }
            }
        }

        public void SaveAsTemplate()
        {
            // user templates are always saved in the same folder.
            // if it doesn't exist create it.
            string dir = PathManager.UserTemplatesFolder();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string templatefileName = Name.Trim().Replace(" ", "_");
            templatefileName = Path.Combine(dir, templatefileName + ".def");
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Defs");
            doc.AppendChild(docNode);

            XmlElement dnode = doc.CreateElement("ProjectDefinition");
            dnode.SetAttribute("Name", Name);
            docNode.AppendChild(dnode);

            XmlElement descnode = doc.CreateElement("Description");
            descnode.InnerText = Description;
            dnode.AppendChild(descnode);

            XmlElement inifilenode = doc.CreateElement("InitialFile");
            inifilenode.InnerText = InitialFile;
            dnode.AppendChild(inifilenode);

            foreach (ProjectTemplateFolder nf in folders)
            {
                XmlElement foldernode = doc.CreateElement("Folder");
                dnode.AppendChild(foldernode);
                nf.SaveAsTemplate(foldernode, doc);
            }

            doc.Save(templatefileName);
        }
    }
}