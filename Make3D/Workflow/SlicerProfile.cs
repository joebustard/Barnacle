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

        public SlicerProfile(string fileName)
        {
            LoadOverrides(fileName);
        }

        public SlicerProfile()
        {
        }

        public void SaveAsXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Profile");

            foreach (SettingOverride so in Overrides)
            {
                XmlElement ovr = doc.CreateElement("Ovr");
                ovr.SetAttribute("s", so.Section);
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
            string section = "";
            string key = "";
            string description = "";
            string value = "";

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
                        if (words.GetLength(0) == 4)
                        {                            
                            section = words[0];
                            key = words[1];
                            description = words[2];
                            value = words[3].Replace("\"", "");
                            Overrides.Add(new SettingOverride(section,key, description,value));
                        }
                    }
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
                    if (so.Key != "machine_start_gcode" && so.Key != "machine_end_gcode")
                    {
                        sw.WriteLine($"{so.Section}${so.Key}${so.Description}$\"{so.Value}\"");
                    }
                }
                sw.Close();
            }
            catch
            {
            }
        }
    }
}