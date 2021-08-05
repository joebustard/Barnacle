using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class ExecutionStack
    {
        private static ExecutionStack Singleton = null;
        private List<StackItem> StackItems;

        // Instance constructor
        private ExecutionStack()
        {
            StackItems = new List<StackItem>();
        }

        public static ExecutionStack Instance()
        {
            if (Singleton == null)
            {
                Singleton = new ExecutionStack();
            }
            return Singleton;
        }

        public StackItem Pull()
        {
            StackItem sti = null;
            if (StackItems.Count == 0)
            {
                Log.Instance().AddEntry("Run Time Error : Stack Underflow");
            }
            else
            {
                int iStackIndex = StackItems.Count - 1;
                sti = StackItems[iStackIndex];
                StackItems.RemoveAt(iStackIndex);
            }
            return sti;
        }

        public void Push(int v)
        {
            StackItem sti = new StackItem();
            sti.IntValue = v;
            sti.MyType = StackItem.ItemType.ival;
            StackItems.Add(sti);
        }

        public void Push(double v)
        {
            StackItem sti = new StackItem();
            sti.DoubleValue = v;
            sti.MyType = StackItem.ItemType.dval;
            StackItems.Add(sti);
        }

        public void Push(String v)
        {
            StackItem sti = new StackItem();
            sti.StringValue = v;
            sti.MyType = StackItem.ItemType.sval;
            StackItems.Add(sti);
        }

        public void Push(bool v)
        {
            StackItem sti = new StackItem();
            sti.BooleanValue = v;
            sti.MyType = StackItem.ItemType.bval;
            StackItems.Add(sti);
        }

        public void PushSolid(int v)
        {
            StackItem sti = new StackItem();
            sti.SolidValue = v;
            sti.MyType = StackItem.ItemType.sldval;
            StackItems.Add(sti);
        }

        internal void Clear()
        {
            StackItems.Clear();
        }

        internal StackItem.ItemType TypeOfTop()
        {
            StackItem.ItemType Result = StackItem.ItemType.noval;
            if (StackItems.Count > 0)
            {
                int iStackIndex = StackItems.Count - 1;
                Result = StackItems[iStackIndex].MyType;
            }
            return Result;
        }
    }
}