using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeCrossGrillNode : ExpressionNode
    {
        private ExpressionNode crossBeamWidthExp;
        private ExpressionNode edgeWidthExp;
        private ExpressionNode grilleHeightExp;
        private ExpressionNode grilleLengthExp;
        private ExpressionNode grilleWidthExp;
        private ExpressionNode numberOfCrossBeamsExp;
        private ExpressionNode showEdgeExp;

        public MakeCrossGrillNode
            (
            ExpressionNode grilleLength, ExpressionNode grilleHeight, ExpressionNode grilleWidth, ExpressionNode numberOfCrossBeams, ExpressionNode crossBeamWidth, ExpressionNode showEdge, ExpressionNode edgeWidth
            )
        {
            this.grilleLengthExp = grilleLength;
            this.grilleHeightExp = grilleHeight;
            this.grilleWidthExp = grilleWidth;
            this.numberOfCrossBeamsExp = numberOfCrossBeams;
            this.crossBeamWidthExp = crossBeamWidth;
            this.showEdgeExp = showEdge;
            this.edgeWidthExp = edgeWidth;
        }

        public MakeCrossGrillNode
                (ExpressionCollection coll)
        {
            this.grilleLengthExp = coll.Get(0);
            this.grilleHeightExp = coll.Get(1);
            this.grilleWidthExp = coll.Get(2);
            this.numberOfCrossBeamsExp = coll.Get(3);
            this.crossBeamWidthExp = coll.Get(4);
            this.showEdgeExp = coll.Get(5);
            this.edgeWidthExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valGrilleLength = 0;
            double valGrilleHeight = 0;
            double valGrilleWidth = 0;
            int valNumberOfCrossBeams = 0;
            double valCrossBeamWidth = 0;
            bool valShowEdge = false;
            double valEdgeWidth = 0;

            if (
               EvalExpression(grilleLengthExp, ref valGrilleLength, "GrilleLength", "MakeCrossGrill") &&
               EvalExpression(grilleHeightExp, ref valGrilleHeight, "GrillHeight", "MakeCrossGrill") &&
               EvalExpression(grilleWidthExp, ref valGrilleWidth, "GrillWidth", "MakeCrossGrill") &&
               EvalExpression(numberOfCrossBeamsExp, ref valNumberOfCrossBeams, "NumberOfCrossBeams", "MakeCrossGrill") &&
               EvalExpression(crossBeamWidthExp, ref valCrossBeamWidth, "CrossBeamWidth", "MakeCrossGrill") &&
               EvalExpression(showEdgeExp, ref valShowEdge, "ShowEdge", "MakeCrossGrill") &&
               EvalExpression(edgeWidthExp, ref valEdgeWidth, "EdgeWidth", "MakeCrossGrill"))
            {
                CrossGrillMaker maker = new CrossGrillMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "GrilleLength", valGrilleLength) && inRange;
                inRange = RangeCheck(maker, "GrilleHeight", valGrilleHeight) && inRange;
                inRange = RangeCheck(maker, "GrilleWidth", valGrilleWidth) && inRange;
                inRange = RangeCheck(maker, "NumberOfCrossBeams", valNumberOfCrossBeams) && inRange;
                inRange = RangeCheck(maker, "CrossBeamWidth", valCrossBeamWidth) && inRange;
                inRange = RangeCheck(maker, "EdgeWidth", valEdgeWidth) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "CrossGrille";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valGrilleLength, valGrilleHeight, valGrilleWidth, valNumberOfCrossBeams, valCrossBeamWidth, valShowEdge, valEdgeWidth);

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
                    Log.Instance().AddEntry("MakeCrossGrille : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeCrossGrille") + "( ";

            result += grilleLengthExp.ToRichText() + ", ";
            result += grilleHeightExp.ToRichText() + ", ";
            result += grilleWidthExp.ToRichText() + ", ";
            result += numberOfCrossBeamsExp.ToRichText() + ", ";
            result += crossBeamWidthExp.ToRichText() + ", ";
            result += showEdgeExp.ToRichText() + ", ";
            result += edgeWidthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeCrossGrille( ";

            result += grilleLengthExp.ToString() + ", ";
            result += grilleHeightExp.ToString() + ", ";
            result += grilleWidthExp.ToString() + ", ";
            result += numberOfCrossBeamsExp.ToString() + ", ";
            result += crossBeamWidthExp.ToString() + ", ";
            result += showEdgeExp.ToString() + ", ";
            result += edgeWidthExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(CrossGrillMaker maker, string paramName, double val)
        {
            bool inRange = maker.CheckLimits(paramName, val);
            if (!inRange)
            {
                ParamLimit pl = maker.GetLimits(paramName);
                if (pl != null)
                {
                    Log.Instance().AddEntry($"MakeCrossGrille : {paramName} value {val} out of range ({pl.Low}..{pl.High}");
                }
                else
                {
                    Log.Instance().AddEntry($"MakeCrossGrille : Can't check parameter {paramName}");
                }
            }

            return inRange;
        }
    }
}