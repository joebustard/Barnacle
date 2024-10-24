using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class Make//TOOLNAMENode : ExpressionNode
    {
        //NODEFIELDS

        public Make//TOOLNAMENode
            (
            //CONSTRUCTORPARAMETERS
            )
        {
            //COPYFIELDS
        }

        public Make//TOOLNAMENode
                (ExpressionCollection coll)
        {
            //COPYCOLLFIELDS
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            //EXECUTIONVALUEDECLARATIONS

            //EVALEXPRESSIONS
            {
                //TOOLNAMEMaker maker = new //TOOLNAMEMaker();
                // check calculated values are in range
                bool inRange = true;
                //RANGECHECKS
                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "//TOOLNAME";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(//MAKERPARAMS);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    int id = Script.NextObjectId;
                    Script.ResultArtefacts[id] = obj;
                    ExecutionStack.Instance().PushSolid(id);
                }
                else
                {
                    Log.Instance().AddEntry("Make//TOOLNAME : Illegal value");
                }
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("Make//TOOLNAME") + "( ";
            //EXPRESSIONTORICHTEXT
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Make//TOOLNAME( ";
            //EXPRESSIONTOSTRING
            result += " )";
            return result;
        }

        private static bool RangeCheck(//TOOLNAMEMaker maker, string paramName, double val)
        {
            bool inRange = maker.CheckLimits(paramName, val);
            if (!inRange)
            {
                ParamLimit pl = maker.GetLimits(paramName);
                if (pl != null)
                {
                    Log.Instance().AddEntry($"Make//TOOLNAME : {paramName} value {val} out of range ({pl.Low}..{pl.High}");
                }
                else
                {
                    Log.Instance().AddEntry($"MakeRailWheel : Can't check parameter {paramName}");
                }
            }

            return inRange;
        }
    }
}