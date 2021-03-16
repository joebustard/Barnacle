using System;
using System.Collections.Generic;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplateDefinition
    {
        internal String Name { get; set; }
        internal String Description { get; set; }
        internal List<ProjectTemplateFolder> folders;

        internal List<ProjectTemplateFolder> Folders
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

        private List<TemplateSubstitution> substitutions;

        internal List<TemplateSubstitution> Substitutions
        {
            get { return substitutions; }
            set
            {
                if (substitutions != value)
                {
                    substitutions = value;
                }
            }
        }

        public ProjectTemplateDefinition()
        {
            Name = String.Empty;
            Description = String.Empty;
            Folders = new List<ProjectTemplateFolder>();
            substitutions = new List<TemplateSubstitution>();
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
                        ProjectTemplateFolder nf = new ProjectTemplateFolder();
                        nf.Name = folderName;
                        Folders.Add(nf);

                        XmlNodeList fileNodes = fel.SelectNodes("File");
                        foreach (XmlNode filn in fileNodes)
                        {
                            ProjectTemplateFile pf = new ProjectTemplateFile();
                            pf.Load(doc, filn);
                            nf.Files.Add(pf);
                        }
                    }
                }
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
    }
}