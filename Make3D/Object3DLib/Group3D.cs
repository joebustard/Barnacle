using CSGLib;
using HullLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

//using TestCSGLib;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Solid = CSGLib.Solid;

namespace Barnacle.Object3DLib
{
    public class Group3D : Object3D
    {
        private Scale3D groupScale;
        private Object3D leftObject;
        private Solid leftSolid;
        private BooleanModeller modeller;
        private Object3D rightObject;

        private Solid rightSolid;

        public Group3D()
        {
            leftObject = null;
            leftSolid = null;

            rightObject = null;
            rightSolid = null;
            XmlType = "groupobj";
        }

        public Object3D LeftObject
        {
            get
            {
                return leftObject;
            }
            set
            {
                if (leftObject != value)
                {
                    leftObject = value;
                }
            }
        }

        public Object3D RightObject
        {
            get
            {
                return rightObject;
            }
            set
            {
                if (rightObject != value)
                {
                    rightObject = value;
                }
            }
        }

        public override Object3D Clone()
        {
            Group3D res = new Group3D();
            res.Name = this.Name;
            res.Description = this.Description;
            res.primType = this.primType;
            res.Exportable = this.Exportable;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);

            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Color = this.Color;
            res.leftObject = this.leftObject.Clone();
            res.rightObject = this.rightObject.Clone();
            // res.RelativeObjectVertices = new Point3DCollection();
            res.RelativeObjectVertices = new List<P3D>();
            //  foreach (Point3D po in this.RelativeObjectVertices)
            foreach (P3D po in this.RelativeObjectVertices)
            {
                //Point3D pn = new Point3D(po.X, po.Y, po.Z);
                P3D pn = new P3D(po.X, po.Y, po.Z);
                res.RelativeObjectVertices.Add(po);
            }
            res.AbsoluteObjectVertices = new Point3DCollection();
            foreach (Point3D po in this.AbsoluteObjectVertices)
            {
                Point3D pn = new Point3D(po.X, po.Y, po.Z);
                res.AbsoluteObjectVertices.Add(po);
            }
            res.AbsoluteBounds = new Bounds3D();
            res.AbsoluteBounds.Lower = new Point3D(this.AbsoluteBounds.Lower.X, this.AbsoluteBounds.Lower.Y, this.AbsoluteBounds.Lower.Z);
            res.AbsoluteBounds.Upper = new Point3D(this.AbsoluteBounds.Upper.X, this.AbsoluteBounds.Upper.Y, this.AbsoluteBounds.Upper.Z);
            res.TriangleIndices = new Int32Collection();
            for (int i = 0; i < TriangleIndices.Count; i++)
            {
                res.TriangleIndices.Add(this.TriangleIndices[i]);
            }
            return res;
        }

        public override Object3D ConvertToMesh()
        {
            Object3D neo = new Object3D();
            neo.Name = Name;
            neo.Description = Description;
            neo.PrimType = "Mesh";
            neo.Color = Color;
            neo.Scale = new Scale3D(Scale.X, Scale.Y, Scale.Z);

            neo.Position = new Point3D(Position.X, Position.Y, Position.Z);
            // var hullMaker = new ConvexHullCalculator();

            //  hullMaker.GeneratePoint3DHull(RelativeObjectVertices, TriangleIndices);
            neo.RelativeObjectVertices = RelativeObjectVertices;
            RelativeObjectVertices = null;
            neo.TriangleIndices = TriangleIndices;
            TriangleIndices = null;
            neo.AbsoluteObjectVertices = AbsoluteObjectVertices;
            AbsoluteObjectVertices = null;
            neo.AbsoluteBounds = AbsoluteBounds;
            AbsoluteBounds = null;
            //neo.Unitise();
            return neo;
        }

