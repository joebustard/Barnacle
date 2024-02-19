using Barnacle.Object3DLib;
using PrintPlacementLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CreateMorphablePrimitives
{
    internal class Program
    {
        private static string body =
            @"using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib.MorphableModel
{
    public partial class MorphablePrimitivesGenerator
    {
        public static double [,] Generate!PRIM!()
        {
            // resolution !RES! degrees
            double [,] distances =
            {
!VALS!
            };
           return distances;
        }
    }
}
";

        private static string vectorbody =
            @"using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib.MorphableModel
{
    public partial class MorphableModelMaker : MakerBase
    {
        public  float [,,] DirectionVectors()
        {
            // resolution !RES! degrees
            float [,,] vectors =
{
!VALS!;
}
           return vectors;
        }
    }
}
";

        private static double resDegrees = 1.5;

        private static double FindInterceptionDistance(Object3D model, Vector3D origin, Vector3D dir)
        {
            bool res = false;
            int faceIndex = 0;
            double distance = -1;
            double d1;
            while (faceIndex < model.TriangleIndices.Count && res == false)
            {
                int f0 = model.TriangleIndices[faceIndex];
                int f1 = model.TriangleIndices[faceIndex + 1];
                int f2 = model.TriangleIndices[faceIndex + 2];
                Vector3D v0 = new Vector3D(model.RelativeObjectVertices[f0].X, model.RelativeObjectVertices[f0].Y, model.RelativeObjectVertices[f0].Z);
                Vector3D v1 = new Vector3D(model.RelativeObjectVertices[f1].X, model.RelativeObjectVertices[f1].Y, model.RelativeObjectVertices[f1].Z);
                Vector3D v2 = new Vector3D(model.RelativeObjectVertices[f2].X, model.RelativeObjectVertices[f2].Y, model.RelativeObjectVertices[f2].Z);

                if (Utils.RayTriangleIntersect(origin, dir, v0, v1, v2, out d1))
                {
                    res = true;
                    distance = d1;
                }

                faceIndex += 3;
            }
            return distance;
        }

        private static void Main(string[] args)
        {
            MakeVectorTable();
            MakeMorphableShape("Cube");

            MakeMorphableShape("Star6", true);
            MakeMorphableShape("Cylinder");
            MakeMorphableShape("Octahedron");
            MakeMorphableShape("Pyramid", true);
            MakeMorphableShape("Pyramid2", true);
            MakeMorphableShape("Roof", true);
            MakeMorphableShape("RoundRoof", true);
            MakeMorphableSphere("Sphere");
        }

        private static void MakeVectorTable()
        {
            Vector3D origin = new Vector3D(0, 0, 0);
            string outline = "";
            int rows = 0;
            int cols = 0;
            for (double theta = 0; theta < 360; theta += resDegrees)
            {
                outline += "\t\t{ ";

                cols = 0;
                for (double phi = -90; phi < 90; phi += resDegrees)
                {
                    Vector3D dir = GetDirectionVector(theta, phi);
                    outline += "{" + $"{(float)dir.X}F,{(float)dir.Y}F,{(float)dir.Z}F" + "},";
                    cols++;
                }
                outline += " },\r\n";
                rows++;
            }
            string b = vectorbody;
            b = b.Replace("!RES!", resDegrees.ToString("F1"));
            b = b.Replace("!ROWS!", rows.ToString());
            b = b.Replace("!COLS!", cols.ToString());
            b = b.Replace("!VALS!", outline);

            File.WriteAllText("c:\\tmp\\DirVectors.cs", b);
        }

        private static void MakeMorphableShape(string v, bool rotate = false)
        {
            Object3D ob = new Object3D();
            ob.BuildPrimitive(v);
            if (rotate)
            {
                ob.Rotate(new Point3D(-90, 0, 0));
            }
            Vector3D origin = new Vector3D(0, 0, 0);
            string outline = "\t\t\t";
            for (double theta = 0; theta < 360; theta += resDegrees)
            {
                outline += "{ ";
                for (double phi = -90; phi < 90; phi += resDegrees)
                {
                    Vector3D dir = GetDirectionVector(theta, phi);

                    double dist = FindInterceptionDistance(ob, origin, dir);
                    //   System.Diagnostics.Debug.WriteLine($"theta={theta}, phi = {phi}, dir = {dir.X},{dir.Y},{dir.Z}, dist={dist}");
                    outline += dist.ToString("F8") + ", ";
                }
                outline += " },\r\n\t\t\t";
            }
            string b = body;
            b = b.Replace("!RES!", resDegrees.ToString("F1"));
            b = b.Replace("!VALS!", outline);
            b = b.Replace("!PRIM!", v);
            File.WriteAllText("c:\\tmp\\Morphable" + v + ".cs", b);
        }

        private static Vector3D GetDirectionVector(double azimuth, double elevation)
        {
            double x = Math.Cos(Rad(elevation)) * Math.Cos(Rad(azimuth));
            double z = Math.Cos(Rad(elevation)) * Math.Sin(Rad(azimuth));
            double y = Math.Sin(Rad(elevation));

            Vector3D dir = new Vector3D(x, y, z);
            dir.Normalize();

            return dir;
        }

        private static void MakeMorphableSphere(string v)
        {
            Object3D ob = new Object3D();
            ob.BuildPrimitive(v);
            Vector3D origin = new Vector3D(0, 0, 0);
            string outline = "\t\t\t";
            for (double theta = 0; theta < 360; theta += resDegrees)
            {
                outline += "{ ";
                for (double phi = -90; phi < 90; phi += resDegrees)
                {
                    double dist = 0.5;
                    outline += dist.ToString("F1") + ", ";
                }
                outline += " },\r\n\t\t\t";
            }
            string b = body;
            b = b.Replace("!VALS!", outline);
            b = b.Replace("!PRIM!", v);
            File.WriteAllText("c:\\tmp\\Morphable" + v + ".cs", b);
        }

        private static double Rad(double theta)
        {
            return (theta * Math.PI) / 180.0;
        }
    }
}