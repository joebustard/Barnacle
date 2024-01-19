using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LoggerLib
{
    public class Logger
    {
        private static bool started = false;
        private static string logFile = "";
        public static void Log(string s)
        {
            System.Diagnostics.Debug.Write(s);
            if (!started)
            {
                String logPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar
                                + "Barnacle" + System.IO.Path.DirectorySeparatorChar;
                logFile = logPath + "BarnacleLog.txt";
                File.WriteAllText(logFile, s);
                started = true;
            }
            else
            {
                File.AppendAllText(logFile, s);
            }
        }
        public static void LogLine(string s)
        {
            Log(s);
            Log(System.Environment.NewLine);
        }
        public static void LogDateTime(string s)
        {
            Log(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " ");
            Log(s);
            Log(System.Environment.NewLine);
        }
    }
}
