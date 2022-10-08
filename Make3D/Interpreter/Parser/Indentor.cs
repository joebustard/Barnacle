using System;

namespace ScriptLanguage
{
    internal class Indentor
    {
        private static int Level = 0;

        public static String Indentation()
        {
            String result = "";
            for (int i = 0; i < Level; i++)
            {
                result += "    ";
            }
            return result;
        }

        public static void Indent()
        {
            Level++;
        }

        public static void Outdent()
        {
            if (Level > 0)
            {
                Level--;
            }
        }

        public static void Reset()
        {
            Level = 0;
        }
    }
}