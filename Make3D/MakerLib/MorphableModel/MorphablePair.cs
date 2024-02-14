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
        private double[] distances1;
        private double[] distances2;

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

        private double[] GetDistances(string n)
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
                    int index = GetOffset(theta, phi);
                    double d1 = distances1[index];
                    double d2 = distances2[index];
                    res = d1 + t * (d2 - d1);
                }
            }
            return res;
        }

        private int GetOffset(double theta, double phi)
        {
            int row = (int)theta / 2;
            int col = ((int)phi / 2) + 45;
            return (row * 90) + col;
        }

        public void Generate(double warpfactor, Point3DCollection vertices, Int32Collection faces)
        {
            Vertices = vertices;
            Faces = faces;
            vertices.Clear();
            faces.Clear();
            CreateOctree(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
            if (distances1 != null && distances2 != null)
            {
                double resolution = 2.0;
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
                        double d2 = MorphedDistance(theta, phi + ph2, warpfactor);
                        double d3 = MorphedDistance(t2, phi, warpfactor);
                        double d4 = MorphedDistance(t2, phi + ph2, warpfactor);

                        Point3D p1 = ConvertPolarTo3D(theta, phi, d1);
                        Point3D p2 = ConvertPolarTo3D(theta, phi + ph2, d2);
                        Point3D p3 = ConvertPolarTo3D(t2, phi, d3);
                        Point3D p4 = ConvertPolarTo3D(t2, phi + ph2, d4);
                        int v1 = AddVerticeOctTree(p1);
                        int v2 = AddVerticeOctTree(p2);
                        int v3 = AddVerticeOctTree(p3);
                        int v4 = AddVerticeOctTree(p4);
                        AddFace(v1, v2, v3);
                        AddFace(v3, v2, v4);
                    }
                }
            }
        }

        private Point3D ConvertPolarTo3D(double theta, double phi, double t)
        {
            double x = t * Math.Sin(phi) * Math.Cos(theta);
            double z = t * Math.Sin(phi) * Math.Sin(theta);
            double y = t * Math.Cos(phi);
            return new Point3D(x, y, z);
        }
    }
}