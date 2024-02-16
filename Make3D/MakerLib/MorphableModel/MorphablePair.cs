using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib.MorphableModel
{
    public class MorphableModelMaker : MakerBase
    {
        private string name1;
        private string name2;
        private double[,] distances1;
        private double[,] distances2;

        public MorphableModelMaker(string n1, string n2)
        {
            Shape1 = n1;
            Shape2 = n2;
        }

        public string Shape1
        {
            get { return name1; }
            set
            {
                if (value != name1)
                {
                    name1 = value;
                    distances1 = GetDistances(name1);
                };
            }
        }

        private double[,] GetDistances(string n)
        {
            switch (n.ToLower())
            {
                case "cube":
                case "box":
                    {
                        return MorphablePrimitivesGenerator.GenerateCube();
                    }

                case "cone":
                    {
                        return MorphablePrimitivesGenerator.GenerateCone();
                    }

                case "cylinder":
                    {
                        return MorphablePrimitivesGenerator.GenerateCylinder();
                    }

                case "octahedron":
                    {
                        return MorphablePrimitivesGenerator.GenerateOctahedron();
                    }

                case "pyramid":
                    {
                        return MorphablePrimitivesGenerator.GeneratePyramid();
                    }

                case "pyramid2":
                    {
                        return MorphablePrimitivesGenerator.GeneratePyramid2();
                    }

                case "roof":
                    {
                        return MorphablePrimitivesGenerator.GenerateRoof();
                    }

                case "roundroof":
                    {
                        return MorphablePrimitivesGenerator.GenerateRoundRoof();
                    }

                case "sphere":
                    {
                        return MorphablePrimitivesGenerator.GenerateSphere();
                    }
            }
            return null;
        }

        public string Shape2
        {
            get { return name2; }
            set
            {
                if (value != name2)
                {
                    name2 = value;
                    distances2 = GetDistances(name2);
                };
            }
        }

        private double MorphedDistance(double theta, double phi, double t)
        {
            double res = -1;
            if (distances1 != null && distances2 != null)
            {
                if (theta >= 0 && theta < 360 && phi >= -90 && phi < 90)
                {
                    double d1 = distances1[(int)theta, (int)(phi + 90)];
                    double d2 = distances2[(int)theta, (int)(phi + 90)];
                    res = d1 + t * (d2 - d1);
                }
            }
            return res;
        }

        private double GetDistance(double theta, double phi, double[,] distances)
        {
            double res = -1;
            if (distances != null)
            {
                if (theta >= 0 && theta < 360 && phi >= -90 && phi <= 90)
                {
                    res = distances[(int)theta, (int)phi + 90];
                }
            }
            return res;
        }

        private int GetOffset(double theta, double phi)
        {
            int columnWidth = (int)(180 / resolution) + 1;
            int row = (int)(theta / resolution);
            int col = (int)(phi / resolution) + (int)(90 / resolution);
            return (row * columnWidth) + col;
        }

        private double resolution = 1;

        public void Generate(double warpfactor, Point3DCollection vertices, Int32Collection faces)
        {
            Vertices = vertices;
            Faces = faces;
            vertices.Clear();
            faces.Clear();
            CreateOctree(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
            if (distances1 != null && distances2 == null)
            {
                GenerateSingle(distances1);
            }
            else if (distances1 == null && distances2 != null)
            {
                GenerateSingle(distances2);
            }
            else if (distances1 != null && distances2 != null)
            {
                for (double theta = 0; theta < 360; theta += resolution)
                {
                    double t2 = theta + resolution;
                    if (t2 >= 360)
                    {
                        t2 = 0;
                    }
                    for (double phi = -90; phi < 90; phi += resolution)
                    {
                        double ph2 = phi + resolution;
                        double d1 = MorphedDistance(theta, phi, warpfactor);
                        double d2 = MorphedDistance(theta, ph2, warpfactor);
                        double d3 = MorphedDistance(t2, phi, warpfactor);
                        double d4 = MorphedDistance(t2, ph2, warpfactor);

                        PolarCoordinate pc1 = new PolarCoordinate(DegToRad(theta), DegToRad(phi), d1);
                        PolarCoordinate pc2 = new PolarCoordinate(DegToRad(theta), DegToRad(ph2), d2);
                        PolarCoordinate pc3 = new PolarCoordinate(DegToRad(t2), DegToRad(phi), d3);
                        PolarCoordinate pc4 = new PolarCoordinate(DegToRad(t2), DegToRad(ph2), d4);

                        Point3D p1 = pc1.GetPoint3D();
                        Point3D p2 = pc2.GetPoint3D();
                        Point3D p3 = pc3.GetPoint3D();
                        Point3D p4 = pc4.GetPoint3D();
                        if (phi < 0)
                        {
                            p1.X *= -1.0;
                            p2.X *= -1.0;
                            p3.X *= -1.0;
                            p4.X *= -1.0;

                            p1.Y *= -1.0;
                            p2.Y *= -1.0;
                            p3.Y *= -1.0;
                            p4.Y *= -1.0;

                            p1.Z *= -1.0;
                            p2.Z *= -1.0;
                            p3.Z *= -1.0;
                            p4.Z *= -1.0;
                            int v1 = AddVerticeOctTree(p1);
                            int v2 = AddVerticeOctTree(p2);
                            int v3 = AddVerticeOctTree(p3);
                            int v4 = AddVerticeOctTree(p4);
                            AddFace(v1, v3, v2);
                            AddFace(v3, v4, v2);
                        }
                        else
                        {
                            int v1 = AddVerticeOctTree(p1);
                            int v2 = AddVerticeOctTree(p2);
                            int v3 = AddVerticeOctTree(p3);
                            int v4 = AddVerticeOctTree(p4);
                            AddFace(v1, v3, v2);
                            AddFace(v3, v4, v2);
                        }
                    }
                    //                    break;
                }
            }
        }

        private void GenerateSingle(double[,] distances)
        {
            for (double theta = 0; theta < 360; theta += resolution)
            {
                double t2 = theta + resolution;
                if (t2 >= 360)
                {
                    t2 = 0;
                }
                for (double phi = -90; phi < 90; phi += resolution)
                {
                    double ph2 = phi + resolution;
                    double d1 = GetDistance(theta, phi, distances);
                    double d2 = GetDistance(theta, ph2, distances);
                    double d3 = GetDistance(t2, phi, distances);
                    double d4 = GetDistance(t2, ph2, distances);
                    PolarCoordinate pc1 = new PolarCoordinate(DegToRad(theta), DegToRad(phi), d1);
                    PolarCoordinate pc2 = new PolarCoordinate(DegToRad(theta), DegToRad(ph2), d2);
                    PolarCoordinate pc3 = new PolarCoordinate(DegToRad(t2), DegToRad(phi), d3);
                    PolarCoordinate pc4 = new PolarCoordinate(DegToRad(t2), DegToRad(ph2), d4);
                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();
                    if (phi < 0)
                    {
                        p1.X *= -1.0;
                        p2.X *= -1.0;
                        p3.X *= -1.0;
                        p4.X *= -1.0;

                        p1.Y *= -1.0;
                        p2.Y *= -1.0;
                        p3.Y *= -1.0;
                        p4.Y *= -1.0;

                        p1.Z *= -1.0;
                        p2.Z *= -1.0;
                        p3.Z *= -1.0;
                        p4.Z *= -1.0;
                        int v1 = AddVerticeOctTree(p1);
                        int v2 = AddVerticeOctTree(p2);
                        int v3 = AddVerticeOctTree(p3);
                        int v4 = AddVerticeOctTree(p4);
                        AddFace(v1, v3, v2);
                        AddFace(v3, v4, v2);
                    }
                    else
                    {
                        int v1 = AddVerticeOctTree(p1);
                        int v2 = AddVerticeOctTree(p2);
                        int v3 = AddVerticeOctTree(p3);
                        int v4 = AddVerticeOctTree(p4);
                        AddFace(v1, v3, v2);
                        AddFace(v3, v4, v2);
                    }
                }
                //                    break;
            }
        }

        private Point3D ConvertPolarTo3D(double theta, double phi, double t)
        {
            theta = ((theta * Math.PI) / 180) - Math.PI;
            phi = (phi * Math.PI) / 180;
            if (phi < 0)
            {
                phi += 2 * Math.PI;
            }
            double x = t * Math.Sin(phi) * Math.Cos(theta);
            double z = t * Math.Sin(phi) * Math.Sin(theta);
            double y = t * Math.Cos(phi);
            return new Point3D(x, y, z);
        }
    }
}