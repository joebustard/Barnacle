using System.Collections.Generic;
using System.IO;

namespace Barnacle
{
    public static class undoer
    {
        private static Dictionary<string, int> checkIds = new Dictionary<string, int>();

        private static string undoFolderName = "";

        public static bool CanUndo(string idString)
        {
            if (!checkIds.ContainsKey(idString))
            {
                checkIds[idString] = 0;
            }
            return checkIds[idString] > 0;
        }

        public static void ClearUndoFiles()
        {
            try
            {
                if (!Directory.Exists(undoFolderName))
                {
                    Directory.CreateDirectory(undoFolderName);
                }
                foreach (string s in Directory.EnumerateFiles(undoFolderName))
                {
                    File.Delete(s);
                }
            }
            catch
            {
            }
            checkIds.Clear();
        }

        public static string GetLastCheckPointName(string idString)
        {
            if (!checkIds.ContainsKey(idString))
            {
                checkIds[idString] = 0;
            }
            string s = undoFolderName + "\\" + idString + "_" + checkIds[idString].ToString() + ".bob";
            checkIds[idString] = checkIds[idString] - 1;
            return s;
        }

        public static string GetNextCheckPointName(string idString)
        {
            if (!checkIds.ContainsKey(idString))
            {
                checkIds[idString] = 0;
            }
            checkIds[idString] = checkIds[idString] + 1;

            return undoFolderName + "\\" + idString + "_" + checkIds[idString].ToString() + ".bob";
        }

        public static void Initialise(string ufn)
        {
            undoFolderName = ufn;
            ClearUndoFiles();
        }
    }
}