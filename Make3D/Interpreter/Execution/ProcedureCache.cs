using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class CProcedureCache
    {
        static private CProcedureCache Singleton;
        private List<ProcedureNode> Procedures;

        // Instance constructor
        private CProcedureCache()
        {
            Procedures = new List<ProcedureNode>();
        }

        public static CProcedureCache Instance()
        {
            if (Singleton == null)
            {
                Singleton = new CProcedureCache();
            }
            return Singleton;
        }

        public void AddProcedure(ProcedureNode proc)
        {
            Procedures.Add(proc);
        }

        public void Clear()
        {
            Procedures.Clear();
        }

        public ProcedureNode FindProcedure(string strName)
        {
            ProcedureNode result = null;
            foreach (ProcedureNode proc in Procedures)
            {
                if (proc.Name.ToLower() == strName.ToLower())
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
            foreach (ProcedureNode proc in Procedures)
            {
                if (proc.IsInLibrary == false)
                {
                    Result += proc.ToRichText();
                }
            }
            return Result;
        }

        public override String ToString()
        {
            String Result = "";
            foreach (ProcedureNode proc in Procedures)
            {
                if (proc.IsInLibrary == false)
                {
                    Result += proc.ToString();
                }
            }
            return Result;
        }
    }
}