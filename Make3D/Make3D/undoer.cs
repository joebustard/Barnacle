using System.IO;

namespace Make3D
{
    public static class undoer
    {
        private static string undoFolderName = "";
        private static int checkId = -1;
        private static int maxCheckId = -1;

        public static void Initialise(string ufn)
        {
            undoFolderName = ufn;
            ClearUndoFiles();
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
            checkId = -1;
            maxCheckId = -1;
        }

        public static bool CanUndo()
        {
            return checkId > -1;
        }

        public static string GetNextCheckPointName()
        {
            checkId++;
            if (checkId > maxCheckId)
            {
                maxCheckId = checkId;
            }
            return undoFolderName + "\\tmpfile" + checkId.ToString() + ".xml";
        }

        public static string GetLastCheckPointName()
        {
            string s = undoFolderName + "\\tmpfile" + checkId.ToString() + ".xml";
            checkId--;
            return s;
        }
    }
}