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
            System.Diagnostics.Debug.WriteLine(p.ToString());
            string s = p.ToString();

            string[] parts = s.Split('M');
            string souttter = "M"+parts[parts.GetLength(0) - 1];
            List<string> sinners = new List<string>();
            if (parts.GetLength(0) > 2)
            {
                for (int i = 1; i < parts.GetLength(0) - 1; i++)
                {
                    sinners.Add("M" + parts[i]);
                }
            }
                res = s;
                return res;
        }
    }
}