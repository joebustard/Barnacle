using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class StatementNode : ParseTreeNode
    {
        protected bool isInLibrary;

        // Instance constructor
        public StatementNode()
        {
        }

        // Copy constructor
        public StatementNode(StatementNode it)
        {
        }

        public bool IsInLibrary
        {
            get { return isInLibrary; }
            set { isInLibrary = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            return true;
        }

        public bool PullByte(out byte a)
        {
            bool result = false;
            a = 0;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                if (sti.MyType == StackItem.ItemType.ival)
                {
                    int v = sti.IntValue;
                    if (v < 0)
                    {
                        v = 0;
                    }
                    if (v > 255)
                    {
                        v = 255;
                    }
                    a = (byte)v;
                    result = true;
                }
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
            return result;
        }

        public override String ToString()
        {
            String result = "";
            return result;
        }

        protected bool PullDouble(out double a)
        {
            bool result = false;
            a = 0;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                if (sti.MyType == StackItem.ItemType.ival)
                {
                    int v = sti.IntValue;
                    a = (double)v;
                    result = true;
                }
                else
                {
                    if (sti.MyType == StackItem.ItemType.dval)
                    {
                        a = sti.DoubleValue;

                        result = true;
                    }
                }
            }
            return result;
        }

        protected bool PullInt(out int a)
        {
            bool result = false;
            a = 0;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                if (sti.MyType == StackItem.ItemType.ival)
                {
                    a = sti.IntValue;

                    result = true;
                }
                else
                {
                    if (sti.MyType == StackItem.ItemType.dval)
                    {
                        a = (int)sti.DoubleValue;

                        result = true;
                    }
                }
            }
            return result;
        }

        protected bool PullSolid(out int a)
        {
            bool result = false;
            a = 0;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                if (sti.MyType == StackItem.ItemType.sldval)
                {
                    a = sti.SolidValue;
                    result = true;
                }
            }
            return result;
        }

        protected bool PullString(out string a)
        {
            bool result = false;
            a = "";
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                if (sti.MyType == StackItem.ItemType.sval)
                {
                    a = sti.StringValue;
                    result = true;
                }
            }
            return result;
        }
    }
}