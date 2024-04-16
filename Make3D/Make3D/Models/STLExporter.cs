using Barnacle.Object3DLib;
using CSGLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using OctTreeLib;
using OctTree = OctTreeLib.OctTree;

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

        public void Import(string filename, ref Vector3DCollection normals, ref Point3DCollection pnts, ref Int32Collection tris, bool swapYZ)
        {
            normals.Clear();
            pnts.Clear();
            tris.Clear();
            // we are trying to import a binary stl file but
            // it might actually be a different format sneakily pretending to be stl
            // Take a peek
            String ft = CheckFileFormat(filename);
            if (ft == "BinSTL")
            {
                ReadBinaryStl(filename, normals, pnts, tris, swapYZ);
            }
            else if (ft == "AsciiSTL")
            {
                ReadAsciiStl(filename, normals, pnts, tris, swapYZ);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="normals"></param>
        /// <param name="pnts"></param>
        /// <param name="tris"></param>
        /// <param name="swapYZ"></param>
        /// <returns>returns true if stl was binary</returns>
        public bool Import(string filename, ref Vector3DCollection normals, ref List<P3D> pnts, ref Int32Collection tris, bool swapYZ)
        {
            bool binary = false;
            normals.Clear();
            pnts.Clear();
            tris.Clear();
            // we are trying to import a binary stl file but
            // it might actually be a different format sneakily pretending to be stl
            // Take a peek
            String ft = CheckFileFormat(filename);
            if (ft == "BinSTL")
            {
                binary = true;
                ReadBinaryStl(filename, normals, pnts, tris, swapYZ);
            }
            else if (ft == "AsciiSTL")
            {
                //ReadAsciiStl(filename, normals, pnts, tris, swapYZ);
                ReadAsciiStl(filename, normals, pnts, tris, swapYZ);
            }
            return binary;
        }

        private static int AddVertex(Point3DCollection verts, Point3D pn)
        {
            int res = -1;
            for (int i = 0; i < verts.Count; i++)
            {
                if (PointUtils.equals(verts[i], pn.X, pn.Y, pn.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                verts.Add(pn);
                res = verts.Count - 1;
            }
            return res;
        }

        private static int AddVertex(OctTree tree, List<P3D> verts, Point3D pn)
        {
            int res = -1;
            /*
            for (int i = 0; i < verts.Count; i++)
            {
                if (PointUtils.equals(verts[i], pn.X, pn.Y, pn.Z))
                {
                    res = i;
                    break;
                }
            }
            */
            res = tree.PointPresent(pn);
            if (res == -1)
            {
                verts.Add(new P3D(pn));

                res = verts.Count - 1;
                tree.AddPoint(res, pn);
            }
            return res;
        }

        private static Point3D GetVFromLine(string line, bool swap)
        {
            Point3D result = new Point3D(0, 0, 0);
            line = line.Trim();
            line = line.Substring(7);
            line = line.Trim();
            string[] words = line.Split(' ');
            try
            {
                double x = Convert.ToDouble(words[0]);
                double y = Convert.ToDouble(words[1]);
                double z = Convert.ToDouble(words[2]);
                if (swap)
                {
                    result = new Point3D(x, z, y);
                }
                else
                {
                    result = new Point3D(x, y, z);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        private static void ReadAsciiStl(string filename, Vector3DCollection normals, Point3DCollection pnts, Int32Collection tris, bool swap)
        {
            //            facet normal ni nj nk
            //                 outer loop
            //                  vertex v1x v1y v1z
            //                  vertex v2x v2y v2z
            //                  vertex v3x v3y v3z
            //                 endloop
            //            endfacet
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (stream != null)
            {
                var reader = new StreamReader(stream);
                string line = "";
                while (!line.ToLower().StartsWith("endsolid") && !reader.EndOfStream)
                {
                    line = reader.ReadLine().ToLower().Trim();
                    if (line.StartsWith("facet"))
                    {
                        // skip outer loop string
                        line = reader.ReadLine().ToLower();

                        // p0
                        line = reader.ReadLine().ToLower();
                        Point3D pnt = GetVFromLine(line, swap);
                        int p0 = AddVertex(pnts, pnt);

                        // p1
                        line = reader.ReadLine().ToLower();
                        pnt = GetVFromLine(line, swap);
                        int p1 = AddVertex(pnts, pnt);

                        // p2
                        line = reader.ReadLine().ToLower();
                        pnt = GetVFromLine(line, swap);
                        int p2 = AddVertex(pnts, pnt);

                        tris.Add(p0);
                        tris.Add(p1);
                        tris.Add(p2);
                    }
                }
                stream.Close();
            }
        }

        private static void ReadAsciiStl(string filename, Vector3DCollection normals, List<P3D> pnts, Int32Collection tris, bool swap)
        {
            //            facet normal ni nj nk
            //                 outer loop
            //                  vertex v1x v1y v1z
            //                  vertex v2x v2y v2z
            //                  vertex v3x v3y v3z
            //                 endloop
            //            endfacet
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (stream != null)
            {
                Point3DCollection treePoints = new Point3DCollection();
                OctTree octTree = new OctTree(treePoints, new Point3D(-1000, -1000, -1000), new Point3D(1000, 1000, 1000), 50);

                var reader = new StreamReader(stream);
                string line = "";
                while (!line.ToLower().StartsWith("endsolid") && !reader.EndOfStream)
                {
                    line = reader.ReadLine().ToLower().Trim();
                    if (line.StartsWith("facet"))
                    {
                        // skip outer loop string
                        line = reader.ReadLine().ToLower();

                        // p0
                        line = reader.ReadLine().ToLower();
                        Point3D pnt = GetVFromLine(line, swap);
                        int p0 = AddVertex(octTree, pnts, pnt);

                        // p1
                        line = reader.ReadLine().ToLower();
                        pnt = GetVFromLine(line, swap);
                        int p1 = AddVertex(octTree, pnts, pnt);

                        // p2
                        line = reader.ReadLine().ToLower();
                        pnt = GetVFromLine(line, swap);
                        int p2 = AddVertex(octTree, pnts, pnt);

                        tris.Add(p0);
                        tris.Add(p1);
                        tris.Add(p2);
                    }
                }
                stream.Close();
            }
        }

        private static void ReadBinaryStl(string filename, Vector3DCollection normals, Point3DCollection pnts, Int32Collection tris, bool swap)
        {
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
                    if (swap)
                    {
                        normals.Add(new Vector3D(x, z, y));
                    }
                    else
                    {
                        normals.Add(new Vector3D(x, y, z));
                    }
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new Point3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new Point3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new Point3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new Point3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new Point3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new Point3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    ushort dummy = reader.ReadUInt16();
                }
                stream.Close();
            }
        }

        private static void ReadBinaryStl(string filename, Vector3DCollection normals, List<P3D> pnts, Int32Collection tris, bool swap)
        {
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
                    if (swap)
                    {
                        normals.Add(new Vector3D(x, z, y));
                    }
                    else
                    {
                        normals.Add(new Vector3D(x, y, z));
                    }
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new P3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new P3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new P3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new P3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    if (swap)
                    {
                        pnts.Add(new P3D(x, z, y));
                    }
                    else
                    {
                        pnts.Add(new P3D(x, y, z));
                    }
                    tris.Add(pnts.Count - 1);
                    ushort dummy = reader.ReadUInt16();
                }
                stream.Close();
            }
        }

        private string CheckFileFormat(string filename)
        {
            String result = "BinSTL";
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (stream != null)
            {
                try
                {
                    StreamReader sr = new StreamReader(stream);

                    string ln = sr.ReadLine();
                    if (ln.ToLower().StartsWith("solid"))
                    {
                        result = "AsciiSTL";
                    }
                }
                catch
                {
                }
                stream.Close();
            }
            return result;
        }
    }
}