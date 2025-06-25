using System;
using System.Collections.Generic;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplateFile
    {
        public ProjectTemplateFile()
        {
            Name = String.Empty;
            Source = String.Empty;
            Attributes = new Dictionary<string, string>();
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