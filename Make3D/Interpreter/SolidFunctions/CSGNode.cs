using Make3D.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class CSGNode : ExpressionNode
    {
        private ExpressionNode leftSolid;
        private ExpressionNode rightSolid;

        public CSGNode()
        {
        }

        public CSGNode(ExpressionNode ls, ExpressionNode rs, string prim) : base(ls)
        {
            this.leftSolid = ls;
            this.rightSolid = rs;
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
            int rs = -1;

            if (EvalExpression(leftSolid, ref ls, "Solid1") &&
                EvalExpression(rightSolid, ref rs, "RightSolid"))
            {
                Object3D leftie = Script.ResultArtefacts[ls];
                Object3D rightie = Script.ResultArtefacts[rs];

                Group3D grp = new Group3D();
                grp.Name = leftie.Name;
                grp.Description = leftie.Description;
                grp.LeftObject = leftie;
                grp.RightObject = rightie;
                grp.PrimType = PrimType;

                if (grp.Init())
                {
                    grp.CalcScale(false);
                    grp.Remesh();
                    Script.ResultArtefacts.Add(grp);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                    //  Script.ResultArtefacts[ls] = null;
                    //  Script.ResultArtefacts[rs] = null;
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

            result += leftSolid.ToRichText() + ", ";
            result += rightSolid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = Id() + "( ";
            result += leftSolid.ToString() + ", ";
            result += rightSolid.ToString();
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
                Log.Instance().AddEntry("Union : " + v + " expression error");
            }
            return result;
        }

        private string Id()
        {
            string res = "Union";
            if (PrimType == "groupdifference")
            {
                res = "Difference";
            }
            else if (PrimType == "groupintersection")
            {
                res = "Intersect";
            }
            return res;
        }
    }
}