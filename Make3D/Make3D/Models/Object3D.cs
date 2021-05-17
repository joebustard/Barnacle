using CSGLib;
using HullLibrary;
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
        protected Point3D position;
        protected string primType;
        protected Scale3D scale;
        private Point3DCollection absoluteObjectVertices;
        private EditorParameters editorParameters;
        private Vector3DCollection normals;
        private Point3DCollection relativeObjectVertices;

        // This mesh is used by the view to display the object
        // Its vertices are absolute, i.e. have been translated
        // and rotated etc.
        private MeshGeometry3D surfaceMesh;

        private Int32Collection triangleIndices;

        public Object3D()
        {
            try
            {
                surfaceMesh = new MeshGeometry3D();
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
                editorParameters = new EditorParameters();
                XmlType = "obj";
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

        public EditorParameters EditorParameters
        {
            get
            {
                return editorParameters;
            }
            set
            {
                editorParameters = value;
            }
        }

        public MeshGeometry3D Mesh
        {
            get
            {
                return surfaceMesh;
            }
            set
            {
                if (surfaceMesh != value)
                {
                    surfaceMesh = value;
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
                }
            }
        }

        public int TotalFaces
        {
            get
            {
                int result = 0;
                if (triangleIndices != null)
                {
                    result = triangleIndices.Count / 3;
                }
                return result;
            }
        }

        public Int32Collection TriangleIndices
        {
            get { return triangleIndices; }
            set { triangleIndices = value; }
        }

        protected String XmlType { get; set; }

        public virtual Object3D Clone()
        {
            Object3D res = new Object3D();
            res.Name = this.Name;
            res.Description = this.Description;
            res.primType = this.primType;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);
            res.rotation = new Point3D(this.rotation.X, this.rotation.Y, this.rotation.Z);
            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Color = this.Color;
            if (res.PrimType != "Mesh")
            {
                res.BuildPrimitive(res.primType);
            }
            else
            {
                foreach (Point3D p in this.relativeObjectVertices)
                {
                    res.relativeObjectVertices.Add(new Point3D(p.X, p.Y, p.Z));
                }

                foreach (int i in this.triangleIndices)
                {
                    res.triangleIndices.Add(i);
                }
            }
            res.EditorParameters.ToolName = EditorParameters.ToolName;
            foreach (EditorParameter ep in editorParameters.Parameters)
            {
                EditorParameter np = new EditorParameter(ep.Name, ep.Value);
                res.EditorParameters.Parameters.Add(np);
            }
            res.Remesh();
            return res;
        }

        public virtual Object3D ConvertToMesh()
        {
            PrimType = "Mesh";

            return this;
        }

        public virtual bool IsSizable()
        {
            return true;
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
                surfaceMesh.Positions = absoluteObjectVertices;
                surfaceMesh.TriangleIndices = triangleIndices;
                surfaceMesh.Normals = normals;
            }
        }

        public void MoveToCentre()
        {
            // actually centre by x and z, then floor y
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue); ;
            PointUtils.MinMax(absoluteObjectVertices, ref min, ref max);
            Position = new Point3D(0, Position.Y - min.Y, 0);
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

                Point3DCollection tmp;
                if (Rotation != null && (Rotation.X != 0 || Rotation.Y != 0 || Rotation.Z != 0))
                {
                    tmp = RotatePoints(relativeObjectVertices, Rotation.Y, Rotation.Z, Rotation.X); // ?????
                }
                else
                {
                    tmp = relativeObjectVertices;
                }

                absoluteBounds = new Bounds3D();
                foreach (Point3D cp in tmp)
                {
                    Point3D ap = new Point3D();
                    ap.X = Position.X + (cp.X);
                    ap.Y = Position.Y + (cp.Y);
                    ap.Z = Position.Z + (cp.Z);
                    AdjustBounds(ap);
                    absoluteObjectVertices.Add(ap);
                }
            }
        }

        public virtual void Rotate(Point3D RotateBy)
        {
            double r1 = DegreesToRad(RotateBy.Y);
            double r2 = DegreesToRad(RotateBy.Z);
            double r3 = DegreesToRad(RotateBy.X);
            if (relativeObjectVertices != null)
            {
                if (IsSizable())
                {
                    relativeObjectVertices = RotatePoints(relativeObjectVertices, r1, r2, r3);
                }
                else
                {
                    rotation.X += r3;
                    rotation.Y += r1;
                    rotation.Z += r2;
                }
            }
        }

        public void RotateRad(Point3D Rotation)
        {
            if (relativeObjectVertices != null)
            {
                double r1 = Rotation.Y;
                double r2 = Rotation.Z;
                double r3 = Rotation.X;

                relativeObjectVertices = RotatePoints(relativeObjectVertices, r1, r2, r3);
            }
        }

        public void RotateRad2(Point3D Rotation)
        {
            if (relativeObjectVertices != null)
            {
                double r1 = Rotation.X;
                double r2 = Rotation.Z;
                double r3 = Rotation.Y;

                relativeObjectVertices = RotatePoints(relativeObjectVertices, r1, r2, r3);
            }
        }

        public void ScaleMesh(double sx, double sy, double sz)
        {
            Point3DCollection tmp = new Point3DCollection();
            foreach (Point3D cp in relativeObjectVertices)
            {
                Point3D ap = new Point3D();
                ap.X = sx * cp.X;
                ap.Y = sy * cp.Y;
                ap.Z = sz * cp.Z;
                //AdjustBounds(ap);
                tmp.Add(ap);
            }
            relativeObjectVertices = tmp;
        }

        public void SwapYZAxies()
        {
            Point3DCollection tmp = new Point3DCollection();
            foreach (Point3D pn in relativeObjectVertices)
            {
                tmp.Add(new Point3D(pn.X, pn.Z, pn.Y));
            }
            relativeObjectVertices = tmp;
        }

        public void Unitise()
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
            Scale = new Scale3D(scaleX, scaleY, scaleZ);
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
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Lavender);
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
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LightSeaGreen);
                    }
                    break;

                case "pyramid":
                    {
                        PrimitiveGenerator.GeneratePyramid(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Yellow);
                    }
                    break;

                case "pyramid2":
                    {
                        PrimitiveGenerator.GeneratePyramid2(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.CadetBlue);
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
                        AddPrimitiveToObject(pnts, indices, normals, Color.FromRgb(0x00, 0xAB, 0xEF));
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

        internal void CalcScale(bool move = true)
        {
            // a quick function to calculate the scale of an object read in, into a unit sized object
            // centered on 0,0,0
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(relativeObjectVertices, ref min, ref max);

            double scaleX = max.X - min.X;

            double scaleY = max.Y - min.Y;

            double scaleZ = max.Z - min.Z;

            double midx = min.X + (scaleX / 2);
            double midy = min.Y + (scaleY / 2);
            double midz = min.Z + (scaleZ / 2);
            /*
            for (int i = 0; i < relativeObjectVertices.Count; i++)
            {
                Point3D moved = new Point3D(relativeObjectVertices[i].X - midx,
                                             relativeObjectVertices[i].Y - midy,
                                             relativeObjectVertices[i].Z - midz);

                relativeObjectVertices[i] = moved;
            }
            */
            Scale = new Scale3D(scaleX, scaleY, scaleZ);
            if (move)
            {
                Position = new Point3D(midx, midy, midz);
            }
        }

        internal void ConvertToHull()
        {
            var hullMaker = new ConvexHullCalculator();

            hullMaker.GeneratePoint3DHull(RelativeObjectVertices, TriangleIndices);
            Remesh();
        }

        internal void FlipInside()
        {
            for (int i = 0; i < triangleIndices.Count - 2; i += 3)
            {
                int tmp = triangleIndices[i + 1];
                triangleIndices[i + 1] = triangleIndices[i + 2];
                triangleIndices[i + 2] = tmp;
            }
            Remesh();
        }

        internal void FlipX()
        {
            if (PrimType == "Mesh")
            {
                Point3DCollection tmp = new Point3DCollection();
                foreach (Point3D v in relativeObjectVertices)
                {
                    Point3D pn = new Point3D(-v.X, v.Y, v.Z);
                    tmp.Add(pn);
                }
                relativeObjectVertices = tmp;
                Remesh();
            }
        }

        internal void FlipY()
        {
            if (PrimType == "Mesh")
            {
                Point3DCollection tmp = new Point3DCollection();
                foreach (Point3D v in relativeObjectVertices)
                {
                    Point3D pn = new Point3D(v.X, -v.Y, v.Z);
                    tmp.Add(pn);
                }
                relativeObjectVertices = tmp;
                Remesh();
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

            XmlNode ed = nd.SelectSingleNode("EditorParameters");
            if (ed != null)
            {
                EditorParameters.Load(ed as XmlElement);
            }
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
            if (relativeObjectVertices != null && relativeObjectVertices.Count > 0)
            {
                RelativeToAbsolute();
                SetMesh();
            }
        }

        internal void SetMesh()
        {
            surfaceMesh.Positions = absoluteObjectVertices;
            surfaceMesh.TriangleIndices = triangleIndices;
            surfaceMesh.Normals = normals;
        }

        internal void SkewMesh(int moveSide, double x, double y, double z)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue); ;
            PointUtils.MinMax(relativeObjectVertices, ref min, ref max);
            List<Point3D> tmp = new List<Point3D>();
            foreach (Point3D p in relativeObjectVertices)
            {
                switch (moveSide)
                {
                    case 0:
                        {
                            if (p.X < 0)
                            {
                                double offset = (-p.X / max.X) * (1 - y);
                                tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                tmp.Add(new Point3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 1:
                        {
                            if (p.X > 0)
                            {
                                double offset = (p.X / max.X) * (1 - y);
                                tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                tmp.Add(new Point3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 3:
                        {
                            if (p.Z < 0)
                            {
                                double offset = (p.Z / max.Z) * (1 - y);
                                tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                tmp.Add(new Point3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 2:
                        {
                            if (p.Z > 0)
                            {
                                double offset = (p.Z / max.Z) * (1 - y);
                                tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                tmp.Add(new Point3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;
                }
            }
            relativeObjectVertices.Clear();
            foreach (Point3D p in tmp)
            {
                relativeObjectVertices.Add(p);
            }
        }

        internal virtual XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement(XmlType);
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
            if (EditorParameters.HasContent)
            {
                EditorParameters.Write(doc, ele);
            }

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

            return ele;
        }

        protected double DegreesToRad(double x)
        {
            return ((x / 360.0) * Math.PI * 2);
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

        private Point3D RotateMesh(Point3D cp)
        {
            Point3D res = new Point3D();

            return res;
        }

        private Point3DCollection RotatePoints(Point3DCollection pnts, double r1, double r2, double r3)
        {
            Point3DCollection tmp = new Point3DCollection();
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
            foreach (Point3D cp in pnts)
            {
                Point3D rp = new Point3D();
                rp.X = Axx * cp.X + Axy * cp.Y + Axz * cp.Z;
                rp.Y = Ayx * cp.X + Ayy * cp.Y + Ayz * cp.Z;
                rp.Z = Azx * cp.X + Azy * cp.Y + Azz * cp.Z;
                AdjustBounds(rp);
                tmp.Add(rp);
            }
            return tmp;
        }
    }
}