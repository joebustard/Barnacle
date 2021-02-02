using System;
using System.IO;
using System.Xml;

namespace ConvertAerofoilsToXML
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            String pth = @"C:\tmp\coord_seligFmt";
            ProcessDats(pth);
        }

        private static void ProcessDats(string pth)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode af = doc.CreateElement("Aerofoils");
            doc.AppendChild(af);

            string[] files = Directory.GetFiles(pth, "*.dat");
            foreach (string f in files)
            {
                try
                {
                    String[] lines = File.ReadAllLines(f);
                    if (lines[0] != "" && lines.GetLength(0) > 5)
                    {
                        XmlElement el = doc.CreateElement("af");
                        string name = lines[0];
                        name = name.ToUpper();
                        name = name.Replace("AIRFOIL", "");
                        name = name.Trim();
                        el.SetAttribute("Name", name);
                        string co = "";
                        int cocount = 0;
                        for (int i = 1; i < lines.GetLength(0); i++)
                        {
                            if (lines[i] != "")
                            {
                                String s = lines[i].Trim();
                                s = s.Replace("\t", " ");
                                string tmp = s;
                                do
                                {
                                    tmp = s;
                                    s = s.Replace("  ", " ");
                                } while (tmp != s);

                                char c = s[0];
                                if (char.IsDigit(c) || c == '+' || c == '-')
                                {
                                    string[] words = s.Split(' ');
                                    words[0] = words[0].Replace(",", "");
                                    words[1] = words[1].Replace(",", "");
                                    double x = Convert.ToDouble(words[0]);
                                    double y = Convert.ToDouble(words[1]);
                                    co += x.ToString() + "," + y.ToString();

                                    co += ", ";
                                    cocount++;
                                }
                            }
                        }
                        if (co.Length > 0 && cocount > 6)
                        {
                            co = co.Substring(0, co.Length - 2);
                            el.InnerText = co;
                            af.AppendChild(el);
                        }
                    }
                }
                catch
                {
                    // if data looks bad just skip the profile
                }
            }
            doc.Save(pth + "\\Aerfoils.xml");
        }
    }
}