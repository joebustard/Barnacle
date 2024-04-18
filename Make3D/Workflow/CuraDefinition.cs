using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Workflow
{
    public class CuraDefinition
    {
        public CuraDefinition()
        {
            if (definitionSettings == null)
            {
                definitionSettings = new Dictionary<string, SettingDefinition>();
            }
        }

        public static Dictionary<string, SettingDefinition> definitionSettings { get; set; }
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
                // if this settings hasn't aleady been added by a base file then add it
                if (!allsettings.Keys.Contains(k))
                {
                    allsettings[k] = definitionSettings[k];
                }
                else
                {
                    // already exists so we are overriding it just up date the the value/default_value
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
    }
}