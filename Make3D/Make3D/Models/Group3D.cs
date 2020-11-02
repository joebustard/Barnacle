using CSGLib;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class Group3D : Object3D
    {
        private Scale3D groupScale;
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

        internal bool Init()
        {
            bool result = false;
            if (leftObject != null && rightObject != null)
            {
                Position = new Point3D(leftObject.Position.X, leftObject.Position.Y, leftObject.Position.Z);

                // this is wrong, should join objects then divide bounds of new object somehow
                //Scale = new Scale3D(1, 1, 1);
                Color = leftObject.Color;
                result = PerformOperation();
            }

            return result;
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

        internal bool PerformOperation()
        {
            bool res = false;
            absoluteBounds = new Bounds3D();
            leftObject.RelativeToAbsolute();
            rightObject.RelativeToAbsolute();
            Logger.Log("Left Solid\r\n============\r\n");
            leftSolid = new Solid(leftObject.AbsoluteObjectVertices, leftObject.TriangleIndices, false);
            // The original objects have their own  scale that is taken into account by their own RelativeToAbsolutes
            // But the combined object may have been scaled too so take that into acccount now
            //  leftSolid.Scale(Scale.X, Scale.Y, Scale.Z);
            Logger.Log("Right Solid\r\n============\r\n");
            rightSolid = new Solid(rightObject.AbsoluteObjectVertices, rightObject.TriangleIndices, false);
            //  rightSolid.Scale(Scale.X, Scale.Y, Scale.Z);
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
                    res = true;
                }
            }
            return res;
        }

        private void AbsoluteToRelative()
        {
            GetScaleFromAbsoluteExtent();

            RelativeObjectVertices = new Point3DCollection();
            foreach (Point3D pnt in AbsoluteObjectVertices)
            {
                // Point3D p = new Point3D((pnt.X / groupScale.X) , (pnt.Y / groupScale.Y), (pnt.Z / groupScale.Z) );
                Point3D p = new Point3D((pnt.X - Position.X), (pnt.Y - Position.Y), (pnt.Z - Position.Z));
                RelativeObjectVertices.Add(p);
            }
            scale = groupScale;
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
            RelativeObjectVertices = new Point3DCollection();
            XmlNodeList vtl = nd.SelectNodes("v");
            foreach (XmlNode vn in vtl)
            {
                XmlElement el = vn as XmlElement;
                Point3D pv = new Point3D();
                pv.X = GetDouble(el, "X");
                pv.Y = GetDouble(el, "Y");
                pv.Z = GetDouble(el, "Z");
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
            RelativeToAbsolute();
            SetMesh();
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
            foreach (Point3D v in RelativeObjectVertices)
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

        public override Object3D Clone()
        {
            Group3D res = new Group3D();
            res.Name = "";
            res.primType = this.primType;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);

            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Color = this.Color;
            res.leftObject = this.leftObject.Clone();
            res.rightObject = this.rightObject.Clone();
            res.RelativeObjectVertices = new Point3DCollection();
            foreach (Point3D po in this.RelativeObjectVertices)
            {
                Point3D pn = new Point3D(po.X, po.Y, po.Z);
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
            neo.RelativeObjectVertices = RelativeObjectVertices;
            RelativeObjectVertices = null;
            neo.TriangleIndices = TriangleIndices;
            TriangleIndices = null;
            AbsoluteObjectVertices = AbsoluteObjectVertices;
            AbsoluteObjectVertices = null;
            neo.AbsoluteBounds = AbsoluteBounds;
            AbsoluteBounds = null;
            //neo.Unitise();
            return neo;
        }
    }
}