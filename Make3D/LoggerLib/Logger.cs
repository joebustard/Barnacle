using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileUtils;

namespace LoggerLib
{
    public class Logger
    {
        private static string logFile = "";
        private static bool started = false;
        private static DateTime startTime = DateTime.Now;

        public static void Log(string s)
        {
            System.Diagnostics.Debug.Write(s);
            if (!started)
            {
                String logPath = PathManager.ApplicationDataFolder() + System.IO.Path.DirectorySeparatorChar;
                logFile = logPath + "BarnacleLog.txt";
                File.WriteAllText(logFile, s);
                started = true;
            }
            else
            {
                File.AppendAllText(logFile, s);
            }
        }

        public static void LogDateTime(string s)
        {
            Log(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " ");
            Log(s);
            Log(System.Environment.NewLine);
        }

        public static void LogException(Exception ex,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            LogLine("Exception:");
            LogLine("member name: " + memberName);
            LogLine("source file path: " + sourceFilePath);
            LogLine("source line number: " + sourceLineNumber);

            LogLine(ex.Message);
        }

        public static void LogLine(string s)
        {
            Log(s);
            Log(System.Environment.NewLine);
        }

        public static void MarkEnd(string s)
        {
            DateTime endTime = DateTime.Now;
            TimeSpan ts = endTime - startTime;

            Log(s);
            Log(endTime.ToShortDateString() + " " + endTime.ToShortTimeString() + " ");
            Log(System.Environment.NewLine);
            Log(" Duration ");
            Log($"{ts.Hours}:{ts.Minutes}:{ts.Seconds}:{ts.Milliseconds}");
            Log(System.Environment.NewLine);
        }

        public static void MarkStart(string s)
        {
            startTime = DateTime.Now;
            Log(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " ");
            Log(s);
            Log(System.Environment.NewLine);
        }
    }
}