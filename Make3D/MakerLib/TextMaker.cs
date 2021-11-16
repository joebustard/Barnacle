using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TextMaker : MakerBase
    {
        private string fontName;
        private double fontsize;
        private double height;
        private string text;

        public TextMaker(string t, string fn, double fs, double h)
        {
            text = t;
            fontName = fn;
            fontsize = fs;
            height = h;
        }

        public string Generate(Point3DCollection pnts, Int32Collection faces)
        {
            string res = "";
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            PathGeometry p = TextHelper.PathFrom(text, "", true, fontName, fontsize);
            //Assume each letter is a separate figure
            foreach (PathFigure pf in p.Figures)
            {
                PathFigure flat = pf.GetFlattenedPathFigure();

                string part = flat.ToString();
                res += flat + " ";
            }
            return res;
        }
    }
}