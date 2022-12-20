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
    }
}