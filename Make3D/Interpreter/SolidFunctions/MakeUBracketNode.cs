using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeUBracketNode : ExpressionNode
    {
        private static string label = "MakeUBracket";
        private ExpressionNode legGapExp;
        private ExpressionNode legHeightExp;
        private ExpressionNode legWIdthExp;
        private ExpressionNode sweepAngleExp;

        public MakeUBracketNode()
        {
        }

        public MakeUBracketNode(
            ExpressionNode legHeight, ExpressionNode legWIdth, ExpressionNode legGap, ExpressionNode sweepAngle
            )
        {
            this.legHeightExp = legHeight;
            this.legWIdthExp = legWIdth;
            this.legGapExp = legGap;
            this.sweepAngleExp = sweepAngle;
        }

        public MakeUBracketNode(ExpressionCollection coll)
        {
            this.legHeightExp = coll.Get(0);
            this.legWIdthExp = coll.Get(1);
            this.legGapExp = coll.Get(2);
            this.sweepAngleExp = coll.Get(3);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valLegHeight = 0; double valLegWIdth = 0; double valLegGap = 0; double valSweepAngle = 0;

            if (
               EvalExpression(legHeightExp, ref valLegHeight, "LegHeight", "MakeUBracket") &&
               EvalExpression(legWIdthExp, ref valLegWIdth, "LegWIdth", "MakeUBracket") &&
               EvalExpression(legGapExp, ref valLegGap, "LegGap", "MakeUBracket") &&
               EvalExpression(sweepAngleExp, ref valSweepAngle, "SweepAngle", "MakeUBracket"))
            {
                UBracketMaker maker = new UBracketMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "LegHeight", valLegHeight) && inRange;
                inRange = RangeCheck(maker, "LegWIdth", valLegWIdth) && inRange;
                inRange = RangeCheck(maker, "LegGap", valLegGap) && inRange;
                inRange = RangeCheck(maker, "SweepAngle", valSweepAngle) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "UBracket";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valLegHeight, valLegWIdth, valLegGap, valSweepAngle);

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
                    Log.Instance().AddEntry($"{label} : Illegal value");
                }
            }

            return result;
        }

        public override void SetExpressions(ExpressionCollection coll)
        {
            this.legHeightExp = coll.Get(0);
            this.legWIdthExp = coll.Get(1);
            this.legGapExp = coll.Get(2);
            this.sweepAngleExp = coll.Get(3);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += legHeightExp.ToRichText() + ", ";
            result += legWIdthExp.ToRichText() + ", ";
            result += legGapExp.ToRichText() + ", ";
            result += sweepAngleExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += legHeightExp.ToString() + ", ";
            result += legWIdthExp.ToString() + ", ";
            result += legGapExp.ToString() + ", ";
            result += sweepAngleExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(UBracketMaker maker, string paramName, double val)
        {
            bool inRange = maker.CheckLimits(paramName, val);
            if (!inRange)
            {
                ParamLimit pl = maker.GetLimits(paramName);
                if (pl != null)
                {
                    Log.Instance().AddEntry($"{label} : {paramName} value {val} out of range ({pl.Low}..{pl.High}");
                }
                else
                {
                    Log.Instance().AddEntry($"{label} : Can't check parameter {paramName}");
                }
            }

            return inRange;
        }
    }
}