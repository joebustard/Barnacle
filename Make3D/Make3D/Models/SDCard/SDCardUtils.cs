// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

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