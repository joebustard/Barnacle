using System;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class ProjectSettings
    {
        public string Description { get; set; }
        public string ExportRootName { get; set; }
        public string BaseScale { get; set; }
        public Point3D ExportRotation { get; set; }
        public bool ExportAxisSwap { get; set; }
        public bool FloorAll { get; internal set; }
        public string ExportScale { get; set; }

        public ProjectSettings()
        {
            BaseScale = "1";
            ExportScale = "1";
            ExportRootName = "<FileName>";
            Description = "A 3D model created by Barnacle";
            ExportRotation = new Point3D(0, 0, 0);
            ExportAxisSwap = true;
            FloorAll = true;
        }

        internal void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("Settings");
            docNode.AppendChild(ele);
            ele.SetAttribute("ExportRootName", ExportRootName);
            ele.SetAttribute("BaseScale", BaseScale);
            ele.SetAttribute("ExportScale", ExportScale);
            ele.SetAttribute("SwapAxis", ExportAxisSwap.ToString());
            ele.SetAttribute("FloorAll", FloorAll.ToString());
            XmlElement rot = doc.CreateElement("Rotation");
            rot.SetAttribute("X", ExportRotation.X.ToString());
            rot.SetAttribute("Y", ExportRotation.Y.ToString());
            rot.SetAttribute("Z", ExportRotation.Z.ToString());
            ele.AppendChild(rot);
            XmlElement des = doc.CreateElement("Description");
            des.InnerText = Description;
        }

        internal void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele.HasAttribute("ExportRootName"))
            {
                ExportRootName = ele.GetAttribute("ExportRootName");
            }
            if (ele.HasAttribute("BaseScale"))
            {
                BaseScale = ele.GetAttribute("BaseScale");
            }
            if (ele.HasAttribute("ExportScale"))
            {
                ExportScale = ele.GetAttribute("ExportScale");
            }
            if (ele.HasAttribute("SwapAxis"))
            {
                string s = ele.GetAttribute("SwapAxis");
                ExportAxisSwap = Convert.ToBoolean(s);
            }
            if (ele.HasAttribute("FloorAll"))
            {
                string s = ele.GetAttribute("FloorAll");
                FloorAll = Convert.ToBoolean(s);
            }
            XmlNode rt = nd.SelectSingleNode("Rotation");
            if (rt != null)
            {
                Point3D r = new Point3D();
                r.X = GetDouble(rt, "X");
                r.Y = GetDouble(rt, "Y");
                r.Z = GetDouble(rt, "Z");
                ExportRotation = r;
            }
            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText;
            }
        }

        protected double GetDouble(XmlNode pn, string v)
        {
            XmlElement el = pn as XmlElement;
            string val = el.GetAttribute(v);
            return Convert.ToDouble(val);
        }
    }
}