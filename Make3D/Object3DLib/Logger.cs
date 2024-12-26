using System;
using System.IO;
using System.Windows.Forms;

namespace Barnacle.Object3DLib
{
    public class Logger
    {
        private static bool initialised = false;
        private static string path = "";

        public static void Log(string s)
        {
            if (!initialised)
            {
                path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Barnacle");
                try
                {
                    Directory.CreateDirectory(path);
                    path += "\\log.txt";
                    File.WriteAllText(path, DateTime.Now.ToString() + "\r\n");
                    initialised = true;
                    File.AppendAllText(path, "\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            try
            {
                File.AppendAllText(path, s);
                File.AppendAllText(path, "\n");
                System.Diagnostics.Debug.WriteLine(s);
            }
            catch
            {
            }
        }
    }
}