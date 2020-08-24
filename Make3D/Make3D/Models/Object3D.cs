using CSGLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class Object3D
    {
        public Point3D rotation;
        protected Bounds3D absoluteBounds;
        private Point3DCollection absoluteObjectVertices;

        // This mesh is used by the view to display the object
        // Its vertices are absolute, i.e. have been translated
        // and rotated etc.
        private MeshGeometry3D mesh;

        private Vector3DCollection normals;
        protected Point3D position;
        private Point3DCollection relativeObjectVertices;
        protected Scale3D scale;
        private Int32Collection triangleIndices;
        protected string primType;

        public Object3D()
        {
            try
            {
                mesh = new MeshGeometry3D();
                relativeObjectVertices = new Point3DCollection();
                absoluteObjectVertices = new Point3DCollection();
                triangleIndices = new Int32Collection();
                normals = new Vector3DCollection();
                Color = Colors.Red;
                scale = new Scale3D();
                Position = new Point3D();
                Rotation = new Point3D();
                absoluteBounds = new Bounds3D();
                PrimType = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public Bounds3D AbsoluteBounds
        {
            get { return absoluteBounds; }
            set { absoluteBounds = value; }
        }

        public Point3DCollection AbsoluteObjectVertices
        {
            get { return absoluteObjectVertices; }
            set { absoluteObjectVertices = value; }
        }

        public Color Color { get; set; }
        public string Description { get; set; }

        public MeshGeometry3D Mesh
        {
            get { return mesh; }
            set
            {
                if (mesh != value)
                {
                    mesh = value;
                }
            }
        }

        public string Name { get; set; }

        public Vector3DCollection Normals
        {
            get { return normals; }
            set { normals = value; }
        }

        public Point3D Position
        {
            get
            {
                return position;
            }
            set
            {
                if (position != value)
                {
                    position = value;
                    Remesh();
                }
            }
        }

        public string PrimType { get => primType; set => primType = value; }

        public Point3DCollection RelativeObjectVertices
        {
            get { return relativeObjectVertices; }
            set { relativeObjectVertices = value; }
        }

        public Point3D Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    Remesh();
                }
            }
        }

        public Scale3D Scale
        {
            get
            {
                return scale;
            }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    //Remesh();
                }
            }
        }

        public Int32Collection TriangleIndices
        {
            get { return triangleIndices; }
            set { triangleIndices = value; }
        }

        public void LoadObject(string v)
        {
            relativeObjectVertices.Clear();
            absoluteObjectVertices.Clear();
            Scale = new Scale3D();
            Position = new Point3D();
            triangleIndices.Clear();
            normals.Clear();
            if (File.Exists(v))
            {
                string[] lines = File.ReadAllLines(v);
                foreach (string s in lines)
                {
                    string t = s.Trim();
                    if (t != "")
                    {
                        if (!t.StartsWith("#)"))
                        {
                            if (t.StartsWith("v "))
                            {
                                t = t.Replace("  ", " ");
                                string[] words = t.Split(' ');
                                double x = Convert.ToDouble(words[1]);
                                double y = Convert.ToDouble(words[2]);
                                double z = Convert.ToDouble(words[3]);
                                Point3D p = new Point3D(x, y, z);
                                relativeObjectVertices.Add(p);
                            }

                            if (t.StartsWith("vn "))
                            {
                                t = t.Replace("  ", " ");
                                string[] words = t.Split(' ');
                                double x = Convert.ToDouble(words[1]);
                                double y = Convert.ToDouble(words[2]);
                                double z = Convert.ToDouble(words[3]);
                                Vector3D p = new Vector3D(x, y, z);
                                normals.Add(p);
                            }
                            if (t.StartsWith("f "))
                            {
                                t = t.Replace("  ", " ");
                                string[] words = t.Split(' ');
                                if (words.GetLength(0) == 4)
                                {
                                    int ind = Convert.ToInt32(words[1]);
                                    triangleIndices.Add(ind - 1);
                                    ind = Convert.ToInt32(words[2]);
                                    triangleIndices.Add(ind - 1);
                                    ind = Convert.ToInt32(words[3]);
                                    triangleIndices.Add(ind - 1);
                                }
                                else
                                {
                                    List<int> indices = new List<int>();

                                    double cx = 0;
                                    double cy = 0;
                                    double cz = 0;
                                    for (int j = 1; j < words.GetLength(0); j++)
                                    {
                                        int pi = Convert.ToInt32(words[j]);
                                        indices.Add(pi);
                                        cx += relativeObjectVertices[pi].X;
                                        cy += relativeObjectVertices[pi].Y;
                                        cz += relativeObjectVertices[pi].Z;
                                    }
                                    cx = cx / indices.Count;
                                    cy = cy / indices.Count;
                                    cz = cz / indices.Count;
                                    relativeObjectVertices.Add(new Point3D(cx, cy, cz));
                                    int id = relativeObjectVertices.Count - 1;
                                    for (int j = 0; j < indices.Count - 1; j++)
                                    {
                                        triangleIndices.Add(indices[j]);
                                        triangleIndices.Add(indices[j + 1]);
                                        triangleIndices.Add(id);
                                    }
                                }
                            }
                        }
                    }
                }
                Unitise();
                RelativeToAbsolute();
                mesh.Positions = absoluteObjectVertices;
                mesh.TriangleIndices = triangleIndices;
                mesh.Normals = normals;
            }
        }

        public void MoveToCentre()
        {
            // actually centre by x and z, then floor y
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;

            double maxX = double.MinValue;
            double maxZ = double.MinValue;
            foreach (Point3D pd in absoluteObjectVertices)
            {
                if (pd.X < minX)
                {
                    minX = pd.X;
                }
                if (pd.Y < minY)
                {
                    minY = pd.Y;
                }
                if (pd.Z < minZ)
                {
                    minZ = pd.Z;
                }

                if (pd.X > maxX)
                {
                    maxX = pd.X;
                }

                if (pd.Z > maxZ)
                {
                    maxZ = pd.Z;
                }
            }

            Position = new Point3D(0, Position.Y - minY, 0);
        }

        public void MoveToFloor()
        {
            double minY = double.MaxValue;
            foreach (Point3D pd in absoluteObjectVertices)
            {
                if (pd.Y < minY)
                {
                    minY = pd.Y;
                }
            }

            Position = new Point3D(Position.X, Position.Y - minY, Position.Z);
        }

        public void RelativeToAbsolute()
        {
            if (absoluteObjectVertices != null)
            {
                absoluteObjectVertices.Clear();
                absoluteBounds = new Bounds3D();
                double r1 = DegreesToRad(Rotation.Y);
                double r2 = DegreesToRad(Rotation.Z);
                double r3 = DegreesToRad(Rotation.X);

                var cosa = Math.Cos(r2);
                var sina = Math.Sin(r2);

                var cosb = Math.Cos(r1);
                var sinb = Math.Sin(r1);

                var cosc = Math.Cos(r3);
                var sinc = Math.Sin(r3);

                var Axx = cosa * cosb;
                var Axy = cosa * sinb * sinc - sina * cosc;
                var Axz = cosa * sinb * cosc + sina * sinc;

                var Ayx = sina * cosb;
                var Ayy = sina * sinb * sinc + cosa * cosc;
                var Ayz = sina * sinb * cosc - cosa * sinc;

                var Azx = -sinb;
                var Azy = cosb * sinc;
                var Azz = cosb * cosc;
                foreach (Point3D cp in relativeObjectVertices)
                {
                    /*
                     *   Point3D rp = new Point3D();
                    rp.X = Axx * cp.X + Axy * cp.Y + Axz * cp.Z;
                    rp.Y = Ayx * cp.X + Ayy * cp.Y + Ayz * cp.Z;
                    rp.Z = Azx * cp.X + Azy * cp.Y + Azz * cp.Z;
                    Point3D ap = new Point3D();
                    ap.X = Position.X + (rp.X * Scale.X);
                    ap.Y = Position.Y + (rp.Y * Scale.Y);
                    ap.Z = Position.Z + (rp.Z * Scale.Z);
                    */

                    Point3D rp = new Point3D();
                    rp.X = Axx * cp.X * Scale.X + Axy * cp.Y * Scale.Y + Axz * cp.Z * Scale.Z;
                    rp.Y = Ayx * cp.X * Scale.X + Ayy * cp.Y * Scale.Y + Ayz * cp.Z * Scale.Z;
                    rp.Z = Azx * cp.X * Scale.X + Azy * cp.Y * Scale.Y + Azz * cp.Z * Scale.Z;
                    Point3D ap = new Point3D();
                    ap.X = Position.X + (rp.X);
                    ap.Y = Position.Y + (rp.Y);
                    ap.Z = Position.Z + (rp.Z);
                    AdjustBounds(ap);
                    absoluteObjectVertices.Add(ap);
                }
            }
        }

        internal void BuildPrimitive(string obType)
        {
            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimType = obType;
            switch (obType.ToLower())
            {
                case "box":
                    {
                        PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Pink);
                    }
                    break;

                case "sphere":
                    {
                        PrimitiveGenerator.GenerateSphere(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.CadetBlue);
                    }
                    break;

                case "cylinder":
                    {
                        PrimitiveGenerator.GenerateCylinder(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Orange);
                    }
                    break;

                case "roof":
                    {
                        PrimitiveGenerator.GenerateRoof(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Green);
                    }
                    break;

                case "roundroof":
                    {
                        PrimitiveGenerator.GenerateRoundRoof(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Aquamarine);
                    }
                    break;

                case "cone":
                    {
                        PrimitiveGenerator.GenerateCone(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Purple);
                    }
                    break;

                case "pyramid":
                    {
                        PrimitiveGenerator.GeneratePyramid(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Yellow);
                    }
                    break;

                case "torus":
                    {
                        PrimitiveGenerator.GenerateTorus(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.MistyRose);
                    }
                    break;

                case "cap":
                    {
                        PrimitiveGenerator.GenerateCap(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LimeGreen);
                    }
                    break;

                case "polygon":
                    {
                        PrimitiveGenerator.GeneratePolygon(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LightYellow);
                    }
                    break;

                case "tube":
                    {
                        PrimitiveGenerator.GenerateTube(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Magenta);
                    }
                    break;

                case "rightangle":
                    {
                        PrimitiveGenerator.GenerateRightAngle(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals,  Color.FromRgb(0x00, 0xAB, 0xEF));
                    }
                    break;
                case "pointy":
                    {
                        PrimitiveGenerator.GeneratePointy(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Color.FromRgb(0xEF, 0xAB, 0x00));
                    }
                    break;
                default:
                    {
                        //   obj.LoadObject(pth + obType+".txt");
                    }
                    break;
            }
        }

        internal List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < TriangleIndices.Count; i += 3)
            {
                int i1 = TriangleIndices[i];
                Point3D p1 = AbsoluteObjectVertices[i1];
                Vertex v1 = new Vertex(p1.X, p1.Y, p1.Z);

                int i2 = TriangleIndices[i + 1];
                Point3D p2 = AbsoluteObjectVertices[i2];
                Vertex v2 = new Vertex(p2.X, p2.Y, p2.Z);

                int i3 = TriangleIndices[i + 2];
                Point3D p3 = AbsoluteObjectVertices[i3];
                Vertex v3 = new Vertex(p3.X, p3.Y, p3.Z);

                Face f = new Face(v1, v2, v3);
                faces.Add(f);
            }
            return faces;
        }

        internal virtual void Read(XmlNode nd)
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
            if (PrimType != "Mesh")
            {
                BuildPrimitive(PrimType);
            }
            else
            {
                relativeObjectVertices = new Point3DCollection();
                XmlNodeList vtl = nd.SelectNodes("v");
                foreach (XmlNode vn in vtl)
                {
                    XmlElement el = vn as XmlElement;
                    Point3D pv = new Point3D();
                    pv.X = GetDouble(el, "X");
                    pv.Y = GetDouble(el, "Y");
                    pv.Z = GetDouble(el, "Z");
                    relativeObjectVertices.Add(pv);
                }

                triangleIndices = new Int32Collection();
                XmlNodeList ftl = nd.SelectNodes("f");
                foreach (XmlNode vn in ftl)
                {
                    XmlElement el = vn as XmlElement;
                    string tri = el.GetAttribute("v");
                    String[] words = tri.Split(',');
                    triangleIndices.Add(Convert.ToInt32(words[0]));
                    triangleIndices.Add(Convert.ToInt32(words[1]));
                    triangleIndices.Add(Convert.ToInt32(words[2]));
                }
                RelativeToAbsolute();
                SetMesh();
            }
        }

        internal virtual void Remesh()
        {
            RelativeToAbsolute();
            SetMesh();
        }

        internal void SetMesh()
        {
            mesh.Positions = absoluteObjectVertices;
            mesh.TriangleIndices = triangleIndices;
            mesh.Normals = normals;
        }

        internal virtual void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("obj");
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
            if (PrimType == "Mesh")
            {
                foreach (Point3D v in relativeObjectVertices)
                {
                    XmlElement vertEle = doc.CreateElement("v");
                    vertEle.SetAttribute("X", v.X.ToString("F5"));
                    vertEle.SetAttribute("Y", v.Y.ToString("F5"));
                    vertEle.SetAttribute("Z", v.Z.ToString("F5"));
                    ele.AppendChild(vertEle);
                }
                for (int i = 0; i < triangleIndices.Count; i += 3)
                {
                    XmlElement faceEle = doc.CreateElement("f");
                    faceEle.SetAttribute("v", triangleIndices[i].ToString() + "," +
                                              triangleIndices[i + 1].ToString() + "," +
                                              triangleIndices[i + 2].ToString());
                    ele.AppendChild(faceEle);
                }
            }
        }

        protected double GetDouble(XmlNode pn, string v)
        {
            XmlElement el = pn as XmlElement;
            string val = el.GetAttribute(v);
            return Convert.ToDouble(val);
        }

        private void AddPrimitiveToObject(Point3DCollection pnts, Int32Collection indices, Vector3DCollection normals, Color c)
        {
            RelativeObjectVertices = pnts;
            TriangleIndices = indices;
            Normals = normals;
            RelativeToAbsolute();
            SetMesh();
            Color = c;
        }

        private void AdjustBounds(Point3D ap)
        {
            Point3D l = absoluteBounds.Lower;
            if (ap.X < l.X)
            {
                l.X = ap.X;
            }
            if (ap.Y < l.Y)
            {
                l.Y = ap.Y;
            }
            if (ap.Z < l.Z)
            {
                l.Z = ap.Z;
            }

            Point3D u = absoluteBounds.Upper;
            if (ap.X > u.X)
            {
                u.X = ap.X;
            }
            if (ap.Y > u.Y)
            {
                u.Y = ap.Y;
            }
            if (ap.Z > u.Z)
            {
                u.Z = ap.Z;
            }
            absoluteBounds.Lower = l;
            absoluteBounds.Upper = u;
        }

        private double DegreesToRad(double x)
        {
            return ((x / 360.0) * Math.PI * 2);
        }

        private Point3D Rotate(Point3D cp)
        {
            Point3D res = new Point3D();

            return res;
        }

        private void Unitise()
        {
            // a quick function to convert the coordinates of an object read in, into a unit sized object
            // centered on 0,0,0
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            foreach (Point3D pt in relativeObjectVertices)
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
                scaleX /= dx;
            }
            double scaleY = 1.0;
            double dY = max.Y - min.Y;
            if (dY > 0)
            {
                scaleY /= dY;
            }

            double scaleZ = 1.0;
            double dZ = max.Z - min.Z;
            if (dZ > 0)
            {
                scaleZ /= dZ;
            }

            for (int i = 0; i < relativeObjectVertices.Count; i++)
            {
                Point3D moved = new Point3D(relativeObjectVertices[i].X - min.X,
                                             relativeObjectVertices[i].Y - min.Y,
                                             relativeObjectVertices[i].Z - min.Z);
                moved.X *= scaleX;
                moved.Y *= scaleY;
                moved.Z *= scaleZ;

                moved.X -= 0.5;
                moved.Y -= 0.5;
                moved.Z -= 0.5;
                relativeObjectVertices[i] = moved;
            }

            // generate some c# !!

            File.WriteAllText("C:\\tmp\\t.txt", "");
            foreach (Point3D p in relativeObjectVertices)
            {
                File.AppendAllText("C:\\tmp\\t.txt", $" pnts.Add( new Point3D( {p.X:F3},{p.Y:F3},{p.Z:F3}));");
                File.AppendAllText("C:\\tmp\\t.txt", "\r\n");
            }
            foreach (int f in TriangleIndices)
            {
                File.AppendAllText("C:\\tmp\\t.txt", $"indices.Add({f});");
                File.AppendAllText("C:\\tmp\\t.txt", "\r\n");
            }
        }

        public virtual Object3D Clone()
        {
            Object3D res = new Object3D();
            res.primType = this.primType;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);
            res.rotation = new Point3D(this.rotation.X, this.rotation.Y, this.rotation.Z);
            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Color = this.Color;
            res.BuildPrimitive(res.primType);

            return res;
        }
    }
}