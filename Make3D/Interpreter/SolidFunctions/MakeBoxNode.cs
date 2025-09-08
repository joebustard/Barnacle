using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBoxNode : ExpressionNode
    {
        private ExpressionNode backThicknessExp;
        private ExpressionNode baseThicknessExp;
        private ExpressionNode boxHeightExp;
        private ExpressionNode boxLengthExp;
        private ExpressionNode boxWidthExp;
        private ExpressionNode frontThicknessExp;
        private ExpressionNode leftThicknessExp;
        private ExpressionNode rightThicknessExp;

        public MakeBoxNode
            (
            ExpressionNode boxLength, ExpressionNode boxHeight, ExpressionNode boxWidth, ExpressionNode baseThickness, ExpressionNode leftThickness, ExpressionNode rightThickness, ExpressionNode frontThickness, ExpressionNode backThickness
            )
        {
            this.boxLengthExp = boxLength;
            this.boxHeightExp = boxHeight;
            this.boxWidthExp = boxWidth;
            this.baseThicknessExp = baseThickness;
            this.leftThicknessExp = leftThickness;
            this.rightThicknessExp = rightThickness;
            this.frontThicknessExp = frontThickness;
            this.backThicknessExp = backThickness;
        }

        public MakeBoxNode
                (ExpressionCollection coll)
        {
            this.boxLengthExp = coll.Get(0);
            this.boxHeightExp = coll.Get(1);
            this.boxWidthExp = coll.Get(2);
            this.baseThicknessExp = coll.Get(3);
            this.leftThicknessExp = coll.Get(4);
            this.rightThicknessExp = coll.Get(5);
            this.frontThicknessExp = coll.Get(6);
            this.backThicknessExp = coll.Get(7);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valBoxLength = 0;
            double valBoxHeight = 0;
            double valBoxWidth = 0;
            double valBaseThickness = 0;
            double valLeftThickness = 0;
            double valRightThickness = 0;
            double valFrontThickness = 0;
            double valBackThickness = 0;

            if (
               EvalExpression(boxLengthExp, ref valBoxLength, "BoxLength", "MakeBox") &&
               EvalExpression(boxHeightExp, ref valBoxHeight, "BoxHeight", "MakeBox") &&
               EvalExpression(boxWidthExp, ref valBoxWidth, "BoxWidth", "MakeBox") &&
               EvalExpression(baseThicknessExp, ref valBaseThickness, "BaseThickness", "MakeBox") &&
               EvalExpression(leftThicknessExp, ref valLeftThickness, "LeftThickness", "MakeBox") &&
               EvalExpression(rightThicknessExp, ref valRightThickness, "RightThickness", "MakeBox") &&
               EvalExpression(frontThicknessExp, ref valFrontThickness, "FrontThickness", "MakeBox") &&
               EvalExpression(backThicknessExp, ref valBackThickness, "BackThickness", "MakeBox"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valBoxLength < 1 || valBoxLength > 200)
                {
                    Log.Instance().AddEntry("MakeBox : BoxLength value out of range (1..200)");
                    inRange = false;
                }

                if (valBoxHeight < 1 || valBoxHeight > 200)
                {
                    Log.Instance().AddEntry("MakeBox : BoxHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valBoxWidth < 1 || valBoxWidth > 200)
                {
                    Log.Instance().AddEntry("MakeBox : BoxWidth value out of range (1..200)");
                    inRange = false;
                }

                if (valBaseThickness < 1 || valBaseThickness > 20)
                {
                    Log.Instance().AddEntry("MakeBox : BaseThickness value out of range (1..20)");
                    inRange = false;
                }

                if (valLeftThickness < 1 || valLeftThickness > 20)
                {
                    Log.Instance().AddEntry("MakeBox : LeftThickness value out of range (1..20)");
                    inRange = false;
                }

                if (valRightThickness < 1 || valRightThickness > 20)
                {
                    Log.Instance().AddEntry("MakeBox : RightThickness value out of range (1..20)");
                    inRange = false;
                }

                if (valFrontThickness < 1 || valFrontThickness > 20)
                {
                    Log.Instance().AddEntry("MakeBox : FrontThickness value out of range (1..20)");
                    inRange = false;
                }

                if (valBackThickness < 1 || valBackThickness > 20)
                {
                    Log.Instance().AddEntry("MakeBox : BackThickness value out of range (1..20)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Box";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    BoxMaker maker = new BoxMaker(valBoxLength, valBoxHeight, valBoxWidth, valBaseThickness, valLeftThickness, valRightThickness, valFrontThickness, valBackThickness);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    obj.CalculateAbsoluteBounds();
                    int id = Script.NextObjectId;
                    Script.ResultArtefacts[id] = obj;
                    ExecutionStack.Instance().PushSolid(id);
                }
                else
                {
                    Log.Instance().AddEntry("MakeBox : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeBox") + "( ";

            result += boxLengthExp.ToRichText() + ", ";
            result += boxHeightExp.ToRichText() + ", ";
            result += boxWidthExp.ToRichText() + ", ";
            result += baseThicknessExp.ToRichText() + ", ";
            result += leftThicknessExp.ToRichText() + ", ";
            result += rightThicknessExp.ToRichText() + ", ";
            result += frontThicknessExp.ToRichText() + ", ";
            result += backThicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeBox( ";

            result += boxLengthExp.ToString() + ", ";
            result += boxHeightExp.ToString() + ", ";
            result += boxWidthExp.ToString() + ", ";
            result += baseThicknessExp.ToString() + ", ";
            result += leftThicknessExp.ToString() + ", ";
            result += rightThicknessExp.ToString() + ", ";
            result += frontThicknessExp.ToString() + ", ";
            result += backThicknessExp.ToString();
            result += " )";
            return result;
        }
    }
}