        public bool Init()
        {
            bool result = false;
            if (leftObject != null && rightObject != null)
            {
                if (leftObject.RelativeObjectVertices != null && leftObject.RelativeObjectVertices.Count > 2)
                {
                    if (rightObject.RelativeObjectVertices != null && rightObject.RelativeObjectVertices.Count > 2)
                    {
                        Color = leftObject.Color;
                        Bounds3D leftBnd = leftObject.AbsoluteBounds;
                        Bounds3D rightBnd = rightObject.AbsoluteBounds;
                        Point3D leftPnt = leftObject.Position;
                        Point3D rightPnt = rightObject.Position;
                        Bounds3D combined = new Bounds3D();
                        combined.Add(leftBnd);
                        combined.Add(rightBnd);

                        result = PerformOperation();
                    }
                }
            }

            return result;
        }

        public bool PerformOperation()
        {
            bool res = false;
            absoluteBounds = new Bounds3D();
            leftObject.RelativeToAbsolute();
            rightObject.RelativeToAbsolute();
            Logger.Log($"Left Solid {leftObject.AbsoluteObjectVertices.Count} pnts\r\n============\r\n");
            leftSolid = new Solid(leftObject.AbsoluteObjectVertices, leftObject.TriangleIndices, false);

            Logger.Log($"Right Solid {rightObject.AbsoluteObjectVertices.Count} pnts\r\n============\r\n");
            rightSolid = new Solid(rightObject.AbsoluteObjectVertices, rightObject.TriangleIndices, false);

            modeller = new BooleanModeller(leftSolid, rightSolid);
            if (modeller.State == true)
            {
                Solid result = null;
                switch (PrimType)
                {
                    case "groupunion":
                        {
                            result = modeller.GetUnion();
                        }
                        break;

                    case "groupdifference":
                        {
                            result = modeller.GetDifference();
                        }
                        break;

                    case "groupreversedifference":
                        {
                            result = modeller.GetReverseDifference();
                        }
                        break;

                    case "groupintersection":
                        {
                            result = modeller.GetIntersection();
                        }
                        break;
                }

                if (result != null)
                {
                    AbsoluteObjectVertices = new Point3DCollection();
                    TriangleIndices = new System.Windows.Media.Int32Collection();
                    Vector3D[] vc = result.GetVertices();
                    if (vc.GetLength(0) > 0)
                    {
                        foreach (Vector3D v in vc)
                        {
                            Point3D p = new Point3D(v.X, v.Y, v.Z);
                            AbsoluteObjectVertices.Add(p);
                            absoluteBounds.Adjust(p);
                        }
                        int[] ids = result.GetIndices();
                        for (int i = 0; i < ids.Length; i++)
                        {
                            TriangleIndices.Add(ids[i]);
                        }

                        double nx = absoluteBounds.MidPoint().X;
                        double ny = absoluteBounds.MidPoint().Y;
                        double nz = absoluteBounds.MidPoint().Z;
                        Position = new Point3D(nx, ny, nz);

                        AbsoluteToRelative();
                        SetMesh();
                        modeller = null;
                        GC.Collect();
                        res = true;
                    }
                }
            }
            return res;
        }

        public override void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            Name = ele.GetAttribute("Name");
            Description = ele.GetAttribute("Description");
            string cl = ele.GetAttribute("Colour");
            this.Color = (Color)ColorConverter.ConvertFromString(cl);
            PrimType = ele.GetAttribute("Primitive");
            if (ele.HasAttribute("Exportable"))
            {
                Exportable = Convert.ToBoolean(ele.GetAttribute("Exportable"));
            }
            else
            {
                Exportable = true;
            }
            XmlNode pn = nd.SelectSingleNode("Position");
            Point3D p = new Point3D();
            p.X = GetDouble(pn, "X");
            p.Y = GetDouble(pn, "Y");
            p.Z = GetDouble(pn, "Z");
            Position = p;

            XmlNode sn = nd.SelectSingleNode("Scale");
            Scale3D sc = new Scale3D();
            sc.X = GetDouble(sn, "X");
            sc.Y = GetDouble(sn, "Y");
            sc.Z = GetDouble(sn, "Z");
            Scale = sc;
            //RelativeObjectVertices = new Point3DCollection();
            RelativeObjectVertices = new List<P3D>();
            XmlNodeList vtl = nd.SelectNodes("v");
            foreach (XmlNode vn in vtl)
            {
                XmlElement el = vn as XmlElement;
                // Point3D pv = new Point3D();
                P3D pv = new P3D();
                pv.X = (float)GetDouble(el, "X");
                pv.Y = (float)GetDouble(el, "Y");
                pv.Z = (float)GetDouble(el, "Z");
                RelativeObjectVertices.Add(pv);
            }

