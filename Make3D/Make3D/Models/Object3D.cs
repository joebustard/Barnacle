using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Models
{
    public class Object3D
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Color Color { get; set; }

        public Point3D Position { get; set; }

        public Point3D Rotation { get; set; }
        public Scale3D Scale { get; set; }
        public string PrimType { get; set; }
        private Bounds3D absoluteBounds;

        public Bounds3D AbsoluteBounds
        {
            get { return absoluteBounds; }
            set { absoluteBounds = value; }
        }

        // This mesh is used by the view to display the object
        // Its vertices are absolute, i.e. have been translated
        // and rotated etc.
        private MeshGeometry3D mesh;

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

        public Object3D()
        {
            mesh = new MeshGeometry3D();
            relativeObjectVertices = new Point3DCollection();
            absoluteObjectVertices = new Point3DCollection();
            triangleIndices = new Int32Collection();
            normals = new Vector3DCollection();
            Color = Colors.Red;
            Scale = new Scale3D();
            Position = new Point3D();
            Rotation = new Point3D();
            absoluteBounds = new Bounds3D();
            PrimType = "";
        }

        private Point3DCollection relativeObjectVertices;

        public Point3DCollection RelativeObjectVertices
        {
            get { return relativeObjectVertices; }
            set { relativeObjectVertices = value; }
        }

        private Point3DCollection absoluteObjectVertices;

        public Point3DCollection AbsoluteObjectVertices
        {
            get { return absoluteObjectVertices; }
            set { absoluteObjectVertices = value; }
        }

        private Int32Collection triangleIndices;

        public Int32Collection TriangleIndices
        {
            get { return triangleIndices; }
            set { triangleIndices = value; }
        }

        private Vector3DCollection normals;

        public Vector3DCollection Normals
        {
            get { return normals; }
            set { normals = value; }
        }

        private void MoveToFloor()
        {
            double minY = double.MaxValue;
            foreach (Point3D pd in relativeObjectVertices)
            {
                if (pd.Y < minY)
                {
                    minY = pd.Y;
                }
            }
            //  if (minY < 0)
            {
                Point3DCollection tmp = new Point3DCollection();
                foreach (Point3D pd in relativeObjectVertices)
                {
                    Point3D n = new Point3D(pd.X, pd.Y - minY, pd.Z);
                    tmp.Add(n);
                }
                relativeObjectVertices = tmp;
            }
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

        public void RelativeToAbsolute()
        {
            absoluteObjectVertices.Clear();
            absoluteBounds = new Bounds3D();
            foreach (Point3D rp in relativeObjectVertices)
            {
                Point3D ap = new Point3D();
                ap.X = Position.X + (rp.X * Scale.X);
                ap.Y = Position.Y + (rp.Y * Scale.Y);
                ap.Z = Position.Z + (rp.Z * Scale.Z);
                AdjustBounds(ap);
                absoluteObjectVertices.Add(ap);
            }
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

        internal void Read(XmlNode nd)
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

            XmlNode rt = nd.SelectSingleNode("Rotate");
            Point3D r = new Point3D();
            r.X = GetDouble(pn, "X");
            r.Y = GetDouble(pn, "Y");
            r.Z = GetDouble(pn, "Z");
            Rotation = r;

            BuildPrimitive(PrimType);
        }

        private double GetDouble(XmlNode pn, string v)
        {
            XmlElement el = pn as XmlElement;
            string val = el.GetAttribute(v);
            return Convert.ToDouble(val);
        }

        internal void Write(XmlDocument doc, XmlElement docNode)
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
            rot.SetAttribute("X", Scale.X.ToString());
            rot.SetAttribute("Y", Scale.Y.ToString());
            rot.SetAttribute("Z", Scale.Z.ToString());
            ele.AppendChild(scl);

            /*
            foreach (Point3D p in relativeObjectVertices)
            {
                XmlElement ep = doc.CreateElement("v");
                ep.SetAttribute("x", p.X.ToString());
                ep.SetAttribute("y", p.Y.ToString());
                ep.SetAttribute("z", p.Z.ToString());
                ele.AppendChild(ep);
            }
            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                XmlElement f = doc.CreateElement("f");
                f.SetAttribute("p1", triangleIndices[i].ToString());
                f.SetAttribute("p2", triangleIndices[i + 1].ToString());
                f.SetAttribute("p3", triangleIndices[i + 2].ToString());
                ele.AppendChild(f);
            }
            */
        }

        internal void SetMesh()
        {
            mesh.Positions = absoluteObjectVertices;
            mesh.TriangleIndices = triangleIndices;
            mesh.Normals = normals;
        }

        internal void Remesh()
        {
            RelativeToAbsolute();
            SetMesh();
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

                default:
                    {
                        //   obj.LoadObject(pth + obType+".txt");
                    }
                    break;
            }
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
    }
}