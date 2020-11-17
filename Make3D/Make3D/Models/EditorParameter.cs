using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Make3D.Models
{
    public class EditorParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public EditorParameter( String n, string v)
        {
            Name = n;
            Value = v;
        }

        internal void Write(XmlDocument doc, XmlElement prms)
        {
            XmlElement p = doc.CreateElement("P");
            p.SetAttribute("Name", Name);
            p.SetAttribute("Val", Value);
            prms.AppendChild(p);
        }
    }
}
