using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptLanguage
{
    public class COpenedInputFiles
    {
        public class StreamId
        {
            public StreamReader rdr;
            public String Path;

            public StreamId()
            {
                rdr = null;
                Path = "";
            }
        }

        private List<StreamId> OpenedFiles;
        private static COpenedInputFiles Singleton = null;

        #region ctors

        // Instance constructor
        private COpenedInputFiles()
        {
            OpenedFiles = new List<StreamId>();
        }

        public static COpenedInputFiles Instance()
        {
            if (Singleton == null)
            {
                Singleton = new COpenedInputFiles();
            }
            return Singleton;
        }

        #endregion ctors

        public StreamReader OpenedFile(String Path)
        {
            StreamReader ExistingFile = null;
            foreach (StreamId s in OpenedFiles)
            {
                if (s.Path == Path)
                {
                    ExistingFile = s.rdr;
                    break;
                }
            }

            return ExistingFile;
        }

        public void CloseAll()
        {
            try
            {
                foreach (StreamId s in OpenedFiles)
                {
                    s.rdr.Close();
                }
            }
            catch
            {
            }
            OpenedFiles.Clear();
        }

        public bool ReadLine(String Path, out String Line)
        {
            bool result = false;
            Line = "";
            StreamReader reader = OpenedFile(Path);
            if (reader != null)
            {
                if (reader.EndOfStream == false)
                {
                    Line = reader.ReadLine();
                }
                result = true;
            }
            else
            {
                if (File.Exists(Path))
                {
                    StreamId NewFile = new StreamId();
                    NewFile.Path = Path;
                    NewFile.rdr = new StreamReader(Path);
                    if (NewFile.rdr != null)
                    {
                        Line = NewFile.rdr.ReadLine();
                        OpenedFiles.Add(NewFile);
                        result = true;
                    }
                }
            }

            return result;
        }

        public bool OpenFile(String Path)
        {
            bool result = false;
            if (File.Exists(Path))
            {
                StreamReader reader = OpenedFile(Path);
                if (reader != null)
                {
                    //
                    // Close it so it can be reopened again. This is the only way to get a text
                    // stream reader seeking to zero
                    //
                    reader.Close();
                    Remove(Path);
                }
                //
                // Exists but has not been opened yet
                // so do that now
                //
                StreamId NewFile = new StreamId();
                NewFile.Path = Path;
                NewFile.rdr = new StreamReader(Path);
                if (NewFile.rdr != null)
                {
                    OpenedFiles.Add(NewFile);
                    result = true;
                }
            }

            return result;
        }

        private void Remove(string Path)
        {
            for (int i = 0; i < OpenedFiles.Count; i++)
            {
                StreamId s = OpenedFiles[i];
                if (s.Path == Path)
                {
                    OpenedFiles.RemoveAt(i);
                    break;
                }
            }
        }

        internal bool CloseFile(string Path)
        {
            bool result = false;
            StreamReader reader = OpenedFile(Path);
            if (reader != null)
            {
                //
                // Close it so it can be reopened again. This is the only way to get a text
                // stream reader seeking to zero
                //
                reader.Close();
                Remove(Path);

                result = true;
            }
            return result;
        }
    }
}