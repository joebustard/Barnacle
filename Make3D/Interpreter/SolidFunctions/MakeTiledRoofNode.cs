using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTiledRoofNode : ExpressionNode
    {
        private ExpressionNode gapBetweenTilesExp;
        private ExpressionNode heightExp;
        private ExpressionNode lengthExp;
        private ExpressionNode tileHeightExp;
        private ExpressionNode tileLengthExp;
        private ExpressionNode tileOverlapExp;
        private ExpressionNode tileWidthExp;
        private ExpressionNode widthExp;

        public MakeTiledRoofNode
            (
            ExpressionNode length, ExpressionNode height, ExpressionNode width, ExpressionNode tileLength, ExpressionNode tileHeight, ExpressionNode tileWidth, ExpressionNode overlap, ExpressionNode gapBetweenTiles
            )
        {
            this.lengthExp = length;
            this.heightExp = height;
            this.widthExp = width;
            this.tileLengthExp = tileLength;
            this.tileHeightExp = tileHeight;
            this.tileWidthExp = tileWidth;
            this.gapBetweenTilesExp = gapBetweenTiles;
            this.tileOverlapExp = overlap;
        }

        public MakeTiledRoofNode
                (ExpressionCollection coll)
        {
            this.lengthExp = coll.Get(0);
            this.heightExp = coll.Get(1);
            this.widthExp = coll.Get(2);
            this.tileLengthExp = coll.Get(3);
            this.tileHeightExp = coll.Get(4);
            this.tileWidthExp = coll.Get(5);
            this.tileOverlapExp = coll.Get(6);
            this.gapBetweenTilesExp = coll.Get(7);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valLength = 0;
            double valHeight = 0;
            double valWidth = 0; double valTileLength = 0;
            double valTileHeight = 0;
            double valTileWidth = 0;
            double valGapBetweenTiles = 0;
            double valOverlap = 0;

            if (
                       EvalExpression(lengthExp, ref valLength, "Length", "MakeTiledRoof") &&
                       EvalExpression(heightExp, ref valHeight, "Height", "MakeTiledRoof") &&
                       EvalExpression(widthExp, ref valWidth, "Width", "MakeTiledRoof") &&
                       EvalExpression(tileLengthExp, ref valTileLength, "TileLength", "MakeTiledRoof") &&
                       EvalExpression(tileHeightExp, ref valTileHeight, "TileHeight", "MakeTiledRoof") &&
                       EvalExpression(tileWidthExp, ref valTileWidth, "TileWidth", "MakeTiledRoof") &&
                       EvalExpression(tileOverlapExp, ref valOverlap, "TileOverlap", "MakeTiledRoof") &&
                       EvalExpression(gapBetweenTilesExp, ref valGapBetweenTiles, "GapBetweenTiles", "MakeTiledRoof")

                       )
            {
                // check calculated values are in range
                bool inRange = true;

                if (valLength < 10 || valLength > 200)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : Length value out of range (50..200)");
                    inRange = false;
                }

                if (valHeight < 1 || valHeight > 20)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : Height value out of range (4..20)");
                    inRange = false;
                }

                if (valWidth < 1 || valWidth > 20)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : Width value out of range (5..20)");
                    inRange = false;
                }

                if (valTileLength < 0 || valTileLength > 10)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : TileLength value out of range (0..10)");
                    inRange = false;
                }

                if (valTileHeight < 1 || valTileHeight > 10)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : TileHeight value out of range (1..10)");
                    inRange = false;
                }

                if (valTileWidth < 0.1 || valTileWidth > 10)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : TileWidth value out of range (0.1..10)");
                    inRange = false;
                }

                if (valGapBetweenTiles < 0.1 || valGapBetweenTiles > 10)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : GapBetweenTiles value out of range (0.1..10)");
                    inRange = false;
                }

                if (valOverlap < 0.1 || valOverlap > 0.9)
                {
                    Log.Instance().AddEntry("MakeTiledRoof : Tile Overlap value out of range (1..10)");
                    inRange = false;
                }
                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "TiledRoof";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    TiledRoofMaker maker = new TiledRoofMaker(valLength, valHeight, valWidth, valTileLength, valTileHeight, valTileWidth, valOverlap, valGapBetweenTiles);

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
                    Log.Instance().AddEntry("MakeTiledRoof : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTiledRoof") + "( ";

            result += lengthExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += widthExp.ToRichText() + ", ";
            result += tileLengthExp.ToRichText() + ", ";
            result += tileHeightExp.ToRichText() + ", ";
            result += tileWidthExp.ToRichText() + ", ";
            result += tileOverlapExp.ToRichText() + ", ";
            result += gapBetweenTilesExp.ToRichText();
            result += tileOverlapExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTiledRoof( ";

            result += lengthExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += widthExp.ToString() + ", ";
            result += tileLengthExp.ToString() + ", ";
            result += tileHeightExp.ToString() + ", ";
            result += tileWidthExp.ToString() + ", ";
            result += tileOverlapExp.ToString() + ", ";
            result += gapBetweenTilesExp.ToString();
            result += " )";
            return result;
        }
    }
}