using System;

namespace ScriptLanguage
{
    public class LogEntry
    {
        private String _DateStamp;
        private String _Text;

        // Instance constructor
        public LogEntry()
        {
            _Text = "";
            _DateStamp = "";
        }

        // Copy constructor
        public LogEntry(LogEntry it)
        {
            _Text = it.Text;
            _DateStamp = it.DateStamp;
        }

        public LogEntry(string dateStamp, string text)
        {
            DateStamp = dateStamp;
            Text = text;
        }

        public String DateStamp
        {
            get { return _DateStamp; }
            set { _DateStamp = value; }
        }

        public String Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
    }
}