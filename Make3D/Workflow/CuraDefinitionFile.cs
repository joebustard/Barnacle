using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LoggerLib;
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
                            ReadChildren(overrides, "");
                        }

                        if (definition.inherits != null && definition.inherits != "")
                        {
                            String pth = Path.GetDirectoryName(fileName);
                            String baseName = Path.Combine(pth, definition.inherits + ".def.json");
                            BaseFile = new CuraDefinitionFile();
                            BaseFile.Load(baseName);
                        }
                        res = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    Logger.LogException(ex);
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
                SettingOverride so = new SettingOverride(s, v, sd.Description);
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

        // tmp code. remove after interpretvalue is complete
        private String entry =
@"

            case ""<STDVALUE>"":
                    {
                            // <KEY>
                            // <ORIGINALVALUE>
                            SettingOverride so = FindOveride(overrides, ""line_width"");
                            if (so != null)
                            {
                                double val = Convert.ToDouble(so.Value);
                                val = val* 2;
                                or.Value = val.ToString();
                            }
                    }
                    break;

";

        private void InterpretValue(List<SettingOverride> overrides)
        {
            int count = 0;
            int runcount = 5;
            bool rerun;
            do
            {
                rerun = false;
                count = 0;
                foreach (SettingOverride or in overrides)
                {
                    String v = GetStdValue(or.Value);
                    try
                    {
                        switch (v)
                        {
                            case "extrudervalue(support_bottom_extruder_nr,'support_interface_line_width')":
                                {
                                    // extruderValue(support_bottom_extruder_nr, 'support_interface_line_width')
                                    SettingOverride so = FindOveride(overrides, "support_interface_line_width");
                                    if (so != null)
                                    {
                                        or.Value = so.Value;
                                    }
                                }
                                break;


                            case "extrudervalue(support_roof_extruder_nr,'support_interface_line_width')":
                                {
                                    // extruderValue(support_roof_extruder_nr, 'support_interface_line_width')
                                    SettingOverride so = FindOveride(overrides, "support_interface_line_width");
                                    if (so != null)
                                    {

                                        or.Value = so.Value;
                                    }
                                }
                                break;

                            case "reprap(marlin/sprinter)":
                                {
                                    // do nothing this is fine
                                }
                                break;

                            case "machine_gcode_flavor!=\"ultigcode\"":
                                {
                                    String val = "True";
                                    SettingOverride so = FindOveride(overrides, "machine_gcode_flavor");
                                    if (so != null)
                                    {
                                        if (so.Value.ToLower() == "ultigcode")
                                        {
                                            val = "False";
                                        }
                                    }
                                    or.Value = val;
                                }
                                break;

                            case "machine_gcode_flavor=='reprap(volumetric)'ormachine_gcode_flavor=='ultigcode'ormachine_gcode_flavor=='bfb'":
                                {
                                    String val = "False";
                                    SettingOverride so = FindOveride(overrides, "machine_gcode_flavor");
                                    if (so != null)
                                    {
                                        if (so.Value.ToLower() == "ultigcode" || so.Value.ToLower() == "reprap (volumetric)" || so.Value.ToLower() == "bfb")
                                        {
                                            val = "True";
                                        }
                                    }
                                    or.Value = val;
                                }
                                break;

                            case "line_width*2":
                                {
                                    Multiply(or, "line_width", 2);
                                }
                                break;

                            case "speed_print/2":
                                {
                                    Multiply(or, "speed_print", 0.5);
                                }
                                break;

                            case "machine_nozzle_size*.85":
                                {
                                    Multiply(or, "machine_nozzle_size", 0.85);
                                }
                                break;
                            case "small_hole_max_size*math.pi":
                                {
                                    // small_hole_max_size * math.pi
                                    Multiply(or, "small_hole_max_size", Math.PI);
                                }
                                break;

                            case "max(cool_min_speed,speed_topbottom/2)":
                                {
                                    // max(cool_min_speed, speed_topbottom / 2)
                                    SettingOverride so = FindOveride(overrides, "cool_min_speed");
                                    if (so != null)
                                    {
                                        SettingOverride so2 = FindOveride(overrides, "speed_topbottom");
                                        double val1 = Convert.ToDouble(so.Value);
                                        double val2 = Convert.ToDouble(so2.Value) / 2.0;
                                        double val = Math.Max(val1, val2);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "max(cool_min_speed,speed_wall_0/2)":
                                {
                                    // max(cool_min_speed, speed_wall_0 / 2)
                                    SettingOverride so = FindOveride(overrides, "cool_min_speed");
                                    if (so != null)
                                    {
                                        SettingOverride so2 = FindOveride(overrides, "speed_wall_0");
                                        double val1 = Convert.ToDouble(so.Value);
                                        double val2 = Convert.ToDouble(so2.Value) / 2.0;
                                        double val = Math.Max(val1, val2);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "line_width+support_xy_distance+1.0":
                                {
                                    // line_width + support_xy_distance + 1.0
                                    SettingOverride so = FindOveride(overrides, "line_width");
                                    if (so != null)
                                    {
                                        SettingOverride so2 = FindOveride(overrides, "support_xy_distance");
                                        double val1 = Convert.ToDouble(so.Value);
                                        double val2 = Convert.ToDouble(so2.Value);
                                        double val = val1 + val2 + 1;
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "layer_height*4":
                                {
                                    // layer_height * 4
                                    Multiply(or, "layer_height", 4);
                                }
                                break;

                            case "resolveorvalue('layer_height')":
                                {
                                    // resolveOrValue('layer_height')
                                    SettingOverride so = FindOveride(overrides, "layer_height");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "0ifsupport_bottom_enableelse0.3":
                                {
                                    // 0 if support_bottom_enable else 0.3
                                    or.Value = AltIfFalse("support_bottom_enable", 0, 0.3);
                                }
                                break;

                            case "1ifinfill_meshelse0":
                                {
                                    // 1 if infill_mesh else 0
                                    or.Value = AltIfFalse("infill_mesh", 1, 0);
                                }
                                break;

                            case "extruders_enabled_count>1":
                                {
                                    // extruders_enabled_count > 1
                                    SettingOverride so = FindOveride(overrides, "extruders_enabled_count");
                                    if (so != null)
                                    {
                                        or.Value = "false";
                                        double val = Convert.ToDouble(so.Value);
                                        if (val > 1.0)
                                        {
                                            or.Value = "true";
                                        }
                                    }
                                }
                                break;

                            case "raft_base_line_width*2":
                                {
                                    // raft_base_line_width * 2
                                    Multiply(or, "raft_base_line_width", 2);

                                }
                                break;

                            case "resolveorvalue('adhesion_type')in['raft','brim']":
                                {
                                    // resolveOrValue('adhesion_type') in ['raft', 'brim']
                                    SettingOverride so = FindOveride(overrides, "adhesion_type");
                                    if (so != null)
                                    {
                                        or.Value = "false";
                                        if (so.Value.ToLower() == "raft" || so.Value.ToLower() == "brim")
                                        {
                                            or.Value = "true";
                                        }
                                    }
                                }
                                break;

                            case "raft_airgap/2":
                                {
                                    Multiply(or, "raft_airgap", 0.5);
                                }
                                break;

                            case "2ifsupport_structure=='normal'else0":
                                {
                                    // 2 if support_structure == 'normal' else 0
                                    double val = 0;
                                    SettingOverride so = FindOveride(overrides, "support_structure");
                                    if (so != null)
                                    {
                                        if (so.Value.ToLower() == "normal")
                                        {
                                            val = 2;
                                        }
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "2*wall_line_width_0":
                                {
                                    // 2 * wall_line_width_0
                                    Multiply(or, "wall_line_width_0", 2.0);
                                }
                                break;

                            case "magic_mesh_surface_mode!='surface'":
                                {
                                    // magic_mesh_surface_mode != 'surface'
                                    or.Value = "true";
                                    SettingOverride so = FindOveride(overrides, "magic_mesh_surface_mode");
                                    if (so != null)
                                    {
                                        if (so.Value.ToLower() == "surface")
                                        {
                                            or.Value = "false";
                                        }
                                    }
                                }
                                break;

                            case "machine_gcode_flavor==\"reprap(reprap)\"":
                                {
                                    // machine_gcode_flavor=="RepRap (RepRap)"
                                    or.Value = "false";
                                    SettingOverride so = FindOveride(overrides, "machine_gcode_flavor");
                                    if (so != null)
                                    {
                                        string tmp = GetStdValue(so.Value);
                                        if (tmp == "reprap(reprap)" || tmp == "\"reprap(reprap)\"")
                                        {
                                            or.Value = "true";
                                        }
                                    }
                                }
                                break;

                            case "speed_print/60*30":
                                {
                                    // speed_print / 60 * 30
                                    Multiply(or, "speed_print", 0.5);
                                }
                                break;

                            case "machine_nozzle_size*2":
                                {
                                    // machine_nozzle_size * 2
                                    Multiply(or, "machine_nozzle_size", 2);
                                }
                                break;

                            case "resolveorvalue('layer_height_0')*1.2":
                                {
                                    // resolveOrValue('layer_height_0') * 1.2
                                    Multiply(or, "layer_height_0", 1.2);
                                }
                                break;

                            case "raft_speed*0.75":
                                {
                                    // raft_speed * 0.75
                                    Multiply(or, "raft_speed", 0.75);
                                }
                                break;

                            case "raft_interface_line_width+0.2":
                                {
                                    // raft_interface_line_width + 0.2
                                    SettingOverride so = FindOveride(overrides, "raft_interface_line_width");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        val = val + 0.2;
                                        or.Value = val.ToString();
                                    }
                                }
                                break;


                            case "0.75*raft_speed":
                                {
                                    // 0.75 * raft_speed
                                    Multiply(or, "raft_speed", 0.75);
                                }
                                break;

                            case "resolveorvalue('layer_height')*1.5":
                                {
                                    // resolveOrValue('layer_height') * 1.5
                                    Multiply(or, "layer_height", 1.5);
                                }
                                break;

                            case "extrudervalue(support_roof_extruder_nr,'support_interface_height')":
                                {
                                    // extruderValue(support_roof_extruder_nr, 'support_interface_height')
                                    SettingOverride so = FindOveride(overrides, "support_interface_height");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "extrudervalue(support_roof_extruder_nr,'support_interface_enable')":
                                {
                                    // extruderValue(support_roof_extruder_nr, 'support_interface_enable')
                                    or.Value = "false";
                                    SettingOverride so = FindOveride(overrides, "support_interface_enable");
                                    if (so != null)
                                    {
                                        or.Value = so.Value;
                                    }
                                }
                                break;

                            case "wall_line_width_0*2":
                                {
                                    // wall_line_width_0 * 2
                                    Multiply(or, "wall_line_width_0", 2);
                                }
                                break;

                            case "extrudervalue(support_bottom_extruder_nr,'support_interface_enable')":
                                {
                                    // extruderValue(support_bottom_extruder_nr, 'support_interface_enable')
                                    or.Value = "false";
                                    SettingOverride so = FindOveride(overrides, "support_interface_enable");
                                    if (so != null)
                                    {
                                        or.Value = so.Value;
                                    }
                                }
                                break;

                            case "layer_heightiflayer_height>=0.16elselayer_height*2":
                                {
                                    // layer_height if layer_height >= 0.16 else layer_height * 2
                                    SettingOverride so = FindOveride(overrides, "line_Height");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        if (val < 0.16)
                                        {
                                            val = val * 2;
                                        }
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "0ifsupport_enableandsupport_structure=='tree'else20":
                                {
                                    double val = 20;

                                    // 0 if support_enable and support_structure == 'tree' else 20
                                    SettingOverride so = FindOveride(overrides, "support_enable");
                                    if (so != null)
                                    {
                                        SettingOverride so2 = FindOveride(overrides, "support_structure");
                                        if (so.Value.ToLower() == "tree" && so2.Value.ToLower() == "tree")
                                        {
                                            val = 0;
                                        }
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "3ifresolveorvalue('skirt_gap')>0.0else1":
                                {
                                    // 3 if resolveOrValue('skirt_gap') > 0.0 else 1
                                    double val = 1;
                                    SettingOverride so = FindOveride(overrides, "skirt_gap");
                                    if (so != null)
                                    {
                                        double val2 = Convert.ToDouble(so.Value);
                                        if (val2 > 0)
                                        {
                                            val = 3;
                                        }

                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "0ifadhesion_extruder_nr==-1elseadhesion_extruder_nr":
                                {
                                    // 0 if adhesion_extruder_nr == -1 else adhesion_extruder_nr
                                    double val = 0;
                                    SettingOverride so = FindOveride(overrides, "adhesion_extruder_nr");
                                    if (so != null)
                                    {
                                        double val2 = Convert.ToDouble(so.Value);
                                        if (val2 != -1)
                                        {
                                            val = val2;
                                        }
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "10000ifmagic_fuzzy_skin_point_density==0else1/magic_fuzzy_skin_point_density":
                                {
                                    // 10000 if magic_fuzzy_skin_point_density == 0 else 1 / magic_fuzzy_skin_point_density
                                    double val = 10000;
                                    SettingOverride so = FindOveride(overrides, "magic_fuzzy_skin_point_density");
                                    if (so != null)
                                    {
                                        double val2 = Convert.ToDouble(so.Value);
                                        if (val2 != 0)
                                        {
                                            val = 1.0 / val2;
                                        }
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "extrudervalue(support_bottom_extruder_nr,'support_interface_offset')":
                                {
                                    // extruderValue(support_bottom_extruder_nr, 'support_interface_offset')
                                    SettingOverride so = FindOveride(overrides, "support_interface_offset");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "resolveorvalue('raft_margin')ifresolveorvalue('adhesion_type')=='raft'elseresolveorvalue('brim_width')":
                                {
                                    // resolveOrValue('raft_margin') if resolveOrValue('adhesion_type') == 'raft' else resolveOrValue('brim_width')
                                    SettingOverride rm = FindOveride(overrides, "raft_margin");
                                    double val = Convert.ToDouble(rm.Value);

                                    SettingOverride so = FindOveride(overrides, "adhesion_type");
                                    if (so != null)
                                    {
                                        if (so.Value.ToLower() != "raft")
                                        {
                                            SettingOverride bw = FindOveride(overrides, "brim_width");
                                            if (bw != null)
                                            {
                                                val = Convert.ToDouble(bw.Value);
                                            }
                                        }
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "extrudervalue(support_bottom_extruder_nr,'minimum_interface_area')":
                                {
                                    // extruderValue(support_bottom_extruder_nr, 'minimum_interface_area')
                                    SettingOverride so = FindOveride(overrides, "minimum_interface_area");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "extrudervalue(support_roof_extruder_nr,'support_interface_offset')":
                                {
                                    // extruderValue(support_roof_extruder_nr, 'support_interface_offset')
                                    SettingOverride so = FindOveride(overrides, "support_interface_offset");
                                    if (so != null)
                                    {
                                        double val = Convert.ToDouble(so.Value);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "int(defaultextruderposition())ifresolveorvalue('adhesion_type')=='raft'else-1":
                                {
                                    // int(defaultExtruderPosition()) if resolveOrValue('adhesion_type') == 'raft' else -1
                                    or.Value = "-1";
                                }
                                break;

                            case "math.ceil(brim_width/(skirt_brim_line_width*initial_layer_line_width_factor/100.0))":
                                {
                                    // math.ceil(brim_width / (skirt_brim_line_width * initial_layer_line_width_factor / 100.0))
                                    double brim_width = 0;
                                    double skirt_brim_line_width = 0;
                                    double initial_layer_line_width_factor = 0;
                                    if (FetchValue("brim_width", ref brim_width))
                                    {
                                        if (FetchValue("skirt_brim_line_width", ref skirt_brim_line_width))
                                        {
                                            if (FetchValue("initial_layer_line_width_factor", ref initial_layer_line_width_factor))
                                            {
                                                double val = Math.Ceiling(brim_width / (skirt_brim_line_width * initial_layer_line_width_factor / 100.0));
                                                or.Value = val.ToString();
                                            }
                                        }
                                    }
                                }
                                break;
                            /*
                                                        case "(resolveorvalue('machine_width')/2+resolveorvalue('prime_tower_size')/2)ifresolveorvalue('machine_shape')=='elliptic'else(resolveorvalue('machine_width')-(resolveorvalue('prime_tower_base_size')if(resolveorvalue('adhesion_type')=='raft'orresolveorvalue('prime_tower_brim_enable'))else0)-max(max(extrudervalues('travel_avoid_distance'))+max(extrudervalues('machine_nozzle_offset_x'))+max(extrudervalues('support_offset'))+(extrudervalue(skirt_brim_extruder_nr,'skirt_brim_line_width')*extrudervalue(skirt_brim_extruder_nr,'skirt_line_count')*extrudervalue(skirt_brim_extruder_nr,'initial_layer_line_width_factor')/100+extrudervalue(skirt_brim_extruder_nr,'skirt_gap')ifresolveorvalue('adhesion_type')=='skirt'else0)+(resolveorvalue('draft_shield_dist')ifresolveorvalue('draft_shield_enabled')else0),max(map(abs,extrudervalues('machine_nozzle_offset_x'))),1))-(resolveorvalue('machine_width')/2ifresolveorvalue('machine_center_is_zero')else0)":
                                                            {
                                                                // (resolveOrValue('machine_width') / 2 + resolveOrValue('prime_tower_size') / 2) if resolveOrValue('machine_shape') == 'elliptic' else (resolveOrValue('machine_width') - (resolveOrValue('prime_tower_base_size') if (resolveOrValue('adhesion_type') == 'raft' or resolveOrValue('prime_tower_brim_enable')) else 0) - max(max(extruderValues('travel_avoid_distance')) + max(extruderValues('machine_nozzle_offset_x')) + max(extruderValues('support_offset')) + (extruderValue(skirt_brim_extruder_nr, 'skirt_brim_line_width') * extruderValue(skirt_brim_extruder_nr, 'skirt_line_count') * extruderValue(skirt_brim_extruder_nr, 'initial_layer_line_width_factor') / 100 + extruderValue(skirt_brim_extruder_nr, 'skirt_gap') if resolveOrValue('adhesion_type') == 'skirt' else 0) + (resolveOrValue('draft_shield_dist') if resolveOrValue('draft_shield_enabled') else 0), max(map(abs, extruderValues('machine_nozzle_offset_x'))), 1)) - (resolveOrValue('machine_width') / 2 if resolveOrValue('machine_center_is_zero') else 0)
                                                                double machine_width = 0;
                                                                double prime_tower_size = 0;
                                                                string machine_shape = "";
                                                                double prime_tower_base_size = 0;
                                                                string adhesion_type = "";
                                                                bool prime_tower_brim_enable = false;
                                                                double travel_avoid_distance = 0;
                                                                double machine_nozzle_offset_x = 0;
                                                                double support_offset = 0;
                                                                double skirt_brim_line_width = 0;
                                                                double skirt_line_count = 0;
                                                                double initial_layer_line_width_factor = 0;
                                                                double skirt_gap = 0;
                                                                bool draft_shield_enabled = false;
                                                                bool machine_center_is_zero = false;

                                                                SettingOverride so = FindOveride(overrides, "line_width");
                                                                if (so != null)
                                                                {
                                                                    double val = Convert.ToDouble(so.Value);
                                                                    val = val * 2;
                                                                    or.Value = val.ToString();
                                                                }
                                                            }
                                                            break;

                                                            */


                            case "1ifmagic_spiralizeelsemax(1,round((wall_thickness-wall_line_width_0)/wall_line_width_x)+1)ifwall_thickness!=0else0":
                                {
                                    // wall_line_count
                                    // 1 if magic_spiralize else max(1, round((wall_thickness - wall_line_width_0) / wall_line_width_x) + 1) if wall_thickness != 0 else 0
                                    SettingOverride so = FindOveride(overrides, "line_width");
                                    if (so != null)
                                    {
                                        or.Value = "3";
                                    }
                                }
                                break;

                            case ".25*machine_nozzle_size":
                                {
                                    Multiply(or, "machine_nozzle_size", 0.25);
                                }
                                break;

                            case "(machine_nozzle_size-wall_line_width_0)/2if(wall_line_width_0<machine_nozzle_sizeandinset_direction!=\"outside_in\")else0":
                                {
                                    // wall_0_inset
                                    // (machine_nozzle_size - wall_line_width_0) / 2 if (wall_line_width_0 < machine_nozzle_size and inset_direction != "outside_in") else 0
                                    or.Value = "0";

                                }
                                break;

                            case "wall_line_width_0/4":
                                {
                                    // min_feature_size
                                    // wall_line_width_0 / 4
                                    Multiply(or, "wall_line_width_0", 0.25);
                                }
                                break;

                            case "(0if(z_seam_position=='frontleft'orz_seam_position=='left'orz_seam_position=='backleft')elsemachine_width/2if(z_seam_position=='front'orz_seam_position=='back')elsemachine_width)-(machine_width/2ifz_seam_relativeormachine_center_is_zeroelse0)":
                                {
                                    // z_seam_x
                                    // (0 if (z_seam_position == 'frontleft' or z_seam_position == 'left' or z_seam_position == 'backleft') else machine_width / 2 if (z_seam_position == 'front' or z_seam_position == 'back') else machine_width) - (machine_width / 2 if z_seam_relative or machine_center_is_zero else 0)
                                    /*
                                    double val = 0;
                                    string zsp = "";
                                    if (FetchValue("z_seam_position", ref zsp))
                                    {
                                        zsp = zsp.ToLower();
                                        if (zsp == "front" || zsp == "back")
                                        {
                                            double mw = 0;
                                            if (FetchValue("machine_width", ref mw))
                                            {
                                                val = mw / 2;
                                            }
                                        }
                                        else
                                        {
                                            bool zseamrelative = false;
                                            bool machinecentreiszero = false;
                                            if ( FetchValue("z_seam_relative",ref zseamrelative) && FetchValue("machine_center_is_zero", ref machinecentreiszero))
                                            {

                                            }
                                        }
                                    }
                                    */
                                    Multiply(or, "machine_width", 0.5);
                                }
                                break;

                            case "layer_height_0+layer_height*3":
                                {
                                    double lh = 0;
                                    double lh0 = 0;
                                    if (FetchValue("layer_height_0", ref lh0) &&
                                         FetchValue("layer_height", ref lh))
                                    {
                                        double val = lh0 + lh * 3;
                                        or.Value = val.ToString();
                                    }
                                }
                                break;


                            case "skin_line_width*2":
                                {
                                    // small_skin_width
                                    // skin_line_width * 2
                                    Multiply(or, "skin_line_width", 2);
                                }
                                break;

                            case "support_line_width+0.4ifsupport_structure=='normal'else0.0":
                                {
                                    // support_offset
                                    // support_line_width + 0.4 if support_structure == 'normal' else 0.0
                                    double val = 0;
                                    double slw = 0;
                                    string ss = "";
                                    if (FetchValue("support_structure", ref ss))
                                    {
                                        if (ss == "normal")
                                        {
                                            if (FetchValue("support_line_width", ref slw))
                                            {
                                                val = slw + 0.4;
                                            }
                                        }
                                    }
                                    or.Value = val.ToString();

                                }
                                break;


                            case "support_pattern=='cross'orsupport_pattern=='gyroid'":
                                {
                                    // zig_zaggify_support
                                    // support_pattern == 'cross' or support_pattern == 'gyroid'
                                    or.Value = "false";
                                    string sp = "";
                                    if (FetchValue("support_patten", ref sp))
                                    {
                                        if (sp == "cross" || sp == "gyroid")
                                        {
                                            or.Value = "true";
                                        }
                                    }
                                }
                                break;

                            case "1if(support_interface_pattern=='zigzag')else0":
                                {
                                    // support_bottom_wall_count
                                    // 1 if (support_interface_pattern == 'zigzag') else 0
                                    or.Value = "0";
                                    if (Match("support_interface_pattern","zigzag"))
                                    {
                                        or.Value = "1";
                                    }
                                }
                                break;


                            case "math.floor(math.degrees(math.atan(line_width/2.0/layer_height)))":
                                {
                                    // support_angle
                                    // math.floor(math.degrees(math.atan(line_width / 2.0 /layer_height)))
                                    double lw = 0;
                                    double lh = 0;
                                    double val = 40;
                                    if (FetchValue("line_width", ref lw) &&
                                        FetchValue("layer_height", ref lh))
                                    {
                                        double ang = Math.Atan(lw / 2 / lh);
                                        val = Math.Floor(Degrees(ang));
                                    }
                                    or.Value = val.ToString();
                                }
                                break;

                            case "'buildplate'ifsupport_type=='buildplate'else'graceful'":
                                {
                                    // support_tree_rest_preference
                                    // 'buildplate' if support_type == 'buildplate' else 'graceful'
                                    or.Value = "graceful";
                                    if ( Match("support_type","buildplate"))
                                    {
                                        or.Value = "buildplate";
                                    }
                                }
                                break;

                            case "support_line_width*2":
                                {
                                    // support_tree_tip_diameter
                                    // support_line_width * 2
                                    Multiply(or, "support_line_width", 2);
                                }
                                break;

                            case "30ifsupport_roof_enableelse10":
                                {
                                    // support_tree_top_rate
                                    // 30 if support_roof_enable else 10
                                    or.Value = "10";
                                    if ( Match("support_roof_enable","true"))
                                    {
                                        or.Value = "30";
                                    }
                                    
                                }
                                break;


                            case "support_tree_angle*2/3":
                                {
                                    // support_tree_angle_slow
                                    // support_tree_angle * 2 / 3
                                    Multiply(or, "support_tree_angle", 2.0 / 3.0);
                                }
                                break;

                            case "max(min(support_angle,85),20)":
                                {
                                    // support_tree_angle
                                    // max(min(support_angle, 85), 20)
                                    double sa = 0;
                                    if ( FetchValue("support_angle", ref sa))
                                    {
                                        double val = Math.Min(sa, 85);
                                        val = Math.Max(val, 20);
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "layer_height_0+2*layer_height":
                                {
                                    // cool_fan_full_at_height
                                    // layer_height_0 + 2 * layer_height
                                    double lh0 = 0;
                                    double lh = 0;

                                    if (FetchValue("layer_height_0", ref lh0) &&
                                        FetchValue("layer_height", ref lh))
                                    {
                                        double val = lh0 + 2 * lh;
                                        or.Value = val.ToString();
                                    }
                                }
                                break;

                            case "100.0ifcool_fan_enabledelse0.0":
                                {
                                    // cool_fan_speed_max
                                    // 100.0 if cool_fan_enabled else 0.0
                                    or.Value = "0.0";
                                    if ( Match("cool_fan_enabled","true"))
                                    {
                                        or.Value = "100.0";
                                    }
                                }
                                break;

                            case "machine_nozzle_tip_outer_diameter/2*1.25":
                                {
                                    // travel_avoid_distance
                                    // machine_nozzle_tip_outer_diameter / 2 * 1.25
                                    Multiply(or, "machine_nozzle_tip_outer_diameter", 0.5 * 1.25);
                                }
                                break;

                            default:
                                if (IsCalculated(or.Value))
                                {
                                    count++;
                                    String l = entry;
                                    l = l.Replace("<KEY>", or.Key);
                                    l = l.Replace("<ORIGINALVALUE>", or.Value);
                                    l = l.Replace("<STDVALUE>", v);
                                    System.Diagnostics.Debug.WriteLine(l);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                runcount--;
            } while (runcount >= 0);
            System.Diagnostics.Debug.WriteLine($"count of calculated entries={count}");
        }

        private bool Match(string key, string val)
        {
            bool res = false;
            string entryVal = "";
            if ( FetchValue(key,ref entryVal))
            {
                res = (entryVal.ToLower() == val.ToLower());
            }
            return res;
        }

        private double Degrees(double angle)
        {
            return angle * 180.0 / Math.PI;
        }

        private bool FetchValue(string v, ref double value)
        {
            bool res = false;
            SettingOverride so = FindOveride(overrides, v);
            if (so != null)
            {
                value = Convert.ToDouble(so.Value);
                res = true;
            }
            return res; ;
        }
        private bool FetchValue(string v, ref string value)
        {
            bool res = false;
            SettingOverride so = FindOveride(overrides, v);
            if (so != null)
            {
                value = so.Value;
                res = true;
            }
            return res; ;
        }

        private bool FetchValue(string v, ref bool value)
        {
            bool res = false;
            SettingOverride so = FindOveride(overrides, v);
            if (so != null)
            {
                value = Convert.ToBoolean(so.Value);
                res = true;
            }
            return res; ;
        }
        private void Multiply(SettingOverride or, string v1, double v2)
        {
            SettingOverride so = FindOveride(overrides, v1);
            if (so != null)
            {
                double val = Convert.ToDouble(so.Value);
                val = val * v2;
                or.Value = val.ToString();
            }
        }

        private string AltIfFalse(string key, double val0, double val1)
        {
            string res = "";
            double val = val0;
            SettingOverride so = FindOveride(overrides, key);
            if (so != null)
            {
                if (so.Value.ToLower() == "false")
                {
                    val = val1;
                }
            }
            res = val.ToString();
            return res;
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
            ReadChildren(children, property.Name);
        }

        private void ReadChildren(JObject children, string section)
        {
            foreach (JProperty prop in children.Properties())
            {
                JObject oneSetting = (JObject)children[prop.Name];
                // if this property is a new one then create a new setting definition
                // if its old, then override the values in the existing one
                SettingDefinition cdf = new SettingDefinition();
                cdf.Name = prop.Name;
                cdf.Section = section;
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
                                    ReadChildren(subChildren, section);
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
                        Logger.LogException(ex);
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