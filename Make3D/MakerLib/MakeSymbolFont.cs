using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class SymbolFontMaker : MakerBase
    {
        private double symbolLength;
        private double symbolHeight;
        private double symbolWidth;
        private string symbolCode;
        private string symbolFontName;

        public SymbolFontMaker(double symbolLength, double symbolHeight, double symbolWidth, string symbolCode, string fontName)
        {
            this.symbolLength = symbolLength;
            this.symbolHeight = symbolHeight;
            this.symbolWidth = symbolWidth;
            this.symbolCode = symbolCode;
            this.symbolFontName = fontName;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            MakeSymbolUtils msu = new MakeSymbolUtils();
            msu.Faces = Faces;
            msu.Vertices = Vertices;
            if (!String.IsNullOrEmpty(symbolCode) && !String.IsNullOrEmpty(symbolFontName))
            {
                msu.GenerateSymbol(symbolCode, symbolFontName, symbolLength, symbolHeight, symbolWidth);
            }
        }
    }
}