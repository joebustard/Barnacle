using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Object3DLib.OFF
{
    public class OFFFormat
    {
        public static bool ReadOffFile(String fileName,
                                Point3DCollection vertices,
                                Int32Collection faces,
                                bool swapYZ)
        {
            bool res = false;
            vertices.Clear();
            faces.Clear();
            try
            {
                string[] lines = File.ReadAllLines(fileName);
                if (lines[0] == "OFF")
                {
                    int lineIndex = 1;

                    while (lines[lineIndex].Trim().StartsWith("#"))
                    {
                        lineIndex++;
                    }
                    string[] words = lines[lineIndex].Trim().Split(' ');
                    if (words.GetLength(0) == 3)
                    {
                        int numV = Convert.ToInt32(words[0]);
                        int numF = Convert.ToInt32(words[1]);
                        lineIndex++;
                        while (String.IsNullOrEmpty(lines[lineIndex].Trim()))
                        {
                            lineIndex++;
                        }
                        for (int i = 0; i < numV; i++)
                        {
                            words = lines[lineIndex].Trim().Split(' ');
                            if (words.GetLength(0) == 3)
                            {
                                double x = Convert.ToDouble(words[0]);
                                double y = Convert.ToDouble(words[1]);
                                double z = Convert.ToDouble(words[2]);

                                Point3D p;
                                if (swapYZ)
                                {
                                    p = new Point3D(x, z, y);
                                }
                                else
                                {
                                    p = new Point3D(x, y, z);
                                }

                                vertices.Add(p);

                                lineIndex++;
                            }
                        }

                        for (int i = 0; i < numF; i++)
                        {
                            lines[lineIndex] = lines[lineIndex].Replace("  ", " ");
                            words = lines[lineIndex].Trim().Split(' ');
                            if (words.GetLength(0) == 4)
                            {
                                int numE = Convert.ToInt32(words[0]);
                                if (numE == 3)
                                {
                                    int e0 = Convert.ToInt32(words[1]);
                                    int e1 = Convert.ToInt32(words[2]);
                                    int e2 = Convert.ToInt32(words[3]);
                                    faces.Add(e0);
                                    faces.Add(e1);
                                    faces.Add(e2);
                                }
                            }
                            lineIndex++;
                        }
                    }
                }

                res = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return res;
        }

        public static bool WriteOffFile(String fileName,
                                                Point3DCollection vertices,
                                        Int32Collection faces)
        {
            bool res = false;
            try
            {
                File.WriteAllText(fileName, "OFF\r\n");
                String l = $"{vertices.Count} {faces.Count / 3} 0\r\n";
                File.AppendAllText(fileName, l);
                foreach (Point3D p in vertices)
                {
                    l = $"{p.X} {p.Y} {p.Z}\r\n";
                    File.AppendAllText(fileName, l);
                }
                for (int i = 0; i < faces.Count; i += 3)
                {
                    l = $"3 {faces[i]} {faces[i + 1]} {faces[i + 2]}\r\n";
                    File.AppendAllText(fileName, l);
                }
                res = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return res;
        }
    }
}