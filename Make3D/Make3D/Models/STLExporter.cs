using CSGLib;
using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class STLExporter
    {
        public STLExporter()
        {
        }

        public void Export(string filename, List<Object3D> items, Point3D rot, bool swapAxis, Bounds3D bnds)
        {
            double minZ = bnds.Lower.Z;
            double maxZ = bnds.Upper.Z;
            double swappedZ = 0;
            try
            {
                FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

                if (stream != null)
                {
                    var writer = new BinaryWriter(stream);

                    // write header
                    var header = new byte[80]; // can be a garbage value
                    writer.Write(header);
                    uint totalTriangles = 0;
                    foreach (Object3D od in items)
                    {
                        totalTriangles += (uint)od.TriangleIndices.Count / 3;
                    }
                    // write vertex count
                    writer.Write(totalTriangles);
                    List<Face> faces = null;
                    foreach (Object3D od in items)
                    {
                        Object3D cl = od.Clone();
                        cl.Rotation = new Point3D(cl.Rotation.X + rot.X, cl.Rotation.Y + rot.Y, cl.Rotation.Z + rot.Z);
                        cl.Remesh();
                        faces = cl.GetFaces();
                        // write triangles
                        foreach (var triangle in faces)
                        {
                            Vector3D normal = triangle.GetNormal();
                            writer.Write((float)normal.X);
                            writer.Write((float)normal.Z);
                            writer.Write((float)normal.Y);

                            writer.Write((float)triangle.V1.Position.X);
                            if (swapAxis)
                            {
                                swappedZ = maxZ - (triangle.V1.Position.Z - minZ);
                                writer.Write((float)swappedZ);
                                writer.Write((float)triangle.V1.Position.Y);
                            }
                            else
                            {
                                writer.Write((float)triangle.V1.Position.Y);
                                writer.Write((float)triangle.V1.Position.Z);
                            }

                            //   System.Diagnostics.Debug.WriteLine($"V1 {triangle.V1.Position.X},{triangle.V1.Position.Y},{triangle.V1.Position.Z}");

                            writer.Write((float)triangle.V2.Position.X);
                            if (swapAxis)
                            {
                                swappedZ = maxZ - (triangle.V2.Position.Z - minZ);
                                writer.Write((float)swappedZ);
                                writer.Write((float)triangle.V2.Position.Y);
                            }
                            else
                            {
                                writer.Write((float)triangle.V2.Position.Y);
                                writer.Write((float)triangle.V2.Position.Z);
                            }

                            //    System.Diagnostics.Debug.WriteLine($"V2 {triangle.V2.Position.X},{triangle.V2.Position.Y},{triangle.V2.Position.Z}");

                            writer.Write((float)triangle.V3.Position.X);
                            if (swapAxis)
                            {
                                swappedZ = maxZ - (triangle.V3.Position.Z - minZ);
                                writer.Write((float)swappedZ);
                                writer.Write((float)triangle.V3.Position.Y);
                            }
                            else
                            {
                                writer.Write((float)triangle.V3.Position.Y);
                                writer.Write((float)triangle.V3.Position.Z);
                            }

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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Import(string filename, ref Vector3DCollection normals, ref Point3DCollection pnts, ref Int32Collection tris)
        {
            normals.Clear();
            pnts.Clear();
            tris.Clear();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (stream != null)
            {
                var reader = new BinaryReader(stream);
                var header = new byte[80]; // can be a garbage value
                reader.Read(header, 0, 80);
                uint totalTriangles = reader.ReadUInt32();
                float x, y, z;
                for (int i = 0; i < totalTriangles; i++)
                {
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    normals.Add(new Vector3D(x, z, y));
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    pnts.Add(new Point3D(x, z, y));
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    pnts.Add(new Point3D(x, z, y));
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    pnts.Add(new Point3D(x, z, y));
                    tris.Add(pnts.Count - 1);
                    ushort dummy = reader.ReadUInt16();
                }
                stream.Close();
            }
        }
    }
}