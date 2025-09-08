using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBrickWallNode : ExpressionNode
    {
        private ExpressionNode brickHeightExp;
        private ExpressionNode largeBrickLengthExp;
        private ExpressionNode mortarGapExp;
        private ExpressionNode smallBrickLengthExp;
        private ExpressionNode wallHeightExp;
        private ExpressionNode wallLengthExp;
        private ExpressionNode wallWidthExp;

        public MakeBrickWallNode
            (
            ExpressionNode wallLength, ExpressionNode wallHeight, ExpressionNode wallWidth, ExpressionNode largeBrickLength, ExpressionNode smallBrickLength, ExpressionNode brickHeight, ExpressionNode mortarGap
            )
        {
            this.wallLengthExp = wallLength;
            this.wallHeightExp = wallHeight;
            this.wallWidthExp = wallWidth;
            this.largeBrickLengthExp = largeBrickLength;
            this.smallBrickLengthExp = smallBrickLength;
            this.brickHeightExp = brickHeight;
            this.mortarGapExp = mortarGap;
        }

        public MakeBrickWallNode
                (ExpressionCollection coll)
        {
            this.wallLengthExp = coll.Get(0);
            this.wallHeightExp = coll.Get(1);
            this.wallWidthExp = coll.Get(2);
            this.largeBrickLengthExp = coll.Get(3);
            this.smallBrickLengthExp = coll.Get(4);
            this.brickHeightExp = coll.Get(5);
            this.mortarGapExp = coll.Get(6);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valWallLength = 0; double valWallHeight = 0; double valWallWidth = 0; double valLargeBrickLength = 0; double valSmallBrickLength = 0; double valBrickHeight = 0; double valMortarGap = 0;

            if (
                       EvalExpression(wallLengthExp, ref valWallLength, "WallLength", "MakeBrickWall") &&
                       EvalExpression(wallHeightExp, ref valWallHeight, "WallHeight", "MakeBrickWall") &&
                       EvalExpression(wallWidthExp, ref valWallWidth, "WallWidth", "MakeBrickWall") &&
                       EvalExpression(largeBrickLengthExp, ref valLargeBrickLength, "LargeBrickLength", "MakeBrickWall") &&
                       EvalExpression(smallBrickLengthExp, ref valSmallBrickLength, "SmallBrickLength", "MakeBrickWall") &&
                       EvalExpression(brickHeightExp, ref valBrickHeight, "BrickHeight", "MakeBrickWall") &&
                       EvalExpression(mortarGapExp, ref valMortarGap, "MortarGap", "MakeBrickWall"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valWallLength < 1 || valWallLength > 200)
                {
                    Log.Instance().AddEntry("MakeBrickWall : WallLength value out of range (1..200)");
                    inRange = false;
                }

                if (valWallHeight < 1 || valWallHeight > 200)
                {
                    Log.Instance().AddEntry("MakeBrickWall : WallHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valWallWidth < 1 || valWallWidth > 200)
                {
                    Log.Instance().AddEntry("MakeBrickWall : WallWidth value out of range (1..200)");
                    inRange = false;
                }

                if (valLargeBrickLength < 1 || valLargeBrickLength > 100)
                {
                    Log.Instance().AddEntry("MakeBrickWall : LargeBrickLength value out of range (1..100)");
                    inRange = false;
                }

                if (valSmallBrickLength < 1 || valSmallBrickLength > 100)
                {
                    Log.Instance().AddEntry("MakeBrickWall : SmallBrickLength value out of range (1..100)");
                    inRange = false;
                }

                if (valBrickHeight < 1 || valBrickHeight > 100)
                {
                    Log.Instance().AddEntry("MakeBrickWall : BrickHeight value out of range (1..100)");
                    inRange = false;
                }

                if (valMortarGap < 0 || valMortarGap > 100)
                {
                    Log.Instance().AddEntry("MakeBrickWall : MortarGap value out of range (0..100)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "BrickWall";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    BrickWallMaker maker = new BrickWallMaker(valWallLength, valWallHeight, valWallWidth, valLargeBrickLength, valSmallBrickLength, valBrickHeight, valMortarGap);

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
                    Log.Instance().AddEntry("MakeBrickWall : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeBrickWall") + "( ";

            result += wallLengthExp.ToRichText() + ", ";
            result += wallHeightExp.ToRichText() + ", ";
            result += wallWidthExp.ToRichText() + ", ";
            result += largeBrickLengthExp.ToRichText() + ", ";
            result += smallBrickLengthExp.ToRichText() + ", ";
            result += brickHeightExp.ToRichText() + ", ";
            result += mortarGapExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeBrickWall( ";

            result += wallLengthExp.ToString() + ", ";
            result += wallHeightExp.ToString() + ", ";
            result += wallWidthExp.ToString() + ", ";
            result += largeBrickLengthExp.ToString() + ", ";
            result += smallBrickLengthExp.ToString() + ", ";
            result += brickHeightExp.ToString() + ", ";
            result += mortarGapExp.ToString();
            result += " )";
            return result;
        }
    }
}