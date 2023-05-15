using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeWagonWheelNode : ExpressionNode
    {
        private ExpressionNode hubRadiusExp;
        private ExpressionNode hubThicknessExp;
        private ExpressionNode rimInnerRadiusExp;
        private ExpressionNode rimThicknessExp;
        private ExpressionNode rimDepthExp;
        private ExpressionNode numberOfSpokesExp;
        private ExpressionNode spokeRadiusExp;
        private ExpressionNode axleBoreExp;

        public MakeWagonWheelNode(ExpressionNode hubRadius, ExpressionNode hubThickness, ExpressionNode rimInnerRadius, ExpressionNode rimThickness, ExpressionNode rimDepth, ExpressionNode numberOfSpokes, ExpressionNode spokeRadius, ExpressionNode axleBore)
        {
            this.hubRadiusExp = hubRadius;
            this.hubThicknessExp = hubThickness;
            this.rimInnerRadiusExp = rimInnerRadius;
            this.rimThicknessExp = rimThickness;
            this.rimDepthExp = rimDepth;
            this.numberOfSpokesExp = numberOfSpokes;
            this.spokeRadiusExp = spokeRadius;
            this.axleBoreExp = axleBore;
        }

        public MakeWagonWheelNode(ExpressionCollection coll)
        {
            this.hubRadiusExp = coll.Get(0);
            this.hubThicknessExp = coll.Get(1);
            this.rimInnerRadiusExp = coll.Get(2);
            this.rimThicknessExp = coll.Get(3);
            this.rimDepthExp = coll.Get(4);
            this.numberOfSpokesExp = coll.Get(5);
            this.spokeRadiusExp = coll.Get(6);
            this.axleBoreExp = coll.Get(7);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double valHubRadius = 0;
                double valHubThickness = 0;
                double valRimInnerRadius = 0;
                double valRimThickness = 0;
                double valRimDepth = 0;
                double valNumberOfSpokes = 0;
                double valSpokeRadius = 0;
                double valAxleBore = 0;

                if (
                           EvalExpression(hubRadiusExp, ref valHubRadius, "HubRadius", "MakeWagonWheel") &&
                           EvalExpression(hubThicknessExp, ref valHubThickness, "HubThickness", "MakeWagonWheel") &&
                           EvalExpression(rimInnerRadiusExp, ref valRimInnerRadius, "RimInnerRadius", "MakeWagonWheel") &&
                           EvalExpression(rimThicknessExp, ref valRimThickness, "RimThickness", "MakeWagonWheel") &&
                           EvalExpression(rimDepthExp, ref valRimDepth, "RimDepth", "MakeWagonWheel") &&
                           EvalExpression(numberOfSpokesExp, ref valNumberOfSpokes, "NumberOfSpokes", "MakeWagonWheel") &&
                           EvalExpression(spokeRadiusExp, ref valSpokeRadius, "SpokeRadius", "MakeWagonWheel") &&
                           EvalExpression(axleBoreExp, ref valAxleBore, "AxleBore", "MakeWagonWheel")
                   )
                {
                    // check calculated values are in range
                    bool inRange = true;

                    if (valHubRadius < 1 || valHubRadius > 20)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : HubRadius value out of range (1..20)");
                        inRange = false;
                    }

                    if (valHubThickness < 1 || valHubThickness > 20)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : HubThickness value out of range (1..20)");
                        inRange = false;
                    }

                    if (valRimInnerRadius < 1 || valRimInnerRadius > 20)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : RimInnerRadius value out of range (1..20)");
                        inRange = false;
                    }

                    if (valRimThickness < 1 || valRimThickness > 20)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : RimThickness value out of range (1..20)");
                        inRange = false;
                    }

                    if (valRimDepth < 1 || valRimDepth > 20)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : RimDepth value out of range (1..20)");
                        inRange = false;
                    }

                    if (valNumberOfSpokes < 4 || valNumberOfSpokes > 10)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : NumberOfSpokes value out of range (4..10)");
                        inRange = false;
                    }

                    if (valSpokeRadius < 1 || valSpokeRadius > 10)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : SpokeRadius value out of range (1..10)");
                        inRange = false;
                    }

                    if (valAxleBore < 1 || valAxleBore > 10)
                    {
                        Log.Instance().AddEntry("MakeWagonWheel : AxleBore value out of range (1..10)");
                        inRange = false;
                    }

                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "WagonWheel";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        WagonWheelMaker maker = new WagonWheelMaker(valHubRadius, valHubThickness, valRimInnerRadius, valRimThickness, valRimDepth, valNumberOfSpokes, valSpokeRadius, valAxleBore);

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
                        Log.Instance().AddEntry("MakeWagonWheel : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeWagonWheel : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeWagonWheel") + "( ";

            result += hubRadiusExp.ToRichText() + ", ";
            result += hubThicknessExp.ToRichText() + ", ";
            result += rimInnerRadiusExp.ToRichText() + ", ";
            result += rimThicknessExp.ToRichText() + ", ";
            result += rimDepthExp.ToRichText() + ", ";
            result += numberOfSpokesExp.ToRichText() + ", ";
            result += spokeRadiusExp.ToRichText() + ", ";
            result += axleBoreExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeWagonWheel( ";

            result += hubRadiusExp.ToString() + ", ";
            result += hubThicknessExp.ToString() + ", ";
            result += rimInnerRadiusExp.ToString() + ", ";
            result += rimThicknessExp.ToString() + ", ";
            result += rimDepthExp.ToString() + ", ";
            result += numberOfSpokesExp.ToString() + ", ";
            result += spokeRadiusExp.ToString() + ", ";
            result += axleBoreExp.ToString();
            result += " )";
            return result;
        }
    }
}