using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow
{

    public class SlicerProfile
    {
        public string Name { get; set; }
        public string Printer { get; set; }
        public string Extruder { get; set; }
        public String StartGCode { get; set; }
        public String EndGCode { get; set; }
        public List<SettingOverride> Overrides{ get; set;}
        public SlicerProfile()
        {
            Name = "Default";
            Printer=@".\resources\definitions\creality_ender3pro.def.json";
            Extruder = @".\resources\definitions\fdmextruder.def.json";
            StartGCode = @"M117 $NAME \n; Ender 3 Custom Start G-code\nG92 E0 ; Reset Extruder\nG28 ; Home all axes\nG29 ; BLTouch\nG1 Z2.0 F3000 ; Move Z Axis up little to prevent scratching of Heat Bed\nG1 X0.1 Y20 Z0.3 F5000.0 ; Move to start position\nG1 X0.1 Y200.0 Z0.3 F1500.0 E15 ; Draw the first line\nG1 X0.4 Y200.0 Z0.3 F5000.0 ; Move to side a little\nG1 X0.4 Y20 Z0.3 F1500.0 E30 ; Draw the second line\nG92 E0 ; Reset Extruder\nG1 Z2.0 F3000 ; Move Z Axis up little to prevent scratching of Heat Bed\nG1 X5 Y20 Z0.3 F5000.0 ; Move over to prevent blob squish\n";
            EndGCode = @"G91 ;Relative positioning\nG1 E-2 F2700 ;Retract a bit\nG1 E-2 Z0.2 F2400 ;Retract and raise Z\nG1 X5 Y5 F3000 ;Wipe out\nG1 Z10 ;Raise Z more\nG90 ;Absolute positioning\n";
            Overrides = new List<SettingOverride>();

            // We use a dictionary to read in the profile.
            // This means if there are duplicate values, its only the last one thats taken.
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            string[] lines = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"Data\Ender3ProRaft.profile");
            for ( int i = 0; i < lines.GetLength(0);  i ++ )
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

        public void Save(String fName)
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
