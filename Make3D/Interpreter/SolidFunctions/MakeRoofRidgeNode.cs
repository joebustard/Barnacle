using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeRoofRidgeNode : ExpressionNode
    {
        private ExpressionNode armAngleExp;
        private ExpressionNode armLengthExp;
        private ExpressionNode armThicknessExp;
        private ExpressionNode crownRadiusExp;
        private ExpressionNode flatCrestWidthExp;
        private ExpressionNode ridgeLengthExp;
        private ExpressionNode shapeExp;


        public MakeRoofRidgeNode
            (
            ExpressionNode armLength, ExpressionNode armAngle, ExpressionNode armThickness, ExpressionNode ridgeLength, ExpressionNode crownRadius, ExpressionNode flatCrestWidth, ExpressionNode shape
            )
        {
            this.armLengthExp = armLength;
            this.armAngleExp = armAngle;
            this.armThicknessExp = armThickness;
            this.ridgeLengthExp = ridgeLength;
            this.crownRadiusExp = crownRadius;
            this.flatCrestWidthExp = flatCrestWidth;
            this.shapeExp = shape;

        }

        public MakeRoofRidgeNode
                (ExpressionCollection coll)
        {
            this.armLengthExp = coll.Get(0);
            this.armAngleExp = coll.Get(1);
            this.armThicknessExp = coll.Get(2);
            this.ridgeLengthExp = coll.Get(3);
            this.crownRadiusExp = coll.Get(4);
            this.flatCrestWidthExp = coll.Get(5);
            this.shapeExp = coll.Get(6);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valArmLength = 0; 
            double valArmAngle = 0; 
            double valCrownRadius = 0; 
            double valFlatCrestWidth = 0; 
            int valShape = 0;
            double valRidgeLength = 0;
            double valArmThickness = 0;

            if (
               EvalExpression(armLengthExp, ref valArmLength, "ArmLength", "MakeRoofRidge") &&
               EvalExpression(armAngleExp, ref valArmAngle, "ArmAngle", "MakeRoofRidge") &&
               EvalExpression(armThicknessExp, ref valArmThickness, "ArmThickness", "MakeRoofRidge") &&
               EvalExpression(ridgeLengthExp, ref valRidgeLength, "RidgeLength", "MakeRoofRidge") &&

               EvalExpression(crownRadiusExp, ref valCrownRadius, "CrownRadius", "MakeRoofRidge") &&
               EvalExpression(flatCrestWidthExp, ref valFlatCrestWidth, "FlatCrestWidth", "MakeRoofRidge") &&
               EvalExpression(shapeExp, ref valShape, "Shape", "MakeRoofRidge"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valArmLength < 0.1 || valArmLength > 100)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : ArmLength value out of range (0.1..100)");
                    inRange = false;
                }

                if (valArmAngle < 0 || valArmAngle > 180)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : ArmAngle value out of range (0..180)");
                    inRange = false;
                }

                if (valArmThickness < 1 || valArmThickness > 10)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : ArmThickness value out of range (1..10)");
                    inRange = false;
                }
                if (valRidgeLength < 0.1 || valRidgeLength > 200)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : RidgeLength value out of range (0.1..200)");
                    inRange = false;
                }
                if (valArmAngle < 0 || valArmAngle > 180)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : ArmAngle value out of range (0..180)");
                    inRange = false;
                }
                if (valCrownRadius < 0.1 || valCrownRadius > 100)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : CrownRadius value out of range (0.1..100)");
                    inRange = false;
                }

                if (valFlatCrestWidth < 0.1 || valFlatCrestWidth > 100)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : FlatCrestWidth value out of range (0.1..100)");
                    inRange = false;
                }

                if (valShape < 0 || valShape > 4)
                {
                    Log.Instance().AddEntry("MakeRoofRidge : Shape value out of range (0..4)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "RoofRidge";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    RoofRidgeMaker maker = new RoofRidgeMaker(valArmLength, valArmAngle,valArmThickness,valRidgeLength, valCrownRadius, valFlatCrestWidth, valShape);

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
                    Log.Instance().AddEntry("MakeRoofRidge : Illegal value");
                }
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeRoofRidge") + "( ";

            result += armLengthExp.ToRichText() + ", ";
            result += armAngleExp.ToRichText() + ", ";
            result += crownRadiusExp.ToRichText() + ", ";
            result += flatCrestWidthExp.ToRichText() + ", ";
            result += shapeExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeRoofRidge( ";

            result += armLengthExp.ToString() + ", ";
            result += armAngleExp.ToString() + ", ";
            result += crownRadiusExp.ToString() + ", ";
            result += flatCrestWidthExp.ToString() + ", ";
            result += shapeExp.ToString();
            result += " )";
            return result;
        }
    }
}
