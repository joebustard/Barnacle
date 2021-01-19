using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TemplateLib
{
  
    public class ProjectDefinition
    {
        internal String Name { get; set; }
        internal String Description { get; set; }
        internal List<ProjectFolder> folders;
        internal List<ProjectFolder> Folders
        {
            get { return folders; }
            set
            {
                if ( value != folders)
                {
                    folders = value;
                }
            }
        }
        private List<Substitution> substitutions;
        internal List<Substitution> Substitutions
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

        public ProjectDefinition()
        {
            Name = String.Empty;
            Description = String.Empty;
            Folders = new List<ProjectFolder>();
            substitutions = new List<Substitution>();

        }

        internal void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if ( ele != null )
            {
                if ( ele.HasAttribute("Name"))
                {
                    Name = ele.GetAttribute("Name");
                }
            }
            XmlNode des = nd.SelectSingleNode("Description");
            if ( des != null)
            {
                Description = des.InnerText.Trim();
            }
            XmlNodeList flds = ele.SelectNodes("Folder");
            foreach ( XmlNode fl in flds)
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
                        foreach( XmlNode filn in fileNodes)
                        {
                            ProjectFile pf = new ProjectFile();
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
                            Substitution nf = new Substitution();
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
