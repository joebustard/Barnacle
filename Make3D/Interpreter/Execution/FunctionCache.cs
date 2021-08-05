using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class FunctionCache
    {
        static private FunctionCache Singleton;
        private List<CFunctionNode> Functions;

        // Instance constructor
        private FunctionCache()
        {
            Functions = new List<CFunctionNode>();
        }

        public static FunctionCache Instance()
        {
            if (Singleton == null)
            {
                Singleton = new FunctionCache();
            }
            return Singleton;
        }

        public void AddFunction(CFunctionNode proc)
        {
            Functions.Add(proc);
        }

        public void Clear()
        {
            Functions.Clear();
        }

        public CFunctionNode FindFunction(string strName)
        {
            CFunctionNode result = null;
            foreach (CFunctionNode proc in Functions)
            {
                if (proc.Name == strName)
                {
                    result = proc;
                    break;
                }
            }
            return result;
        }

        public String ToRichText()
        {
            String Result = "";
            foreach (CFunctionNode proc in Functions)
            {
                if (proc.IsInLibrary == false)
                {
                    Result += proc.ToRichText();
                    Result += @" \par ";
                }
            }
            return Result;
        }

        public override String ToString()
        {
            String Result = "";
            foreach (CFunctionNode proc in Functions)
            {
                if (proc.IsInLibrary == false)
                {
                    Result += proc.ToString();
                    Result += "\n";
                }
            }
            return Result;
        }
    }
}