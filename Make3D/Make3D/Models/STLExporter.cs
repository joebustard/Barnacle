using CSGLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class STLExporter
    {
        public STLExporter()
        {
        }
        public void Export(string filename, List<Object3D> items)
        {
            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
         
            if ( stream != null )
            { 
                var writer = new BinaryWriter(stream);

                // write header
                var header = new byte[80]; // can be a garbage value
                writer.Write(header);
                uint totalTriangles = 0;
                foreach ( Object3D od in items)
                {
                    totalTriangles += (uint) od.TriangleIndices.Count / 3;
                }
                // write vertex count
                writer.Write((uint)totalTriangles);
                List<Face> faces = null;
                foreach (Object3D od in items)
                {
                    faces = od.GetFaces();
                    // write triangles
                    foreach (var triangle in faces)
                    {
                        Vector3D normal = triangle.GetNormal();
                        writer.Write((float)normal.X);
                        writer.Write((float)normal.Y);
                        writer.Write((float)normal.Z);
                    //    System.Diagnostics.Debug.WriteLine($"n {normal.X},{normal.Y},{normal.Z}");
                        writer.Write((float)triangle.V1.Position.X);
                        writer.Write((float)triangle.V1.Position.Y);
                        writer.Write((float)triangle.V1.Position.Z);
                     //   System.Diagnostics.Debug.WriteLine($"V1 {triangle.V1.Position.X},{triangle.V1.Position.Y},{triangle.V1.Position.Z}");

                        writer.Write((float)triangle.V2.Position.X);
                        writer.Write((float)triangle.V2.Position.Y);
                        writer.Write((float)triangle.V2.Position.Z);
                    //    System.Diagnostics.Debug.WriteLine($"V2 {triangle.V2.Position.X},{triangle.V2.Position.Y},{triangle.V2.Position.Z}");


                        writer.Write((float)triangle.V3.Position.X);
                        writer.Write((float)triangle.V3.Position.Y);
                        writer.Write((float)triangle.V3.Position.Z);
                     //   System.Diagnostics.Debug.WriteLine($"V3 {triangle.V3.Position.X},{triangle.V3.Position.Y},{triangle.V3.Position.Z}");
                        writer.Write((ushort)0); // garbage value
                    }

                }

                writer.Flush();
                int size = 84 + (int)totalTriangles * 50;
                System.Diagnostics.Debug.WriteLine($"File should be {size} bytes");
            }
            stream.Close();
        }
    }
}
