using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class ProcedureNode : StatementNode
    {
        protected CompoundNode _Body;
        protected SymbolTable.SymbolType _ReturnType;

        protected String name;

        protected List<CParameter> Parameters;

        // Instance constructor
        public ProcedureNode()
        {
            _ReturnType = SymbolTable.SymbolType.unknown;
            name = "";
            Parameters = new List<CParameter>();
            _Body = null;
        }

        public CompoundNode Body
        {
            set { _Body = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public SymbolTable.SymbolType ReturnType
        {
            get { return _ReturnType; }
            set { _ReturnType = value; }
        }

        public void AddParameter(String ParamName, SymbolTable.SymbolType ParamType)
        {
            CParameter param = new CParameter();
            param.Name = ParamName;
            param.ParamType = ParamType;
            Parameters.Add(param);
        }

        /// If the procedure declaration is executed it shouldn't do anything
        /// It should only execute when its called by a calling statement
        ///
        public override bool Execute()
        {
            return true;
        }

        public bool ExecuteFromCall()
        {
            bool result = false;
            int i = 0;
            bool bParamsOk = true;
            while ((i < Parameters.Count) && (bParamsOk == true))
            {
                CParameter param = Parameters[i];
                bParamsOk = param.Execute();
                if (!bParamsOk)
                {
                    Log.Instance().AddEntry("Problem with parameter " + param.Name.Substring(name.Length));
                }
                i = i + 1;
            }
            if ((_Body != null) &&
                 (bParamsOk))
            {
                result = _Body.Execute();
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Procedure ");

                result += RichTextFormatter.Procedure(name);
                result += "( ";
                if (Parameters.Count > 0)
                {
                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        result += Parameters[i].ToRichText(name);
                        if (i < Parameters.Count - 1)
                        {
                            result += ", ";
                        }
                    }
                }
                result += " )";
                result += @"\par";
                result += _Body.ToRichText();
                result += @"\par";
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "Procedure ";

                result += name;
                result += "( ";
                if (Parameters.Count > 0)
                {
                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        result += Parameters[i].ToText(name);
                        if (i < Parameters.Count - 1)
                        {
                            result += ", ";
                        }
                    }
                }
                result += " )";
                result += "\n";
                result += _Body.ToString();
                result += "\n";
            }
            return result;
        }

        internal void AddStructParameter(string paramName, StructDefinition def)
        {
            CParameter param = new CParameter();
            param.Name = paramName;
            param.ParamType = SymbolTable.SymbolType.structname;
            param.StructDefinition = def;
            Parameters.Add(param);
        }
    }
}