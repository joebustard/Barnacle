using System;
using System.Xml;

namespace TemplateLib
{
    internal class ProjectTemplateFile
    {
        public String Name { get; set; }
        public String Source { get; set; }

        public ProjectTemplateFile()
        {
            Name = String.Empty;
            Source = String.Empty;
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

                if (ele.HasAttribute("Source"))
                {
                    Source = ele.GetAttribute("Source");
                }
            }
        }
    }
}