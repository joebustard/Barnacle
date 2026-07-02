using System;
using System.Collections.Generic;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplateFile
    {
        private List<TemplateSubstitution> substitutions;

        public ProjectTemplateFile()
        {
            Name = String.Empty;
            Source = String.Empty;
            Attributes = new Dictionary<string, string>();
            substitutions = new List<TemplateSubstitution>();
        }

        public Dictionary<string, string> Attributes
        {
            get; set;
        }

        public String Name
        {
            get; set;
        }

        public String Source
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

                if (ele.HasAttribute("Source"))
                {
                    Source = ele.GetAttribute("Source");
                }

                XmlAttributeCollection atrs = ele.Attributes;
                foreach (XmlAttribute a in atrs)
                {
                    Attributes[a.Name] = a.Value;
                }
            }
        }
    }
}