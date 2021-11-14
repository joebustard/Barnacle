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
        private string text;
        private string fontName;
        private double fontsize;
        private double height;
        public TextMaker(string t, string fn, double fs, double h)
        {
            text = t;
            fontName = fn;
            fontsize = fs;
            height = h;
        }
        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
        }
    }
}
