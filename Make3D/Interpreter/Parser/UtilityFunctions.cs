using System.Threading;

namespace ScriptLanguage
{
    public class UtilityFunctions
    {
        public static void RemoveFile(string p)
        {
            if (System.IO.File.Exists(p))
            {
                System.IO.File.Delete(p);
            }
        }

        /// <summary>
        /// Pause for the given number of seconds
        /// </summary>
        /// <param name="Seconds"></param>
        public static void Pause(double Seconds)
        {
            Thread.Sleep((int)(1000 * Seconds));
        }
    }
}