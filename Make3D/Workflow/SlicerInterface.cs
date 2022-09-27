using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Workflow
{
    public class SlicerInterface
    {
        private static string sliceCmd =
 @"
echo <STLPATH>
pushd
cd ""C:\Program Files\slic3r""
slic3r.exe --no-gui -load ""cl_config.ini""  -o ""<GCODEPATH>""  ""<STLPATH>"" 1>""<LOGPATH>"" 2>&1
popd
";

        public static void Slice(string stlPath, string gcodePath, string logPath, string sdCardName)
        {
            try
            {
                string sdcard = "";
                if (sdCardName != "")
                {
                    sdcard = FindSDCard(sdCardName);
                }
                string cmdsToRun =
     @"pushd
";

                string cmd = sliceCmd.Replace("<STLPATH>", stlPath);
                cmd = cmd.Replace("<GCODEPATH>", gcodePath);
                cmd = cmd.Replace("<LOGPATH>", logPath);
                cmdsToRun += cmd;
                if (sdcard != "")
                {
                    cmdsToRun += "copy /y " + gcodePath + " " + sdcard + "\r\n";
                }

                cmdsToRun +=
     @"
popd";
                string tmpFile = Path.GetTempPath();
                tmpFile = Path.Combine(tmpFile, "slice.cmd");
                File.WriteAllText(tmpFile, cmdsToRun);
                ExecuteCmds(tmpFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                pi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle";
                Process.Start(pi);
            }
        }

        private static string FindSDCard(string label)
        {
            string res = "";
            var driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    if (drive.VolumeLabel.ToLower() == label.ToLower())
                    {
                        res = drive.Name;
                    }
                }
            }
            return res;
        }
    }
}