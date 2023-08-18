using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeRectGrilleNode : ExpressionNode
    {
        private ExpressionNode grillLengthExp;
        private ExpressionNode grillHeightExp;
        private ExpressionNode makeEdgeExp;
        private ExpressionNode edgeThicknessExp;
        private ExpressionNode verticalBarsExp;
        private ExpressionNode verticalBarThicknessExp;
        private ExpressionNode horizontalBarsExp;
        private ExpressionNode horizontalBarThicknessExp;

        public MakeRectGrilleNode
            (
            ExpressionNode grillLength, ExpressionNode grillHeight, ExpressionNode makeEdge, ExpressionNode edgeThickness, ExpressionNode verticalBars, ExpressionNode verticalBarThickness, ExpressionNode horizontalBars, ExpressionNode horizontalBarThickness
            )
        {
            this.grillLengthExp = grillLength;
            this.grillHeightExp = grillHeight;
            this.makeEdgeExp = makeEdge;
            this.edgeThicknessExp = edgeThickness;
            this.verticalBarsExp = verticalBars;
            this.verticalBarThicknessExp = verticalBarThickness;
            this.horizontalBarsExp = horizontalBars;
            this.horizontalBarThicknessExp = horizontalBarThickness;
        }

        public MakeRectGrilleNode
                (ExpressionCollection coll)
        {
            this.grillLengthExp = coll.Get(0);
            this.grillHeightExp = coll.Get(1);
            this.makeEdgeExp = coll.Get(2);
            this.edgeThicknessExp = coll.Get(3);
            this.verticalBarsExp = coll.Get(4);
            this.verticalBarThicknessExp = coll.Get(5);
            this.horizontalBarsExp = coll.Get(6);
            this.horizontalBarThicknessExp = coll.Get(7);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valGrillLength = 0; double valGrillHeight = 0; bool valMakeEdge = false; double valEdgeThickness = 0; double valVerticalBars = 0; double valVerticalBarThickness = 0; double valHorizontalBars = 0; double valHorizontalBarThickness = 0;

            if (
               EvalExpression(grillLengthExp, ref valGrillLength, "GrillLength", "MakeRectGrille") &&
               EvalExpression(grillHeightExp, ref valGrillHeight, "GrillHeight", "MakeRectGrille") &&
               EvalExpression(makeEdgeExp, ref valMakeEdge, "MakeEdge", "MakeRectGrille") &&
               EvalExpression(edgeThicknessExp, ref valEdgeThickness, "EdgeThickness", "MakeRectGrille") &&
               EvalExpression(verticalBarsExp, ref valVerticalBars, "VerticalBars", "MakeRectGrille") &&
               EvalExpression(verticalBarThicknessExp, ref valVerticalBarThickness, "VerticalBarThickness", "MakeRectGrille") &&
               EvalExpression(horizontalBarsExp, ref valHorizontalBars, "HorizontalBars", "MakeRectGrille") &&
               EvalExpression(horizontalBarThicknessExp, ref valHorizontalBarThickness, "HorizontalBarThickness", "MakeRectGrille"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valGrillLength < 5 || valGrillLength > 200)
                {
                    Log.Instance().AddEntry("MakeRectGrille : GrillLength value out of range (5..200)");
                    inRange = false;
                }

                if (valGrillHeight < 5 || valGrillHeight > 200)
                {
                    Log.Instance().AddEntry("MakeRectGrille : GrillHeight value out of range (5..200)");
                    inRange = false;
                }

                if (valEdgeThickness < 1 || valEdgeThickness > 200)
                {
                    Log.Instance().AddEntry("MakeRectGrille : EdgeThickness value out of range (1..200)");
                    inRange = false;
                }

                if (valVerticalBars < 0 || valVerticalBars > 100)
                {
                    Log.Instance().AddEntry("MakeRectGrille : VerticalBars value out of range (0..100)");
                    inRange = false;
                }

                if (valVerticalBarThickness < 1 || valVerticalBarThickness > 100)
                {
                    Log.Instance().AddEntry("MakeRectGrille : VerticalBarThickness value out of range (1..100)");
                    inRange = false;
                }

                if (valHorizontalBars < 1 || valHorizontalBars > 1)
                {
                    Log.Instance().AddEntry("MakeRectGrille : HorizontalBars value out of range (1..1)");
                    inRange = false;
                }

                if (valHorizontalBarThickness < 1 || valHorizontalBarThickness > 100)
                {
                    Log.Instance().AddEntry("MakeRectGrille : HorizontalBarThickness value out of range (1..100)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "RectGrille";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    RectGrilleMaker maker = new RectGrilleMaker(valGrillLength, valGrillHeight, valMakeEdge, valEdgeThickness, valVerticalBars, valVerticalBarThickness, valHorizontalBars, valHorizontalBarThickness);

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
                    Log.Instance().AddEntry("MakeRectGrille : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeRectGrille") + "( ";

            result += grillLengthExp.ToRichText() + ", ";
            result += grillHeightExp.ToRichText() + ", ";
            result += makeEdgeExp.ToRichText() + ", ";
            result += edgeThicknessExp.ToRichText() + ", ";
            result += verticalBarsExp.ToRichText() + ", ";
            result += verticalBarThicknessExp.ToRichText() + ", ";
            result += horizontalBarsExp.ToRichText() + ", ";
            result += horizontalBarThicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeRectGrille( ";

            result += grillLengthExp.ToString() + ", ";
            result += grillHeightExp.ToString() + ", ";
            result += makeEdgeExp.ToString() + ", ";
            result += edgeThicknessExp.ToString() + ", ";
            result += verticalBarsExp.ToString() + ", ";
            result += verticalBarThicknessExp.ToString() + ", ";
            result += horizontalBarsExp.ToString() + ", ";
            result += horizontalBarThicknessExp.ToString();
            result += " )";
            return result;
        }
    }
}