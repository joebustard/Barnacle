using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeStoneWallNode : ExpressionNode
    {
        private ExpressionNode stoneSize;
        private ExpressionNode wallHeightExp;
        private ExpressionNode wallLengthExp;
        private ExpressionNode wallThicknessExp;

        public MakeStoneWallNode
            (
            ExpressionNode wallLength, ExpressionNode wallHeight, ExpressionNode wallThickness, ExpressionNode stoneSize
            )
        {
            this.wallLengthExp = wallLength;
            this.wallHeightExp = wallHeight;
            this.wallThicknessExp = wallThickness;
            this.stoneSize = stoneSize;
        }

        public MakeStoneWallNode
                (ExpressionCollection coll)
        {
            this.wallLengthExp = coll.Get(0);
            this.wallHeightExp = coll.Get(1);
            this.wallThicknessExp = coll.Get(2);
            this.stoneSize = coll.Get(3);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valWallLength = 0;
            double valWallHeight = 0;
            double valWallThickness = 0;
            int valStoneSize = 0;

            if (
                       EvalExpression(wallLengthExp, ref valWallLength, "WallLength", "MakeStoneWall") &&
                       EvalExpression(wallHeightExp, ref valWallHeight, "WallHeight", "MakeStoneWall") &&
                       EvalExpression(wallThicknessExp, ref valWallThickness, "WallThickness", "MakeStoneWall") &&
                       EvalExpression(stoneSize, ref valStoneSize, "Stone Size", "MakeStoneWall"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valWallLength < 1 || valWallLength > 200)
                {
                    Log.Instance().AddEntry("MakeStoneWall : WallLength value out of range (1..200)");
                    inRange = false;
                }

                if (valWallHeight < 1 || valWallHeight > 200)
                {
                    Log.Instance().AddEntry("MakeStoneWall : WallHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valWallThickness < 1 || valWallThickness > 200)
                {
                    Log.Instance().AddEntry("MakeStoneWall : WallThickness value out of range (1..200)");
                    inRange = false;
                }

                if (valStoneSize < 5 || valStoneSize > valWallLength / 2 || valStoneSize > valWallHeight / 2)
                {
                    Log.Instance().AddEntry("MakeStoneWall : Stone Size  value out of range (5.. WallLength /2 , wallHeight/2)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "StoneWall";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    StoneWallMaker maker = new StoneWallMaker(valWallLength, valWallHeight, valWallThickness, valStoneSize);

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
                    Log.Instance().AddEntry("MakeStoneWall : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeStoneWall") + "( ";

            result += wallLengthExp.ToRichText() + ", ";
            result += wallHeightExp.ToRichText() + ", ";
            result += wallThicknessExp.ToRichText() + ", ";
            result += stoneSize.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeStoneWall( ";

            result += wallLengthExp.ToString() + ", ";
            result += wallHeightExp.ToString() + ", ";
            result += wallThicknessExp.ToString() + ", ";
            result += stoneSize.ToString();
            result += " )";
            return result;
        }
    }
}