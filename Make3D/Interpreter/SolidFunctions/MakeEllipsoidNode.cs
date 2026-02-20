using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeEllipsoidNode : ExpressionNode
    {
        private static string label = "MakeEllipsoid";
        private ExpressionNode halfExp;
        private ExpressionNode leftLengthExp;
        private ExpressionNode rightLengthExp;
        private ExpressionNode shapeHeightExp;
        private ExpressionNode widthExp;

        public MakeEllipsoidNode()
        {
        }

        public MakeEllipsoidNode(ExpressionNode rightLength,
                                 ExpressionNode leftLength,
                                 ExpressionNode shapeHeight,
                                 ExpressionNode width,
                                 ExpressionNode half)
        {
            this.rightLengthExp = rightLength;
            this.leftLengthExp = leftLength;
            this.shapeHeightExp = shapeHeight;
            this.widthExp = width;
            this.halfExp = half;
        }

        public MakeEllipsoidNode(ExpressionCollection coll)
        {
            this.rightLengthExp = coll.Get(0);
            this.leftLengthExp = coll.Get(1);
            this.shapeHeightExp = coll.Get(2);
            this.widthExp = coll.Get(3);
            this.halfExp = coll.Get(4);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            double valRightLength = 0;
            double valLeftLength = 0;
            double valShapeHeight = 0;
            double valWidth = 0;
            bool valHalf = false;

            if (EvalExpression(rightLengthExp, ref valRightLength, "RightLength", "MakeEllipsoid") &&
               EvalExpression(leftLengthExp, ref valLeftLength, "LeftLength", "MakeEllipsoid") &&
               EvalExpression(shapeHeightExp, ref valShapeHeight, "ShapeHeight", "MakeEllipsoid") &&
               EvalExpression(widthExp, ref valWidth, "Width", "MakeEllipsoid") &&
               EvalExpression(halfExp, ref valHalf, "Half", "MakeEllipsoid"))
            {
                EllipsoidMaker maker = new EllipsoidMaker();

                // check calculated values are in range
                bool inRange = true;
                inRange = RangeCheck(maker, "RightLength", valRightLength) && inRange;
                inRange = RangeCheck(maker, "LeftLength", valLeftLength) && inRange;
                inRange = RangeCheck(maker, "ShapeHeight", valShapeHeight) && inRange;
                inRange = RangeCheck(maker, "ShapeWidth", valWidth) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Ellipsoid";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valRightLength, valLeftLength, valShapeHeight, valWidth, valHalf);

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
            this.rightLengthExp = coll.Get(0);
            this.leftLengthExp = coll.Get(1);
            this.shapeHeightExp = coll.Get(2);
            this.widthExp = coll.Get(3);
            this.halfExp = coll.Get(4);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += rightLengthExp.ToRichText() + ", ";
            result += leftLengthExp.ToRichText() + ", ";
            result += shapeHeightExp.ToRichText() + ", ";
            result += widthExp.ToRichText() + ", ";
            result += halfExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += rightLengthExp.ToString() + ", ";
            result += leftLengthExp.ToString() + ", ";
            result += shapeHeightExp.ToString() + ", ";
            result += widthExp.ToString() + ", ";
            result += halfExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(EllipsoidMaker maker, string paramName, double val)
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