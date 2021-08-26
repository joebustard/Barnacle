using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class CFunctionNode : ProcedureNode
    {
        /*
        private CSymbolTable.SymbolType _ReturnType;
        public CSymbolTable.SymbolType ReturnType
        {
            get { return _ReturnType; }
            set { _ReturnType = value; }
        }
        */

        //private String _Name;

        //private List<CParameter> Parameters;

        //public String Name
        //{
        //    get { return _Name; }
        //    set { _Name = value; }
        //}

        //private CCompoundNode _Body;
        //public CCompoundNode Body
        //{
        //    set { _Body = value; }
        //}

        // Instance constructor
        public CFunctionNode()
        {
            _ReturnType = SymbolTable.SymbolType.unknown;
            name = "";
            Parameters = new List<CParameter>();
            _Body = null;
        }

        public string ReturnTypeName { get; internal set; }

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
                i = i + 1;
            }
            if ((_Body != null) &&
                 (bParamsOk))
            {
                result = _Body.Execute();
                if (result)
                {
                    //
                    // If the body has executed and returned properly there should be
                    // something on the top of the stack that matches the function type
                    // Check it is the right type without removing it.
                    //
                    StackItem.ItemType TypeOfTopOfStack = ExecutionStack.Instance().TypeOfTop();
                    SymbolTable.SymbolType ReturnValueType = SymbolTable.SymbolType.unknown;
                    switch (TypeOfTopOfStack)
                    {
                        case StackItem.ItemType.bval:
                            {
                                ReturnValueType = SymbolTable.SymbolType.boolvariable;
                            }
                            break;

                        case StackItem.ItemType.dval:
                            {
                                ReturnValueType = SymbolTable.SymbolType.doublevariable;
                            }
                            break;

                        case StackItem.ItemType.sval:
                            {
                                ReturnValueType = SymbolTable.SymbolType.stringvariable;
                            }
                            break;

                        case StackItem.ItemType.hval:
                            {
                                ReturnValueType = SymbolTable.SymbolType.handlevariable;
                            }
                            break;

                        case StackItem.ItemType.sldval:
                            {
                                ReturnValueType = SymbolTable.SymbolType.solidvariable;
                            }
                            break;

                        case StackItem.ItemType.ival:
                            {
                                ReturnValueType = SymbolTable.SymbolType.intvariable;
                            }
                            break;

                        default:
                            break;
                    }

                    if (ReturnValueType != ReturnType && ReturnType != SymbolTable.SymbolType.structname)
                    {
                        Log.Instance().AddEntry("Function " + name + " did not return the correct type");
                        result = false;
                    }
                }
            }
            return result;
        }

        //public void AddParameter(String ParamName, CSymbolTable.SymbolType ParamType)
        //{
        //    CParameter param = new CParameter();
        //    param.Name = ParamName;
        //    param.ParamType = ParamType;
        //    Parameters.Add(param);

        //}
        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Function ");

                result += ReturnTypeName + " ";
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
                result = Indentor.Indentation() + "Function ";

                result += ReturnTypeName + " ";
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
    }
}