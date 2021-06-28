using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.BezierSurface
{
    internal class Surface
    {
        public Surface()
        {
        }

        public ControlPointManager controlPointManager { get; set; }

        public void GenerateSurface(Point3DCollection vertices, Int32Collection tris, double numDivs = 5)
        {

            double delta = 1.0 / numDivs;
            vertices.Clear();
            tris.Clear();
            Vector3D off = new Vector3D(0, 1, 0);
            if (controlPointManager != null)
            {
                int patchStartRow = 0;
                int patchStartColumn = 0;
                patchStartRow = TopAndBottom(vertices, tris, delta, off, ref patchStartColumn);

                patchStartRow = CloseLeftAndRight(vertices, tris, delta, off, ref patchStartColumn);

            }
        }

        private int TopAndBottom(Point3DCollection vertices, Int32Collection tris, double delta, Vector3D off, ref int patchStartColumn)
        {
            int patchStartRow;
            for (patchStartRow = 0; patchStartRow < controlPointManager.PatchRows - 3; patchStartRow += 3)
            {
                for (patchStartColumn = 0; patchStartColumn < controlPointManager.PatchColumns - 3; patchStartColumn += 3)
                {

                    double u;
                    double v;
                    for (u = 0; u < 1; u += delta)
                    {
                        double u1 = u + delta;
                        double um = u + (delta / 2);
                        for (v = 0; v < 1; v += delta)
                        {
                            double v1 = v + delta;
                            double vm = v + (delta / 2.0);

                            // top
                            Point3D p1 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v);
                            Point3D p2 = GetSurfacePoint(patchStartRow, patchStartColumn, u1, v);
                            Point3D p3 = GetSurfacePoint(patchStartRow, patchStartColumn, u1, v1);
                            Point3D p4 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v1);
                            Point3D p5 = GetSurfacePoint(patchStartRow, patchStartColumn, um, vm);

                            int ve1 = AddVertice(p1, vertices);
                            int ve2 = AddVertice(p2, vertices);
                            int ve3 = AddVertice(p3, vertices);
                            int ve4 = AddVertice(p4, vertices);
                            int ve5 = AddVertice(p5, vertices);

                            tris.Add(ve1);
                            tris.Add(ve5);
                            tris.Add(ve2);

                            tris.Add(ve2);
                            tris.Add(ve5);
                            tris.Add(ve3);

                            tris.Add(ve3);
                            tris.Add(ve5);
                            tris.Add(ve4);

                            tris.Add(ve4);
                            tris.Add(ve5);
                            tris.Add(ve1);

                            // bottom

                            p1 = p1 - off;
                            p2 = p2 - off;
                            p3 = p3 - off;
                            p4 = p4 - off;
                            p5 = p5 - off;

                            ve1 = AddVertice(p1, vertices);
                            ve2 = AddVertice(p2, vertices);
                            ve3 = AddVertice(p3, vertices);
                            ve4 = AddVertice(p4, vertices);
                            ve5 = AddVertice(p5, vertices);

                            tris.Add(ve1);
                            tris.Add(ve2);
                            tris.Add(ve5);

                            tris.Add(ve2);
                            tris.Add(ve3);
                            tris.Add(ve5);

                            tris.Add(ve3);
                            tris.Add(ve4);
                            tris.Add(ve5);

                            tris.Add(ve4);
                            tris.Add(ve1);
                            tris.Add(ve5);
                        }
                    }
                }
            }

            return patchStartRow;
        }

        private int CloseLeftAndRight(Point3DCollection vertices, Int32Collection tris, double delta, Vector3D off, ref int patchStartColumn)
        {
            int patchStartRow;
            for (patchStartRow = 0; patchStartRow < controlPointManager.PatchRows - 3; patchStartRow += 3)
            {
                // left
                patchStartColumn = 0;

                double u;
                double v;
                u = 0;
                for (v = 0; v < 1; v += delta)
                {
                    double v1 = v + delta;

                    // top
                    Point3D p1 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v);
                    Point3D p2 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v1);
                    Point3D p3 = p2 - off;
                    Point3D p4 = p1 - off;

                    int ve1 = AddVertice(p1, vertices);
                    int ve2 = AddVertice(p2, vertices);
                    int ve3 = AddVertice(p3, vertices);
                    int ve4 = AddVertice(p4, vertices);


                    tris.Add(ve1);
                    tris.Add(ve3);
                    tris.Add(ve2);

                    tris.Add(ve1);
                    tris.Add(ve4);
                    tris.Add(ve3);

                }


                // right

                patchStartColumn = controlPointManager.PatchColumns - 4;
                u = 1;
                for (v = 0; v < 1; v += delta)
                {
                    double v1 = v + delta;

                    // top
                    Point3D p1 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v);
                    Point3D p2 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v1);
                    Point3D p3 = p2 - off;
                    Point3D p4 = p1 - off;

                    int ve1 = AddVertice(p1, vertices);
                    int ve2 = AddVertice(p2, vertices);
                    int ve3 = AddVertice(p3, vertices);
                    int ve4 = AddVertice(p4, vertices);


                    tris.Add(ve1);
                    tris.Add(ve2);
                    tris.Add(ve3);

                    tris.Add(ve1);
                    tris.Add(ve3);
                    tris.Add(ve4);

                }
            }

            return patchStartRow;
        }

        public Point3D GetBezier3D(Point3D p1, Point3D p2, Point3D p3, Point3D p4, double t)
        {
            double k1 = (1 - t) * (1 - t) * (1 - t);
            double k2 = 3 * (1 - t) * (1 - t) * t;
            double k3 = 3 * (1 - t) * t * t;
            double k4 = t * t * t;
            double x = (p1.X * k1) + (p2.X * k2) + (p3.X * k3) + (p4.X * k4);
            double y = (p1.Y * k1) + (p2.Y * k2) + (p3.Y * k3) + (p4.Y * k4);
            double z = (p1.Z * k1) + (p2.Z * k2) + (p3.Z * k3) + (p4.Z * k4);
            return new Point3D(x, y, z);
        }

        private int AddVertice(Point3D v, Point3DCollection vertices)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (PointUtils.equals(vertices[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = vertices.Count - 1;
            }
            return res;
        }

        private Point3D GetSurfacePoint(int rs, int rc, double u, double v)
        {
            Point3D[] upoints = new Point3D[4];
            ControlPoint[,] pnts = controlPointManager.AllcontrolPoints;
            for (int r = 0; r < 4; r++)
            {
                upoints[r] = GetBezier3D(
                    pnts[rs + r, rc].Position,
                    pnts[rs + r, rc + 1].Position,
                    pnts[rs + r, rc + 2].Position,
                    pnts[rs + r, rc + 3].Position,
                    u);
            }

            Point3D vpoint = GetBezier3D(upoints[0], upoints[1], upoints[2], upoints[3], v);
            return vpoint;
        }
    }
}