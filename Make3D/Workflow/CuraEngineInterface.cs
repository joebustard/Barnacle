using LoggerLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Workflow.CuraDefinition;

namespace Workflow
{
    public class CuraEngineInterface
    {
        
        // Cura gets upset if you send too many parameters It also gets upset if you don't send
        // these ones even if they still have their default values
        private static string[] mustAlwaysSend =
        {
"roofing_monotonic",
"cool_min_temperature",
"acceleration_enabled",
"acceleration_print",
"acceleration_roofing",
"acceleration_travel",
"acceleration_travel_layer_0",
"adaptive_layer_height_variation",
"adaptive_layer_height_variation_step",
"adhesion_type",
"brim_replaces_support",
"cool_fan_full_at_height",
"cool_min_layer_time",
"fill_outline_gaps",
"infill_before_walls",
"infill_overlap",
"infill_pattern",
"infill_wipe_dist",
"jerk_enabled",
"jerk_print",
"jerk_travel",
"jerk_travel_layer_0",
"machine_acceleration",
"machine_max_acceleration_e",
"machine_max_acceleration_x",
"machine_max_acceleration_y",
"machine_max_acceleration_z",
"machine_max_feedrate_e",
"machine_max_feedrate_x",
"machine_max_feedrate_y",
"machine_max_feedrate_z",
"machine_max_jerk_e",
"machine_max_jerk_xy",
"machine_max_jerk_z",
"material_final_print_temperature",
"material_initial_print_temperature",
"meshfix_maximum_resolution",
"meshfix_maximum_travel_resolution",
"minimum_interface_area",
"min_wall_line_width",
"minimum_support_area",
"optimize_wall_printing_order",
"retraction_combing",
"retraction_combing_max_distance",
"retraction_count_max",
"retraction_extrusion_window",
"retraction_hop",
"retraction_min_travel",
"retraction_prime_speed",
"retraction_retract_speed",
"retraction_speed",
"roofing_layer_count",
"skin_overlap",
"skirt_gap",
"skirt_line_count",
"speed_layer_0",
"speed_prime_tower",
"speed_print",
"speed_roofing",
"speed_support",
"speed_support_interface",
"speed_travel",
"speed_travel_layer_0",
"speed_wall_x",
"speed_z_hop",
"support_angle",
"support_enable",
"support_brim_enable",
"support_brim_width",
"support_infill_rate",
"support_interface_density",
"support_interface_enable",
"support_interface_height",
"support_interface_pattern",
"support_pattern",
"support_structure",
"support_xy_distance",
"support_xy_distance_overhang",
"support_xy_overrides_z",
"support_z_distance",
"support_bottom_angles",
"support_bottom_density",
"support_bottom_distance",
"support_bottom_enable",
"support_bottom_extruder_nr",
"support_bottom_height",
"support_bottom_line_distance",
"support_bottom_line_width",
"support_bottom_material_flow",
"support_bottom_offset",
"support_bottom_pattern",
"support_bottom_stair_step_height",
"support_bottom_stair_step_min_slope",
"support_bottom_stair_step_width",

"support_brim_line_count",

"support_conical_angle",
"support_conical_enabled",
"support_conical_min_width",
"support_connect_zigzags",

"support_extruder_nr_layer_0",
"support_extruder_nr",
"support_fan_enable",
"support_infill_angles",
"support_infill_extruder_nr",

"support_infill_sparse_thickness",
"support_initial_layer_line_distance",
"support_interface_angles",

"support_interface_extruder_nr",

"support_interface_line_width",
"support_interface_material_flow",
"support_interface_offset",
"support_interface_pattern",
"support_interface_priority",
"support_interface_skip_height",
"support_interface_wall_count",
"support_line_distance",
"support_line_width",
"support_material_flow",
"support_mesh_drop_down",
"support_mesh",
"support_meshes_present",
"support_offset",
"support_roof_angles",
"support_roof_density",
"support_roof_enable",
"support_roof_extruder_nr",
"support_roof_height",
"support_roof_line_distance",
"support_roof_line_width",
"support_roof_material_flow",
"support_roof_offset",
"support_roof_pattern",
"support_skip_some_zags",
"support_skip_zag_per_mm",
"support_supported_skin_fan_speed",
"support_top_distance",
"support_tower_diameter",
"support_tower_maximum_supported_diameter",
"support_tower_roof_angle",
"support_tree_angle_slow",
"support_tree_angle",
"support_tree_bp_diameter",
"support_tree_branch_diameter_angle",
"support_tree_branch_diameter",
"support_tree_branch_reach_limit",
"support_tree_limit_branch_reach",
"support_tree_max_diameter_increase_by_merges_when_support_to_model",
"support_tree_max_diameter",
"support_tree_min_height_to_model",
"support_tree_rest_preference",
"support_tree_tip_diameter",
"support_tree_top_rate",
"support_type",
"support_use_towers",
"support_wall_count",
"support_xy_distance_overhang",
"support_xy_distance",
"support_xy_overrides_z",
"support_zag_skip_count",
"support",
"top_bottom_thickness",
"travel_avoid_other_parts",
"travel_avoid_supports",
"travel_retract_before_outer_wall",
"wall_0_wipe_dist",
"wall_thickness",
"z_seam_corner",
"z_seam_type",
"gantry_height",
        };


