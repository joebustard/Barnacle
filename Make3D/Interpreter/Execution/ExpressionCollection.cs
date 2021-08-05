using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class ExpressionCollection : ParseTreeNode
    {
        private List<ExpressionNode> Expressions;

        public ExpressionCollection()
        {
            Expressions = new List<ExpressionNode>();
        }

        public override bool Execute()
        {
            bool result = true;
            if (Expressions != null)
            {
                int i = 0;
                while ((i < Expressions.Count) &&
                        (result == true))
                {
                    result = Expressions[i].Execute();
                    i = i + 1;
                }
            }

            return result;
        }

        public override String ToRichText()
        {
            String result = "";
            if (Expressions.Count > 0)
            {
                result = Expressions[Expressions.Count - 1].ToRichText();
                for (int i = Expressions.Count - 2; i >= 0; i--)
                {
                    result += ", ";
                    result += Expressions[i].ToRichText();
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (Expressions.Count > 0)
            {
                result = Expressions[Expressions.Count - 1].ToString();
                for (int i = Expressions.Count - 2; i >= 0; i--)
                {
                    result += ", ";
                    result += Expressions[i].ToString();
                }
            }
            return result;
        }

        internal void Add(ExpressionNode exp)
        {
            Expressions.Add(exp);
        }

        internal int Count()
        {
            return Expressions.Count;
        }

        internal ExpressionNode Get(int i)
        {
            ExpressionNode result = null;
            if (i < Expressions.Count)
            {
                result = Expressions[i];
            }
            return result;
        }

        internal void InsertAtStart(ExpressionNode exp)
        {
            //
            // Reverse the order of the expressions as they are added
            // so the procedure doesn't have to sort them a t run time.
            // Makes calling a procedure faster!
            //

            if (Expressions.Count == 0)
            {
                Expressions.Add(exp);
            }
            else
            {
                Expressions.Insert(0, exp);
            }
        }

        private void AddExpression(ExpressionNode exp)
        {
            if (Expressions != null)
            {
                Expressions.Add(exp);
            }
        }
    }
}