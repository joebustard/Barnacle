using System;
using System.Collections.Generic;
using static CSGLib.BooleanModeller;

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

        public void Push(ArraySymbol v)
        {
            StackItem sti = new StackItem();
            sti.ObjectValue = v;
            sti.MyType = StackItem.ItemType.arrayval;
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

        internal void LogStackTop()
        {
            if (StackItems.Count > 0)
            {
                int iStackIndex = StackItems.Count - 1;
                StackItem si = StackItems[iStackIndex];
                Log.Instance().AddEntry($"{ si.MyType.ToString()} {si.IntValue} {si.StringValue} {si.BooleanValue} {si.DoubleValue} {si.SolidValue}");
            }
            else
            {
                Log.Instance().AddEntry($"Stack Empty");
            }
        }

        internal bool PullSymbol(Symbol symbol)
        {
            bool result = false;
            StackItem.ItemType tt = TypeOfTop();
            switch (symbol.SymbolType)
            {
                case SymbolTable.SymbolType.intvariable:
                    {
                        if (tt == StackItem.ItemType.ival)
                        {
                            StackItem it = Pull();
                            symbol.IntValue = it.IntValue;
                            result = true;
                        }
                        else
                        {
                            if (tt == StackItem.ItemType.dval)
                            {
                                StackItem it = Pull();
                                symbol.IntValue = (int)it.DoubleValue;
                                result = true;
                            }
                        }
                    }
                    break;

                case SymbolTable.SymbolType.doublevariable:
                    {
                        if (tt == StackItem.ItemType.ival)
                        {
                            StackItem it = Pull();
                            symbol.DoubleValue = it.IntValue;
                            result = true;
                        }
                        else
                        {
                            if (tt == StackItem.ItemType.dval)
                            {
                                StackItem it = Pull();
                                symbol.DoubleValue = it.DoubleValue;
                                result = true;
                            }
                        }
                    }
                    break;

                case SymbolTable.SymbolType.boolvariable:
                    {
                        if (tt == StackItem.ItemType.ival)
                        {
                            StackItem it = Pull();
                            symbol.BooleanValue = (it.IntValue != 0);
                            result = true;
                        }
                        else
                        {
                            if (tt == StackItem.ItemType.bval)
                            {
                                StackItem it = Pull();
                                symbol.BooleanValue = it.BooleanValue;
                                result = true;
                            }
                        }
                    }
                    break;

                case SymbolTable.SymbolType.stringvariable:
                    {
                        if (tt == StackItem.ItemType.sval)
                        {
                            StackItem it = Pull();
                            symbol.StringValue = it.StringValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.solidvariable:
                    {
                        if (tt == StackItem.ItemType.sldval)
                        {
                            StackItem it = Pull();
                            symbol.SolidValue = it.SolidValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.handlevariable:
                    {
                        if (tt == StackItem.ItemType.hval)
                        {
                            StackItem it = Pull();
                            symbol.HandleValue = it.HandleValue;
                            result = true;
                        }
                    }
                    break;
            }
            return result;
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