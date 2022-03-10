using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class UnionAllNode : ExpressionNode
    {


        public UnionAllNode()
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {

                if (Script.ResultArtefacts.Count > 1)
                {
                    result = true;
                    bool grpOk = true;
                    Object3D leftie = Script.ResultArtefacts[0];
                    leftie.Remesh();
                    for (int i = 1; i < Script.ResultArtefacts.Count && grpOk; i++)
                    {
                        Object3D rightie = Script.ResultArtefacts[i];
                        rightie.Remesh();
                        if (leftie != null && rightie != null)
                        {
                            leftie.CalcScale(false);

                            rightie.CalcScale(false);
                            Group3D grp = new Group3D();
                            grp.Name = leftie.Name;
                            grp.Description = leftie.Description;
                            grp.LeftObject = leftie;

                            grp.RightObject = rightie;
                            grp.PrimType = "groupunion";

                            if (grp.Init())
                            {
                                grp.CalcScale(false);
                                grp.Remesh();
                                leftie = grp.ConvertToMesh();
                            }
                            else
                            {
                                Log.Instance().AddEntry("UnionAll : Failed to union two objects");
                            }
                        }
                        leftie.Remesh();
                    }

                    Script.ResultArtefacts.Clear();
                    Script.ResultArtefacts.Add(leftie);
                    leftie.CalcScale(false);
                    leftie.Remesh();
                    ExecutionStack.Instance().PushSolid(0);
                }
                else
                {
                    Log.Instance().AddEntry("UnionAll : At least two objects required");
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"UnionAll : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("UnionAll") + "( )";

            return result;
        }

        public override String ToString()
        {
            String result = "UnionAll( )";

            return result;
        }
    }
}