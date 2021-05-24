using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs.Figure
{
    internal class Bone
    {
        private Point3D endPos;
        private double height;
        private double length;
        private Point3D midPos;

        private Point3D startPos;
        private double xrot;
        private double xRotRadians;
        private double yrot;
        private double yRotRadians;
        private double zrot;
        private double zRotRadians;

        public Bone()
        {
            Name = "";
            ModelName = "";
            Length = 0;
            Width = 5;
            height = 5;
            XRot = 0;
            YRot = 0;
            ZRot = 0;
            Nxr = 0;
            Nyr = 0;
            Nzr = 0;
            xRotRadians = 0;
            yRotRadians = 0;
            zRotRadians = 0;
            MinXRot = -359.0;
            MaxXRot = 359;
            MinYRot = -359;
            MaxYRot = 359;
            MinZRot = -359;
            MaxZRot = 359;
            SubBones = new List<Bone>();
        }

        public Point3D EndPosition
        {
            get => endPos;
            set
            {
                if (value != endPos)
                {
                    endPos = value;
                }
            }
        }

        public string FigureModelName { get; set; }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    height = value;
                }
            }
        }

        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                if (length != value)
                {
                    length = value;
                }
            }
        }

        public double MaxXRot { get; set; }
        public double MaxYRot { get; set; }
        public double MaxZRot { get; set; }

        public Point3D MidPosition
        {
            get => midPos;
            set
            {
                if (value != midPos)
                {
                    midPos = value;
                }
            }
        }

        public double MinXRot { get; set; }
        public double MinYRot { get; set; }
        public double MinZRot { get; set; }
        public string ModelName { get; set; }
        public string Name { get; set; }
        public double Nxr { get; set; }
        public double Nyr { get; set; }
        public double Nzr { get; set; }

        public Point3D StartPosition
        {
            get => startPos;
            set
            {
                if (value != startPos)
                {
                    startPos = value;
                }
            }
        }

        public List<Bone> SubBones { get; set; }

        public double Width { get; set; }

        public double XRot
        {
            get => xrot;
            set
            {
                if (xrot != value)
                {
                    xrot = value;
                    xRotRadians = Rad(xrot);
                }
            }
        }

        public double YRot
        {
            get => yrot;
            set
            {
                if (yrot != value)
                {
                    yrot = value;
                    yRotRadians = Rad(yrot);
                }
            }
        }

        public double ZRot
        {
            get => zrot;
            set
            {
                if (zrot != value)
                {
                    zrot = value;
                    zRotRadians = Rad(zrot);
                }
            }
        }

        public static double Degrees(double v)
        {
            return (v * 180.0 / Math.PI);
        }

        public static double Rad(double v)
        {
            return (v * Math.PI) / 180.0;
        }

        public Bone AddSub(string n, double l, double w, double h, double xr, double yr, double zr, double minx, double maxx, double miny, double maxy, double minz, double maxz, string boneModel = "bone", string figureModel = "bone")

        {
            Bone res = new Bone()
            {
                Name = n,
                Length = l,
                Width = w,
                Height = h,
                XRot = xr,
                YRot = yr,
                ZRot = zr,

                MinXRot = minx,
                MaxXRot = maxx,

                MinYRot = miny,
                MaxYRot = maxy,

                MinZRot = minz,
                MaxZRot = maxz,
                ModelName = boneModel.ToLower(),
                FigureModelName = figureModel.ToLower()
            };

            res.Nxr = Nxr + res.xRotRadians;
            res.Nyr = Nyr + res.yRotRadians;
            res.Nzr = Nzr + res.zRotRadians;
            SubBones.Add(res);

            return res;
        }

        public Point3DCollection RotatedPointCollection(Point3DCollection src, double nxr, double nyr, double nzr)
        {
            Point3DCollection res = new Point3DCollection();
            Matrix3D m3d = CalculateRotationMatrix(Degrees(nxr), Degrees(nyr), Degrees(nzr));
            MatrixTransform3D transform = new MatrixTransform3D(m3d);

            for (int i = 0; i < src.Count; i++)
            {
                res.Add(transform.Transform(src[i]));
            }
            return res;
        }

        // only call this on the root bone
        public void Update()
        {
            UpdateSubBones(EndPosition.X, EndPosition.Y, EndPosition.Z, xRotRadians, yRotRadians, zRotRadians);
        }

        internal void AllChildNames(List<string> res)
        {
            foreach (Bone bn in SubBones)
            {
                res.Add(bn.Name);
                bn.AllChildNames(res);
            }
        }

        internal void Dump(int v)
        {
            StringBuilder sb = new StringBuilder();
            sb = sb.Append(' ', v * 2);
            string indent = sb.ToString();
            System.Diagnostics.Debug.WriteLine($"{indent}{Name} {startPos.X},{startPos.Y},{startPos.Z} => {endPos.X},{endPos.Y},{endPos.Z}");

            System.Diagnostics.Debug.WriteLine($"{indent}[");
            foreach (Bone b in SubBones)
            {
                b.Dump(v + 1);
            }
            System.Diagnostics.Debug.WriteLine($"{indent}]");
        }

        internal void GetSubPositions(List<BoneDisplayRecord> pnts, bool figure = false)
        {
            foreach (Bone bn in SubBones)
            {
                BoneDisplayRecord br = new BoneDisplayRecord();
                br.Name = bn.Name;
                if (figure)
                {
                    br.ModelName = bn.FigureModelName;
                }
                else
                {
                    br.ModelName = bn.ModelName;
                }
                br.Position = new Point3D(bn.MidPosition.X, bn.MidPosition.Y, bn.MidPosition.Z);
                br.MarkerPosition = new Point3D(bn.StartPosition.X, bn.StartPosition.Y, bn.StartPosition.Z);

                br.Rotation = new Point3D(bn.Nxr, bn.Nyr, bn.Nzr);
                br.Scale = new Scale3D(bn.Length, bn.Height, bn.Width);
                br.Bone = bn;
                pnts.Add(br);
                bn.GetSubPositions(pnts, figure);
            }
        }

        internal void Load(XmlElement e)
        {
            Name = e.GetAttribute("Name");
            Name = e.GetAttribute("Name");
            XRot = Convert.ToDouble(e.GetAttribute("Xr"));
            YRot = Convert.ToDouble(e.GetAttribute("Yr"));
            ZRot = Convert.ToDouble(e.GetAttribute("Zr"));
            Width = Convert.ToDouble(e.GetAttribute("W"));
            Height = Convert.ToDouble(e.GetAttribute("H"));
            Length = Convert.ToDouble(e.GetAttribute("L"));
            ModelName = e.GetAttribute("M");
            FigureModelName = e.GetAttribute("FM");
            XmlNodeList nl = e.SelectNodes("Bone");
            foreach (XmlNode n in nl)
            {
                Bone bn = new Bone();
                SubBones.Add(bn);
                bn.Load(n as XmlElement);
            }
        }

        internal void Save(XmlDocument doc, XmlElement parent)
        {
            XmlElement ele = doc.CreateElement("Bone");
            ele.SetAttribute("Name", Name);
            
            ele.SetAttribute("L", Length.ToString());
            ele.SetAttribute("W", Width.ToString());

            ele.SetAttribute("H", Height.ToString());

            ele.SetAttribute("Xr", XRot.ToString());
            ele.SetAttribute("Yr", YRot.ToString());
            ele.SetAttribute("Zr", ZRot.ToString());

            ele.SetAttribute("M", ModelName);
            ele.SetAttribute("FM", FigureModelName);
            parent.AppendChild(ele);

            foreach (Bone bn in SubBones)
            {
                bn.Save(doc, ele);
            }
        }

        private Matrix3D CalculateRotationMatrix(double x, double y, double z)
        {
            Matrix3D matrix = new Matrix3D();

            matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), x));
            matrix.Rotate(new Quaternion(new Vector3D(0, 1, 0) * matrix, y));
            matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1) * matrix, z));

            return matrix;
        }

        private Point3D RotatedPoint(double length, double nxr, double nyr, double nzr)
        {
            Matrix3D m3d = CalculateRotationMatrix(Degrees(nxr), Degrees(nyr), Degrees(nzr));
            MatrixTransform3D transform = new MatrixTransform3D(m3d);
            Point3D res = transform.Transform(new Point3D(length, 0, 0));

            return res;
        }

        private void UpdateSubBones(double x, double y, double z, double parentXr, double parentYr, double parentZr)
        {
            if (SubBones != null)
            {
                double sx = x;
                double sy = y;
                double sz = z;
                StartPosition = new Point3D(x, y, z);

                Nxr = parentXr + xRotRadians;
                Nyr = parentYr + yRotRadians;
                Nzr = parentZr + zRotRadians;
                Point3D rotn = RotatedPoint(Length, Nxr, Nyr, Nzr);
                endPos = new Point3D(sx + rotn.X, sy + rotn.Y, sz + rotn.Z);

                rotn = RotatedPoint(Length / 2.0, Nxr, Nyr, Nzr);
                midPos = new Point3D(sx + rotn.X, sy + rotn.Y, sz + rotn.Z);

                foreach (Bone b in SubBones)
                {
                    b.UpdateSubBones(endPos.X, endPos.Y, endPos.Z, Nxr, Nyr, Nzr);
                }
            }
        }
    }
}