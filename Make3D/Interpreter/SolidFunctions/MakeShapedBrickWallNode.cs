using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeShapedBrickWallNode : ExpressionNode
    {
        private ExpressionNode pathExp;
        private ExpressionNode brickLengthExp;
        private ExpressionNode brickHeightExp;
        private ExpressionNode brickDepthExp;
        private ExpressionNode wallWidthExp;
        private ExpressionNode mortarGapExp;

        public MakeShapedBrickWallNode
            (
            ExpressionNode pth,
            ExpressionNode brickLength, ExpressionNode brickHeight, ExpressionNode brickDepth, ExpressionNode wallWidthExp, ExpressionNode mortarGap
            )
        {
            this.pathExp = pth;
            this.brickLengthExp = brickLength;
            this.brickHeightExp = brickHeight;
            this.brickDepthExp = brickDepth;
            this.wallWidthExp = wallWidthExp;
            this.mortarGapExp = mortarGap;
        }

        public MakeShapedBrickWallNode
                (ExpressionCollection coll)
        {
            this.pathExp = coll.Get(0);
            this.brickLengthExp = coll.Get(1);
            this.brickHeightExp = coll.Get(2);
            this.brickDepthExp = coll.Get(3);
            this.wallWidthExp = coll.Get(4);
            this.mortarGapExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valBrickLength = 0;
            double valBrickHeight = 0;
            double valBrickDepth = 0;
            double valMortarGap = 0;
            double valWallWidth = 0;
            string valPath = "";
            if (EvalExpression(pathExp, ref valPath, "Path", "MakeShapedBrickWall") &&
               EvalExpression(brickLengthExp, ref valBrickLength, "BrickLength", "MakeShapedBrickWall") &&
               EvalExpression(brickHeightExp, ref valBrickHeight, "BrickHeight", "MakeShapedBrickWall") &&
               EvalExpression(brickDepthExp, ref valBrickDepth, "BrickDepth", "MakeShapedBrickWall") &&
                EvalExpression(wallWidthExp, ref valWallWidth, "WallWidth", "MakeShapedBrickWall") &&
               EvalExpression(mortarGapExp, ref valMortarGap, "MortarGap", "MakeShapedBrickWall"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (String.IsNullOrEmpty(valPath))
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : Path value can't be empty.");
                    inRange = false;
                }
                if (valBrickLength < 1 || valBrickLength > 50)
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : BrickLength value out of range (1..50)");
                    inRange = false;
                }

                if (valBrickHeight < 1 || valBrickHeight > 50)
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : BrickHeight value out of range (1..50)");
                    inRange = false;
                }

                if (valWallWidth < 1 || valWallWidth > 50)
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : WallWidth value out of range (1..50)");
                    inRange = false;
                }
                if (valBrickDepth < 1 || valBrickDepth > 50)
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : BrickDepth value out of range (1..50)");
                    inRange = false;
                }

                if (valMortarGap < 1 || valMortarGap > 50)
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : MortarGap value out of range (1..50)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "ShapedBrickWall";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    ShapedBrickWallMaker maker = new ShapedBrickWallMaker(valPath, valBrickLength, valBrickHeight, valBrickDepth, valWallWidth, valMortarGap);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeShapedBrickWall : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeShapedBrickWall") + "( ";
            result += pathExp.ToRichText() + ", ";
            result += brickLengthExp.ToRichText() + ", ";
            result += brickHeightExp.ToRichText() + ", ";
            result += brickDepthExp.ToRichText() + ", ";
            result += wallWidthExp.ToRichText() + ", ";
            result += mortarGapExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeShapedBrickWall( ";

            result += pathExp.ToString() + ", ";
            result += brickLengthExp.ToString() + ", ";
            result += brickHeightExp.ToString() + ", ";
            result += brickDepthExp.ToString() + ", ";
            result += wallWidthExp.ToString() + ", ";
            result += mortarGapExp.ToString();
            result += " )";
            return result;
        }
    }
}