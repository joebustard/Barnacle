using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Workflow.CuraDefinition;

namespace Workflow
{
    public class CuraDefinitionFile
    {
        private String FilePath { get; set; }

        public CuraDefinition definition;

        public CuraDefinitionFile BaseFile { get; set; }
        private List<SettingOverride> overrides;

        public List<SettingOverride> Overrides
        {
            get
            {
                return overrides;
            }
        }

        public bool Load(String fileName)
        {
            bool res = false;
            FilePath = fileName;
            BaseFile = null;
            definition = new CuraDefinition();
            if (File.Exists(fileName))
            {
                try
                {
                    using (StreamReader file = File.OpenText(fileName))
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);

                        if (o2["inherits"] != null)
                        {
                            definition.inherits = (string)o2["inherits"];
                        }
                        if (o2["name"] != null)
                        {
                            definition.name = (string)o2["name"];
                        }
                        if (o2["settings"] != null)
                        {
                            JObject settings = (JObject)o2["settings"];
                            System.Diagnostics.Debug.WriteLine("===== Settings ======");

                            foreach (JProperty property in settings.Properties())
                            {
                                System.Diagnostics.Debug.WriteLine($"=== {property.Name.ToLower()} ===");
                                ReadSettings(settings, property);
                            }
                        }
                        if (o2["overrides"] != null)
                        {
                            JObject overrides = (JObject)o2["overrides"];
                            ReadChildren(overrides);
                        }

                        if (definition.inherits != null && definition.inherits != "")
                        {
                            String pth = Path.GetDirectoryName(fileName);
                            String baseName = Path.Combine(pth, definition.inherits + ".def.json");
                            BaseFile = new CuraDefinitionFile();
                            BaseFile.Load(baseName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return res;
        }

        public void ProcessSettings()
        {
            Dictionary<string, SettingDefinition> allsettings = new Dictionary<string, SettingDefinition>();
            AddSettings(allsettings);

            overrides = new List<SettingOverride>();
            foreach (string s in allsettings.Keys)
            {
                SettingDefinition sd = allsettings[s];
                string v = sd.DefaultValue;
                if (sd.OverideValue != "")
                {
                    v = sd.OverideValue;
                }
                SettingOverride so = new SettingOverride(s, v);
                overrides.Add(so);
            }
            Reconcile(overrides);
        }

        /// <summary>
        ///  some settings directlty reference others e.g. something like
        ///  "speed_layer_0" :"speed_layer"
        ///  or similar. Make a pass over the settings.
        ///  If the value of an entry is actually the key of another, replace it with the
        ///  reconciled value
        /// </summary>
        /// <param name="allsettings"></param>
        private void Reconcile(List<SettingOverride> overrides)
        {
            bool found = false;
            do
            {
                found = false;
                for (int i = 0; i < overrides.Count; i++)
                {
                    for (int j = 0; j < overrides.Count; j++)
                    {
                        if (i != j)
                        {
                            if (overrides[j].Value == overrides[i].Key)
                            {
                                overrides[j].Value = overrides[i].Value;
                                found = true;
                            }
                        }
                    }
                }
            } while (found == true);

            InterpretValue(overrides);
        }

        private SettingOverride FindOveride(List<SettingOverride> overrides, string key)
        {
            SettingOverride res = null;
            foreach (SettingOverride or in overrides)
            {
                if (or.Key == key)
                {
                    res = or;
                    break;
                }
            }
            return res;
        }

        private void InterpretValue(List<SettingOverride> overrides)
        {
            foreach (SettingOverride or in overrides)
            {
                if (IsCalculated(or.Value))
                {
                    System.Diagnostics.Debug.WriteLine($"Calculated slice setting: {or.Key} : {or.Value}");
                }
                String v = GetStdValue(or.Value);
                switch (v)
                {
                    default:
                        break;
                }
            }
        }

        private string[] calcTags =
            {
            "if ",
            "(",
            ")",
            ">",
            "<",
            "=",
            "!=",
                "==",
                "else ",
                "*",
                "/",
                "+"
        };

        private bool IsCalculated(string value)
        {
            bool res = false;
            foreach (string s in calcTags)
            {
                if (value.IndexOf(s) != -1)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }

        private string GetStdValue(string value)
        {
            string res = value.ToLower();
            res = res.Replace(" ", "");
            return res;
        }

        public bool SaveSettings(String fileName)
        {
            bool res = false;
            Dictionary<string, SettingDefinition> allsettings = new Dictionary<string, SettingDefinition>();
            AddSettings(allsettings);
            string fileContent = settingsContent;

            File.WriteAllText(fileName, fileContent.ToString());
            return res;
        }

        private string singleSettingContent =
 @"
 ";

        private string settingsContent =
 @"
 {
    ""name"": ""generic settings"",
    ""version"": 2,
    ""metadata"":
    {
        ""type"": ""machine"",
        ""author"": ""Unknown"",
        ""manufacturer"": ""Unknown"",
        ""setting_version"": 22,
        ""file_formats"": ""text/x-gcode;model/stl;application/x-wavefront-obj;application/x3g"",
        ""visible"": false,
        ""has_materials"": true,
        ""has_variants"": false,
        ""has_machine_quality"": false,
        ""preferred_material"": ""generic_pla"",
        ""preferred_quality_type"": ""normal"",
        ""machine_extruder_trains"": { ""0"": ""fdmextruder"" },
        ""supports_usb_connection"": true,
        ""supports_network_connection"": false
    },
    ""settings"":
    {
    <ALLSETTINGS>
    }
}
    ";

        private void AddSettings(Dictionary<string, SettingDefinition> allsettings)
        {
            if (BaseFile != null)
            {
                BaseFile.AddSettings(allsettings);
            }
            definition.AddSettings(allsettings);
        }

        private void ReadSettings(JObject settings, JProperty property)
        {
            JObject machine_settings = (JObject)settings[property.Name.ToLower()];
            JObject children = (JObject)machine_settings["children"];
            ReadChildren(children);
        }

        private void ReadChildren(JObject children)
        {
            foreach (JProperty prop in children.Properties())
            {
                JObject oneSetting = (JObject)children[prop.Name];

                SettingDefinition cdf = new SettingDefinition();
                cdf.Name = prop.Name;
                definition.definitionSettings[cdf.Name] = cdf;
                foreach (JProperty setProp in oneSetting.Properties())
                {
                    try
                    {
                        switch (setProp.Name.ToLower())
                        {
                            case "label":
                                {
                                    cdf.Label = (string)setProp.Value;
                                }
                                break;

                            case "description":
                                {
                                    cdf.Description = (string)setProp.Value;
                                }
                                break;

                            case "resolve":
                                {
                                    cdf.Resolve = (string)setProp.Value;
                                }
                                break;

                            case "warning_value":
                                {
                                    cdf.WarningValue = (string)setProp.Value;
                                }
                                break;

                            case "default_value":
                                {
                                    if (cdf.Type == "polygon")
                                    {
                                        ReadPolygon(cdf, setProp);
                                    }
                                    else
                                        if (cdf.Type == "polygons")
                                    {
                                        ReadPolygon(cdf, setProp);
                                    }
                                    else
                                    {
                                        cdf.DefaultValue = (string)setProp.Value;
                                    }
                                }
                                break;

                            case "minimum_value":
                                {
                                    cdf.Minimum_Value = (string)setProp.Value;
                                }
                                break;

                            case "minimum_value_warning":
                                {
                                    cdf.Minimum_Value_Warning = (string)setProp.Value;
                                }
                                break;

                            case "maximum_value":
                                {
                                    cdf.Maximum_Value = (string)setProp.Value;
                                }
                                break;

                            case "maximum_value_warning":
                                {
                                    cdf.Maximum_Value_Warning = (string)setProp.Value;
                                }
                                break;

                            case "enabled":
                                {
                                    cdf.Enabled = (string)setProp.Value;
                                }
                                break;

                            case "value":
                                {
                                    cdf.OverideValue = (string)setProp.Value;
                                }
                                break;

                            case "type":
                                {
                                    cdf.Type = (string)setProp.Value;
                                }
                                break;

                            case "settable_per_mesh":
                                {
                                    cdf.Settable_per_mesh = (string)setProp.Value;
                                }
                                break;

                            case "settable_per_extruder":
                                {
                                    cdf.Settable_per_extruder = (string)setProp.Value;
                                }
                                break;

                            case "settable_per_meshgroup":
                                {
                                    cdf.Settable_per_meshgroup = (string)setProp.Value;
                                }
                                break;

                            case "settable_globally":
                                {
                                    cdf.Settable_Globally = (string)setProp.Value;
                                }
                                break;

                            case "unit":
                                {
                                    cdf.Unit = (string)setProp.Value;
                                }
                                break;

                            case "limit_to_extruder":
                                {
                                    cdf.Limit_To_Extruder = (string)setProp.Value;
                                }
                                break;

                            case "options":
                                {
                                    JObject opts = (JObject)oneSetting["options"];
                                    foreach (JProperty oneOpt in opts.Properties())
                                    {
                                        cdf.Options[oneOpt.Name] = (string)oneOpt.Value;
                                    }
                                }
                                break;

                            case "children":
                                {
                                    JObject subChildren = (JObject)oneSetting["children"];
                                    ReadChildren(subChildren);
                                }
                                break;

                            default:
                                {
                                    System.Diagnostics.Debug.WriteLine($" unprocessed property:{setProp.Name}");
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Property {prop.Name} :" + ex.Message);
                    }
                }
            }
        }

        private static void ReadPolygon(SettingDefinition cdf, JProperty setProp)
        {
            JArray jar = (JArray)setProp.Value;
            if (jar.Count == 0)
            {
                cdf.DefaultValue = "[]";
            }
            else
            {
                cdf.DefaultValue = "[ ";
                foreach (JToken tk in jar.Children())
                {
                    cdf.DefaultValue += " [ ";
                    JArray j2 = (JArray)tk;
                    foreach (JToken tk2 in j2.Children())
                    {
                        String s = tk2.ToString();
                        cdf.DefaultValue += $"{s},";
                    }
                    cdf.DefaultValue = cdf.DefaultValue.Substring(0, cdf.DefaultValue.Length - 1);
                    cdf.DefaultValue += " ],";
                }
                cdf.DefaultValue = cdf.DefaultValue.Substring(0, cdf.DefaultValue.Length - 1);
                cdf.DefaultValue += "]";
            }
        }

        public void Dump()
        {
            if (BaseFile != null)
            {
                BaseFile.Dump();
            }
            System.Diagnostics.Debug.WriteLine("File:" + FilePath);
            definition.Dump();
        }
    }
}