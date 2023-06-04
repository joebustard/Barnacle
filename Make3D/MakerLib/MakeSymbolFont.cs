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

        public SymbolFontMaker(string symbolCode, string fontName)
        {
           
            this.symbolCode = symbolCode;
            this.symbolFontName = fontName;
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
                await Task.Run(() => msu.GenerateSymbol(symbolCode, symbolFontName));       
            }
            ResultDetails ret = new ResultDetails();
            /*
            ret.pnts = new List<Point3D>();
            foreach(Point3D p in Vertices)
            {
                ret.pnts.Add(new Point3D(p.X, p.Y, p.Z));
            }
            ret.faces = Faces;
            */
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
                msu.GenerateSymbol(symbolCode, symbolFontName);
            }
        }
    }
}