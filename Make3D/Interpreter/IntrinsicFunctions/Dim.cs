using System;

namespace ScriptLanguage
{
    internal class DimNode : SingleParameterFunction
    {
        // Instance constructor
        public DimNode()
        {
            Symbol = null;
            ArrayName = "";
        }

        public String ArrayName { get; set; }
        public ArraySymbol Symbol { get; set; }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (this.Symbol != null)
            {
                ExecutionStack.Instance().Push(this.Symbol.Array.Length);
                result = true;
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("Dim(") + RichTextFormatter.VariableName(ArrayName) + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Dim(" + ArrayName + " )";
            return result;
        }
    }
}