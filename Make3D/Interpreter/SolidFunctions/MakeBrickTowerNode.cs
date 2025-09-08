using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBrickTowerNode : ExpressionNode
    {
        private ExpressionNode brickHeightExp;
        private ExpressionNode brickLengthExp;
        private ExpressionNode brickWidthExp;
        private ExpressionNode gapDepthExp;
        private ExpressionNode gapLengthExp;
        private ExpressionNode towerHeightExp;
        private ExpressionNode towerRadiusExp;

        public MakeBrickTowerNode
            (
            ExpressionNode brickLength, ExpressionNode brickHeight, ExpressionNode brickWidth, ExpressionNode gapLength, ExpressionNode gapDepth, ExpressionNode towerRadius, ExpressionNode towerHeight
            )
        {
            this.brickLengthExp = brickLength;
            this.brickHeightExp = brickHeight;
            this.brickWidthExp = brickWidth;
            this.gapLengthExp = gapLength;
            this.gapDepthExp = gapDepth;
            this.towerRadiusExp = towerRadius;
            this.towerHeightExp = towerHeight;
        }

        public MakeBrickTowerNode
                (ExpressionCollection coll)
        {
            this.brickLengthExp = coll.Get(0);
            this.brickHeightExp = coll.Get(1);
            this.brickWidthExp = coll.Get(2);
            this.gapLengthExp = coll.Get(3);
            this.gapDepthExp = coll.Get(4);
            this.towerRadiusExp = coll.Get(5);
            this.towerHeightExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valBrickLength = 0; double valBrickHeight = 0; double valBrickWidth = 0; double valGapLength = 0; double valGapDepth = 0; double valTowerRadius = 0; double valTowerHeight = 0;

            if (
               EvalExpression(brickLengthExp, ref valBrickLength, "BrickLength", "MakeBrickTower") &&
               EvalExpression(brickHeightExp, ref valBrickHeight, "BrickHeight", "MakeBrickTower") &&
               EvalExpression(brickWidthExp, ref valBrickWidth, "BrickWidth", "MakeBrickTower") &&
               EvalExpression(gapLengthExp, ref valGapLength, "GapLength", "MakeBrickTower") &&
               EvalExpression(gapDepthExp, ref valGapDepth, "GapDepth", "MakeBrickTower") &&
               EvalExpression(towerRadiusExp, ref valTowerRadius, "TowerRadius", "MakeBrickTower") &&
               EvalExpression(towerHeightExp, ref valTowerHeight, "TowerHeight", "MakeBrickTower"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valBrickLength < 1 || valBrickLength > 20)
                {
                    Log.Instance().AddEntry("MakeBrickTower : BrickLength value out of range (1..20)");
                    inRange = false;
                }

                if (valBrickHeight < 1 || valBrickHeight > 20)
                {
                    Log.Instance().AddEntry("MakeBrickTower : BrickHeight value out of range (1..20)");
                    inRange = false;
                }

                if (valBrickWidth < 1 || valBrickWidth > 10)
                {
                    Log.Instance().AddEntry("MakeBrickTower : BrickWidth value out of range (1..10)");
                    inRange = false;
                }

                if (valGapLength < 0.1 || valGapLength > 10)
                {
                    Log.Instance().AddEntry("MakeBrickTower : GapLength value out of range (0.1..10)");
                    inRange = false;
                }

                if (valGapDepth < 0.1 || valGapDepth > 10)
                {
                    Log.Instance().AddEntry("MakeBrickTower : GapDepth value out of range (0.1..10)");
                    inRange = false;
                }

                if (valTowerRadius < 10 || valTowerRadius > 200)
                {
                    Log.Instance().AddEntry("MakeBrickTower : TowerRadius value out of range (10..200)");
                    inRange = false;
                }

                if (valTowerHeight < 10 || valTowerHeight > 500)
                {
                    Log.Instance().AddEntry("MakeBrickTower : TowerHeight value out of range (10..500)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "BrickTower";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    BrickTowerMaker maker = new BrickTowerMaker(valBrickLength, valBrickHeight, valBrickWidth, valGapLength, valGapDepth, valTowerRadius, valTowerHeight);

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
                    Log.Instance().AddEntry("MakeBrickTower : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeBrickTower") + "( ";

            result += brickLengthExp.ToRichText() + ", ";
            result += brickHeightExp.ToRichText() + ", ";
            result += brickWidthExp.ToRichText() + ", ";
            result += gapLengthExp.ToRichText() + ", ";
            result += gapDepthExp.ToRichText() + ", ";
            result += towerRadiusExp.ToRichText() + ", ";
            result += towerHeightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeBrickTower( ";

            result += brickLengthExp.ToString() + ", ";
            result += brickHeightExp.ToString() + ", ";
            result += brickWidthExp.ToString() + ", ";
            result += gapLengthExp.ToString() + ", ";
            result += gapDepthExp.ToString() + ", ";
            result += towerRadiusExp.ToString() + ", ";
            result += towerHeightExp.ToString();
            result += " )";
            return result;
        }
    }
}