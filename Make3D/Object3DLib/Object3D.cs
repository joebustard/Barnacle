using CSGLib;
using HullLibrary;
using Barnacle.EditorParameterLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Object3DLib
{
    public struct P3D
    {
        public float X;
        public float Y;
        public float Z;

        public P3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public P3D(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        public P3D(Point3D pn)
        {
            X = (float)pn.X;
            Y = (float)pn.Y;
            Z = (float)pn.Z;
        }
    }

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

        // private Point3DCollection relativeObjectVertices;
        private List<P3D> relativeObjectVertices;

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
                //relativeObjectVertices = new Point3DCollection();
                relativeObjectVertices = new List<P3D>();
                absoluteObjectVertices = new Point3DCollection();
                triangleIndices = new Int32Collection();
                normals = new Vector3DCollection();
                Color = Colors.Red;
                scale = new Scale3D();
                Position = new Point3D();
                Rotation = new Point3D();
                absoluteBounds = new Bounds3D();
                Exportable = true;
                PrimType = "";
                editorParameters = new EditorParameters();
                XmlType = "obj";
                Name = "";
                Description = "";
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

        public bool Exportable { get; set; }

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
        /*
        public Point3DCollection RelativeObjectVertices
        {
            get { return relativeObjectVertices; }
            set { relativeObjectVertices = value; }
        }
        */

        public List<P3D> RelativeObjectVertices
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

        private List<int> indices;
        public List<int> Indices
        {
            get {
               
                return indices;
            }
            set
            {
                indices = value;
            }
            
        }
        protected String XmlType { get; set; }

        public static XmlElement FindExternalModel(string name, string path)
        {
            XmlElement res = null;
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.Load(path);
                    XmlNode docNode = doc.SelectSingleNode("Document");
                    XmlNodeList nodes = docNode.ChildNodes;
                    foreach (XmlNode nd in nodes)
                    {
                        if (nd.Name == "obj" || nd.Name == "groupobj")
                        {
                            XmlElement ele = nd as XmlElement;
                            if (ele.HasAttribute("Name"))
                            {
                                if (ele.GetAttribute("Name") == name)
                                {
                                    res = ele;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return res;
        }

        public virtual void AbsoluteToRelative()
        {
            if (absoluteObjectVertices != null)
            {
                relativeObjectVertices.Clear();
                relativeObjectVertices = new List<P3D>(absoluteObjectVertices.Count);
                absoluteBounds = new Bounds3D();
                foreach (Point3D cp in absoluteObjectVertices)

                {
                    AdjustBounds(cp);
                    P3D ap = new P3D();
                    ap.X = (float)(cp.X - Position.X);
                    ap.Y = (float)(cp.Y - Position.Y);
                    ap.Z = (float)(cp.Z - Position.Z);

                    relativeObjectVertices.Add(ap);
                }
            }
        }

        public virtual  void DeThread()
        {
            Indices = new List<int>();
            foreach ( int i in TriangleIndices)
            {
                Indices.Add(i);
            }
        }
        public bool BuildPrimitive(string obType)
        {
            bool built = false;
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
                        built = true;
                    }
                    break;

                case "sphere":
                    {
                        PrimitiveGenerator.GenerateSphere(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.CadetBlue);
                        built = true;
                    }
                    break;

                case "cylinder":
                    {
                        PrimitiveGenerator.GenerateCylinder(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Orange);
                        built = true;
                    }
                    break;

                case "roof":
                    {
                        PrimitiveGenerator.GenerateRoof(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Lavender);
                        built = true;
                    }
                    break;

                case "roundroof":
                    {
                        PrimitiveGenerator.GenerateRoundRoof(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Aquamarine);
                        built = true;
                    }
                    break;

                case "cone":
                    {
                        PrimitiveGenerator.GenerateCone(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LightSeaGreen);
                        built = true;
                    }
                    break;

                case "pyramid":
                    {
                        PrimitiveGenerator.GeneratePyramid(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Yellow);
                        built = true;
                    }
                    break;

                case "pyramid2":
                    {
                        PrimitiveGenerator.GeneratePyramid2(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.CadetBlue);
                        built = true;
                    }
                    break;

                case "torus":
                    {
                        PrimitiveGenerator.GenerateTorus(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.MistyRose);
                        built = true;
                    }
                    break;

                case "cap":
                    {
                        PrimitiveGenerator.GenerateCap(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LimeGreen);
                        built = true;
                    }
                    break;

                case "polygon":
                    {
                        PrimitiveGenerator.GeneratePolygon(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.LightYellow);
                        built = true;
                    }
                    break;

                case "tube":
                    {
                        PrimitiveGenerator.GenerateTube(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Magenta);
                        built = true;
                    }
                    break;

                case "rightangle":
                    {
                        PrimitiveGenerator.GenerateRightAngle(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Color.FromRgb(0x00, 0xAB, 0xEF));
                        built = true;
                    }
                    break;

                case "pointy":
                    {
                        PrimitiveGenerator.GeneratePointy(ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Color.FromRgb(0xEF, 0xAB, 0x00));
                        built = true;
                    }
                    break;

                case "pentagoncell":
                    {
                        PrimitiveGenerator.GenerateCell(5,ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.Chartreuse);
                       
                        built = true;
                    }
                    break;

                case "trianglecell":
                    {
                        PrimitiveGenerator.GenerateCell(3, ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.DarkOrchid);
                        built = true;
                    }
                    break;

                case "boxcell":
                    {
                        PrimitiveGenerator.GenerateCell(4, ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.DeepPink);
                        built = true;
                    }
                    break;
                case "hexagoncell":
                    {
                        PrimitiveGenerator.GenerateCell(6, ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Colors.SpringGreen);
                        built = true;
                    }
                    break;
                case "octagoncell":
                    {
                        PrimitiveGenerator.GenerateCell(8, ref pnts, ref indices, ref normals);
                        AddPrimitiveToObject(pnts, indices, normals, Color.FromRgb(0xEF, 0xAB, 0x00));
                        built = true;
                    }
                    break;
                default:
                    {
                        //   obj.LoadObject(pth + obType+".txt");
                    }
                    break;
            }
            return built;
        }

        public void CalcScale(bool move = true)
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

            Scale = new Scale3D(scaleX, scaleY, scaleZ);
            if (move)
            {
                Position = new Point3D(midx, midy, midz);
            }
        }

        public virtual Object3D Clone(bool useIndices = false)
        {
            Object3D res = new Object3D();
            res.Name = this.Name;
            res.Description = this.Description;
            res.primType = this.primType;
            res.scale = new Scale3D(this.scale.X, this.scale.Y, this.scale.Z);
            res.rotation = new Point3D(this.rotation.X, this.rotation.Y, this.rotation.Z);
            res.position = new Point3D(this.position.X, this.position.Y, this.position.Z);
            res.Color = this.Color;
            res.Exportable = this.Exportable;
            if (res.PrimType != "Mesh")
            {
                res.BuildPrimitive(res.primType);
            }
            else
            {
                /*
                foreach (Point3D p in this.relativeObjectVertices)
                {
                    res.relativeObjectVertices.Add(new Point3D(p.X, p.Y, p.Z));
                }
                */
                foreach (P3D p in this.relativeObjectVertices)
                {
                    P3D p3d = new P3D();
                    p3d.X = p.X;
                    p3d.Y = p.Y;
                    p3d.Z = p.Z;
                    res.relativeObjectVertices.Add(p3d);
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
                    foreach (int i in this.triangleIndices)
                    {
                        res.triangleIndices.Add(i);
                    }
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

        public void ConvertToHull()
        {
            var hullMaker = new ConvexHullCalculator();

            //hullMaker.GeneratePoint3DHull(RelativeObjectVertices, TriangleIndices);
            Point3DCollection tmp = new Point3DCollection();
            PointUtils.P3DToPointCollection(relativeObjectVertices, tmp);
            hullMaker.GeneratePoint3DHull(tmp, TriangleIndices);
            PointUtils.PointCollectionToP3D(tmp, relativeObjectVertices);
            Remesh();
        }

        public virtual Object3D ConvertToMesh()
        {
            PrimType = "Mesh";

            return this;
        }

        public void FlipInside()
        {
            for (int i = 0; i < triangleIndices.Count - 2; i += 3)
            {
                int tmp = triangleIndices[i + 1];
                triangleIndices[i + 1] = triangleIndices[i + 2];
                triangleIndices[i + 2] = tmp;
            }
            Remesh();
        }

        public void FlipX()
        {
            if (PrimType == "Mesh")
            {
                // Point3DCollection tmp = new Point3DCollection();
                // foreach (Point3D v in relativeObjectVertices)
                List<P3D> tmp = new List<P3D>();
                foreach (P3D v in relativeObjectVertices)
                {
                    //Point3D pn = new Point3D(-v.X, v.Y, v.Z);
                    P3D pn = new P3D(-v.X, v.Y, v.Z);
                    tmp.Add(pn);
                }
                relativeObjectVertices = tmp;
                Remesh();
            }
        }

        public void FlipY()
        {
            if (PrimType == "Mesh")
            {
                List<P3D> tmp = new List<P3D>();
                foreach (P3D v in relativeObjectVertices)
                {
                    P3D pn = new P3D(v.X, -v.Y, v.Z);
                    tmp.Add(pn);
                }
                relativeObjectVertices = tmp;
                Remesh();
            }
        }

        public void FlipZ()
        {
            if (PrimType == "Mesh")
            {
                List<P3D> tmp = new List<P3D>();
                foreach (P3D v in relativeObjectVertices)
                {
                    P3D pn = new P3D(v.X, v.Y, -v.Z);
                    tmp.Add(pn);
                }
                relativeObjectVertices = tmp;
                Remesh();
            }
        }

        public List<Face> GetFaces()
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

        public virtual bool IsSizable()
        {
            return true;
        }

        /*
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
                               // Point3D p = new Point3D(x, y, z);
                                P3D p = new P3D(x, y, z);
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
                                    //relativeObjectVertices.Add(new Point3D(cx, cy, cz));
                                    relativeObjectVertices.Add(new P3D((float)cx, (float)cy, (float)cz));
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
        }*/

        public void MoveOriginToCentroid()
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(RelativeObjectVertices, ref min, ref max);
            double dx = -(min.X + (max.X - min.X) / 2.0);
            double dy = -(min.Y + (max.Y - min.Y) / 2.0);
            double dz = -(min.Z + (max.Z - min.Z) / 2.0);

            //   Point3DCollection pn = new Point3DCollection();
            List<P3D> pn = new List<P3D>();
            for (int i = 0; i < RelativeObjectVertices.Count; i++)
            {
                
                pn.Add(new P3D((float)RelativeObjectVertices[i].X + (float)dx,
               (float)RelativeObjectVertices[i].Y + (float)dy,
               (float)RelativeObjectVertices[i].Z + (float)dz));
            }
            RelativeObjectVertices.Clear();
            
            RelativeObjectVertices = pn;
            Position = new Point3D(0, 0, 0);
            Remesh();
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

        public virtual void Read(XmlNode nd)
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
                // relativeObjectVertices = new Point3DCollection();
                relativeObjectVertices = new List<P3D>();
                XmlNodeList vtl = nd.SelectNodes("v");
                foreach (XmlNode vn in vtl)
                {
                    XmlElement el = vn as XmlElement;
                    // Point3D pv = new Point3D();
                    P3D pv = new P3D();
                    pv.X = (float)GetDouble(el, "X");
                    pv.Y = (float)GetDouble(el, "Y");
                    pv.Z = (float)GetDouble(el, "Z");
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

        public virtual void ReadBinary(BinaryReader reader)
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

            editorParameters.ReadBinary(reader);

            relativeObjectVertices = new List<P3D>();
            int count = reader.ReadInt32();
            float x1, y1, z1;
            for (int i = 0; i < count; i++)
            {
                x1 = reader.ReadSingle();
                y1 = reader.ReadSingle();
                z1 = reader.ReadSingle();

                relativeObjectVertices.Add(new P3D(x1, y1, z1));
            }

            count = reader.ReadInt32();
            triangleIndices = new Int32Collection();
            for (int i = 0; i < count; i++)
            {
                int index = reader.ReadInt32();
                triangleIndices.Add(index);
            }
            Remesh();
            SetMesh();
        }

        public void RelativeToAbsolute()
        {
            if (absoluteObjectVertices != null)
            {
                absoluteObjectVertices.Clear();
                absoluteObjectVertices = new Point3DCollection(relativeObjectVertices.Count);

                List<P3D> tmp;
                if (Rotation != null && (Rotation.X != 0 || Rotation.Y != 0 || Rotation.Z != 0))
                {
                    tmp = RotatePoints(relativeObjectVertices, Rotation.Y, Rotation.Z, Rotation.X); // ?????
                }
                else
                {
                    tmp = relativeObjectVertices;
                }

                absoluteBounds = new Bounds3D();

                foreach (P3D cp in tmp)
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

        public virtual void Remesh()
        {
            if (relativeObjectVertices != null && relativeObjectVertices.Count > 0)
            {
                RelativeToAbsolute();
                SetMesh();
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
            List<P3D> tmp = new List<P3D>();
            foreach (P3D cp in relativeObjectVertices)
            {
                P3D ap = new P3D();
                ap.X = (float)sx * cp.X;
                ap.Y = (float)sy * cp.Y;
                ap.Z = (float)sz * cp.Z;

                tmp.Add(ap);
            }
            relativeObjectVertices = tmp;
        }

        public void SetMesh()
        {
            surfaceMesh.Positions = absoluteObjectVertices;
            surfaceMesh.TriangleIndices = triangleIndices;
            surfaceMesh.Normals = normals;
        }

        public void SkewMesh(int moveSide, double x, double y, double z)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue); ;
            PointUtils.MinMax(relativeObjectVertices, ref min, ref max);

            List<P3D> tmp = new List<P3D>();

            foreach (P3D p in relativeObjectVertices)
            {
                switch (moveSide)
                {
                    case 0:
                        {
                            if (p.X < 0)
                            {
                                double offset = (-p.X / max.X) * (1 - y);
                                //tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                                tmp.Add(new P3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                //  tmp.Add(new Point3D(p.X, p.Y, p.Z));
                                tmp.Add(new P3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 1:
                        {
                            if (p.X > 0)
                            {
                                double offset = (p.X / max.X) * (1 - y);
                                //  tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                                tmp.Add(new P3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                //tmp.Add(new Point3D(p.X, p.Y, p.Z));
                                tmp.Add(new P3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 3:
                        {
                            if (p.Z < 0)
                            {
                                double offset = (p.Z / max.Z) * (1 - y);
                                // tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                                tmp.Add(new P3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                // tmp.Add(new Point3D(p.X, p.Y, p.Z));
                                tmp.Add(new P3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;

                    case 2:
                        {
                            if (p.Z > 0)
                            {
                                double offset = (p.Z / max.Z) * (1 - y);
                                // tmp.Add(new Point3D(p.X, p.Y - (p.Y * offset), p.Z));
                                tmp.Add(new P3D(p.X, p.Y - (p.Y * offset), p.Z));
                            }
                            else
                            {
                                //tmp.Add(new Point3D(p.X, p.Y, p.Z));
                                tmp.Add(new P3D(p.X, p.Y, p.Z));
                            }
                        }
                        break;
                }
            }
            relativeObjectVertices.Clear();
            //   foreach (Point3D p in tmp)
            foreach (P3D p in tmp)
            {
                relativeObjectVertices.Add(p);
            }
        }

        public void SwapYZAxies()
        {
            List<P3D> tmp = new List<P3D>();
            foreach (P3D pn in relativeObjectVertices)
            {
                tmp.Add(new P3D(pn.X, pn.Z, pn.Y));
            }
            relativeObjectVertices = tmp;
        }

        /*
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
        */

        public virtual XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement(XmlType);
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
            if (EditorParameters.HasContent)
            {
                EditorParameters.Write(doc, ele);
            }

            foreach (P3D v in relativeObjectVertices)
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

                faceEle.SetAttribute("v", $"{triangleIndices[i]},{triangleIndices[i + 1]},{triangleIndices[i + 2]}");
                ele.AppendChild(faceEle);
            }

            return ele;
        }

        public virtual void WriteBinary(BinaryWriter writer)
        {
            writer.Write((byte)0); // need a tag of some sort for deserialisation
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
            editorParameters.WriteBinary(writer);
            writer.Write(relativeObjectVertices.Count);

            foreach (P3D v in relativeObjectVertices)
            {
                writer.Write(v.X);
                writer.Write(v.Y);
                writer.Write(v.Z);
            }

            writer.Write(triangleIndices.Count);
            for (int i = 0; i < triangleIndices.Count; i++)
            {
                writer.Write(triangleIndices[i]);
            }
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
            List<P3D> tmp = new List<P3D>();
            PointUtils.PointCollectionToP3D(pnts, tmp);

            RelativeObjectVertices = tmp;
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

        private void AdjustBounds(P3D ap)
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

        private List<P3D> RotatePoints(List<P3D> pnts, double r1, double r2, double r3)
        {
            List<P3D> tmp = new List<P3D>();
            float cosa = (float)Math.Cos(r2);
            float sina = (float)Math.Sin(r2);

            float cosb = (float)Math.Cos(r1);
            float sinb = (float)Math.Sin(r1);

            float cosc = (float)Math.Cos(r3);
            float sinc = (float)Math.Sin(r3);

            float Axx = cosa * cosb;
            float Axy = cosa * sinb * sinc - sina * cosc;
            float Axz = cosa * sinb * cosc + sina * sinc;

            float Ayx = sina * cosb;
            float Ayy = sina * sinb * sinc + cosa * cosc;
            float Ayz = sina * sinb * cosc - cosa * sinc;

            float Azx = -sinb;
            float Azy = cosb * sinc;
            float Azz = cosb * cosc;
            foreach (P3D cp in pnts)
            {
                P3D rp = new P3D();
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