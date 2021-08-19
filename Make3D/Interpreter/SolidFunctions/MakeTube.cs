using Make3D.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTubeNode : ExpressionNode
    {
        private ExpressionNode heightExp;
        private ExpressionNode lowerBevelExp;
        private ExpressionNode radiusExp;
        private ExpressionNode sweepExp;
        private ExpressionNode thicknessExp;
        private ExpressionNode upperBevelExp;

        public MakeTubeNode()
        {
        }

        public MakeTubeNode(ExpressionNode r1, ExpressionNode th, ExpressionNode up, ExpressionNode low, ExpressionNode h, ExpressionNode swp) : base(r1)
        {
            this.radiusExp = r1;
            this.thicknessExp = th;
            this.lowerBevelExp = low;
            this.upperBevelExp = up;
            this.sweepExp = swp;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double r = 0;
            double th = 0;
            double upper = 0;
            double lower = 0;
            double sweep = 0;
            double g = 0;
            double h = 0;

            if (EvalExpression(radiusExp, ref r, "Radius") &&
                EvalExpression(thicknessExp, ref th, "Thickness") &&
                EvalExpression(heightExp, ref h, "Height") &&
                 EvalExpression(upperBevelExp, ref upper, "UpperBevel") &&
                  EvalExpression(lowerBevelExp, ref lower, "LowerBevel") &&
                  EvalExpression(sweepExp, ref sweep, "SweepAngle")
                )
            {
                if (r > 0 && th > 0 && upper >= 0 && lower >= 0 && h > 0 && upper + lower <= h)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Tube";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    TubeMaker maker = new TubeMaker(r, th, lower, upper, h, sweep);
                    maker.Generate(obj.RelativeObjectVertices, obj.TriangleIndices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTube : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTube") + "( ";

            result += radiusExp.ToRichText() + ", ";
            result += thicknessExp.ToRichText() + ", ";
            result += lowerBevelExp.ToRichText() + ", ";
            result += upperBevelExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += sweepExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTube( ";
            result += radiusExp.ToString() + ", ";
            result += thicknessExp.ToString() + ", ";
            result += lowerBevelExp.ToString() + ", ";
            result += upperBevelExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += sweepExp.ToString();
            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref double x, string v)
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
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        x = sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeTube : " + v + " expression error");
            }
            return result;
        }
    }
}