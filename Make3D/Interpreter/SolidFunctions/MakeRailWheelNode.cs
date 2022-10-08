using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeRailWheelNode : ExpressionNode
    {
        private ExpressionNode axleBoreExp;
        private ExpressionNode flangeRadiusExp;
        private ExpressionNode flangeThicknessExp;
        private ExpressionNode hubRadiusExp;
        private ExpressionNode hubThicknessExp;
        private ExpressionNode mainRadiusExp;
        private ExpressionNode mainThicknessExp;

        public MakeRailWheelNode(ExpressionNode mainRadius, ExpressionNode mainThickness, ExpressionNode flangeRadius, ExpressionNode flangeThickness, ExpressionNode hubRadius, ExpressionNode hubThickness, ExpressionNode axleBore)
        {
            this.mainRadiusExp = mainRadius;
            this.mainThicknessExp = mainThickness;
            this.flangeRadiusExp = flangeRadius;
            this.flangeThicknessExp = flangeThickness;
            this.hubRadiusExp = hubRadius;
            this.hubThicknessExp = hubThickness;
            this.axleBoreExp = axleBore;
        }

        public MakeRailWheelNode(ExpressionCollection coll)
        {
            this.mainRadiusExp = coll.Get(0);
            this.mainThicknessExp = coll.Get(1);
            this.flangeRadiusExp = coll.Get(2);
            this.flangeThicknessExp = coll.Get(3);
            this.hubRadiusExp = coll.Get(4);
            this.hubThicknessExp = coll.Get(5);
            this.axleBoreExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double valMainRadius = 0;
                double valMainThickness = 0;
                double valFlangeRadius = 0;
                double valFlangeThickness = 0;
                double valHubRadius = 0;
                double valHubThickness = 0;
                double valAxleBore = 0;

                if (EvalExpression(mainRadiusExp, ref valMainRadius, "MainRadius", "MakeRailWheel") &&
                    EvalExpression(mainThicknessExp, ref valMainThickness, "MainThickness", "MakeRailWheel") &&
                    EvalExpression(flangeRadiusExp, ref valFlangeRadius, "FlangeRadius", "MakeRailWheel") &&
                    EvalExpression(flangeThicknessExp, ref valFlangeThickness, "FlangeThickness", "MakeRailWheel") &&
                    EvalExpression(hubRadiusExp, ref valHubRadius, "HubRadius", "MakeRailWheel") &&
                    EvalExpression(hubThicknessExp, ref valHubThickness, "HubThickness", "MakeRailWheel") &&
                    EvalExpression(axleBoreExp, ref valAxleBore, "AxleBore", "MakeRailWheel")
                   )
                {
                    // check calculated values are in range
                    bool inRange = true;

                    if (valMainRadius < 1 || valMainRadius > 100)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : MainRadius value out of range (1..100)");
                        inRange = false;
                    }

                    if (valMainThickness < 1 || valMainThickness > 100)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : MainThickness value out of range (1..100)");
                        inRange = false;
                    }

                    if (valFlangeRadius < 1 || valFlangeRadius > 10)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : FlangeRadius value out of range (1..10)");
                        inRange = false;
                    }

                    if (valFlangeThickness < 1 || valFlangeThickness > 10)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : FlangeThickness value out of range (1..10)");
                        inRange = false;
                    }

                    if (valHubRadius < 0 || valHubRadius > 10)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : HubRadius value out of range (0..10)");
                        inRange = false;
                    }

                    if (valHubThickness < 0 || valHubThickness > 10)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : HubThickness value out of range (0..10)");
                        inRange = false;
                    }

                    if (valAxleBore < 1 || valAxleBore > 100)
                    {
                        Log.Instance().AddEntry("MakeRailWheel : AxleBore value out of range (1..100)");
                        inRange = false;
                    }
                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "RailWheel";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        RailWheelMaker maker = new RailWheelMaker(valMainRadius, valMainThickness, valFlangeRadius, valFlangeThickness, valHubRadius, valHubThickness, valAxleBore);

                        maker.Generate(tmp, obj.TriangleIndices);
                        PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                        obj.CalcScale(false);
                        obj.Remesh();
                        Script.ResultArtefacts.Add(obj);
                        ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                    }
                    else
                    {
                        Log.Instance().AddEntry("MakeRailWheel : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeRailWheel : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeRailWheel") + "( ";

            result += mainRadiusExp.ToRichText() + ", ";
            result += mainThicknessExp.ToRichText() + ", ";
            result += flangeRadiusExp.ToRichText() + ", ";
            result += flangeThicknessExp.ToRichText() + ", ";
            result += hubRadiusExp.ToRichText() + ", ";
            result += hubThicknessExp.ToRichText() + ", ";
            result += axleBoreExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeRailWheel( ";

            result += mainRadiusExp.ToString() + ", ";
            result += mainThicknessExp.ToString() + ", ";
            result += flangeRadiusExp.ToString() + ", ";
            result += flangeThicknessExp.ToString() + ", ";
            result += hubRadiusExp.ToString() + ", ";
            result += hubThicknessExp.ToString() + ", ";
            result += axleBoreExp.ToString();
            result += " )";
            return result;
        }
    }
}