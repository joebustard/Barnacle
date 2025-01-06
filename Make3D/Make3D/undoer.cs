// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
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
            catch (Exception ex)
            {
                LoggerLib.Logger.LogLine(ex.Message);
            }
            checkIds.Clear();
        }

        public static string GetLastCheckPointName(string idString)
        {
            if (!checkIds.ContainsKey(idString))
            {
                checkIds[idString] = 0;
            }
            string s = undoFolderName + System.IO.Path.DirectorySeparatorChar + idString + "_" + checkIds[idString].ToString() + ".bob";
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

            return undoFolderName + System.IO.Path.DirectorySeparatorChar + idString + "_" + checkIds[idString].ToString() + ".bob";
        }

        public static void Initialise(string ufn)
        {
            undoFolderName = ufn;
            ClearUndoFiles();
        }
    }
}