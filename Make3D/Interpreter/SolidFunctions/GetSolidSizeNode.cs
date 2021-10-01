using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class GetSolidSizeNode : ExpressionNode
    {
        private ExpressionNode solid;

        public GetSolidSizeNode()
        {
        }

        public GetSolidSizeNode(ExpressionNode ls, string prim) : base(ls)
        {
            this.solid = ls;

            this.PrimType = prim;
        }

        public string PrimType { get; set; }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            int ls = -1;

            if (EvalExpression(solid, ref ls, "Solid") && PrimType != null && PrimType != "")
            {
                Object3D leftie = Script.ResultArtefacts[ls];

                if (leftie != null)
                {
                    leftie.CalcScale(false);
                    double d = 0;
                    switch (PrimType.ToLower())
                    {
                        case "length": d = leftie.Scale.X; break;
                        case "height": d = leftie.Scale.Y; break;
                        case "width": d = leftie.Scale.Z; break;
                    }
                    ExecutionStack.Instance().Push(d);
                    result = true;
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
            String result = RichTextFormatter.KeyWord(Id()) + "( ";

            result += solid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = Id() + "( ";
            result += solid.ToString();
            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref int x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    if (sti.MyType == StackItem.ItemType.sldval)
                    {
                        x = sti.SolidValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry(Id() + " : " + v + " expression error");
            }
            return result;
        }

        private string Id()
        {
            string res = "Length";
            if (PrimType == "width")
            {
                res = "Width";
            }
            else if (PrimType == "height")
            {
                res = "Height";
            }

            return res;
        }
    }
}