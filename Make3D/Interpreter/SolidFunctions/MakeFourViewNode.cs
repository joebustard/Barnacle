using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeFourViewNode : ExpressionNode
    {
        private static string label = "MakeFourView";
        private ExpressionNode biasExp;
        private ExpressionNode distalStepsExp;
        private ExpressionNode frontViewExp;
        private ExpressionNode horizontalStepsExp;
        private ExpressionNode leftViewExp;
        private ExpressionNode rightViewExp;
        private ExpressionNode topViewExp;

        public MakeFourViewNode()
        {
        }

        public MakeFourViewNode(
            ExpressionNode frontView, ExpressionNode leftView, ExpressionNode rightView, ExpressionNode topView, ExpressionNode horizontalSteps, ExpressionNode distalSteps
            )
        {
            this.frontViewExp = frontView;
            this.leftViewExp = leftView;
            this.rightViewExp = rightView;
            this.topViewExp = topView;
            this.horizontalStepsExp = horizontalSteps;
            this.distalStepsExp = distalSteps;
        }

        public MakeFourViewNode(ExpressionCollection coll)
        {
            this.frontViewExp = coll.Get(0);
            this.leftViewExp = coll.Get(1);
            this.rightViewExp = coll.Get(2);
            this.topViewExp = coll.Get(3);
            this.horizontalStepsExp = coll.Get(4);
            this.distalStepsExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            string valFrontView = "";
            string valLeftView = "";
            string valRightView = "";
            string valTopView = "";
            int valHorizontalSteps = 0;
            int valDistalSteps = 0;

            if (
               EvalExpression(frontViewExp, ref valFrontView, "FrontView", "MakeFourView") &&
               EvalExpression(leftViewExp, ref valLeftView, "LeftView", "MakeFourView") &&
               EvalExpression(rightViewExp, ref valRightView, "RightView", "MakeFourView") &&
               EvalExpression(topViewExp, ref valTopView, "TopView", "MakeFourView") &&
               EvalExpression(horizontalStepsExp, ref valHorizontalSteps, "HorizontalSteps", "MakeFourView") &&
               EvalExpression(distalStepsExp, ref valDistalSteps, "DistalSteps", "MakeFourView")
               )
            {
                FourViewMaker maker = new FourViewMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "HorizontalSteps", valHorizontalSteps) && inRange;
                inRange = RangeCheck(maker, "DistalSteps", valDistalSteps) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "FourView";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valFrontView, valLeftView, valRightView, valTopView, valHorizontalSteps, valDistalSteps);

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
            this.frontViewExp = coll.Get(0);
            this.leftViewExp = coll.Get(1);
            this.rightViewExp = coll.Get(2);
            this.topViewExp = coll.Get(3);
            this.horizontalStepsExp = coll.Get(4);
            this.distalStepsExp = coll.Get(5);
            this.biasExp = coll.Get(6);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += frontViewExp.ToRichText() + ", ";
            result += leftViewExp.ToRichText() + ", ";
            result += rightViewExp.ToRichText() + ", ";
            result += topViewExp.ToRichText() + ", ";
            result += horizontalStepsExp.ToRichText() + ", ";
            result += distalStepsExp.ToRichText() + ", ";
            result += biasExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += frontViewExp.ToString() + ", ";
            result += leftViewExp.ToString() + ", ";
            result += rightViewExp.ToString() + ", ";
            result += topViewExp.ToString() + ", ";
            result += horizontalStepsExp.ToString() + ", ";
            result += distalStepsExp.ToString() + ", ";
            result += biasExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(FourViewMaker maker, string paramName, double val)
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