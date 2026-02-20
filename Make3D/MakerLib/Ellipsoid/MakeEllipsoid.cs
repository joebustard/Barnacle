using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class EllipsoidMaker : MakerBase
    {
        private bool half;
        private double leftLength;

        private double rightLength;
        private double shapeHeight;
        private double shapeWidth;

        public EllipsoidMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double lx = -leftLength - 1;
            double ly = -shapeHeight - 1;
            double lz = -shapeWidth / 2 - 1;
            double hx = rightLength + 1;
            double hy = shapeHeight + 1;
            double hz = shapeWidth / 2 + 1;
            CreateOctree(new Point3D(lx, ly, lz), new Point3D(hx, hy, hz));

            if (half)
            {
                GenerateHalf();
            }
            else
            {
                GenerateFull();
            }
        }

        public void SetValues(double rightLength, double leftLength, double upperHeight, double width, bool half)
        {
            this.rightLength = rightLength;
            this.leftLength = leftLength;
            this.shapeHeight = upperHeight;

            this.shapeWidth = width;
            this.half = half;
        }

        private Point3D CalcPoint(double a, double b, double c, double u, double v)
        {
            double x = a * Math.Cos(u) * Math.Sin(v);
            double y = b * Math.Sin(u) * Math.Sin(v);
            double z = c * Math.Cos(v);
            return new Point3D(x, y, z);
        }

        private void GenerateFull()
        {
            double a = rightLength;
            double b = shapeHeight / 2;
            double c = shapeWidth / 2;
            double TwoPi = 2 * Math.PI;
            double PiByTwo = Math.PI / 2;
            double ThreePiByTwo = PiByTwo * 3;
            double u;
            double v;
            //double du = TwoPi / 360;
            //double dv = Math.PI / 180;
            double du = TwoPi / 180;
            double dv = Math.PI / 90;
            for (u = 0; u < TwoPi; u += du)
            {
                double u2 = u + du;
                b = shapeHeight / 2;
                a = rightLength;
                if (u >= PiByTwo && u < ThreePiByTwo)
                {
                    //b = lowerHeight;
                    a = leftLength;
                }
                if (u2 >= TwoPi)
                {
                    u2 = 0;
                }
                for (v = 0; v < Math.PI; v += dv)
                {
                    double v2 = v + dv;
                    if (v2 >= Math.PI)
                    {
                        v2 = 0;
                    }
                    Point3D p1 = CalcPoint(a, b, c, u, v);
                    Point3D p2 = CalcPoint(a, b, c, u2, v);
                    Point3D p3 = CalcPoint(a, b, c, u, v2);
                    Point3D p4 = CalcPoint(a, b, c, u2, v2);
                    int c1 = AddVerticeOctTree(p1);
                    int c2 = AddVerticeOctTree(p2);
                    int c3 = AddVerticeOctTree(p3);
                    int c4 = AddVerticeOctTree(p4);
                    AddFace(c1, c3, c2);
                    AddFace(c2, c3, c4);
                }
            }
        }

        private void GenerateHalf()
        {
            double a = rightLength;
            double b = shapeHeight / 2;
            double c = shapeWidth / 2;
            double TwoPi = 2 * Math.PI;
            double PiByTwo = Math.PI / 2;
            double ThreePiByTwo = PiByTwo * 3;
            double u;
            double v;
            //double du = TwoPi / 360;
            //double dv = Math.PI / 180;
            double du = TwoPi / 180;
            double dv = Math.PI / 90;
            for (u = 0; u < Math.PI; u += du)
            {
                double u2 = u + du;
                b = shapeHeight / 2;
                a = rightLength;
                if (u >= PiByTwo && u < ThreePiByTwo)
                {
                    //b = lowerHeight;
                    a = leftLength;
                }
                if (u2 >= TwoPi)
                {
                    u2 = 0;
                }
                for (v = 0; v < Math.PI; v += dv)
                {
                    double v2 = v + dv;
                    if (v2 >= Math.PI)
                    {
                        v2 = 0;
                    }
                    Point3D p1 = CalcPoint(a, b, c, u, v);
                    Point3D p2 = CalcPoint(a, b, c, u2, v);
                    Point3D p3 = CalcPoint(a, b, c, u, v2);
                    Point3D p4 = CalcPoint(a, b, c, u2, v2);
                    int c1 = AddVerticeOctTree(p1);
                    int c2 = AddVerticeOctTree(p2);
                    int c3 = AddVerticeOctTree(p3);
                    int c4 = AddVerticeOctTree(p4);
                    AddFace(c1, c3, c2);
                    AddFace(c2, c3, c4);
                }
            }
            // close the bottom
            Int32Collection bottomFaces = new Int32Collection();
            for (int i = Faces.Count - 1; i >= 0; i--)
            {
                int f = Faces[i];
                Point3D vi = new Point3D(Vertices[f].X, 0, Vertices[f].Z);
                int cn = AddVerticeOctTree(vi);
                bottomFaces.Add(cn);
            }
            foreach (int fi in bottomFaces)
            {
                Faces.Add(fi);
            }
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("RightLength", 0.1, 200);
            paramLimits.AddLimit("LeftLength", 0.1, 200);
            paramLimits.AddLimit("ShapeHeight", 0.1, 200);
            paramLimits.AddLimit("ShapeWidth", 0.1, 200);
        }
    }
}