using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class ArraySymbolForCallNode : ExpressionNode
    {
        private String externalName;
        private String name;

        private ArraySymbol symbol;

        // Instance constructor
        public ArraySymbolForCallNode()
        {
            name = "";
            externalName = "";
            symbol = null;
        }

        // Copy constructor
        public ArraySymbolForCallNode(StructSymbolForCallNode it)
        {
            name = it.Name;
            externalName = it.Name;
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public ArraySymbol Symbol
        {
            set { symbol = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (symbol != null)
            {
                ExecutionStack.Instance().Push(symbol);
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
            String result = RichTextFormatter.VariableName(externalName);
            return result;
        }

        public override String ToString()
        {
            String result = externalName;
            return result;
        }
    }
}