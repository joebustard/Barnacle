using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib.MorphableModel
{
    public partial class MorphableModelMaker : MakerBase
    {
        private string name1;
        private string name2;
        private double[,] distances1;
        private double[,] distances2;
        private float[,,] dirVectors;

        public MorphableModelMaker(string n1, string n2)
        {
            Shape1 = n1;
            Shape2 = n2;
            dirVectors = DirectionVectors();
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

                case "star6":
                    {
                        return MorphablePrimitivesGenerator.GenerateStar6();
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

        private double MorphedDistance(int row, int col, double t)
        {
            double res = -1;
            if (distances1 != null && distances2 != null)
            {
                if (row >= 0 && row < distances1.GetLength(0) && col >= 0 && col < distances1.GetLength(1))
                {
                    double d1 = distances1[row, (col)];
                    double d2 = distances2[row, (col)];
                    res = d1 + t * (d2 - d1);
                }
            }
            return res;
        }

        private double GetDistance(double row, double col, double[,] distances)
        {
            double res = -1;
            if (distances != null)
            {
                res = distances[(int)row, (int)col];
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
                int rows = distances1.GetLength(0);
                int cols = distances1.GetLength(1);
                double dTheta = 360.0 / rows;
                double dPhi = 180.0 / cols;
                for (int i = 0; i < rows; i++)
                {
                    double theta = dTheta * (double)i;
                    double t2 = theta + dTheta;

                    if (t2 >= 360)
                    {
                        t2 = 0;
                    }
                    int k = i + 1;
                    if (k == rows)
                    {
                        k = 0;
                    }
                    for (int j = 0; j < cols - 1; j++)
                    {
                        double phi = (dPhi * (double)j) - 90.0;
                        double ph2 = phi + dPhi;
                        double d1 = MorphedDistance(i, j, warpfactor);
                        double d2 = MorphedDistance(i, j + 1, warpfactor);
                        double d3 = MorphedDistance(k, j, warpfactor);
                        double d4 = MorphedDistance(k, j + 1, warpfactor);
                        /*
                        Point3D p1 = ConvertPolarTo3D(theta, phi, d1);
                        Point3D p2 = ConvertPolarTo3D(theta, ph2, d2);
                        Point3D p3 = ConvertPolarTo3D(t2, phi, d3);
                        Point3D p4 = ConvertPolarTo3D(t2, ph2, d4);
                        */

                        Point3D p1 = ConvertVectorTo3D(i, j, d1);
                        Point3D p2 = ConvertVectorTo3D(i, j + 1, d2);
                        Point3D p3 = ConvertVectorTo3D(k, j, d3);
                        Point3D p4 = ConvertVectorTo3D(k, j + 1, d4);

                        int v1 = AddVerticeOctTree(p1);
                        int v2 = AddVerticeOctTree(p2);
                        int v3 = AddVerticeOctTree(p3);
                        int v4 = AddVerticeOctTree(p4);
                        AddFace(v1, v2, v3);
                        AddFace(v3, v2, v4);
                    }
                    //                    break;
                }
            }
        }

        private Point3D ConvertVectorTo3D(int i, int j, double d)
        {
            Vector3D v = new Vector3D();
            v.X = dirVectors[i, j, 0];
            v.Y = dirVectors[i, j, 1];
            v.Z = dirVectors[i, j, 2];
            return new Point3D(
            v.X * d,
            v.Y * d,
            v.Z * d
            ); ;
        }

        private void GenerateSingle(double[,] distances)
        {
            int rows = distances.GetLength(0);
            int cols = distances.GetLength(1);
            double dTheta = 360.0 / rows;
            double dPhi = 180.0 / cols;
            for (int i = 0; i < rows; i++)
            {
                double theta = dTheta * (double)i;
                double t2 = theta + dTheta;
                if (t2 >= 360)
                {
                    t2 = 0;
                }
                int k = i + 1;
                if (k == rows)
                {
                    k = 0;
                }
                for (int j = 0; j < cols - 1; j++)
                {
                    double phi = (dPhi * (double)j) - 90.0;
                    double ph2 = phi + dPhi;

                    double d1 = GetDistance(i, j, distances);
                    double d2 = GetDistance(i, j + 1, distances);
                    double d3 = GetDistance(k, j, distances);
                    double d4 = GetDistance(k, j + 1, distances);
                    /*
                    Point3D p1 = ConvertPolarTo3D(theta, phi, d1);
                    Point3D p2 = ConvertPolarTo3D(theta, ph2, d2);
                    Point3D p3 = ConvertPolarTo3D(t2, phi, d3);
                    Point3D p4 = ConvertPolarTo3D(t2, ph2, d4);
                    */

                    Point3D p1 = ConvertVectorTo3D(i, j, d1);
                    Point3D p2 = ConvertVectorTo3D(i, j + 1, d2);
                    Point3D p3 = ConvertVectorTo3D(k, j, d3);
                    Point3D p4 = ConvertVectorTo3D(k, j + 1, d4);
                    //   File.AppendAllText("C:\\tmp\\asdisplayed.txt", $"t={theta},phi={phi}, P ={p1.X},{p1.Y},{p1.Z}\r\n");
                    /*
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
                    */
                    {
                        int v1 = AddVerticeOctTree(p1);
                        int v2 = AddVerticeOctTree(p2);
                        int v3 = AddVerticeOctTree(p3);
                        int v4 = AddVerticeOctTree(p4);
                        AddFace(v1, v2, v3);
                        AddFace(v3, v2, v4);
                    }
                }
                //                    break;
            }
        }

        private Point3D ConvertPolarTo3D(double azimuth, double elevation, double distance)
        {
            azimuth = DegToRad(azimuth);
            elevation = DegToRad(elevation);
            double x = distance * Math.Cos(elevation) * Math.Cos(azimuth);
            double z = distance * Math.Cos(elevation) * Math.Sin(azimuth);
            double y = distance * Math.Sin(elevation);
            /*
                        double x = Math.Cos(Rad(elevation)) * Math.Cos(Rad(azimuth));
                        double z = Math.Cos(Rad(elevation)) * Math.Sin(Rad(azimuth));
                        double y = Math.Sin(Rad(elevation));
                        */
            return new Point3D(x, y, z);
        }
    }
}