        private static string slicecmdtemplate =
        @"
set fld=%~dp0
set CURAPATH=$CPATH
set CURA_ENGINE_SEARCH_PATH=%CURAPATH%\share\cura\resources;%CURAPATH%\share\cura\resources\definitions;%CURAPATH%\share\cura\resources\extruders
cd ""%CURAPATH%""
set path = ""%CURAPATH%"";%CURA_ENGINE_SEARCH_PATH%;%Path%
curaengine.exe slice -v ^
$Printer
$Extruder
$SettingOverrides
$StartGcode
$EndGcode
-e0 ^
-l ""$src"" ^
-o ""$trg"" >""$log"" 2>&1
cd %fld%
exit 0
    ";

        public static bool ExecuteCmds(string pt)
        {
            bool result = false;
            if (File.Exists(pt))
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = pt;
                // pi.WindowStyle = ProcessWindowStyle.Normal;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle";
                Process runner = Process.Start(pi);
                runner.WaitForExit();
                if (runner.ExitCode == 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public static List<String> GetAvailableCuraExtruders(string folder)
        {
            List<string> res = new List<string>();
            if (folder != null && folder != "")
            {
                String[] files = Directory.GetFiles(folder, "*.json");
                if (files.GetLength(0) > 0)
                {
                    foreach (string s in files)
                    {
                        string t = Path.GetFileName(s).Replace(".def.json", "");
                        res.Add(t);
                    }
                }
            }
            return res;
        }

        public static List<String> GetAvailableCuraPrinterDefinitions(string folder)
        {
            List<string> res = new List<string>();
            if (folder != null && folder != "")
            {
                String[] files = Directory.GetFiles(folder, "*.json");
                if (files.GetLength(0) > 0)
                {
                    foreach (string s in files)
                    {
                        string t = Path.GetFileName(s).Replace(".def.json", "");
                        res.Add(Path.GetFileNameWithoutExtension(t));
                    }
                }
            }
            return res;
        }

        public static async Task<SliceResult> Slice(string stlPath, string gcodePath, string logPath, string sdCardName, string slicerPath, string printer, string extruder, string userProfile, String startG, string endG)
        {
            SliceResult res = new SliceResult();
            res.Result = false;
            try
            {
                // we need to construct a cmd file to run the slice Work out where it should be. use
                // different temp names, because we will have async tasks going and we dont want two
                // trying to write to the same cmd or log file
                string tmpCmdFile = Path.GetTempFileName();
                tmpCmdFile = Path.ChangeExtension(tmpCmdFile, "cmd");
                if (File.Exists(tmpCmdFile))
                {
                    File.Delete(tmpCmdFile);
                }

                string tmpFile = Path.GetTempFileName();
                tmpFile = Path.ChangeExtension(tmpFile, "gcode");
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }

                // We need a slicer profile. The profile is based on the ones supplied with Cura BUT
                // it doesn't use Cura's ones directly
                String settingoverrides = "";

                CuraDefinitionFile curaDataForPrinter = new CuraDefinitionFile();

                if (slicerPath != null && slicerPath != "")
                {
                    string curaPrinterName = slicerPath + @"\share\cura\Resources\definitions\" + printer;
                    string curaExtruderName = slicerPath + @"\share\cura\Resources\definitions\" + extruder;

                    curaDataForPrinter.Load(curaPrinterName);
                    curaDataForPrinter.Load(curaExtruderName);
                    curaDataForPrinter.ProcessSettings();
                    curaDataForPrinter.SetUserValues();

                    if (File.Exists(userProfile))
                    {
                        String[] content = File.ReadAllLines(userProfile);
                        for (int i = 0; i < content.GetLength(0); i += 2)
                        {
                            string key = content[i];
                            string val = content[i + 1];
                            foreach (SettingDefinition sd in curaDataForPrinter.Overrides)
                            {
                                if (sd.Name == key)
                                {
                                    sd.UserValue = val;
                                    sd.ModifiedByUser = true;
                                }
                            }
                        }
                    }

                    if (curaDataForPrinter.Overrides != null)
                    {
                        foreach (SettingDefinition sd in curaDataForPrinter.Overrides)
                        {
                            if (sd.ModifiedByUser || sd.Calculated || mustAlwaysSend.Contains(sd.Name))
                            {
                                settingoverrides += $"-s {sd.Name}=\"{sd.UserValue}\" ^\n";
                            }
                        }
                    }
                    if (settingoverrides.EndsWith("\n"))
                    {
                        settingoverrides = settingoverrides.Substring(0, settingoverrides.Length - 1);
                    }
                    string n = System.IO.Path.GetFileNameWithoutExtension(stlPath);
                    if (File.Exists(logPath))
                    {
                        File.Delete(logPath);
                    }
                    WriteSliceFileCmd(tmpCmdFile,
                                      slicerPath,
                                      slicerPath + @"\share\cura\resources\definitions\" + printer,
                                      slicerPath + @"\share\cura\resources\extruders\" + extruder,
                                      settingoverrides,
                                      startG,
                                      endG,
                                      stlPath,
                                      tmpFile,
                                      logPath);

                    res.Result = await DoSlice(gcodePath, tmpCmdFile, tmpFile);

                    if (File.Exists(tmpCmdFile))
                    {
                        File.Delete(tmpCmdFile);
                    }

                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                    if (res.Result && File.Exists(logPath))
                    {
                        string[] logLines = File.ReadAllLines(logPath);
                        int len = logLines.GetLength(0);
                        // if the log has something in it we can try extracting the print duration
                        // etc. if its empty then we can't do anything with it
                        if (len > 0)
                        {
                            for (int lineIndex = 0; lineIndex < len; lineIndex++)
                            {
                                int timeIndex = logLines[lineIndex].IndexOf("Print time (s):");
                                if (timeIndex > -1)
                                {
                                    string dummy = logLines[lineIndex].Substring(timeIndex + 16).Trim();
                                    res.TotalSeconds = Convert.ToInt32(dummy);

                                    TimeSpan ts = new TimeSpan(0, 0, res.TotalSeconds);
                                    res.Hours = ts.Hours;
                                    res.Minutes = ts.Minutes;
                                    res.Seconds = ts.Seconds;
                                }
                                int filamentIndex = logLines[lineIndex].IndexOf("Filament (mm^3):");
                                if (filamentIndex > -1)
                                {
                                    string dummy = logLines[lineIndex].Substring(filamentIndex + 17).Trim();
                                    res.Filament = Convert.ToInt32(dummy);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.LogException(ex);
            }
            return res;
        }

        // examples $Printer =".\resources\definitions\creality_ender3pro.def.json" $Extruder
        // =".\resources\definitions\fdmextruder.def.json" $SettingOverrides = -s
        // material_print_temperature = "200" ^
        // -s material_print_temperature_layer_0 = "200" ^
        // -s adhesion_type = "raft" ^
        // -s layer_height = "0.18" ^
        // -s layer_height_0 = "0.18" ^
        // -s raft_base_thickness = "0.24" ^ $StartGcode = -s machine_start_gcode = "M117 %2 \nM201
        // X500.00 Y500.00 Z100.00 E5000.00 \nM203 X500.00 Y500.00 Z10.00 E50.00 \nM204 P500.00
        // R1000.00 T500.00 \nM205 X8.00 Y8.00 Z0.40 E5.00 \nM220 S100 \nM221 S100 \n\nG28
        // \nG29\nG92 E0 \nG1 Z2.0 F3000 \nG1 X10.1 Y20 Z0.28 F5000.0 \nG1 X10.1 Y200.0 Z0.28
        // F1500.0 E15 \nG1 X10.4 Y200.0 Z0.28 F5000.0 \nG1 X10.4 Y20 Z0.28 F1500.0 E30 \nG92 E0
        // \nG1 Z2.0 F3000 \n" ^ $EndGCode $src = path\file.stl $trg = path\file.gcode
        public static void WriteSliceFileCmd(String File,
                                             string curapath,
                                             string prnter,
                                             string extruder,
                                             string settingoverrides,
                                             string startgcode,
                                             string endgcode,
                                             string src,
                                             string target,
                                             string logPath
                                             )

        {
            string txt = slicecmdtemplate.Replace("$CPATH", curapath);

            if (prnter != "")
            {
                if (!prnter.StartsWith("-j"))
                {
                    prnter = "-j " + $"\"{prnter}\"";
                }
                txt = txt.Replace("$Printer", prnter + " ^");
            }
            else
            {
                txt = txt.Replace("$Printer\r\n", "");
            }

            if (extruder != "")
            {
                if (!extruder.StartsWith("-j"))
                {
                    extruder = "-j " + $"\"{extruder}\"";
                }
                txt = txt.Replace("$Extruder", extruder + " ^");
            }
            else
            {
                txt = txt.Replace("$Extruder\n", "");
            }

            txt = txt.Replace("$SettingOverrides", settingoverrides);
            if (startgcode != "")
            {
                if (!startgcode.StartsWith("-s machine_start_gcode="))
                {
                    startgcode = @"-s machine_start_gcode=""" + startgcode + @"""";
                }
                txt = txt.Replace("$StartGcode", startgcode + " ^");
            }
            else
            {
                txt = txt.Replace("$StartGcode\r\n", "");
            }
            if (endgcode != "")
            {
                if (!endgcode.StartsWith("-s machine_end_gcode="))
                {
                    endgcode = @"-s machine_end_gcode=""" + endgcode + @"""";
                }
                txt = txt.Replace("$EndGcode", endgcode + " ^");
            }
            else
            {
                txt = txt.Replace("$EndGcode\r\n", "");
            }

            txt = txt.Replace("$src", src);
            txt = txt.Replace("$trg", target);
            txt = txt.Replace("$log", logPath);
            String tmp = Path.GetTempPath();
            txt = txt.Replace("$tmp", tmp);

            System.IO.File.WriteAllText(File, txt);
        }

        private static Task<bool> DoSlice(string gcodePath, string tmpCmdFile, string tmpFile)
        {
            bool result = ExecuteCmds(tmpCmdFile);
            if (result)
            {
                result = ReplaceNewLines(tmpFile, gcodePath);
            }

            return Task.FromResult<bool>(result);
        }

        private static bool ReplaceNewLines(string src, string trg)
        {
            bool result = false;
            if (File.Exists(src))
            {
                String[] str = File.ReadAllLines(src);
                if (File.Exists(trg))
                {
                    File.Delete(trg);
                }
                string[] seps = { @"\n" };
                StreamWriter fout = new StreamWriter(trg);
                for (int i = 0; i < str.GetLength(0); i++)
                {
                    string l = str[i];
                    if (!l.Contains(@"\n"))
                    {
                        l = l.Trim();
                        fout.WriteLine(l);
                    }
                    else
                    {
                        string[] lines = l.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < lines.Count(); j++)
                        {
                            fout.WriteLine(lines[j].Trim());
                        }
                    }
                }
                fout.Close();
                result = true;
            }
            return result;
        }
    }
}