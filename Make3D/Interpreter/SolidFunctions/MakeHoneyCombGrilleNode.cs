using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeHoneyCombGrilleNode : ExpressionNode
    {
        private static string label = "MakeHoneyCombGrille";
        private ExpressionNode beamLengthExp;
        private ExpressionNode beamWidthExp;
        private ExpressionNode edgeSizeExp;
        private ExpressionNode grilleLengthExp;
        private ExpressionNode grillHeightExp;
        private ExpressionNode grillThicknessExp;
        private ExpressionNode showEdgeExp;

        public MakeHoneyCombGrilleNode()
        {
        }

        public MakeHoneyCombGrilleNode(
            ExpressionNode grilleLength, ExpressionNode grillHeight, ExpressionNode grillThickness, ExpressionNode beamLength, ExpressionNode beamWidth, ExpressionNode edgeSize, ExpressionNode showEdge
            )
        {
            this.grilleLengthExp = grilleLength;
            this.grillHeightExp = grillHeight;
            this.grillThicknessExp = grillThickness;
            this.beamLengthExp = beamLength;
            this.beamWidthExp = beamWidth;
            this.edgeSizeExp = edgeSize;
            this.showEdgeExp = showEdge;
        }

        public MakeHoneyCombGrilleNode(ExpressionCollection coll)
        {
            this.grilleLengthExp = coll.Get(0);
            this.grillHeightExp = coll.Get(1);
            this.grillThicknessExp = coll.Get(2);
            this.beamLengthExp = coll.Get(3);
            this.beamWidthExp = coll.Get(4);
            this.edgeSizeExp = coll.Get(5);
            this.showEdgeExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valGrilleLength = 0;
            double valGrillHeight = 0;
            double valGrillThickness = 0;

            double valBeamLength = 0;
            double valBeamWidth = 0;
            double valEdgeSize = 0;
            bool valShowEdge = false;

            if (
               EvalExpression(grilleLengthExp, ref valGrilleLength, "GrilleLength", "MakeHoneyCombGrille") &&
               EvalExpression(grillHeightExp, ref valGrillHeight, "GrillHeight", "MakeHoneyCombGrille") &&
               EvalExpression(grillThicknessExp, ref valGrillThickness, "GrillThickness", "MakeHoneyCombGrille") &&
               EvalExpression(beamLengthExp, ref valBeamLength, "BeamLength", "MakeHoneyCombGrille") &&
               EvalExpression(beamWidthExp, ref valBeamWidth, "BeamWidth", "MakeHoneyCombGrille") &&
               EvalExpression(edgeSizeExp, ref valEdgeSize, "EdgeSize", "MakeHoneyCombGrille") &&
               EvalExpression(showEdgeExp, ref valShowEdge, "ShowEdge", "MakeHoneyCombGrille"))
            {
                HoneyCombGrilleMaker maker = new HoneyCombGrilleMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "GrilleLength", valGrilleLength) && inRange;
                inRange = RangeCheck(maker, "GrillHeight", valGrillHeight) && inRange;
                inRange = RangeCheck(maker, "GrillThickness", valGrillThickness) && inRange;
                inRange = RangeCheck(maker, "BeamLength", valBeamLength) && inRange;
                inRange = RangeCheck(maker, "EdgeSize", valEdgeSize) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "HoneyCombGrille";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valGrilleLength, valGrillHeight, valGrillThickness, valBeamLength, valBeamWidth, valEdgeSize, valShowEdge);

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
            this.grilleLengthExp = coll.Get(0);
            this.grillHeightExp = coll.Get(1);
            this.grillThicknessExp = coll.Get(2);
            this.beamLengthExp = coll.Get(3);
            this.edgeSizeExp = coll.Get(4);
            this.showEdgeExp = coll.Get(5);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += grilleLengthExp.ToRichText() + ", ";
            result += grillHeightExp.ToRichText() + ", ";
            result += grillThicknessExp.ToRichText() + ", ";
            result += beamLengthExp.ToRichText() + ", ";
            result += edgeSizeExp.ToRichText() + ", ";
            result += showEdgeExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += grilleLengthExp.ToString() + ", ";
            result += grillHeightExp.ToString() + ", ";
            result += grillThicknessExp.ToString() + ", ";
            result += beamLengthExp.ToString() + ", ";
            result += edgeSizeExp.ToString() + ", ";
            result += showEdgeExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(HoneyCombGrilleMaker maker, string paramName, double val)
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