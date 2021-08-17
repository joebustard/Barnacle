using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidPositionNode : StatementNode
    {
        private ExpressionNode expression;
        private ExpressionCollection expressions;
        private String externalName;
        private String variableName;

        public SolidPositionNode()
        {
            variableName = "";
            expressions = new ExpressionCollection();
        }

        public int ExpressionCount { get { return expressions.Count(); } }

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

        public void AddExpression(ExpressionNode exp)
        {
            expressions.InsertAtStart(exp);
        }

        public override bool Execute()
        {
            bool result = false;
            if (expressions != null)
            {
                result = expressions.Execute();
                if (result)
                {
                    Symbol sym = SymbolTable.Instance().FindSymbol(variableName, SymbolTable.SymbolType.solidvariable);
                    if (sym != null)
                    {
                        int objectIndex = sym.SolidValue;
                        if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count)
                        {
                            double xr;
                            double yr;
                            double zr;

                            result = PullDouble(out xr);
                            if (result)
                            {
                                result = PullDouble(out yr);
                                if (result)
                                {
                                    result = PullDouble(out zr);
                                    if (result)
                                    {
                                        Point3D pos = new Point3D(xr, yr, zr);
                                        Script.ResultArtefacts[objectIndex].Position = pos;
                                        Script.ResultArtefacts[objectIndex].Remesh();
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Position solid name incorrect");
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
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Move ") + RichTextFormatter.VariableName(externalName) + RichTextFormatter.Operator(", ");
                result += expressions.ToRichText();
                result += " ;";
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
                result = Indentor.Indentation() + "Move " + externalName + ", " + expressions.ToString();
                result += " ;";
            }
            return result;
        }

        private bool PullDouble(out double a)
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
    }
}