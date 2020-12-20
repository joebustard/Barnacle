using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class ReferenceGroup3D : Group3D
    {
        public ExternalReference Reference
        {
            get;
            set;
        }

        public override bool IsSizable()
        {
            return false;
        }

        public ReferenceGroup3D()
        {
            Reference = new ExternalReference();
            XmlType = "refgroupobj";
        }

        internal void BaseRead(XmlNode nd)
        {
            base.Read(nd);
        }

        internal virtual void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            Name = ele.GetAttribute("Name");

            XmlNode refo = nd.SelectSingleNode("Reference");
            if (refo != null)
            {
                Reference.Path = (refo as XmlElement).GetAttribute("Path");
                string rs = (refo as XmlElement).GetAttribute("Timestamp");
                DateTime tm = DateTime.Parse(rs);
                Reference.TimeStamp = tm;

                // read in the definition of the object from the source file, not this file

                // read in the definition of the object from the source file, not this file
                XmlElement src = Document.FindExternalModel(Name, Reference.Path);
                if (src == null)
                {
                    MessageBox.Show("Cant find object " + Name + " in file " + Reference.Path);
                }
                else
                {
                    base.Read(src);
                    // read the position and rotation AFTER reading the base node as we wnat it from the
                    // reference object not the source
                    //
                    XmlNode pn = nd.SelectSingleNode("Position");
                    Point3D p = new Point3D();
                    p.X = GetDouble(pn, "X");
                    p.Y = GetDouble(pn, "Y");
                    p.Z = GetDouble(pn, "Z");
                    Position = p;

                    XmlNode sn = nd.SelectSingleNode("Rotation");
                    Point3D sc = new Point3D();
                    sc.X = GetDouble(sn, "X");
                    sc.Y = GetDouble(sn, "Y");
                    sc.Z = GetDouble(sn, "Z");
                    rotation = sc;
                    Remesh();
                }
            }
        }

        internal override XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement(XmlType);
            docNode.AppendChild(ele);
            ele.SetAttribute("Name", Name);

            XmlElement pos = doc.CreateElement("Position");
            pos.SetAttribute("X", Position.X.ToString());
            pos.SetAttribute("Y", Position.Y.ToString());
            pos.SetAttribute("Z", Position.Z.ToString());
            ele.AppendChild(pos);

            XmlElement scl = doc.CreateElement("Rotation");
            scl.SetAttribute("X", rotation.X.ToString());
            scl.SetAttribute("Y", rotation.Y.ToString());
            scl.SetAttribute("Z", rotation.Z.ToString());
            ele.AppendChild(scl);

            XmlElement refele = doc.CreateElement("Reference");
            refele.SetAttribute("Path", Reference.Path);
            refele.SetAttribute("Timestamp", Reference.TimeStamp.ToString());
            ele.AppendChild(refele);
            return ele;
        }
    }
}