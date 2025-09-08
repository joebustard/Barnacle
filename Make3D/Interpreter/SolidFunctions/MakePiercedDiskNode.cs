using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Reflection.Emit;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePiercedDiskNode : ExpressionNode
    {
        private static string label = "MakePiercedDisk";
        private ExpressionNode diskHeightExp;
        private ExpressionNode diskRadiusExp;
        private ExpressionNode holeDistanceFromEdgeExp;
        private ExpressionNode holeRadiusExp;
        private ExpressionNode numberOfHolesExp;

        public MakePiercedDiskNode()
        {
        }

        public MakePiercedDiskNode
            (
            ExpressionNode diskRadius, ExpressionNode holeRadius, ExpressionNode holeDistanceFromEdge, ExpressionNode numberOfHoles, ExpressionNode diskHeight
            )
        {
            this.diskRadiusExp = diskRadius;
            this.holeRadiusExp = holeRadius;
            this.holeDistanceFromEdgeExp = holeDistanceFromEdge;
            this.numberOfHolesExp = numberOfHoles;
            this.diskHeightExp = diskHeight;
        }

        public MakePiercedDiskNode
                (ExpressionCollection coll)
        {
            this.diskRadiusExp = coll.Get(0);
            this.holeRadiusExp = coll.Get(1);
            this.holeDistanceFromEdgeExp = coll.Get(2);
            this.numberOfHolesExp = coll.Get(3);
            this.diskHeightExp = coll.Get(4);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valDiskRadius = 0; double valHoleRadius = 0; double valHoleDistanceFromEdge = 0; double valNumberOfHoles = 0; double valDiskHeight = 0;

            if (
               EvalExpression(diskRadiusExp, ref valDiskRadius, "DiskRadius", "MakePiercedDisk") &&
               EvalExpression(holeRadiusExp, ref valHoleRadius, "HoleRadius", "MakePiercedDisk") &&
               EvalExpression(holeDistanceFromEdgeExp, ref valHoleDistanceFromEdge, "HoleDistanceFromEdge", "MakePiercedDisk") &&
               EvalExpression(numberOfHolesExp, ref valNumberOfHoles, "NumberOfHoles", "MakePiercedDisk") &&
               EvalExpression(diskHeightExp, ref valDiskHeight, "DiskHeight", "MakePiercedDisk"))
            {
                PiercedDiskMaker maker = new PiercedDiskMaker();
                // check calculated values are in range
                bool inRange = true;

                inRange = RangeCheck(maker, "DiskRadius", valDiskRadius) && inRange;
                inRange = RangeCheck(maker, "HoleRadius", valHoleRadius) && inRange;
                inRange = RangeCheck(maker, "HoleDistanceFromEdge", valHoleDistanceFromEdge) && inRange;
                inRange = RangeCheck(maker, "NumberOfHoles", valNumberOfHoles) && inRange;
                inRange = RangeCheck(maker, "DiskHeight", valDiskHeight) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "PiercedDisk";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valDiskRadius, valHoleRadius, valHoleDistanceFromEdge, valNumberOfHoles, valDiskHeight);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    obj.CalculateAbsoluteBounds();
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
            this.diskRadiusExp = coll.Get(0);
            this.holeRadiusExp = coll.Get(1);
            this.holeDistanceFromEdgeExp = coll.Get(2);
            this.numberOfHolesExp = coll.Get(3);
            this.diskHeightExp = coll.Get(4);
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";

            result += diskRadiusExp.ToRichText() + ", ";
            result += holeRadiusExp.ToRichText() + ", ";
            result += holeDistanceFromEdgeExp.ToRichText() + ", ";
            result += numberOfHolesExp.ToRichText() + ", ";
            result += diskHeightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";

            result += diskRadiusExp.ToString() + ", ";
            result += holeRadiusExp.ToString() + ", ";
            result += holeDistanceFromEdgeExp.ToString() + ", ";
            result += numberOfHolesExp.ToString() + ", ";
            result += diskHeightExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(PiercedDiskMaker maker, string paramName, double val)
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