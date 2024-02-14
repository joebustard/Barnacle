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
        public static double [] Generate!PRIM!()
        {
            // resolution !RES! degrees
            double [] distances =
            {
!VALS!
            };
           return distances;
        }
    }
}
";

        private static double resDegrees = 2;

        private static double FindInterceptionDistance(Object3D model, Vector3D origin, Vector3D dir)
        {
            bool res = false;
            int faceIndex = 0;
            double distance = -1;
            while (faceIndex < model.TriangleIndices.Count && res == false)
            {
                int f0 = model.TriangleIndices[faceIndex];
                int f1 = model.TriangleIndices[faceIndex + 1];
                int f2 = model.TriangleIndices[faceIndex + 2];
                Vector3D v0 = new Vector3D(model.RelativeObjectVertices[f0].X, model.RelativeObjectVertices[f0].Y, model.RelativeObjectVertices[f0].Z);
                Vector3D v1 = new Vector3D(model.RelativeObjectVertices[f1].X, model.RelativeObjectVertices[f1].Y, model.RelativeObjectVertices[f1].Z);
                Vector3D v2 = new Vector3D(model.RelativeObjectVertices[f2].X, model.RelativeObjectVertices[f2].Y, model.RelativeObjectVertices[f2].Z);
                if (Utils.RayTriangleIntersect(origin, dir, v0, v1, v2, out distance))
                {
                    res = true;
                }

                faceIndex += 3;
            }
            return distance;
        }

        private static void Main(string[] args)
        {
            MakeMorphableShape("Cube");
            MakeMorphableShape("Cone");
            MakeMorphableShape("Cylinder");
            MakeMorphableShape("Octahedron");
            MakeMorphableShape("Pyramid");
            MakeMorphableShape("Pyramid2");
            MakeMorphableShape("Roof");
            MakeMorphableShape("RoundRoof");
            MakeMorphableSphere("Sphere");
        }

        private static void MakeMorphableShape(string v)
        {
            Object3D ob = new Object3D();
            ob.BuildPrimitive(v);
            Vector3D origin = new Vector3D(0, 0, 0);
            string outline = "\t\t\t";
            for (double theta = 0; theta < 360; theta += resDegrees)
            {
                for (double phi = -90; phi < 90; phi += resDegrees)
                {
                    double x = Math.Cos(Rad(theta)) * Math.Cos(Rad(phi));
                    double z = Math.Sin(Rad(theta)) * Math.Cos(Rad(phi));
                    double y = Math.Sin(Rad(phi));
                    Vector3D dir = new Vector3D(x, y, z);
                    double dist = FindInterceptionDistance(ob, origin, dir);
                    outline += dist.ToString("F6") + ", ";
                }
                outline += "\r\n\t\t\t";
            }
            string b = body;
            b = b.Replace("!RES!", resDegrees.ToString("F1"));
            b = b.Replace("!VALS!", outline);
            b = b.Replace("!PRIM!", v);
            File.WriteAllText("c:\\tmp\\Morphable" + v + ".cs", b);
        }

        private static void MakeMorphableSphere(string v)
        {
            Object3D ob = new Object3D();
            ob.BuildPrimitive(v);
            Vector3D origin = new Vector3D(0, 0, 0);
            string outline = "\t\t\t";
            for (double theta = 0; theta < 360; theta += resDegrees)
            {
                for (double phi = -90; phi < 90; phi += resDegrees)
                {
                    double dist = 0.5;
                    outline += dist.ToString("F1") + ", ";
                }
                outline += "\r\n\t\t\t";
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