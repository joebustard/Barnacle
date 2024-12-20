﻿using System;

namespace ScriptLanguage
{
    internal class FileExistsNode : SingleParameterFunction
    {
        // Instance constructor
        public FileExistsNode()
        {
            parameterExpression = null;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (Expression != null)
            {
                if (Expression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String Path = sti.StringValue;
                            try
                            {
                                bool bFileExists = System.IO.File.Exists(Path);
                                ExecutionStack.Instance().Push(bFileExists);
                                result = true;
                            }
                            catch (Exception e)
                            {
                                Log.Instance().AddEntry(e.Message);
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error FileExists expected string");
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
            String result = RichTextFormatter.KeyWord("FileExists(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "FileExists(" + parameterExpression.ToString() + " )";
            return result;
        }
    }
}