﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
set CURA_ENGINE_SEARCH_PATH=%CURAPATH%\resources
cd ""%CURAPATH%""
set path = ""%CURAPATH%"";%CURA_ENGINE_SEARCH_PATH%;%CURA_ENGINE_SEARCH_PATH%\definitions;%CURA_ENGINE_SEARCH_PATH%\extruders;%Path%
curaengine.exe slice ^
$Printer
$Extruder
$SettingOverrides
$StartGcode
$EndGcode
-e0 ^
-l ""$src"" ^
-o ""$trg"" 2>c:\tmp\slice.log
cd %fld%
pause
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
                                             string target
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
                txt = txt.Replace("$Printer\n", "");
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
            System.IO.File.WriteAllText(File, txt);

        }

        public static void Slice(string stlPath, string gcodePath, string logPath, string sdCardName)
        {
            try
            {
                string sdcard = "";
                if (sdCardName != "")
                {
                    sdcard = FindSDCard(sdCardName);
                }

                string tmpCmdFile = Path.GetTempPath();
                tmpCmdFile = Path.Combine(tmpCmdFile, "slice.cmd");
                string tmpFile = Path.GetTempPath();
                tmpFile = Path.Combine(tmpFile, "tmp.gcode");
                if ( File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
                SlicerProfile defpro = new SlicerProfile();
                string settingoverrides = "";
                foreach (SettingOverride ov in defpro.Overrides)
                {
                    settingoverrides += $"-s {ov.Key}=\"{ov.Value}\" ^\n";
                }
                settingoverrides = settingoverrides.Substring(0, settingoverrides.Length - 1);

                string n = System.IO.Path.GetFileNameWithoutExtension(stlPath);
                string startg = defpro.StartGCode.Replace("$NAME",n);
                
                WriteSliceFileCmd(tmpCmdFile,
                @"C:\Program Files\Ultimaker Cura 4.13.1",
                                  defpro.Printer,
                                  defpro.Extruder,
                                  settingoverrides,
                                  startg,
                                  defpro.EndGCode,
                                  stlPath,
                                 tmpFile);
                ExecuteCmds(tmpCmdFile);
                ReplaceNewLines(tmpFile, gcodePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void ReplaceNewLines(string src,string trg)
        {
            if (File.Exists(src))
            {
                String[] str = File.ReadAllLines(src);
                if ( File.Exists(trg))
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
                    
                        string[] lines = l.Split(seps,StringSplitOptions.RemoveEmptyEntries);
                        for ( int j = 0; j < lines.Count(); j ++)
                        {
                            fout.WriteLine(lines[j].Trim());
                        }
                    }
                }
                fout.Close();
            }

        }

        public static void ExecuteCmds(string pt)
        {
            if (File.Exists(pt))
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = pt;
                pi.WindowStyle = ProcessWindowStyle.Normal;
                pi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\3dModels";
                Process runner = Process.Start(pi);
                runner.WaitForExit();

            }
        }

        private static string FindSDCard(string label)
        {
            string res = "";
            var driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                if (drive.IsReady)
                {
                    if (drive.DriveType == DriveType.Removable)
                    {
                        if (drive.VolumeLabel.ToLower() == label.ToLower())
                        {
                            res = drive.Name;
                        }
                    }
                }
            }
            return res;
        }
    }
}
