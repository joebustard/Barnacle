using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class SymbolFontMaker : MakerBase
    {
        private string symbolCode;
        private string symbolFontName;
        private double length;

        public SymbolFontMaker(string symbolCode, string fontName, double length)
        {
            this.symbolCode = symbolCode;
            this.symbolFontName = fontName;
            this.length = length;
        }

        public struct ResultDetails
        {
            public bool status;
        }

        public async Task<ResultDetails> GenerateAsync(Point3DCollection Vertices, Int32Collection Faces)
        {
            MakeSymbolUtils msu = new MakeSymbolUtils();
            msu.Faces = Faces;
            msu.Vertices = Vertices;
            if (!String.IsNullOrEmpty(symbolCode) && !String.IsNullOrEmpty(symbolFontName))
            {
                await Task.Run(() => msu.GenerateSymbol(symbolCode, symbolFontName, 25.0));
            }
            ResultDetails ret = new ResultDetails();

            ret.status = true;
            return ret;
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
                msu.GenerateSymbol(symbolCode, symbolFontName, length);
            }
        }
    }
}