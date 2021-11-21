using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class ParseTreeNode
    {
        public bool HighLight;
        protected ParseTreeNode Child;
        public static bool continueRunning;
        // Instance constructor
        public ParseTreeNode()
        {
            Child = null;
            HighLight = false;
        }

        public virtual bool Execute()
        {
            bool result = true;
            if (Child != null)
            {
                result = Child.Execute();
            }
            return result;
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

        public virtual void SetChild(ParseTreeNode ch)
        {
            Child = ch;
        }

        public virtual SingleStepController.SteppingStatus SingleStep()
        {
            SingleStepController.SteppingStatus Status = SingleStepController.SteppingStatus.Failed;
            bool result = Execute();
            if (result)
            {
                Status = SingleStepController.SteppingStatus.OK_Complete;
            }
            return Status;
        }

        public virtual String ToRichText()
        {
            return "";
        }

        public override String ToString()
        {
            return "";
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