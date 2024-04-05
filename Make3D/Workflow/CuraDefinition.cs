using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workflow
{
    public class CuraDefinition
    {
        public String name { get; set; }
        public int version { get; set; }
        public String inherits { get; set; }
        public Dictionary<string, SettingDefinition> definitionSettings { get; set; }

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

        public CuraDefinition()
        {
            definitionSettings = new Dictionary<string, SettingDefinition>();
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
            public string Name { get; set; }
            public string OverideValue { get; set; }
            public string DefaultValue { get; set; }
            public string Label { get; set; }
            public string Description { get; set; }
            public string Enabled { get; set; }
            public string Type { get; set; }
            public string Unit { get; set; }
            public string Limit_To_Extruder { get; set; }
            public string Minimum_Value { get; set; }
            public string Minimum_Value_Warning { get; set; }
            public string Maximum_Value { get; set; }
            public string Maximum_Value_Warning { get; set; }
            public string Settable_per_mesh { get; set; }
            public string Settable_per_extruder { get; set; }
            public string Settable_per_meshgroup { get; set; }
            public string Settable_Globally { get; set; }
            public string Resolve { get; set; }
            public string WarningValue { get; set; }
            public Dictionary<string, string> Options { get; set; }

            // the section the setting was read from.
            // used by gui to group together related settings
            public string Section { get; set; }

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
        }
    }
}