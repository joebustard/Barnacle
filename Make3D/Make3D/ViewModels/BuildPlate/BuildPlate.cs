using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Make3D.ViewModel.BuildPlates
{
    internal class BuildPlate
    {
        public BuildPlate()
        {
            PrinterName = "Default";
            Width = 210;
            Height = 210;
            BorderThickness = 5;
            BorderColour = Colors.CadetBlue;
        }

        public Color BorderColour { get; set; }
        public double BorderThickness { get; set; }
        public double Height { get; set; }
        public String PrinterName { get; set; }
        public double Width { get; set; }

        internal virtual XmlElement Read(XmlDocument doc, XmlElement ele)
        {
            PrinterName = ele.GetAttribute("PrinterName");
            Width = GetDouble(ele, "Width");
            Height = GetDouble(ele, "Height");
            BorderColour = Colors.Blue;
            if (ele.HasAttribute("Colour"))
            {
                string s = ele.GetAttribute("Colour");
                BorderColour = (Color)ColorConverter.ConvertFromString(s);
            }
            BorderThickness = GetDouble(ele, "BorderThickness");
            return ele;
        }

        internal virtual XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("BuildPlate");
            docNode.AppendChild(ele);
            ele.SetAttribute("PrinterName", PrinterName);
            ele.SetAttribute("Width", Width.ToString());
            ele.SetAttribute("Height", Height.ToString());
            ele.SetAttribute("Colour", BorderColour.ToString());
            ele.SetAttribute("BorderThickness", BorderThickness.ToString());
            return ele;
        }

        private double GetDouble(XmlElement ele, string nm)
        {
            double res = 0;
            if (ele.HasAttribute(nm))
            {
                try
                {
                    string s = ele.GetAttribute(nm);
                    res = Convert.ToDouble(s);
                }
                catch
                {
                }
            }
            return res;
        }
    }
}