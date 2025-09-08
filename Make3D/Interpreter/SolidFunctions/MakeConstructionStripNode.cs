using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeConstructionStripNode : ExpressionNode
    {
        private ExpressionNode holeRadiusExp;
        private ExpressionNode numberOfHolesExp;
        private ExpressionNode stripHeightExp;
        private ExpressionNode stripRepeatsExp;
        private ExpressionNode stripWidthExp;

        public MakeConstructionStripNode(ExpressionNode stripHeight, ExpressionNode stripWidth, ExpressionNode stripRepeats, ExpressionNode holeRadius, ExpressionNode numberOfHoles)
        {
            this.stripHeightExp = stripHeight;
            this.stripWidthExp = stripWidth;
            this.stripRepeatsExp = stripRepeats;
            this.holeRadiusExp = holeRadius;
            this.numberOfHolesExp = numberOfHoles;
        }

        public MakeConstructionStripNode(ExpressionCollection coll)
        {
            this.stripHeightExp = coll.Get(0);
            this.stripWidthExp = coll.Get(1);
            this.stripRepeatsExp = coll.Get(2);
            this.holeRadiusExp = coll.Get(3);
            this.numberOfHolesExp = coll.Get(4);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valStripHeight = 0;
            double valStripWidth = 0;
            int valStripRepeats = 0;
            double valHoleRadius = 0;
            int valNumberOfHoles = 0;

            if (EvalExpression(stripHeightExp, ref valStripHeight, "StripHeight", "MakeConstructionStrip") &&
               EvalExpression(stripWidthExp, ref valStripWidth, "StripWidth", "MakeConstructionStrip") &&
               EvalExpression(stripRepeatsExp, ref valStripRepeats, "StripRepeats", "MakeConstructionStrip") &&
               EvalExpression(holeRadiusExp, ref valHoleRadius, "HoleRadius", "MakeConstructionStrip") &&
               EvalExpression(numberOfHolesExp, ref valNumberOfHoles, "NumberOfHoles", "MakeConstructionStrip"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valStripHeight < 1 || valStripHeight > 10)
                {
                    Log.Instance().AddEntry("MakeConstructionStrip : StripHeight value out of range (1..10)");
                    inRange = false;
                }

                if (valStripWidth < 5 || valStripWidth > 100)
                {
                    Log.Instance().AddEntry("MakeConstructionStrip : StripWidth value out of range (5..100)");
                    inRange = false;
                }

                if (valStripRepeats < 1 || valStripRepeats > 20)
                {
                    Log.Instance().AddEntry("MakeConstructionStrip : StripRepeats value out of range (1..20)");
                    inRange = false;
                }

                if (valHoleRadius < 2 || valHoleRadius > 98)
                {
                    Log.Instance().AddEntry("MakeConstructionStrip : HoleRadius value out of range (2..98)");
                    inRange = false;
                }

                if (valNumberOfHoles < 2 || valNumberOfHoles > 20)
                {
                    Log.Instance().AddEntry("MakeConstructionStrip : NumberOfHoles value out of range (2..20)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "ConstructionStrip";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    ConstructionStripMaker maker = new ConstructionStripMaker(valStripHeight, valStripWidth, valStripRepeats, valHoleRadius, valNumberOfHoles);

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
                    Log.Instance().AddEntry("MakeConstructionStrip : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeConstructionStrip") + "( ";

            result += stripHeightExp.ToRichText() + ", ";
            result += stripWidthExp.ToRichText() + ", ";
            result += stripRepeatsExp.ToRichText() + ", ";
            result += holeRadiusExp.ToRichText() + ", ";
            result += numberOfHolesExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeConstructionStrip( ";

            result += stripHeightExp.ToString() + ", ";
            result += stripWidthExp.ToString() + ", ";
            result += stripRepeatsExp.ToString() + ", ";
            result += holeRadiusExp.ToString() + ", ";
            result += numberOfHolesExp.ToString();
            result += " )";
            return result;
        }
    }
}