using Barnacle.Object3DLib;
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
            double h = 0;

            if (EvalExpression(radiusExp, ref r, "Radius", "MakeTube") &&
                EvalExpression(thicknessExp, ref th, "Thickness", "MakeTube") &&
                EvalExpression(heightExp, ref h, "Height", "MakeTube") &&
                 EvalExpression(upperBevelExp, ref upper, "UpperBevel", "MakeTube") &&
                  EvalExpression(lowerBevelExp, ref lower, "LowerBevel", "MakeTube") &&
                  EvalExpression(sweepExp, ref sweep, "SweepAngle", "MakeTube")
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
                    Point3DCollection tmp = new Point3DCollection();
                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);
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
    }
}