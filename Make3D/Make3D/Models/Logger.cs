using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Models
{
    class Logger
    {

        private static bool initialised = false;
        private static string path = "";
        public static void Log(string s)
        {
            if ( !initialised)
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "log.txt";
                File.WriteAllText(path, DateTime.Now.ToString()+"\r\n");
                initialised = true;
            }
            File.AppendAllText(path, s);
        }
    }
}
