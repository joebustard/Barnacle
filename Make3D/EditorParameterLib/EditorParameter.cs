using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.EditorParameterLib
{
    public class EditorParameter
    {
        public EditorParameter(String n, string v)
        {
            Name = n;
            Value = v;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        internal void ReadBinary(BinaryReader reader)
        {
            Name = reader.ReadString();
            Value = reader.ReadString();
        }

        internal void Write(XmlDocument doc, XmlElement prms)
        {
            XmlElement p = doc.CreateElement("P");
            p.SetAttribute("Name", Name);
            p.SetAttribute("Val", Value);
            prms.AppendChild(p);
        }

        internal void WriteBinary(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}