using Barnacle.EditorParameterLib;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Object3DLib
{
    public class ReferenceGroup3D : Group3D
    {
        public ReferenceGroup3D()
        {
            Reference = new ExternalReference();
            XmlType = "refgroupobj";
            RefValid = false;
        }

        public ExternalReference Reference
        {
            get;
            set;
        }

        public bool RefValid
        {
            get; set;
        }

        public void BaseRead(XmlNode nd)
        {
            base.Read(nd);
        }

        public override Object3D Clone(bool useIndices = false)
        {
            ReferenceGroup3D res = new ReferenceGroup3D();
            res.Reference.Path = this.Reference.Path;
            res.Reference.TimeStamp = this.Reference.TimeStamp;
            res.Reference.SourceObject = this.Reference.SourceObject;
            res.Name = this.Name;
            res.Description = this.Description;
            res.primType = this.primType;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);

            res.Color = this.Color;
            res.Exportable = this.Exportable;
            res.LockAspectRatio = this.LockAspectRatio;

            foreach (P3D p in this.RelativeObjectVertices)
            {
                P3D p3d = new P3D();
                p3d.X = p.X;
                p3d.Y = p.Y;
                p3d.Z = p.Z;
                res.RelativeObjectVertices.Add(p3d);
            }
            // THIS IS A HACK TO GET aroubd an async issue.
            if (useIndices)
            {
                foreach (int i in this.Indices)
                {
                    res.TriangleIndices.Add(i);
                }
            }
            else
            {
                foreach (int i in this.TriangleIndices)
                {
                    res.TriangleIndices.Add(i);
                }
            }

            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Rotation = new Point3D(this.rotation.X, this.rotation.Y, this.rotation.Z);
            res.Remesh();
            return res;
        }

        public override bool IsSizable()
        {
            return false;
        }

        public override bool Read(XmlNode nd, bool reportMissing = true)
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
                Reference.SourceObject = (refo as XmlElement).GetAttribute("SourceObject");

                // read in the definition of the object from the source file, not this file
                XmlElement src = FindExternalModel(Reference.SourceObject, Reference.Path);
                if (src == null)
                {
                    if (reportMissing)
                    {
                        MessageBox.Show("Cant find object " + Name + " in file " + Reference.Path);
                    }
                }
                else
                {
                    if (src.Name == "groupobj")
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
            return RefValid;
        }

        public override void ReadBinary(BinaryReader reader)
        {
            Name = reader.ReadString();
            double x, y, z;
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            Position = new Point3D(x, y, z);
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            rotation = new Point3D(x, y, z);
            Reference.Path = reader.ReadString();
            int year = reader.ReadInt32();
            int month = reader.ReadInt32();
            int day = reader.ReadInt32();
            int hour = reader.ReadInt32();
            int minute = reader.ReadInt32();
            int second = reader.ReadInt32();
            Reference.TimeStamp = new DateTime(year, month, day, hour, minute, second);
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
            refele.SetAttribute("SourceObject", Reference.SourceObject);
            ele.AppendChild(refele);
            return ele;
        }

        public override void WriteBinary(BinaryWriter writer)
        {
            writer.Write((byte)3); // need a tag of some sort for deserialisation
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