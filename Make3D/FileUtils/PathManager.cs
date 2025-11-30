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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileUtils
{
    public class PathManager
    {
        public static string ApplicationDataFolder()
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Barnacle");
        }

        public static string CommonAppDataFolder()
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Barnacle");
        }

        public static void CreateDefaultFolders()
        {
            CreateIfNeeded(LibraryFolder());
            CreateIfNeeded(PrinterProfileFolder());
            CreateIfNeeded(UserPresetsPath());
            CreateIfNeeded(UserScriptTemplatesFolder());
            CreateIfNeeded(UserTemplatesFolder());
        }

        public static string LibraryFolder()
        {
            string pth = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Barnacle");
            pth += "//Library";
            return pth;
        }

        public static string PrinterProfileFolder()
        {
            string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder += "\\Barnacle\\PrinterProfiles";
            return folder;
        }

        public static string UserPresetsPath()
        {
            string pth = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Barnacle";
            return pth;
        }

        public static string UserScriptTemplatesFolder()
        {
            string pth = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Barnacle");
            pth += "//UserScriptTemplates";
            return pth;
        }

        public static string UserTemplatesFolder()
        {
            string pth = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Barnacle");
            pth += "//UserTemplates";
            return pth;
        }

        private static void CreateIfNeeded(string fld)
        {
            if (!Directory.Exists(fld))
            {
                try
                {
                    Directory.CreateDirectory(fld);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}