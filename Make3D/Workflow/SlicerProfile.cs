using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Workflow
{
    public class SlicerProfile
    {
        public List<SettingOverride> Overrides { get; set; }

        public SlicerProfile()
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + @"Data\Ender3ProRaft.profile";
            LoadOverrides(fileName);
        }

        public void SaveAsXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Profile");

            foreach (SettingOverride so in Overrides)
            {
                XmlElement ovr = doc.CreateElement("Ovr");
                ovr.SetAttribute("n", so.Key);
                ovr.SetAttribute("v", so.Value);
                docNode.AppendChild(ovr);
            }

            doc.Save(fileName);
        }

        public void LoadOverrides(string fileName)
        {
            // We use a dictionary to read in the profile.
            // This means if there are duplicate values, its only the last one thats taken.
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            Overrides = new List<SettingOverride>();
            if (File.Exists(fileName))
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                for (int i = 0; i < lines.GetLength(0); i++)
                {
                    lines[i] = lines[i].Trim();
                    if (lines[i] != "")
                    {
                        string[] words = lines[i].Split('=');
                        if (words.GetLength(0) == 2)
                        {
                            words[1] = words[1].Replace("\"", "");
                        }
                        tmp[words[0]] = words[1];
                    }
                }

                // convert the dictionary back to a list.
                string[] keys = tmp.Keys.ToArray();
                for (int i = 0; i < tmp.Count; i++)
                {
                    Overrides.Add(new SettingOverride(keys[i], tmp[keys[i]]));
                }
            }
        }

        public void SaveOverrides(String fName)
        {
            try
            {
                if (File.Exists(fName))
                {
                    File.Delete(fName);
                }
                StreamWriter sw = File.AppendText(fName);
                foreach (SettingOverride so in Overrides)
                {
                    sw.WriteLine($"{so.Key}=\"{so.Value}\"");
                }
                sw.Close();
            }
            catch
            {
            }
        }
    }
}