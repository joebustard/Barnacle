using Barnacle.Object3DLib;
using System;

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

        public string PrimType
        {
            get; set;
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
                int rs = -1;

                if (EvalExpression(leftSolid, ref ls, "LeftSolid", Id()) &&
                    EvalExpression(rightSolid, ref rs, "RightSolid", Id()))
                {
                    //  Log.Instance().AddEntry(Id() + $"{PrimType} on {ls} and {rs}");
                    if (Script.ResultArtefacts.ContainsKey(ls))
                    {
                        if (Script.ResultArtefacts.ContainsKey(rs))
                        {
                            Object3D leftie = Script.ResultArtefacts[ls];
                            Object3D rightie = Script.ResultArtefacts[rs];
                            if (leftie != null && rightie != null)
                            {
                                leftie.CalcScale(false);
                                rightie.CalcScale(false);

                                Group3D grp = new Group3D();
                                grp.Name = leftie.Name;
                                grp.Description = leftie.Description;
                                grp.LeftObject = leftie;

                                if (PrimType == "groupcut")
                                {
                                    grp.RightObject = rightie.Clone();

                                    grp.RightObject.CalcScale(false);
                                    grp.PrimType = "groupdifference";
                                }
                                else
                                {
                                    grp.RightObject = rightie;
                                    grp.PrimType = PrimType;
                                }

                                if (grp.Init())
                                {
                                    grp.CalcScale(false);
                                    grp.Remesh();
                                    int id = Script.NextObjectId;
                                    //    Log.Instance().AddEntry($"id={id}");
                                    Script.ResultArtefacts[id] = grp;
                                    ExecutionStack.Instance().PushSolid(id);
                                    //   ExecutionStack.Instance().LogStackTop();
                                    leftie.Remesh();
                                    rightie.Remesh();

                                    result = true;
                                    // invalidate the two source objects
                                    Script.ResultArtefacts[ls] = null;
                                    Script.ResultArtefacts[rs] = null;
                                }
                                else
                                {
                                    Log.Instance().AddEntry(Id() + $" : Operation Failed");
                                }
                            }
                            else
                            {
                                Log.Instance().AddEntry(Id() + $" : One of the solids is null (has it been deleted?) Failed");
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry(Id() + $" : Solid " + rightSolid.ToString() + " doesn't exist. Have you deleted it?");
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry(Id() + $" : Solid " + leftSolid.ToString() + "  doesn't exist. Have you deleted it?");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{Id()} : {ex.Message}");
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

        private string Id()
        {
            string res = "Union";
            if (PrimType == "groupdifference")
            {
                res = "Difference";
            }
            else if (PrimType == "groupintersection")
            {
                res = "Intersection";
            }
            else if (PrimType == "groupcut")
            {
                res = "Cutout";
            }
            else if (PrimType == "groupforceunion")
            {
                res = "Forceunion";
            }
            return res;
        }
    }
}