using System;
using System.IO;

namespace Make3D.Models
{
    internal class Logger
    {
        private static bool initialised = false;
        private static string path = "";

        public static void Log(string s)
        {
            if (!initialised)
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "log.txt";
                File.WriteAllText(path, DateTime.Now.ToString() + "\r\n");
                initialised = true;
            }
            try
            {
                File.AppendAllText(path, s);
            }
            catch
            {
            }
        }
    }
}