using Barnacle.Object3DLib;
using System;

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
            try
            {
                int ls = -1;

                if (EvalExpression(solid, ref ls, "Solid", Id()) && PrimType != null && PrimType != "")
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
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"GetSolidSize : {ex.Message}");
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