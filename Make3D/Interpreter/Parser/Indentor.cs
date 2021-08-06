using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguage
{

    class Indentor
    {
        static private int Level = 0;
        static public String Indentation()
        {
            String result = "";
            for (int i = 0; i < Level; i++)
            {
                result += "    ";
            }
            return result;
        }
        static public void Indent()
        {
            Level++;
        }
        static public void Outdent()
        {
            if (Level > 0)
            {
                Level--;
            }
        }
        static public void Reset()
        {
            Level = 0;
        }
    }
}
