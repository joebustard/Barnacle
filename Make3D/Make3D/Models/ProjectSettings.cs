using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Make3D.Models
{
    public class ProjectSettings
    {
        public string Description { get; set; }
        public string ExportRootName { get; set; }
        public string BaseScale { get; set; }
        public ProjectSettings()
        {
            BaseScale = "1";
            ExportRootName = "<FileName>";
            Description = "A 3D model created by Barnacle";
        }

        internal void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("Settings");
            docNode.AppendChild(ele);
            ele.SetAttribute("ExportRootName", ExportRootName);
            ele.SetAttribute("BaseScale", BaseScale);
            ele.InnerText = Description;
        }

        internal void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if ( ele.HasAttribute("ExportRootName"))
            {
                ExportRootName = ele.GetAttribute("ExportRootName");
            }
            if (ele.HasAttribute("BaseScale"))
            {
                BaseScale = ele.GetAttribute("BaseScale");
            }
            Description = ele.InnerText;
        }
    }
}
