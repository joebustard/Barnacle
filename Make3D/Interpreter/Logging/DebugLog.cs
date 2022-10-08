namespace ScriptLanguage
{
    public class DebugLog : Log
    {
        protected static DebugLog Singleton = null;

        // Instance constructor
        private DebugLog()
        {
            LogFileName = "DebugLog.txt";
        }

        public new static DebugLog Instance()
        {
            if (Singleton == null)
            {
                Singleton = new DebugLog();
            }
            return Singleton;
        }
    }
}