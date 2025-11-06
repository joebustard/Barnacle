using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBevelledGearNode : ExpressionNode
    {
        private static string label = "MakeBevelledGear";
        private ExpressionNode baseRadiusExp;
        private ExpressionNode baseThicknessExp;
        private ExpressionNode boreHoleRadiusExp;
        private ExpressionNode fillBaseExp;
        private ExpressionNode gearHeightExp;
        private ExpressionNode numberOfTeethExp;
        private ExpressionNode toothLengthExp;

        public MakeBevelledGearNode()
        {
        }

        public MakeBevelledGearNode(ExpressionNode baseRadius,
                                    ExpressionNode gearHeight,
                                    ExpressionNode toothLength,
                                    ExpressionNode numberOfTeeth,
                                    ExpressionNode boreHoleRadius,
                                    ExpressionNode baseThickness,
                                    ExpressionNode fillBase)
        {
            this.baseRadiusExp = baseRadius;
            this.gearHeightExp = gearHeight;
            this.toothLengthExp = toothLength;
            this.numberOfTeethExp = numberOfTeeth;
            this.boreHoleRadiusExp = boreHoleRadius;
            this.baseThicknessExp = baseThickness;
            this.fillBaseExp = fillBase;
        }

        public MakeBevelledGearNode(ExpressionCollection coll)
        {
            this.baseRadiusExp = coll.Get(0);
            this.gearHeightExp = coll.Get(1);
            this.toothLengthExp = coll.Get(2);
            this.numberOfTeethExp = coll.Get(3);
            this.boreHoleRadiusExp = coll.Get(4);
            this.baseThicknessExp = coll.Get(5);
            this.fillBaseExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valBaseRadius = 0; double valGearHeight = 0; double valToothLength = 0; int valNumberOfTeeth = 0; double valBoreHoleRadius = 0; double valBaseThickness = 0; bool valFillBase = false;

            if (
               EvalExpression(baseRadiusExp, ref valBaseRadius, "BaseRadius", "MakeBevelledGear") &&
               EvalExpression(gearHeightExp, ref valGearHeight, "GearHeight", "MakeBevelledGear") &&
               EvalExpression(toothLengthExp, ref valToothLength, "ToothLength", "MakeBevelledGear") &&
               EvalExpression(numberOfTeethExp, ref valNumberOfTeeth, "NumberOfTeeth", "MakeBevelledGear") &&
               EvalExpression(boreHoleRadiusExp, ref valBoreHoleRadius, "BoreHoleRadius", "MakeBevelledGear") &&
               EvalExpression(baseThicknessExp, ref valBaseThickness, "BaseThickness", "MakeBevelledGear") &&
               EvalExpression(fillBaseExp, ref valFillBase, "FillBase", "MakeBevelledGear"))
            {
                BevelledGearMaker maker = new BevelledGearMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "BaseRadius", valBaseRadius) && inRange;
                inRange = RangeCheck(maker, "GearHeight", valGearHeight) && inRange;
                inRange = RangeCheck(maker, "ToothLength", valToothLength) && inRange;
                inRange = RangeCheck(maker, "NumberOfTeeth", valNumberOfTeeth) && inRange;
                inRange = RangeCheck(maker, "BoreHoleRadius", valBoreHoleRadius) && inRange;
                inRange = RangeCheck(maker, "BaseThickness", valBaseThickness) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "BevelledGear";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valBaseRadius, valGearHeight, valToothLength, valNumberOfTeeth, valBoreHoleRadius, valBaseThickness, valFillBase);

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
            this.baseRadiusExp = coll.Get(0);
            this.gearHeightExp = coll.Get(1);
            this.toothLengthExp = coll.Get(2);
            this.numberOfTeethExp = coll.Get(3);
            this.boreHoleRadiusExp = coll.Get(4);
            this.baseThicknessExp = coll.Get(5);
            this.fillBaseExp = coll.Get(6);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += baseRadiusExp.ToRichText() + ", ";
            result += gearHeightExp.ToRichText() + ", ";
            result += toothLengthExp.ToRichText() + ", ";
            result += numberOfTeethExp.ToRichText() + ", ";
            result += boreHoleRadiusExp.ToRichText() + ", ";
            result += baseThicknessExp.ToRichText() + ", ";
            result += fillBaseExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += baseRadiusExp.ToString() + ", ";
            result += gearHeightExp.ToString() + ", ";
            result += toothLengthExp.ToString() + ", ";
            result += numberOfTeethExp.ToString() + ", ";
            result += boreHoleRadiusExp.ToString() + ", ";
            result += baseThicknessExp.ToString() + ", ";
            result += fillBaseExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(BevelledGearMaker maker, string paramName, double val)
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