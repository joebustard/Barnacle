using System;
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
set CURA_ENGINE_SEARCH_PATH=%CURAPATH%\resources;%CURAPATH%\resources\definitions;%CURAPATH%\resources\extruders
cd ""%CURAPATH%""
set path = ""%CURAPATH%"";%CURA_ENGINE_SEARCH_PATH%;%Path%
curaengine.exe slice ^
$Printer
$Extruder
$SettingOverrides
$StartGcode
$EndGcode
-e0 ^
-l ""$src"" ^
-o ""$trg"" 2>""$tmpslice.log""
cd %fld%
rem pause
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

            String tmp = Path.GetTempPath();
            txt = txt.Replace("$tmp", tmp);

            System.IO.File.WriteAllText(File, txt);

        }

        public static void Slice(string stlPath, string gcodePath, string logPath, string sdCardName, string slicerPath, string printer, string extruder, string userProfile )
        {
            try
            {
                string sdcard = "";
                // look fora named sd card.
                // if we find one  then we can copy the gocde there.
                if (sdCardName != "")
                {
                    sdcard = FindSDCard(sdCardName);
                }

                // weneed toconstruct a cmd file to run the slice
                // WOrk  out where it should be.
                string tmpCmdFile = Path.GetTempPath();
                tmpCmdFile = Path.Combine(tmpCmdFile, "slice.cmd");
                string tmpFile = Path.GetTempPath();
                tmpFile = Path.Combine(tmpFile, "tmp.gcode");
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }

                // We need a slicer profile. 
                // The profile is based on the ones upplied with Cura BUT
                // it doesn't use Cura's ones directly
                SlicerProfile defpro = new SlicerProfile();
                if (userProfile != "")
                {

                    string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    folder += "\\Barnacle\\PrinterProfiles\\";
                    userProfile = folder + userProfile + ".profile";
                    defpro.Load(userProfile);
                }
                string settingoverrides = "";
                foreach (SettingOverride ov in defpro.Overrides)
                {
                    settingoverrides += $"-s {ov.Key}=\"{ov.Value}\" ^\n";
                }
                settingoverrides = settingoverrides.Substring(0, settingoverrides.Length - 1);

                string n = System.IO.Path.GetFileNameWithoutExtension(stlPath);
                string startg = defpro.StartGCode.Replace("$NAME", n);


                WriteSliceFileCmd(tmpCmdFile,
                                    slicerPath,
                                  @".\resources\definitions\"+printer,
                                  @".\resources\extruders\"+extruder,
                                  settingoverrides,
                                  startg,
                                  defpro.EndGCode,
                                  stlPath,
                                 tmpFile);
                ExecuteCmds(tmpCmdFile);
                ReplaceNewLines(tmpFile, gcodePath);

                // debugging, just save the profile so we can have a peek.
                // defpro.Save("C:\\tmp\\sorted.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<string> GetAvailableUserProfiles()
        {
            
           
            string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder += "\\Barnacle\\PrinterProfiles";
            List<string> res = new List<string>();

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

        public static  List<String> GetAvailablePrinters(string folder)
        {

            List<string> res = new List<string>();
            if (folder != null && folder != "")
            {
                String[] files = Directory.GetFiles(folder, "*.json");
                if (files.GetLength(0) > 0)
                {
                    foreach (string s in files)
                    {
                        res.Add(Path.GetFileNameWithoutExtension(s));
                    }
                }
            }
            return res;
        }

        public static List<String> GetAvailableExtruders(string folder)
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

        private static void ReplaceNewLines(string src, string trg)
        {
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

