using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workflow
{
    public class CuraDefinition
    {
        public CuraDefinition()
        {
            definitionSettings = new Dictionary<string, SettingDefinition>();
        }

        public Dictionary<string, SettingDefinition> definitionSettings { get; set; }
        public String inherits { get; set; }
        public String name { get; set; }
        public int version { get; set; }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("Name:" + name);
            System.Diagnostics.Debug.WriteLine("Version:" + version);
            System.Diagnostics.Debug.WriteLine("Inherits:" + inherits);

            if (definitionSettings != null)
            {
                foreach (string k in definitionSettings.Keys)
                {
                    System.Diagnostics.Debug.WriteLine("override: " + k + " " + definitionSettings[k].DefaultValue + " " + definitionSettings[k].OverideValue);
                }
            }
        }

        internal void AddSettings(Dictionary<string, SettingDefinition> allsettings)
        {
            foreach (string k in definitionSettings.Keys)
            {
                // if this settings hasn't aleady been added by a base file
                // then add it
                if (!allsettings.Keys.Contains(k))
                {
                    allsettings[k] = definitionSettings[k];
                }
                else
                {
                    // already exists so we are overriding it
                    // just up date the the value/default_value
                    if (definitionSettings[k].DefaultValue != "")
                    {
                        allsettings[k].DefaultValue = definitionSettings[k].DefaultValue;
                    }
                    if (definitionSettings[k].OverideValue != "")
                    {
                        allsettings[k].OverideValue = definitionSettings[k].OverideValue;
                    }
                }
            }
        }

        public class SettingDefinition
        {
            public SettingDefinition()
            {
                OverideValue = "";
                DefaultValue = "";
                Label = "";
                Description = "";
                Type = "";
                Settable_per_mesh = "";
                Settable_per_extruder = "";
                Settable_per_meshgroup = "";
                Options = new Dictionary<string, string>();
                Settable_Globally = "False";
                Limit_To_Extruder = "";
                Resolve = "";
                WarningValue = "";
                Section = "";
            }

            public SettingDefinition(SettingDefinition src)
            {
                OverideValue = src.OverideValue;
                DefaultValue = src.DefaultValue;
                Label = src.Label;
                Description = src.Description;
                Type = src.Type;
                Settable_per_mesh = src.Settable_per_mesh;
                Settable_per_extruder = src.Settable_per_extruder;
                Settable_per_meshgroup = src.Settable_per_meshgroup;
                Options = new Dictionary<string, string>();
                foreach (string k in src.Options.Keys)
                {
                    string v = src.Options[k];
                    Options[k] = v;
                }
                Settable_Globally = src.Settable_Globally;
                Limit_To_Extruder = src.Limit_To_Extruder;
                Resolve = src.Resolve;
                WarningValue = src.WarningValue;
                Section = src.Section;
            }

            /// <summary>
            /// This constructor interprets a single line of text as a setting.
            /// The assumption is that the line has been previously saved by the WRiteToStream function
            /// </summary>
            /// <param name="src"></param>
            public SettingDefinition(string src)
            {
                try
                {
                    string[] words = src.Split('$');
                    if (words.Length == 20)
                    {
                        Section = words[0];
                        Name = words[1];
                        Description = words[2];
                        Label = words[3];
                        Type = words[4];
                        Unit = words[5];
                        Enabled = words[6];
                        DefaultValue = words[7];
                        OverideValue = words[8];
                        Limit_To_Extruder = words[9];
                        Minimum_Value = words[10];
                        Minimum_Value_Warning = words[11];
                        Maximum_Value = words[12];
                        Maximum_Value_Warning = words[13];
                        Settable_per_mesh = words[14];
                        Settable_per_extruder = words[15];
                        Settable_per_meshgroup = words[16];
                        Settable_Globally = words[17];
                        Resolve = words[18];
                        WarningValue = words[19];
                    }
                }
                catch (Exception ex)
                {
                    LoggerLib.Logger.LogException(ex);
                }
            }

            public string DefaultValue { get; set; }
            public string Description { get; set; }
            public string Enabled { get; set; }
            public string Label { get; set; }
            public string Limit_To_Extruder { get; set; }
            public string Maximum_Value { get; set; }
            public string Maximum_Value_Warning { get; set; }
            public string Minimum_Value { get; set; }
            public string Minimum_Value_Warning { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Options { get; set; }
            public string OverideValue { get; set; }
            public string Resolve { get; set; }

            // the section the setting was read from.
            // used by gui to group together related settings
            public string Section { get; set; }

            public string Settable_Globally { get; set; }

            public string Settable_per_extruder { get; set; }

            public string Settable_per_mesh { get; set; }

            public string Settable_per_meshgroup { get; set; }

            public string Type { get; set; }

            public string Unit { get; set; }

            public string WarningValue { get; set; }

            public void WriteToStream(StreamWriter sw)
            {
                string s = $"{Section}${Name}${Description}${Label}${Type}${Unit}${Enabled}${DefaultValue}${OverideValue}";
                s += $"${Limit_To_Extruder}";
                s += $"${Minimum_Value}${Minimum_Value_Warning}";
                s += $"${Maximum_Value}${Maximum_Value_Warning}";
                s += $"${Settable_per_mesh}";
                s += $"${Settable_per_extruder}";
                s += $"${Settable_per_meshgroup}";
                s += $"${Settable_Globally}";
                s += $"${Resolve}";
                s += $"${WarningValue}";
                sw.WriteLine(s);
            }
        }
    }
}