using System;
using System.IO;

namespace CSGLib
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