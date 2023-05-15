using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeReuleauxNode : ExpressionNode
    {
        private ExpressionNode numberOfSidesExp;
        private ExpressionNode radiusExp;
        private ExpressionNode thicknessExp;

        public MakeReuleauxNode(ExpressionNode numberOfSides, ExpressionNode radius, ExpressionNode thickness)
        {
            this.numberOfSidesExp = numberOfSides;
            this.radiusExp = radius;
            this.thicknessExp = thickness;
        }

        public MakeReuleauxNode(ExpressionCollection coll)
        {
            this.numberOfSidesExp = coll.Get(0);
            this.radiusExp = coll.Get(1);
            this.thicknessExp = coll.Get(2);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                int valNumberOfSides = 0; double valRadius = 0; double valThickness = 0;

                if (

                           EvalExpression(numberOfSidesExp, ref valNumberOfSides, "NumberOfSides", "MakeRuleaux") &&
                           EvalExpression(radiusExp, ref valRadius, "Radius", "MakeRuleaux") &&
                           EvalExpression(thicknessExp, ref valThickness, "Thickness", "MakeRuleaux")
                   )
                {
                    // check calculated values are in range
                    bool inRange = true;

                    if (valNumberOfSides < 3 || valNumberOfSides > 7)
                    {
                        Log.Instance().AddEntry("MakeReuleaux : NumberOfSides value out of range (3..7)");
                        inRange = false;
                    }

                    if (valRadius < 1 || valRadius > 100)
                    {
                        Log.Instance().AddEntry("MakeReuleaux : Radius value out of range (1..100)");
                        inRange = false;
                    }

                    if (valThickness < 0.1 || valThickness > 100)
                    {
                        Log.Instance().AddEntry("MakeReuleaux : Thickness value out of range (0.1..100)");
                        inRange = false;
                    }

                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "Reuleaux";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        ReuleauxMaker maker = new ReuleauxMaker(valNumberOfSides, valRadius, valThickness);

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
                        Log.Instance().AddEntry("MakeReuleaux : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeReuleaux : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeReuleaux") + "( ";

            result += numberOfSidesExp.ToRichText() + ", ";
            result += radiusExp.ToRichText() + ", ";
            result += thicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeReuleaux( ";

            result += numberOfSidesExp.ToString() + ", ";
            result += radiusExp.ToString() + ", ";
            result += thicknessExp.ToString();
            result += " )";
            return result;
        }
    }
}