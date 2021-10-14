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
            doc.XmlResolver = null;
            XmlNode af = doc.CreateElement("Airfoils");
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
                        name = name.Replace("&", " and ");
                        string tmp = name;
                        do
                        {
                            tmp = name;
                            name = name.Replace("  ", " ");
                        } while (tmp != name);

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
                                tmp = s;
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
                            if (!af.HasChildNodes)
                            {
                                af.AppendChild(el);
                            }
                            else
                            {
                                XmlElement frst = af.FirstChild as XmlElement;
                                if (frst.GetAttribute("Name").CompareTo(name) > 0)
                                {
                                    af.InsertBefore(el, af.FirstChild);
                                }
                                else
                                {
                                    XmlElement lst = af.LastChild as XmlElement;
                                    if (lst.GetAttribute("Name").CompareTo(name) <= 0)
                                    {
                                        af.AppendChild(el);
                                    }
                                    else
                                    {
                                        bool done = false;
                                        while (!done)
                                        {
                                            XmlElement nxt = frst.NextSibling as XmlElement;
                                            if (nxt == null)
                                            {
                                                done = true;
                                            }
                                            else
                                            {
                                                if ((frst.GetAttribute("Name").CompareTo(name) < 0) && (nxt.GetAttribute("Name").CompareTo(name) > 0))
                                                {
                                                    af.InsertAfter(el, frst);
                                                    done = true;
                                                }
                                                else
                                                {
                                                    frst = nxt;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // if data looks bad just skip the profile
                }
            }
            doc.Save(pth + "\\Airfoils.xml");
        }
    }
}