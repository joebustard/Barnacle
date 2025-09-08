using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeShapedTiledRoofNode : ExpressionNode
    {
        private ExpressionNode mortarGapExp;
        private ExpressionNode pathExp;
        private ExpressionNode roofWidthExp;
        private ExpressionNode tileDepthExp;
        private ExpressionNode tileHeightExp;
        private ExpressionNode tileLengthExp;

        public MakeShapedTiledRoofNode(ExpressionNode pathNode, ExpressionNode tileLength, ExpressionNode tileHeight, ExpressionNode tileDepth, ExpressionNode mortarGap, ExpressionNode roofWidth)
        {
            this.pathExp = pathNode;
            this.tileLengthExp = tileLength;
            this.tileHeightExp = tileHeight;
            this.tileDepthExp = tileDepth;
            this.mortarGapExp = mortarGap;
            this.roofWidthExp = roofWidth;
        }

        public MakeShapedTiledRoofNode(ExpressionCollection coll)
        {
            this.pathExp = coll.Get(0);
            this.tileLengthExp = coll.Get(1);
            this.tileHeightExp = coll.Get(2);
            this.tileDepthExp = coll.Get(3);
            this.mortarGapExp = coll.Get(4);
            this.roofWidthExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            string valPath = "";

            double valTileLength = 0;
            double valTileHeight = 0;
            double valTileDepth = 0;
            double valMortarGap = 0;
            double valRoofWidth = 0;

            if (EvalExpression(pathExp, ref valPath, "Path", "MakeShapedTiledRoof") &&
               EvalExpression(tileLengthExp, ref valTileLength, "Tile Length", "MakeShapedTiledRoof") &&
               EvalExpression(tileHeightExp, ref valTileHeight, "Tile Height", "MakeShapedTiledRoof") &&
               EvalExpression(tileDepthExp, ref valTileDepth, "Tile Depth", "MakeShapedTiledRoof") &&
               EvalExpression(mortarGapExp, ref valMortarGap, "Mortar Gap", "MakeShapedTiledRoof") &&
               EvalExpression(roofWidthExp, ref valRoofWidth, "Roof Width", "MakeShapedTiledRoof"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valPath == "")
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : Path value can't be blank");
                    inRange = false;
                }
                if (valTileLength < 0.1 || valTileLength > 50)
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : TileLength value out of range (0.1..50)");
                    inRange = false;
                }

                if (valTileHeight < 0.1 || valTileHeight > 50)
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : TileHeight value out of range (0.1..50)");
                    inRange = false;
                }

                if (valTileDepth < 0.1 || valTileDepth > 50)
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : TileDepth value out of range (0.1..50)");
                    inRange = false;
                }

                if (valMortarGap < 0.1 || valMortarGap > 50)
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : MortarGap value out of range (0.1..50)");
                    inRange = false;
                }

                if (valRoofWidth < 0.1 || valRoofWidth > 50)
                {
                    Log.Instance().AddEntry("MakeShapedTiledRoof : RoofWidth value out of range (0.1..50)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "ShapedTiledRoof";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    ShapedTiledRoofMaker maker = new ShapedTiledRoofMaker(valPath, valTileLength, valTileHeight, valTileDepth, valMortarGap, valRoofWidth);

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
                    Log.Instance().AddEntry("MakeShapedTiledRoof : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeShapedTiledRoof") + "( ";
            result += pathExp.ToRichText() + ", ";
            result += tileLengthExp.ToRichText() + ", ";
            result += tileHeightExp.ToRichText() + ", ";
            result += tileDepthExp.ToRichText() + ", ";
            result += mortarGapExp.ToRichText() + ", ";
            result += roofWidthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeShapedTiledRoof( ";
            result += pathExp.ToString() + ", ";
            result += tileLengthExp.ToString() + ", ";
            result += tileHeightExp.ToString() + ", ";
            result += tileDepthExp.ToString() + ", ";
            result += mortarGapExp.ToString() + ", ";
            result += roofWidthExp.ToString();
            result += " )";
            return result;
        }
    }
}