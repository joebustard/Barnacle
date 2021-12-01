using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class ExpressionNode : ParseTreeNode
    {
        protected bool isInLibrary;

        // Instance constructor
        public ExpressionNode()
        {
            isInLibrary = false;
        }

        // Copy constructor
        public ExpressionNode(ExpressionNode it)
        {
        }

        public bool IsInLibrary
        {
            get { return isInLibrary; }
            set { isInLibrary = value; }
        }

        public virtual bool EvalExpression(ExpressionNode exp, ref int x, string v, string id = "")
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    if (sti.MyType == StackItem.ItemType.sldval)
                    {
                        x = sti.SolidValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry($"{id} : {v}  expression error");
            }
            return result;
        }

        public virtual bool EvalExpression(ExpressionNode exp, ref double x, string v, string id = "")
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        x = sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry($"{id} : {v}  expression error");
            }
            return result;
        }

        public virtual bool EvalExpression(ExpressionNode exp, ref bool x, string v, string id = "")
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.bval)
                    {
                        x = sti.BooleanValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry($"{id} : {v}  expression error");
            }
            return result;
        }

        public virtual bool EvalExpression(ExpressionNode exp, ref double[] x, string v, string label = "")
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.arrayval)
                    {
                        if ((sti.ObjectValue as ArraySymbol).SymbolType == SymbolTable.SymbolType.doublearrayvariable)
                        {
                            object[] tmp = (sti.ObjectValue as ArraySymbol).Array.GetAll();
                            x = new double[tmp.Length];
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                x[i] = (double)tmp[i];
                            }
                            result = true;
                        }
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry($"{label} : {v}  expression error");
            }
            return result;
        }

        public virtual bool EvalExpression(ExpressionNode exp, ref string x, string v, string label = "")
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.sval)
                    {
                        x = sti.StringValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry($"{label} : {v}  expression error");
            }
            return result;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            return true;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            return result;
        }

        public override String ToString()
        {
            String result = "";
            return result;
        }
    }
}