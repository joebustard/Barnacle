using CSGLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class Group3D : Object3D
    {
        private BooleanModeller modeller;
        private Object3D leftObject;

        public Object3D LeftObject
        {
            get { return leftObject; }
            set
            {
                if (leftObject != value)
                {
                    leftObject = value;
                }
            }
        }

        private Object3D rightObject;

        public Object3D RightObject
        {
            get { return rightObject; }
            set
            {
                if (rightObject != value)
                {
                    rightObject = value;
                }
            }
        }

        private Solid leftSolid;
        private Solid rightSolid;

        public Group3D()
        {
            leftObject = null;
            leftSolid = null;

            rightObject = null;
            rightSolid = null;
        }

        internal void Init()
        {
            if (leftObject != null && rightObject != null)
            {
                Position = new Point3D(leftObject.Position.X, leftObject.Position.Y, leftObject.Position.Z);

                // this is wrong, should join objects then divide bounds of new object somehow
                Scale = new Scale3D(1, 1, 1);
                Color = leftObject.Color;
                PerformOperation();
            }
        }

        internal void PerformOperation()
        {
            absoluteBounds = new Bounds3D();
            leftObject.RelativeToAbsolute();
            rightObject.RelativeToAbsolute();
            Logger.Log("Left Solid\r\n============\r\n");
            leftSolid = new Solid(leftObject.AbsoluteObjectVertices, leftObject.TriangleIndices, false);
            Logger.Log("Right Solid\r\n============\r\n");
            rightSolid = new Solid(rightObject.AbsoluteObjectVertices, rightObject.TriangleIndices, false);
            modeller = new BooleanModeller(leftSolid, rightSolid);
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
                AbsoluteToRelative();
                SetMesh();
            }
        }

        private void AbsoluteToRelative()
        {
            RelativeObjectVertices = new Point3DCollection();
            foreach (Point3D pnt in AbsoluteObjectVertices)
            {
                Point3D p = new Point3D(pnt.X - Position.X, pnt.Y - Position.Y, pnt.Z - Position.Z);
                RelativeObjectVertices.Add(p);
            }
        }

        internal override void Remesh()
        {
            RelativeToAbsolute();

            SetMesh();
        }

        internal override void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            Name = ele.GetAttribute("Name");
            Description = ele.GetAttribute("Description");
            string cl = ele.GetAttribute("Colour");
            this.Color = (Color)ColorConverter.ConvertFromString(cl);
            PrimType = ele.GetAttribute("Primitive");
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

            XmlNode rt = nd.SelectSingleNode("Rotation");
            Point3D r = new Point3D();
            r.X = GetDouble(rt, "X");
            r.Y = GetDouble(rt, "Y");
            r.Z = GetDouble(rt, "Z");
            Rotation = r;
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

            PerformOperation();
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

        internal override void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("groupobj");
            docNode.AppendChild(ele);
            ele.SetAttribute("Name", Name);
            ele.SetAttribute("Description", Description);
            ele.SetAttribute("Colour", Color.ToString());
            ele.SetAttribute("Primitive", PrimType);
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

            XmlElement rot = doc.CreateElement("Rotation");
            rot.SetAttribute("X", Rotation.X.ToString());
            rot.SetAttribute("Y", Rotation.Y.ToString());
            rot.SetAttribute("Z", Rotation.Z.ToString());
            ele.AppendChild(rot);
            leftObject.Write(doc, ele);
            rightObject.Write(doc, ele);
        }
    }
}