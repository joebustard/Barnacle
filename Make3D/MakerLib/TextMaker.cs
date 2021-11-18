using System;
using System.Collections.Generic;
using System.Drawing;
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
            res = s;
            foreach (PathFigure pf in p.Figures)
            {
                var flatpf = pf.GetFlattenedPathFigure();
                s = flatpf.ToString();
                string[] parts = s.Split('M');

                // get a list of pointfs defining the outter polygon
                string soutter = "M" + parts[parts.GetLength(0) - 1];
                List<System.Drawing.PointF> poutter = new List<System.Drawing.PointF>();
                GetPathPoints(soutter, poutter);

                List<string> sinners = new List<string>();
                // does the shape have holes
                if (parts.GetLength(0) > 2)
                {
                    //for each hole
                    for (int i = 1; i < parts.GetLength(0) - 1; i++)
                    {
                        string sin = "M" + parts[i];
                        sinners.Add(sin);
                        // get a list of pointfs for the hole
                        List<System.Drawing.PointF> pin = new List<System.Drawing.PointF>();
                        GetPathPoints(sin, pin);
                    }
                }
            }
            return res;
        }

        private void GetPathPoints(string txt, List<PointF> pnts)
        {
            float x = 0;
            float y = 0;

            pnts.Clear();
            txt = txt.Trim();
            while (txt.Length > 0)
            {
                if (txt.StartsWith("M"))
                {
                    txt = txt.Substring(1);
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
                else
                if (txt.StartsWith("L"))
                {
                    txt = txt.Substring(1);
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
                else if (txt.StartsWith(" "))
                {
                    txt = txt.Substring(1);
                }
                else if (txt.StartsWith("z") || txt.StartsWith("Z"))
                {
                    txt = txt.Substring(1);
                }
                else
                {
                    txt = GetTwoFloats(txt, out x, out y);
                    pnts.Add(new PointF(x, y));
                }
            }
        }

        private string GetTwoFloats(string txt, out float x, out float y)
        {
            x = 0;
            y = 0;
            txt = txt.Trim();
            string dummy = "";
            while (txt.Length > 0 && txt[0] != ' ')
            {
                dummy += text[0];

                txt = txt.Substring(1);
            }
            string[] words = dummy.Split(',');
            if (words.GetLength(0) == 2)
            {
                try
                {
                    x = Convert.ToSingle(words[0]);
                    y = Convert.ToSingle(words[1]);
                }
                catch
                {
                }
            }
            return txt;
        }
    }
}