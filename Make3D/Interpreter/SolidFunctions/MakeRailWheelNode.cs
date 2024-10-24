using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeRailWheelNode : ExpressionNode
    {
        private ExpressionNode flangeDiameterExp;
        private ExpressionNode flangeHeightExp;
        private ExpressionNode hubDiameterExp;
        private ExpressionNode hubHeightExp;
        private ExpressionNode upperRimDiameterExp;
        private ExpressionNode lowerRimDiameterExp;
        private ExpressionNode rimThicknessExp;
        private ExpressionNode rimHeightExp;
        private ExpressionNode axleBoreDiameterExp;

        public MakeRailWheelNode(ExpressionNode flangeDiameterExp,
                                 ExpressionNode flangeHeightExp,
                                 ExpressionNode hubDiameterExp,
                                 ExpressionNode hubHeightExp,
                                 ExpressionNode upperRimDiameterExp,
                                 ExpressionNode lowerRimDiameterExp,
                                 ExpressionNode rimThicknessExp,
                                 ExpressionNode rimHeightExp,
                                 ExpressionNode axleBoreDiameterExp
                                 )
        {
            this.flangeDiameterExp = flangeDiameterExp;
            this.flangeHeightExp = flangeHeightExp;
            this.hubDiameterExp = hubDiameterExp;
            this.hubHeightExp = hubHeightExp;
            this.upperRimDiameterExp = upperRimDiameterExp;
            this.lowerRimDiameterExp = lowerRimDiameterExp;
            this.rimThicknessExp = rimThicknessExp;
            this.rimHeightExp = rimHeightExp;
            this.axleBoreDiameterExp = axleBoreDiameterExp;
        }

        public MakeRailWheelNode(ExpressionCollection coll)
        {
            this.flangeDiameterExp = coll.Get(0);
            this.flangeHeightExp = coll.Get(1);
            this.hubDiameterExp = coll.Get(2);
            this.hubHeightExp = coll.Get(3);
            this.upperRimDiameterExp = coll.Get(4);
            this.lowerRimDiameterExp = coll.Get(5);
            this.rimThicknessExp = coll.Get(6);
            this.rimHeightExp = coll.Get(7);
            this.axleBoreDiameterExp = coll.Get(8);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            double vflangeDiameter = 0;
            double vflangeHeight = 0;
            double vhubDiameter = 0;
            double vhubHeight = 0;
            double vupperRimDiameter = 0;
            double vlowerRimDiameter = 0;
            double vRimThickness = 0;
            double vRimHeight = 0;
            double vAxleBoreDiameter = 0;
            try
            {
                if (EvalExpression(flangeDiameterExp, ref vflangeDiameter, "FlangeDiameter", "MakeRailWheel") &&
                    EvalExpression(flangeHeightExp, ref vflangeHeight, "FlangeHeight", "MakeRailWheel") &&
                    EvalExpression(hubDiameterExp, ref vhubDiameter, "HubDiameter", "MakeRailWheel") &&
                     EvalExpression(hubHeightExp, ref vhubHeight, "HubHeight", "MakeRailWheel") &&
                    EvalExpression(upperRimDiameterExp, ref vupperRimDiameter, "UpperRimDiameter", "MakeRailWheel") &&
                    EvalExpression(lowerRimDiameterExp, ref vlowerRimDiameter, "LowerRimDiameter", "MakeRailWheel") &&
                    EvalExpression(rimThicknessExp, ref vRimThickness, "RimThickness", "MakeRailWheel") &&
                    EvalExpression(rimHeightExp, ref vRimHeight, "RimHeight", "MakeRailWheel") &&
                    EvalExpression(axleBoreDiameterExp, ref vAxleBoreDiameter, "AxleBoreDiameter", "MakeRailWheel")
                   )
                {
                    RailWheelMaker maker = new RailWheelMaker();
                    // check calculated values are in range
                    bool inRange = true;
                    inRange = RangeCheck(maker, "FlangeDiameter", vflangeDiameter);
                    inRange = RangeCheck(maker, "FlangeHeight", vflangeHeight) && inRange;
                    inRange = RangeCheck(maker, "HubDiameter", vhubDiameter) && inRange;
                    inRange = RangeCheck(maker, "HubHeight", vhubHeight) && inRange;
                    inRange = RangeCheck(maker, "UpperRimDiameter", vupperRimDiameter) && inRange;
                    inRange = RangeCheck(maker, "LowerRimDiameter", vlowerRimDiameter) && inRange;
                    inRange = RangeCheck(maker, "RimThickness", vRimThickness) && inRange;
                    inRange = RangeCheck(maker, "RimHeight", vRimHeight) && inRange;
                    inRange = RangeCheck(maker, "AxleBoreDiameter", vAxleBoreDiameter) && inRange;

                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "RailWheel";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        maker.SetValues(vflangeDiameter,
                                        vflangeHeight,
                                        vhubDiameter,
                                        vhubHeight,
                                        vupperRimDiameter,
                                        vlowerRimDiameter,
                                        vRimThickness,
                                        vRimHeight,
                                        vAxleBoreDiameter);

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

        private static bool RangeCheck(RailWheelMaker maker, string paramName, double val)
        {
            bool inRange = maker.CheckLimits(paramName, val);
            if (!inRange)
            {
                ParamLimit pl = maker.GetLimits(paramName);
                if (pl != null)
                {
                    Log.Instance().AddEntry($"MakeRailWheel : {paramName} value {val} out of range ({pl.Low}..{pl.High}");
                }
                else
                {
                    Log.Instance().AddEntry($"MakeRailWheel : Can't check parameter {paramName}");
                }
            }

            return inRange;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeRailWheel") + "( ";

            result += flangeDiameterExp.ToRichText() + ", ";
            result += flangeHeightExp.ToRichText() + ", ";
            result += hubDiameterExp.ToRichText() + ", ";
            result += hubHeightExp.ToRichText() + ", ";
            result += upperRimDiameterExp.ToRichText() + ", ";
            result += lowerRimDiameterExp.ToRichText() + ", ";
            result += rimThicknessExp.ToRichText() + ", ";
            result += rimHeightExp.ToRichText() + ", ";
            result += axleBoreDiameterExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeRailWheel( ";

            result += flangeDiameterExp.ToString() + ", ";
            result += flangeHeightExp.ToString() + ", ";
            result += hubDiameterExp.ToString() + ", ";
            result += hubHeightExp.ToString() + ", ";
            result += upperRimDiameterExp.ToString() + ", ";
            result += lowerRimDiameterExp.ToString() + ", ";
            result += rimThicknessExp.ToString() + ", ";
            result += rimHeightExp.ToString() + ", ";
            result += axleBoreDiameterExp.ToString();
            return result;
        }
    }
}