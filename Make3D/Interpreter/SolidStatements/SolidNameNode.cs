using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class SolidNameNode : StatementNode
    {
        private ExpressionNode expression;
        private String externalName;
        private String variableName;

        public SolidNameNode()
        {
            variableName = "";
            expression = null;
        }

        public ExpressionNode ExpressionNode
        {
            get { return expression; }
            set { expression = value; }
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public String VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        public override bool Execute()
        {
            bool result = false;
            if (expression != null)
            {
                result = expression.Execute();
                if (result)
                {
                    Symbol sym = SymbolTable.Instance().FindSymbol(variableName, SymbolTable.SymbolType.solidvariable);
                    if (sym != null)
                    {
                        int objectIndex = sym.SolidValue;
                        if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count)
                        {
                            StackItem sti = ExecutionStack.Instance().Pull();
                            if (sti != null && sti.MyType == StackItem.ItemType.sval)
                            {
                                String Line = sti.StringValue;
                                Script.ResultArtefacts[objectIndex].Name = Line;
                                result = true;
                            }
                            else
                            {
                                Log.Instance().AddEntry("Run Time Error : SetName expression incorrect");
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : SetName solid name incorrect");
                        }
                    }
                }
            }
            return result;
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("SetName ") + RichTextFormatter.VariableName(externalName) + RichTextFormatter.Operator(", ") + expression.ToRichText() + " ;";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + "SetName " + externalName + ", " + expression.ToString() + " ;";
            }
            return result;
        }
    }
}