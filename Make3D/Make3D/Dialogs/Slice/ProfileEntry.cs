using System;
using System.IO;

namespace Barnacle.Dialogs.Slice
{
    public class ProfileEntry
    {
        public String SettingName { get; set; }
        public String SettingValue { get; set; }
        public String SettingDescription { get; set; }
        public String SettingSection { get; set; }

        public ProfileEntry(string n, string v, string d, string s)
        {
            SettingName = n;
            SettingValue = v;
            SettingDescription = d;
            SettingSection = s;
        }

        public ProfileEntry()
        {
            SettingName = "";
            SettingDescription = "";
            SettingValue = "";
            SettingSection = "";
        }

        public void Write(StreamWriter sw)
        {
            if (sw != null)
            {
                try
                {
                    sw.WriteLine($"{SettingName}$SettingDescription${SettingSection}$\"{SettingValue}\"");
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Load(string line)
        {
            line = line.Trim();
            if (line != "")
            {
                string[] words = line.Split('$');
                if (words.GetLength(0) == 4)
                {
                    words[3] = words[3].Replace("\"", "");
                }
                SettingName = words[0];
                SettingDescription = words[1];
                SettingSection = words[2];
                SettingValue = words[3];
            }
        }
    }
}