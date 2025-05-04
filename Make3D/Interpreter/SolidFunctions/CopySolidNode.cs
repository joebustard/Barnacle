using Barnacle.Object3DLib;
using System;

namespace ScriptLanguage
{
    internal class CopySolidNode : ExpressionNode
    {
        private ExpressionNode solid;

        public CopySolidNode()
        {
        }

        public CopySolidNode(ExpressionNode ls) : base(ls)
        {
            this.solid = ls;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                int ls = -1;

                if (EvalExpression(solid, ref ls, "Solid Id", "Copy"))
                {
                    Object3D src = Script.ResultArtefacts[ls];

                    if (src != null)
                    {
                        src.CalcScale(false);
                        Object3D clone = src.Clone();
                        clone.CalcScale(false);
                        clone.Remesh();

                        int id = Script.NextObjectId;
                        Script.ResultArtefacts[id] = clone;
                        ExecutionStack.Instance().PushSolid(id);
                        result = true;
                    }
                    else
                    {
                        Log.Instance().AddEntry($"Run Time Error: Copy source not found {ls}");
                    }
                }
                else
                {
                    Log.Instance().AddEntry($"Run Time Error : Copy solid name incorrect {solid.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"Copy : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("Copy") + "( ";

            result += solid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Copy( ";
            result += solid.ToString();
            result += " )";
            return result;
        }
    }
}