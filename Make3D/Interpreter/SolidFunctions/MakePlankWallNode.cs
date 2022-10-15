using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePlankWallNode : ExpressionNode
    {
        private ExpressionNode wallLengthExp;
        private ExpressionNode wallHeightExp;
        private ExpressionNode wallWidthExp;
        private ExpressionNode plankWidthExp;
        private ExpressionNode gapExp;
        private ExpressionNode gapDepthExp;

        public MakePlankWallNode(
            ExpressionNode wallLength, 
            ExpressionNode wallHeight, 
            ExpressionNode wallWidth, 
            ExpressionNode plankWidth, 
            ExpressionNode gap, 
            ExpressionNode gapDepth)
        {
            this.wallLengthExp = wallLength;
            this.wallHeightExp = wallHeight;
            this.wallWidthExp = wallWidth;
            this.plankWidthExp = plankWidth;
            this.gapExp = gap;
            this.gapDepthExp = gapDepth;
        }

        public MakePlankWallNode (ExpressionCollection coll)
        {
            this.wallLengthExp = coll.Get(0);
            this.wallHeightExp = coll.Get(1);
            this.wallWidthExp = coll.Get(2);
            this.plankWidthExp = coll.Get(3);
            this.gapExp = coll.Get(4);
            this.gapDepthExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valWallLength = 0; 
            double valWallHeight = 0; 
            double valWallWidth = 0; 
            double valPlankWidth = 0; 
            double valGap = 0; 
            double valGapDepth = 0;

            if (EvalExpression(wallLengthExp, ref valWallLength, "WallLength", "MakePlankWall") &&
               EvalExpression(wallHeightExp, ref valWallHeight, "WallHeight", "MakePlankWall") &&
               EvalExpression(wallWidthExp, ref valWallWidth, "WallWidth", "MakePlankWall") &&
               EvalExpression(plankWidthExp, ref valPlankWidth, "PlankWidth", "MakePlankWall") &&
               EvalExpression(gapExp, ref valGap, "Gap", "MakePlankWall") &&
               EvalExpression(gapDepthExp, ref valGapDepth, "GapDepth", "MakePlankWall"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valWallLength < 1 || valWallLength > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : WallLength value out of range (1..300)");
                    inRange = false;
                }

                if (valWallHeight < 1 || valWallHeight > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : WallHeight value out of range (1..300)");
                    inRange = false;
                }

                if (valWallWidth < 1 || valWallWidth > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : WallWidth value out of range (1..300)");
                    inRange = false;
                }

                if (valPlankWidth < 1 || valPlankWidth > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : PlankWidth value out of range (1..300)");
                    inRange = false;
                }

                if (valGap < 1 || valGap > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : Gap value out of range (1..300)");
                    inRange = false;
                }

                if (valGapDepth < 1 || valGapDepth > 300)
                {
                    Log.Instance().AddEntry("MakePlankWall : GapDepth value out of range (1..300)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "PlankWall";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    PlankWallMaker maker = new PlankWallMaker(valWallLength, valWallHeight, valWallWidth, valPlankWidth, valGap, valGapDepth);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakePlankWall : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakePlankWall") + "( ";

            result += wallLengthExp.ToRichText() + ", ";
            result += wallHeightExp.ToRichText() + ", ";
            result += wallWidthExp.ToRichText() + ", ";
            result += plankWidthExp.ToRichText() + ", ";
            result += gapExp.ToRichText() + ", ";
            result += gapDepthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePlankWall( ";

            result += wallLengthExp.ToString() + ", ";
            result += wallHeightExp.ToString() + ", ";
            result += wallWidthExp.ToString() + ", ";
            result += plankWidthExp.ToString() + ", ";
            result += gapExp.ToString() + ", ";
            result += gapDepthExp.ToString();
            result += " )";
            return result;
        }
    }
}