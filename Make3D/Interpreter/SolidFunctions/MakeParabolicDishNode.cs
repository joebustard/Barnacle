using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeParabolicDishNode : ExpressionNode
    {
        private ExpressionNode radiusExp;
        private ExpressionNode wallThicknessExp;
        private ExpressionNode pitchExp;

        public MakeParabolicDishNode(ExpressionNode radius, ExpressionNode wallThickness, ExpressionNode pitch)
        {
            this.radiusExp = radius;
            this.wallThicknessExp = wallThickness;
            this.pitchExp = pitch;
        }

        public MakeParabolicDishNode(ExpressionCollection coll)
        {
            this.radiusExp = coll.Get(0);
            this.wallThicknessExp = coll.Get(1);
            this.pitchExp = coll.Get(2);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double valRadius = 0; double valWallThickness = 0; int valPitch = 0;

                if (

                           EvalExpression(radiusExp, ref valRadius, "Radius", "MakeParabolicDish") &&
                           EvalExpression(wallThicknessExp, ref valWallThickness, "WallThickness", "MakeParabolicDish") &&
                           EvalExpression(pitchExp, ref valPitch, "Pitch", "MakeParabolicDish")
                   )
                {
                    // check calculated values are in range
                    bool inRange = true;

                    if (valRadius < 0.1 || valRadius > 100)
                    {
                        Log.Instance().AddEntry("MakeParabolicDish : Radius value out of range (0.1..100)");
                        inRange = false;
                    }

                    if (valWallThickness < 0.5 || valWallThickness > 99.5)
                    {
                        Log.Instance().AddEntry("MakeParabolicDish : WallThickness value out of range (0.5..99.5)");
                        inRange = false;
                    }

                    if (valPitch < 1 || valPitch > 20)
                    {
                        Log.Instance().AddEntry("MakeParabolicDish : Pitch value out of range (1..20)");
                        inRange = false;
                    }

                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "ParabolicDish";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        ParabolicDishMaker maker = new ParabolicDishMaker(valRadius, valWallThickness, valPitch);

                        maker.Generate(tmp, obj.TriangleIndices);
                        PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                        obj.CalcScale(false);
                        obj.Remesh();
                        Script.ResultArtefacts.Add(obj);
                        ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                    }
                    else
                    {
                        Log.Instance().AddEntry("MakeParabolicDish : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeParabolicDish : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeParabolicDish") + "( ";

            result += radiusExp.ToRichText() + ", ";
            result += wallThicknessExp.ToRichText() + ", ";
            result += pitchExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeParabolicDish( ";

            result += radiusExp.ToString() + ", ";
            result += wallThicknessExp.ToString() + ", ";
            result += pitchExp.ToString();
            result += " )";
            return result;
        }
    }
}