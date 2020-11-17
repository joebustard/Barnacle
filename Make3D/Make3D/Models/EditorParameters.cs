using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Make3D.Models
{
    public class EditorParameters
    {
        public List<EditorParameter> Parameters { get; set; }
        public String ToolName { get; set; }
        public bool HasContent
        {
            get
            {
                bool res = false;
                if ( ToolName != "" && Parameters.Count > 0)
                {
                    res = true;
                }
                return res;
            }
        }
        public EditorParameters()
        {
            Parameters = new List<EditorParameter>();
            ToolName = "";
        }

        internal void Set(string key, string val)
        {
            bool found = false;
            foreach( EditorParameter p in Parameters)
            {
                if ( p.Name.ToLower() == key.ToLower())
                {
                    p.Value = val;
                    found = true;
                    break;

                }
            }
            if ( !found)
            {
                Parameters.Add(new EditorParameter(key, val));
            }
        }

        internal string Get(string key)
        {
            string res = "";
            foreach (EditorParameter p in Parameters)
            {
                if (p.Name.ToLower() == key.ToLower())
                {
                    res = p.Value;
                    break;
                }
            }
            return res;
        }

        internal void Write(XmlDocument doc, XmlElement ele)
        {
            XmlElement prms = doc.CreateElement("EditorParameters");
            prms.SetAttribute("ToolName", ToolName);
            foreach (EditorParameter p in Parameters)
            {
                p.Write(doc, prms);
            }
            ele.AppendChild(prms);
        }
        internal void Load( XmlElement ele)
        {
            Parameters.Clear();
            if (ele.HasAttribute("ToolName"))
            {
                ToolName = ele.GetAttribute("ToolName");
            }
            XmlNodeList nds = ele.SelectNodes("P");
            foreach ( XmlNode n in nds)
            {
                XmlElement pel = n as XmlElement;
                Parameters.Add(new EditorParameter(pel.GetAttribute("Name"), pel.GetAttribute("Val")));
            }
        }
    }
}

