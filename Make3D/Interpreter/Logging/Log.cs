using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ScriptLanguage
{
    public class Log
    {
        protected List<LogEntry> logEntrys;

        protected TextBox txtbox;

        private static Log singleton = null;

        private string logFileName = "Log.Txt";

        // Instance constructor
        protected Log()
        {
            logEntrys = new List<LogEntry>();
            txtbox = null;
        }

        public delegate void UpdateExternalText(string s);

        public string LogFileName
        {
            get
            {
                return logFileName = "Log.Txt";
            }
            set
            {
                if (logFileName != value)
                {
                    logFileName = value;
                }
            }
        }

        public UpdateExternalText UpdateText { get; set; }

        public static Log Instance()
        {
            if (singleton == null)
            {
                singleton = new Log();
            }
            return singleton;
        }

        public virtual void AddEntry(String Line)
        {
            LogEntry le = new LogEntry();
            le.Text = Line;
            le.DateStamp = DateTime.Now.ToString();
            logEntrys.Add(le);
            if (UpdateText != null)
            {
                UpdateText(le.DateStamp + " " + le.Text + "\r\n");
            }
            /*
                        if (txtbox != null)
                        {
                            int iStartOfLine = txtbox.Text.Length;

                            txtbox.Text += le.DateStamp + " " + le.Text + "\r\n";
                            txtbox.Select(iStartOfLine, 1);
                            txtbox.ScrollToEnd();
                        }
                        */
        }

        public void Clear()
        {
            logEntrys.Clear();
            if (txtbox != null)
            {
                txtbox.Text = "";
            }
        }

        public void Save()
        {
            String logPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\"
                             + AppDomain.CurrentDomain.FriendlyName + "\\";
            try
            {
                System.IO.StreamWriter fout = new System.IO.StreamWriter(logPath + logFileName, true);
                if (fout != null)
                {
                    foreach (LogEntry en in logEntrys)
                    {
                        fout.WriteLine(en.DateStamp + " " + en.Text);
                    }
                    fout.Close();
                }
            }
            catch
            {
            }
        }

        public void SetTextBox(TextBox t)
        {
            txtbox = t;
        }
    }
}