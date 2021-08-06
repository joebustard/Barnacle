using System;

namespace ScriptLanguage
{
    internal class CFilesDifferNode : ExpressionNode
    {
        private ExpressionNode _LeftExpression;

        private ExpressionNode _RightExpression;

        // Instance constructor
        public CFilesDifferNode()
        {
            _RightExpression = null;
            _LeftExpression = null;
        }

        public ExpressionNode LeftExpression
        {
            get { return _LeftExpression; }
            set { _LeftExpression = value; }
        }

        public ExpressionNode RightExpression
        {
            get { return _RightExpression; }
            set { _RightExpression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (_LeftExpression != null)
            {
                if (_LeftExpression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String Path1 = sti.StringValue;

                            if (_RightExpression != null)
                            {
                                if (_RightExpression.Execute())
                                {
                                    result = false;
                                    sti = ExecutionStack.Instance().Pull();
                                    if (sti != null)
                                    {
                                        if (sti.MyType == StackItem.ItemType.sval)
                                        {
                                            String Path2 = sti.StringValue;

                                            bool bSame = false;
                                            if (System.IO.File.Exists(Path1))
                                            {
                                                if (System.IO.File.Exists(Path2))
                                                {
                                                    try
                                                    {
                                                        byte[] File1 = System.IO.File.ReadAllBytes(Path1);
                                                        byte[] File2 = System.IO.File.ReadAllBytes(Path2);
                                                        if (File1.GetLength(0) == File2.GetLength(0))
                                                        {
                                                            bool bMatch = true;
                                                            for (long Index = 0; Index < File1.GetLength(0) && (bMatch == true); Index++)
                                                            {
                                                                if (File1[Index] != File2[Index])
                                                                {
                                                                    bMatch = false;
                                                                }
                                                            }
                                                            bSame = bMatch;
                                                        }
                                                        result = true;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Log.Instance().AddEntry(e.Message);
                                                    }
                                                    ExecutionStack.Instance().Push(!bSame);
                                                }
                                                else
                                                {
                                                    Log.Instance().AddEntry("Run Time Error : FilesDiffer could not find file " + Path2);
                                                }
                                            }
                                            else
                                            {
                                                Log.Instance().AddEntry("Run Time Error : FilesDiffer could not find file " + Path1);
                                            }
                                        }
                                        else
                                        {
                                            Log.Instance().AddEntry("Run Time Error FilesDiffer expected string");
                                        }
                                    }
                                    else
                                    {
                                        Log.Instance().AddEntry("Run Time Error FilesDiffer expected string");
                                    }
                                }
                            }
                        }
                    }
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
            String result = RichTextFormatter.KeyWord("FilesDiffer") + "( " + _LeftExpression.ToRichText() + ", " + _RightExpression.ToRichText() + " )";

            return result;
        }
    }
}