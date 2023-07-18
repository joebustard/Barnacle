using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Barnacle.EditorParameterLib
{
    public class EditorParameters
    {
        public EditorParameters()
        {
            Parameters = new List<EditorParameter>();
            ToolName = "";
        }

        public bool HasContent
        {
            get
            {
                bool res = false;
                if (ToolName != "" && Parameters.Count > 0)
                {
                    res = true;
                }
                return res;
            }
        }

        public List<EditorParameter> Parameters { get; set; }
        public String ToolName { get; set; }

        public string Get(string key)
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

        public bool GetBoolean(string key, bool def = false)
        {
            bool res = def;
            string s = Get(key);
            if (s != "")
            {
                try
                {
                    res = Convert.ToBoolean(s);
                }
                catch
                {
                    // if the conversion fails just use the supplied default
                }
            }
            return res;
        }

        public double GetDouble(string key, double def = 0.0)
        {
            double res = def;
            string s = Get(key);
            if (s != "")
            {
                try
                {
                    res = Convert.ToDouble(s);
                }
                catch
                {
                    // if the conversion fails just use the supplied default
                }
            }
            return res;
        }

        public int GetInt(string key, int def = 0)
        {
            int res = def;
            string s = Get(key);
            if (s != "")
            {
                try
                {
                    res = Convert.ToInt32(s);
                }
                catch
                {
                    // if the conversion fails just use the supplied default
                }
            }
            return res;
        }

        public void Load(XmlElement ele)
        {
            Parameters.Clear();
            if (ele.HasAttribute("ToolName"))
            {
                ToolName = ele.GetAttribute("ToolName");
            }
            XmlNodeList nds = ele.SelectNodes("P");
            foreach (XmlNode n in nds)
            {
                XmlElement pel = n as XmlElement;
                Parameters.Add(new EditorParameter(pel.GetAttribute("Name"), pel.GetAttribute("Val")));
            }
        }

        public void ReadBinary(BinaryReader reader)
        {
            ToolName = reader.ReadString();
            Parameters = new List<EditorParameter>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                EditorParameter p = new EditorParameter("", "");
                p.ReadBinary(reader);
                Parameters.Add(p);
            }
        }

        public void Set(string key, string val)
        {
            bool found = false;
            foreach (EditorParameter p in Parameters)
            {
                if (p.Name.ToLower() == key.ToLower())
                {
                    p.Value = val;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Parameters.Add(new EditorParameter(key, val));
            }
        }

        public void Set(string key, double val)
        {
            Set(key, val.ToString());
        }

        public void Set(string key, bool val)
        {
            Set(key, val.ToString());
        }

        public void Set(string key, int val)
        {
            Set(key, val.ToString());
        }

        public void Write(XmlDocument doc, XmlElement ele)
        {
            XmlElement prms = doc.CreateElement("EditorParameters");
            prms.SetAttribute("ToolName", ToolName);
            foreach (EditorParameter p in Parameters)
            {
                p.Write(doc, prms);
            }
            ele.AppendChild(prms);
        }

        public void WriteBinary(BinaryWriter writer)
        {
            writer.Write(ToolName);
            writer.Write(Parameters.Count);
            foreach (EditorParameter p in Parameters)
            {
                p.WriteBinary(writer);
            }
        }
    }
}