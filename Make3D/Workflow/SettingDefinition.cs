using System.Collections.Generic;

namespace Workflow
{
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
            ModifiedByUser = false;
            UserValue = "";
            Calculated = false;
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
            ModifiedByUser = src.ModifiedByUser;
            Name = src.Name;
            UserValue = src.UserValue;
            Calculated = src.Calculated;
        }

        public bool Calculated { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public string Enabled { get; set; }
        public string Label { get; set; }
        public string Limit_To_Extruder { get; set; }
        public string Maximum_Value { get; set; }
        public string Maximum_Value_Warning { get; set; }
        public string Minimum_Value { get; set; }
        public string Minimum_Value_Warning { get; set; }
        public bool ModifiedByUser { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string OverideValue { get; set; }
        public string Resolve { get; set; }

        // the section the setting was read from. used by gui to group together related settings
        public string Section { get; set; }

        public string Settable_Globally { get; set; }
        public string Settable_per_extruder { get; set; }
        public string Settable_per_mesh { get; set; }
        public string Settable_per_meshgroup { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string UserValue { get; set; }
        public string WarningValue { get; set; }
    }
}