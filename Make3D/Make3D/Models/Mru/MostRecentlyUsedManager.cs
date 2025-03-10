﻿// **************************************************************************
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

namespace Barnacle.Models.Mru
{
    public class MostRecentlyUsedManager
    {
        private int numberToRemember;
        private List<MruEntry> recentFilesList;

        public MostRecentlyUsedManager()
        {
            recentFilesList = new List<MruEntry>();
            Name = "Mru";
            NumberToRemember = 150;
            LoadMru();
        }

        /// <summary>
        /// Name should be just mru or recentfiles or recentprocs. No path or extension
        /// </summary>
        public string Name { get; set; }

        public int NumberToRemember
        {
            get
            {
                return numberToRemember;
            }
            set
            {
                if (value > 0 && value < 200)
                {
                    numberToRemember = value;
                }
            }
        }

        public List<MruEntry> RecentFilesList
        {
            get { return recentFilesList; }
        }

        public void SaveMru()
        {
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (!Directory.Exists(mruPath))
            {
                Directory.CreateDirectory(mruPath);
            }
            StreamWriter fout = new StreamWriter(mruPath + System.IO.Path.DirectorySeparatorChar + Name + ".txt");
            foreach (MruEntry me in recentFilesList)
            {
                fout.WriteLine(me.Path);
            }
            fout.Close();
        }

        public void UpdateRecentFiles(string fileName)
        {
            MruEntry existing = null;
            foreach (MruEntry mru in recentFilesList)
            {
                if (mru.Path == fileName)
                {
                    existing = mru;
                    break;
                }
            }
            if (existing == null)
            {
                string shortName = AbbreviatePath(fileName, 30);
                MruEntry m = new MruEntry(shortName, fileName);
                recentFilesList.Insert(0, m);
                if (recentFilesList.Count > NumberToRemember)
                {
                    recentFilesList.RemoveAt(recentFilesList.Count - 1);
                }
                SaveMru();                
            }
            else
            {
                // bump existing to front of list
                recentFilesList.Remove(existing);
                recentFilesList.Insert(0, existing);
                SaveMru();
            }
        }

        private string AbbreviatePath(string fileName, int max)
        {
            string result = "";
            string[] fldrs = fileName.Split('\\');
            int last = fldrs.GetLength(0) - 1;
            int i = 0;
            while ((result.Length + fldrs[last].Length < max) && i < last)
            {
                result += fldrs[i] + System.IO.Path.DirectorySeparatorChar;
                i++;
            }
            if (last != i)
            {
                result += "....\\";
            }
            result += fldrs[last];
            return result;
        }

        private void LoadMru()
        {
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            if (Directory.Exists(mruPath))
            {
                StreamReader fin = new StreamReader(mruPath + System.IO.Path.DirectorySeparatorChar + Name + ".txt");
                while (!fin.EndOfStream)
                {
                    String s = fin.ReadLine();
                    string shortName = AbbreviatePath(s, 30);
                    MruEntry m = new MruEntry(shortName, s);
                    recentFilesList.Add(m);
                }
                fin.Close();
            }
        }
    }
}