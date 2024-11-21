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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Windows;

namespace Barnacle.Models
{
    public class ZipUtils
    {
        public static List<string> ListFilesInZip(string zipPath, string ext)
        {
            List<string> res = new List<string>();
            try
            {
                ext = ext.ToLower();

                if (File.Exists(zipPath))
                {
                    ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
                    var ets = zipArchive.Entries;
                    foreach (ZipArchiveEntry et in ets)
                    {
                        if (et.FullName.ToLower().EndsWith(ext))
                        {
                            res.Add(et.FullName);
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

        public static bool ExtractFileFromZip(string zipPath, string fileName, string targetFile)
        {
            bool res = false;
            try
            {
                fileName = fileName.ToLower();
                if (File.Exists(zipPath))
                {
                    ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
                    var ets = zipArchive.Entries;
                    foreach (ZipArchiveEntry et in ets)
                    {
                        if (et.FullName.ToLower() == fileName)
                        {
                            et.ExtractToFile(targetFile);
                            res = true;
                            break;
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

        public static bool CreateZipFromFiles(string zipPath, List<String> filePathsToAdd, List<String> emptyFoldersToAdd, string filePathRoot)
        {
            bool res = false;
            try
            {
                // delete the zip if it already exists
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var fPath in filePathsToAdd)
                    {
                        string entryName = Path.GetFileName(fPath);
                        if (fPath.StartsWith(filePathRoot))
                        {
                            // remove the root and the slash
                            entryName = fPath.Substring(filePathRoot.Length + 1);
                        }
                        archive.CreateEntryFromFile(fPath, entryName);
                    }

                    foreach (var fPath in emptyFoldersToAdd)
                    {
                        string entryName = fPath;
                        if (fPath.StartsWith(filePathRoot))
                        {
                            // remove the root and the slash
                            entryName = fPath.Substring(filePathRoot.Length + 1);
                        }
                        archive.CreateEntry(entryName + "\\");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return res;
        }
    }
}