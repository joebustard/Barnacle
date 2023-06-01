using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSymbolNode : ExpressionNode
    {
        private ExpressionNode symbolCodeExp;
        private ExpressionNode symbolFontExp;
        private ExpressionNode symbolHeightExp;
        private ExpressionNode symbolLengthExp;
        private ExpressionNode symbolWidthExp;

        public MakeSymbolNode
            (
            ExpressionNode symbolLength, ExpressionNode symbolHeight, ExpressionNode symbolWidth, ExpressionNode symbolCode
            , ExpressionNode symbolFont
            )
        {
            this.symbolLengthExp = symbolLength;
            this.symbolHeightExp = symbolHeight;
            this.symbolWidthExp = symbolWidth;
            this.symbolCodeExp = symbolCode;
            this.symbolFontExp = symbolFont;
        }

        public MakeSymbolNode
                (ExpressionCollection coll)
        {
            this.symbolLengthExp = coll.Get(0);
            this.symbolHeightExp = coll.Get(1);
            this.symbolWidthExp = coll.Get(2);
            this.symbolCodeExp = coll.Get(3);
            this.symbolFontExp = coll.Get(4);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valSymbolLength = 0; double valSymbolHeight = 0; double valSymbolWidth = 0; string valSymbolCode = "";
            string valFontName = "";
            if (
               EvalExpression(symbolLengthExp, ref valSymbolLength, "SymbolLength", "MakeSymbol") &&
               EvalExpression(symbolHeightExp, ref valSymbolHeight, "SymbolHeight", "MakeSymbol") &&
               EvalExpression(symbolWidthExp, ref valSymbolWidth, "SymbolWidth", "MakeSymbol") &&
               EvalExpression(symbolCodeExp, ref valSymbolCode, "SymbolCode", "MakeSymbol") &&
               EvalExpression(symbolFontExp, ref valFontName, "FontName", "MakeSymbol")
               )
            {
                // check calculated values are in range
                bool inRange = true;

                if (valSymbolLength < 2 || valSymbolLength > 200)
                {
                    Log.Instance().AddEntry("MakeSymbol : SymbolLength value out of range (2..200)");
                    inRange = false;
                }

                if (valSymbolHeight < 2 || valSymbolHeight > 200)
                {
                    Log.Instance().AddEntry("MakeSymbol : SymbolHeight value out of range (2..200)");
                    inRange = false;
                }

                if (valSymbolWidth < 2 || valSymbolWidth > 200)
                {
                    Log.Instance().AddEntry("MakeSymbol : SymbolWidth value out of range (2..200)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Symbol";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    SymbolFontMaker maker = new SymbolFontMaker(valSymbolLength, valSymbolHeight, valSymbolWidth, valSymbolCode, valFontName);

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
                    Log.Instance().AddEntry("MakeSymbol : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeSymbol") + "( ";

            result += symbolLengthExp.ToRichText() + ", ";
            result += symbolHeightExp.ToRichText() + ", ";
            result += symbolWidthExp.ToRichText() + ", ";
            result += symbolCodeExp.ToRichText() + ", ";
            result += symbolFontExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSymbol( ";

            result += symbolLengthExp.ToString() + ", ";
            result += symbolHeightExp.ToString() + ", ";
            result += symbolWidthExp.ToString() + ", ";
            result += symbolCodeExp.ToString() + ", ";
            result += symbolFontExp.ToString();
            result += " )";
            return result;
        }
    }
}