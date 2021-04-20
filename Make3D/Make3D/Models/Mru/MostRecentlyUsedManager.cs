using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Models.Mru
{
    public class MostRecentlyUsedManager
    {
        private List<MruEntry> recentFilesList;

        public MostRecentlyUsedManager()
        {
            recentFilesList = new List<MruEntry>();
            Name = "Mru";
            LoadMru();
        }

        /// <summary>
        /// Name should be just mru or recentfiles or recentprocs. No path or extension
        /// </summary>
        public string Name { get; set; }

        public List<MruEntry> RecentFilesList
        {
            get { return recentFilesList; }
        }

        public void SaveMru()
        {
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (!Directory.Exists(mruPath))
            {
                Directory.CreateDirectory(mruPath);
            }
            StreamWriter fout = new StreamWriter(mruPath + "\\" + Name + ".txt");
            foreach (MruEntry me in recentFilesList)
            {
                fout.WriteLine(me.Path);
            }
            fout.Close();
        }

        public void UpdateRecentFiles(string fileName)
        {
            bool found = false;
            foreach (MruEntry mru in recentFilesList)
            {
                if (mru.Path == fileName)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                string shortName = AbbreviatePath(fileName, 30);
                MruEntry m = new MruEntry(shortName, fileName);
                recentFilesList.Insert(0, m);
                if (recentFilesList.Count > 10)
                {
                    recentFilesList.RemoveAt(recentFilesList.Count - 1);
                }
                SaveMru();
                // CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
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
                result += fldrs[i] + "\\";
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
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            if (Directory.Exists(mruPath))
            {
                StreamReader fin = new StreamReader(mruPath + "\\" + Name + ".txt");
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