using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Object3DLib
{
    public class ReferenceObject3D : Object3D
    {
        public ReferenceObject3D()
        {
            Reference = new ExternalReference();
            XmlType = "refobj";
            RefValid = false;
        }

        public ExternalReference Reference
        {
            get;
            set;
        }

        public bool RefValid { get; set; }

        public void BaseRead(XmlNode nd)
        {
            base.Read(nd);
        }

        public override Object3D ConvertToMesh()
        {
            PrimType = "Mesh";

            return this;
        }

        public override bool IsSizable()
        {
            return false;
        }

        public override void Read(XmlNode nd)
        {
            RefValid = false;
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
                XmlElement src = FindExternalModel(Name, Reference.Path);
                if (src == null)
                {
                    MessageBox.Show("Cant find object " + Name + " in file " + Reference.Path);
                }
                else
                {
                    if (src.Name == "obj")
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
                        RefValid = true;
                    }
                }
            }
        }

        public override XmlElement Write(XmlDocument doc, XmlElement docNode)
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

        public override void WriteBinary(BinaryWriter writer)
        {
            writer.Write((byte)2); // need a tag of some sort for deserialisation
            writer.Write(Name);

            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            writer.Write(rotation.X);
            writer.Write(rotation.Y);
            writer.Write(rotation.Z);

            writer.Write(Reference.Path);
            writer.Write(Reference.TimeStamp.Year);
            writer.Write(Reference.TimeStamp.Month);
            writer.Write(Reference.TimeStamp.Day);
            writer.Write(Reference.TimeStamp.Minute);
            writer.Write(Reference.TimeStamp.Second);
        }
    }
}