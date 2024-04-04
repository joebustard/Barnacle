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
            string fileName = AppDomain.CurrentDomain.BaseDirectory + @"Data\DefaultPrinter.profile";
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
                ovr.SetAttribute("d", so.Description);
                docNode.AppendChild(ovr);
            }
            doc.AppendChild(docNode);
            doc.Save(fileName);
        }

        public void LoadOverrides(string fileName)
        {
            // We use a dictionary to read in the profile.
            // This means if there are duplicate values, its only the last one thats taken.
            Dictionary<string, string> valueTmp = new Dictionary<string, string>();
            Dictionary<string, string> descriptionTmp = new Dictionary<string, string>();

            Overrides = new List<SettingOverride>();
            if (File.Exists(fileName))
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                for (int i = 0; i < lines.GetLength(0); i++)
                {
                    lines[i] = lines[i].Trim();
                    if (lines[i] != "")
                    {
                        string[] words = lines[i].Split('$');
                        if (words.GetLength(0) == 3)
                        {
                            words[2] = words[1].Replace("\"", "");
                        }
                        descriptionTmp[words[0]] = words[1];
                        valueTmp[words[0]] = words[2];
                    }
                }

                // convert the dictionary back to a list.
                string[] keys = valueTmp.Keys.ToArray();
                for (int i = 0; i < valueTmp.Count; i++)
                {
                    Overrides.Add(new SettingOverride(keys[i], valueTmp[keys[i]], descriptionTmp[keys[i]]);
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
                    sw.WriteLine($"{so.Key}${so.Description}$\"{so.Value}\"$");
                }
                sw.Close();
            }
            catch
            {
            }
        }
    }
}