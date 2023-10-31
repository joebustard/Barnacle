using System;

namespace ScriptLanguage

{
    internal class SourceFileStackEntry
    {
        public char by;
        public char[] m_Buffer;
        public int m_BufferIndex;
        public int m_PutBackPos;

        public SourceFileStackEntry()
        {
            m_Buffer = null;
            m_BufferIndex = 0;
            by = ' ';
        }

        public char GetBy()
        {
            by = (char)0;
            m_BufferIndex++;
            if (m_Buffer != null)
            {
                if (m_BufferIndex < m_Buffer.GetLength(0))
                {
                    by = m_Buffer[m_BufferIndex];
                }
            }
            return by;
        }

        public bool IsNoise(char by)
        {
            bool result = false;
            if ((by == ' ') ||
                 (by == '\r') ||
                 (by == '\n') ||
                 (by == '\t'))
            {
                result = true;
            }
            return result;
        }

        public void PutTokenBack()
        {
            m_BufferIndex = m_PutBackPos;
        }

        public char SkipNoise()
        {
            while (IsNoise(by) == true)
            {
                GetBy();
            }

            return by;
        }

        public void SkipToEndOfLine()
        {
            bool bDone = false;
            while ((m_BufferIndex < m_Buffer.GetLength(0)) &&
                    (bDone == false))
            {
                by = m_Buffer[m_BufferIndex];

                if ((by == '\r') || (by == '\n'))
                {
                    bDone = true;
                }
                else
                {
                    m_BufferIndex++;
                }
            }
        }

        internal string GetToEndOfLine()
        {
            String Result = "";
            bool bDone = false;
            while ((m_BufferIndex < m_Buffer.GetLength(0)) &&
                    (bDone == false))
            {
                by = m_Buffer[m_BufferIndex];

                if ((by == '\r') || (by == '\n'))
                {
                    bDone = true;
                }
                else
                {
                    Result += (char)by;
                    m_BufferIndex++;
                }
            }

            return Result;
        }

        internal void RecordPutBackPos()
        {
            m_PutBackPos = m_BufferIndex;
        }

        internal void SetContent(string text)
        {
            m_Buffer = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                m_Buffer[i] = text[i];
            }
            by = m_Buffer[0];
        }

        internal bool SetSource(string FilePath, string basefolder)
        {
            bool result = false;
            m_Buffer = null;

            if (System.IO.File.Exists(FilePath))
            {
                result = ReadTarget(FilePath);
            }
            else
            {
                if (basefolder != "")
                {
                    if (!basefolder.EndsWith(@"\"))
                    {
                        basefolder += System.IO.Path.DirectorySeparatorChar;
                    }
                    string target = basefolder + FilePath;
                    if (System.IO.File.Exists(target))
                    {
                        result = ReadTarget(target);
                    }
                    else
                    {
                        target = AppContext.BaseDirectory;
                        target = System.IO.Path.Combine(target, FilePath);
                        if (System.IO.File.Exists(target))
                        {
                            result = ReadTarget(target);
                        }
                    }
                }
                else
                {
                   string  target = AppContext.BaseDirectory;
                    target = System.IO.Path.Combine(target, FilePath);
                    if (System.IO.File.Exists(target))
                    {
                        result = ReadTarget(target);
                    }
                }
            }
            return result;
        }

        private bool ReadTarget(string target)
        {
            bool result = false;
            System.IO.StreamReader fin = new System.IO.StreamReader(target);

            int FileSize = (int)fin.BaseStream.Length;
            if (FileSize > 0)
            {
                result = true;
                m_Buffer = new char[FileSize];
                fin.Read((char[])m_Buffer, 0, FileSize);
                by = m_Buffer[0];
            }
            fin.Close();
            m_BufferIndex = 0;
            return result;
        }
    }
}