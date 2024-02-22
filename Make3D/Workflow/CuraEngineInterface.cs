using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Workflow
{
    public class CuraEngineInterface
    {
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

        // examples
        // $Printer =".\resources\definitions\creality_ender3pro.def.json"
        // $Extruder =".\resources\definitions\fdmextruder.def.json"
        // $SettingOverrides = -s material_print_temperature = "200" ^
        // -s material_print_temperature_layer_0 = "200" ^
        // -s adhesion_type = "raft" ^
        // -s layer_height = "0.18" ^
        // -s layer_height_0 = "0.18" ^
        // -s raft_base_thickness = "0.24" ^
        // $StartGcode = -s machine_start_gcode = "M117 %2 \nM201 X500.00 Y500.00 Z100.00 E5000.00 \nM203 X500.00 Y500.00 Z10.00 E50.00 \nM204 P500.00 R1000.00 T500.00 \nM205 X8.00 Y8.00 Z0.40 E5.00 \nM220 S100 \nM221 S100 \n\nG28 \nG29\nG92 E0 \nG1 Z2.0 F3000 \nG1 X10.1 Y20 Z0.28 F5000.0 \nG1 X10.1 Y200.0 Z0.28 F1500.0 E15 \nG1 X10.4 Y200.0 Z0.28 F5000.0 \nG1 X10.4 Y20 Z0.28 F1500.0 E30 \nG92 E0 \nG1 Z2.0 F3000 \n" ^
        // $EndGCode
        // $src = path\file.stl
        // $trg = path\file.gcode
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

        public static async Task<SliceResult> Slice(string stlPath, string gcodePath, string logPath, string sdCardName, string slicerPath, string printer, string extruder, string userProfile, String startG, string endG)
        {
            SliceResult res = new SliceResult();
            res.Result = false;
            try
            {
                // we need to construct a cmd file to run the slice
                // Work  out where it should be.
                // use different temp names, because we will have async tasks going
                // and we dont want two trying to write to the same cmd or log file
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

                // We need a slicer profile.
                // The profile is based on the ones supplied with Cura BUT
                // it doesn't use Cura's ones directly
                SlicerProfile userSettings = new SlicerProfile();
                SlicerProfile defaultSettings = null;
                if (userProfile != "" && userProfile.ToLower() != "none")
                {
                    string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    folder += "\\Barnacle\\PrinterProfiles\\";
                    userProfile = folder + userProfile + ".profile";

                    userSettings.LoadOverrides(userProfile);
                    userSettings.SaveAsXml(folder + "\\test.xaml");
                    string appFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                    string defProfile = System.IO.Path.Combine(appFolder, "DefaultPrinter.profile");
                    if (File.Exists(defProfile))
                    {
                        defaultSettings = new SlicerProfile();
                        defaultSettings.LoadOverrides(defProfile);
                    }
                }

                string settingoverrides = "";
                foreach (SettingOverride ov in userSettings.Overrides)
                {
                    bool add = true;
                    if (defaultSettings != null)
                    {
                        foreach (SettingOverride df in defaultSettings.Overrides)
                        {
                            if (df.Key == ov.Key)
                            {
                                if (df.Value == ov.Value)
                                {
                                    add = false;
                                }
                                break;
                            }
                        }
                    }
                    if (add)
                    {
                        settingoverrides += $"-s {ov.Key}=\"{ov.Value}\" ^\n";
                    }
                }
                settingoverrides = settingoverrides.Substring(0, settingoverrides.Length - 1);

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
                    int i = logLines.GetLength(0);
                    // if the log has something in it we can try extracting the print duration etc.
                    // if its empty then we can't do anything with it
                    if (i > 0)
                    {
                        if (logLines[i - 1].Trim().ToLower().StartsWith("filament"))
                        {
                            string[] words = logLines[i - 1].Split(':');
                            words[1] = words[1].Trim();
                            res.Filament = Convert.ToInt32(words[1]);
                        }
                        if (logLines[i - 3].Trim().ToLower().StartsWith("print time (s)"))
                        {
                            string[] words = logLines[i - 3].Split(':');
                            words[1] = words[1].Trim();
                            res.TotalSeconds = Convert.ToInt32(words[1]);

                            TimeSpan ts = new TimeSpan(0, 0, res.TotalSeconds);
                            res.Hours = ts.Hours;
                            res.Minutes = ts.Minutes;
                            res.Seconds = ts.Seconds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return res;
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

        public static List<string> GetAvailableUserProfiles()
        {
            string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder += "\\Barnacle\\PrinterProfiles";
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch
                {
                }
            }
            List<string> res = new List<string>();
            res.Add("None");
            String[] files = Directory.GetFiles(folder, "*.profile");
            if (files.GetLength(0) > 0)
            {
                foreach (string s in files)
                {
                    res.Add(Path.GetFileNameWithoutExtension(s));
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

        public static bool ExecuteCmds(string pt)
        {
            bool result = false;
            if (File.Exists(pt))
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = pt;
                //  pi.WindowStyle = ProcessWindowStyle.Normal;
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
    }
}