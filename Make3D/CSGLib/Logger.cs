using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGLib
{
    class Logger
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
            File.AppendAllText(path, s);
        }
    }
}
