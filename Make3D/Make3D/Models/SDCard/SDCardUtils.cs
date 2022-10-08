using System;
using System.IO;

namespace Barnacle.Models.SDCard
{
    public class SDCardUtils
    {
        public static string FindSDCard(string label)
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

        internal static void ClearFolder(string sdPath)
        {
            String[] fileNames = Directory.GetFiles(sdPath);
            foreach (string fmn in fileNames)
            {
                try
                {
                    File.Delete(fmn);
                }
                catch
                {
                }
            }
        }
    }
}