            TriangleIndices = new Int32Collection();
            XmlNodeList ftl = nd.SelectNodes("f");
            foreach (XmlNode vn in ftl)
            {
                XmlElement el = vn as XmlElement;
                string tri = el.GetAttribute("v");
                String[] words = tri.Split(',');
                TriangleIndices.Add(Convert.ToInt32(words[0]));
                TriangleIndices.Add(Convert.ToInt32(words[1]));
                TriangleIndices.Add(Convert.ToInt32(words[2]));
            }

            Remesh();
            XmlNodeList nodes = nd.ChildNodes;
            bool left = true;
            foreach (XmlNode sub in nodes)
            {
                if (sub.Name == "obj" || sub.Name == "groupobj")
                {
                    if (left)
                    {
                        leftObject = ReadObject(sub);
                        left = false;
                    }
                    else
                    {
                        rightObject = ReadObject(sub);
                    }
                }
            }

            //   Init();
            Position = p;
            Scale = sc;
            RelativeToAbsolute();
            SetMesh();
        }

        public override void ReadBinary(BinaryReader reader)
        {
            Name = reader.ReadString();
            Description = reader.ReadString();

            byte A, R, G, B;
            A = reader.ReadByte();
            R = reader.ReadByte();
            G = reader.ReadByte();
            B = reader.ReadByte();
            Color = Color.FromArgb(A, R, G, B);
            PrimType = reader.ReadString();
            Exportable = reader.ReadBoolean();

            double x, y, z;
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            Position = new Point3D(x, y, z);
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            Scale = new Scale3D(x, y, z);
            EditorParameters.ReadBinary(reader);
            //RelativeObjectVertices = new Point3DCollection();
            RelativeObjectVertices = new List<P3D>();
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                // RelativeObjectVertices.Add(new Point3D(x, y, z));
                RelativeObjectVertices.Add(new P3D(x, y, z));
            }

            count = reader.ReadInt32();
            TriangleIndices = new Int32Collection();
            for (int i = 0; i < count; i++)
            {
                int index = reader.ReadInt32();
                TriangleIndices.Add(index);
            }
            byte type = reader.ReadByte();
            switch (type)
            {
                case 0:
                    {
                        Object3D ob = new Object3D();
                        ob.ReadBinary(reader);
                        leftObject = ob;
                    }
                    break;

                case 1:
                    {
                        Group3D ob = new Group3D();
                        ob.ReadBinary(reader);
                        leftObject = ob;
                    }
                    break;
            }
            type = reader.ReadByte();
            switch (type)
            {
                case 0:
                    {
                        Object3D ob = new Object3D();
                        ob.ReadBinary(reader);
                        rightObject = ob;
                    }
                    break;

                case 1:
                    {
                        Group3D ob = new Group3D();
                        ob.ReadBinary(reader);
                        rightObject = ob;
                    }
                    break;
            }

            Remesh();
            SetMesh();
        }

        public override void Remesh()
        {
            if (RelativeObjectVertices != null && RelativeObjectVertices.Count > 0)
            {
                RelativeToAbsolute();

                SetMesh();
            }
        }

        public override XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = null;
            if (leftObject != null && rightObject != null)
            {
                ele = doc.CreateElement(XmlType);
                docNode.AppendChild(ele);
                ele.SetAttribute("Name", Name);
                ele.SetAttribute("Description", Description);
                ele.SetAttribute("Colour", Color.ToString());
                ele.SetAttribute("Primitive", PrimType);
                ele.SetAttribute("Exportable", Exportable.ToString());
                XmlElement pos = doc.CreateElement("Position");
                pos.SetAttribute("X", Position.X.ToString());
                pos.SetAttribute("Y", Position.Y.ToString());
                pos.SetAttribute("Z", Position.Z.ToString());
                ele.AppendChild(pos);

                XmlElement scl = doc.CreateElement("Scale");
                scl.SetAttribute("X", Scale.X.ToString());
                scl.SetAttribute("Y", Scale.Y.ToString());
                scl.SetAttribute("Z", Scale.Z.ToString());
                ele.AppendChild(scl);
                //  foreach (Point3D v in RelativeObjectVertices)
                foreach (P3D v in RelativeObjectVertices)
                {
                    XmlElement vertEle = doc.CreateElement("v");
                    vertEle.SetAttribute("X", v.X.ToString("F5"));
                    vertEle.SetAttribute("Y", v.Y.ToString("F5"));
                    vertEle.SetAttribute("Z", v.Z.ToString("F5"));
                    ele.AppendChild(vertEle);
                }
                for (int i = 0; i < TriangleIndices.Count; i += 3)
                {
                    XmlElement faceEle = doc.CreateElement("f");
                    faceEle.SetAttribute("v", TriangleIndices[i].ToString() + "," +
                                              TriangleIndices[i + 1].ToString() + "," +
                                              TriangleIndices[i + 2].ToString());
                    ele.AppendChild(faceEle);
                }
                leftObject.Write(doc, ele);
                rightObject.Write(doc, ele);
            }
            return ele;
        }

        public override void WriteBinary(BinaryWriter writer)
        {
            writer.Write((byte)1); // need a tag of some sort for deserialisation
            writer.Write(Name);
            writer.Write(Description);
            writer.Write(Color.A);
            writer.Write(Color.R);
            writer.Write(Color.G);
            writer.Write(Color.B);
            writer.Write(PrimType);
            writer.Write(Exportable);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);
            EditorParameters.WriteBinary(writer);
            writer.Write(RelativeObjectVertices.Count);
            // foreach (Point3D v in RelativeObjectVertices)
            foreach (P3D v in RelativeObjectVertices)
            {
                writer.Write(v.X);
                writer.Write(v.Y);
                writer.Write(v.Z);
            }

            writer.Write(TriangleIndices.Count);
            for (int i = 0; i < TriangleIndices.Count; i++)
            {
                writer.Write(TriangleIndices[i]);
            }
            leftObject.WriteBinary(writer);
            rightObject.WriteBinary(writer);
        }

        private void AbsoluteToRelative()
        {
            GetScaleFromAbsoluteExtent();

            //RelativeObjectVertices = new Point3DCollection();
            RelativeObjectVertices = new List<P3D>();
            foreach (Point3D pnt in AbsoluteObjectVertices)
            {
                // Point3D p = new Point3D((pnt.X - Position.X), (pnt.Y - Position.Y), (pnt.Z - Position.Z));
                P3D p = new P3D((pnt.X - Position.X), (pnt.Y - Position.Y), (pnt.Z - Position.Z));
                RelativeObjectVertices.Add(p);
            }
            scale = groupScale;
        }

        private int AddVertex(Point3DCollection av, double x, double y, double z)
        {
            int i = -1;
            bool found = false;

            for (i = 0; i < av.Count && found == false; i++)
            {
                if (PointUtils.equals(av[i], x, y, z))
                {
                    found = true;
                }
            }
            if (!found)
            {
                i = av.Count;
                av.Add(new Point3D(x, y, z));
                absoluteBounds.Adjust(av[i]);
            }
            return i;
        }

        private void GetScaleFromAbsoluteExtent()
        {
            // a quick function to convert the coordinates of an object read in, into a unit sized object
            // centered on 0,0,0
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            foreach (Point3D pt in AbsoluteObjectVertices)
            {
                if (pt.X < min.X)
                {
                    min.X = pt.X;
                }
                if (pt.Y < min.Y)
                {
                    min.Y = pt.Y;
                }
                if (pt.Z < min.Z)
                {
                    min.Z = pt.Z;
                }

                if (pt.X > max.X)
                {
                    max.X = pt.X;
                }
                if (pt.Y > max.Y)
                {
                    max.Y = pt.Y;
                }
                if (pt.Z > max.Z)
                {
                    max.Z = pt.Z;
                }
            }

            double scaleX = 1.0;
            double dx = max.X - min.X;
            if (dx > 0)
            {
                scaleX = dx;
            }
            double scaleY = 1.0;
            double dY = max.Y - min.Y;
            if (dY > 0)
            {
                scaleY = dY;
            }

            double scaleZ = 1.0;
            double dZ = max.Z - min.Z;
            if (dZ > 0)
            {
                scaleZ = dZ;
            }
            groupScale = new Scale3D(scaleX, scaleY, scaleZ);
        }

        private Object3D ReadObject(XmlNode nd)
        {
            if (nd.Name == "obj")
            {
                Object3D obj = new Object3D();
                obj.Read(nd);

                obj.SetMesh();
                return obj;
            }
            if (nd.Name == "groupobj")
            {
                Group3D obj = new Group3D();
                obj.Read(nd);

                obj.SetMesh();
                return obj;
            }
            return null;
        }

        /*
        private TestCSGLib.Solid SolidFromPoints(Point3DCollection pnts, Int32Collection triangleIndices)
        {
            TestCSGLib.Solid res = new TestCSGLib.Solid();
            if (res.Polygons == null)
            {
                res.Polygons = new List<Polygon>();
            }
            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                TestCSGLib.Vertex[] verts = new TestCSGLib.Vertex[3];
                int j = triangleIndices[i];
                int k = triangleIndices[i + 1];
                int l = triangleIndices[i + 2];
                verts[0] = new TestCSGLib.Vertex(new TestCSGLib.Vector3D(pnts[j].X, pnts[j].Y, pnts[j].Z), new Vector2D(0, 0));
                verts[1] = new TestCSGLib.Vertex(new TestCSGLib.Vector3D(pnts[k].X, pnts[k].Y, pnts[k].Z), new Vector2D(0, 0));
                verts[2] = new TestCSGLib.Vertex(new TestCSGLib.Vector3D(pnts[l].X, pnts[l].Y, pnts[l].Z), new Vector2D(0, 0));
                Polygon pl = new Polygon(verts);
                res.Polygons.Add(pl);
            }
            return res;
        }

        private void SolidToAbsolute(TestCSGLib.Solid sl3, Point3DCollection av, Int32Collection tri)
        {
            av.Clear();
            tri.Clear();
            absoluteBounds = new Bounds3D();
            foreach (TestCSGLib.Polygon pl in sl3.Polygons)
            {
                if (pl.Vertices.Count == 3)
                {
                    int i0 = AddVertex(av, pl.Vertices[0].Pos.X, pl.Vertices[0].Pos.Y, pl.Vertices[0].Pos.Z);
                    int i1 = AddVertex(av, pl.Vertices[1].Pos.X, pl.Vertices[1].Pos.Y, pl.Vertices[1].Pos.Z);
                    int i2 = AddVertex(av, pl.Vertices[2].Pos.X, pl.Vertices[2].Pos.Y, pl.Vertices[2].Pos.Z);
                    tri.Add(i0);
                    tri.Add(i1);
                    tri.Add(i2);
                }
                else
                {
                    int i0 = AddVertex(av, pl.Vertices[0].Pos.X, pl.Vertices[0].Pos.Y, pl.Vertices[0].Pos.Z);
                    int i1 = AddVertex(av, pl.Vertices[1].Pos.X, pl.Vertices[1].Pos.Y, pl.Vertices[1].Pos.Z);
                    int i2 = AddVertex(av, pl.Vertices[2].Pos.X, pl.Vertices[2].Pos.Y, pl.Vertices[2].Pos.Z);
                    int i3 = AddVertex(av, pl.Vertices[3].Pos.X, pl.Vertices[3].Pos.Y, pl.Vertices[3].Pos.Z);

                    tri.Add(i0);
                    tri.Add(i1);
                    tri.Add(i2);

                    tri.Add(i0);
                    tri.Add(i2);
                    tri.Add(i3);
                }
            }
        }
        */
    }
}