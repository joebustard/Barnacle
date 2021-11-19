using EarClipperLib;
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
        private double thickness;

        public TextMaker(string t, string fn, double fs, double h)
        {
            text = t;
            fontName = fn;
            fontsize = fs;
            height = h;
            thickness = 10;
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
            res = p.ToString();
            string s;
            List<string> sfigures = new List<string>();
            List<TextPolygon> pfigures = new List<TextPolygon>();
            foreach (PathFigure pf in p.Figures)
            {
                var flatpf = pf.GetFlattenedPathFigure();
                s = flatpf.ToString();

                string[] parts = s.Split('M');
                foreach (string ap in parts)
                {
                    if (ap != "")
                    {
                        TextPolygon tp = new TextPolygon();
                        // get a list of pointfs defining the outter polygon
                        tp.SPath = "M" + ap;
                        List<System.Drawing.PointF> poutter = new List<System.Drawing.PointF>();
                        GetPathPoints(tp.SPath, tp.Points);
                        pfigures.Add(tp);
                    }
                }
            }
            HuntForHoles(pfigures);
            TriangulateFigureWalls(pfigures);
            /*
            // remove the holes
            RemoveHoles(pfigures);
            foreach (TextPolygon pf in pfigures)
            {
                TriangulatePerimiter(pf.Points, thickness);
            }
            */
            foreach (TextPolygon pf in pfigures)
            {
                bool reverse = false;
                for (float py = 0; py <= thickness; py += (float)thickness)
                {
                    EarClipping earClipping = new EarClipping();
                    List<Vector3m> rootPoints = new List<Vector3m>();

                    foreach (PointF rp in pf.Points)
                    {
                        rootPoints.Add(new Vector3m(rp.X, py, rp.Y));
                    }

                    List<List<Vector3m>> holes = new List<List<Vector3m>>();
                    foreach (TextPolygon hole in pf.Holes)
                    {
                        List<Vector3m> holePoints = new List<Vector3m>();

                        foreach (PointF rp in hole.Points)
                        {
                            holePoints.Add(new Vector3m(rp.X, py, rp.Y));
                        }

                        holes.Add(holePoints);
                    }

                    earClipping.SetPoints(rootPoints, holes);
                    earClipping.Triangulate();
                    var surface = earClipping.Result;
                    for (int i = 0; i < surface.Count; i += 3)
                    {
                        int v1 = AddVertice(surface[i].X, surface[i].Y, surface[i].Z);
                        int v2 = AddVertice(surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                        int v3 = AddVertice(surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);
                        if (reverse)
                        {
                            Faces.Add(v1);
                            Faces.Add(v3);
                            Faces.Add(v2);
                        }
                        else
                        {
                            Faces.Add(v1);
                            Faces.Add(v2);
                            Faces.Add(v3);
                        }
                    }
                    reverse = !reverse;
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
            while (txt.Length > 0 && txt[0] != ' ' && txt[0] != 'L' && txt[0] != 'Z' && txt[0] != 'z')
            {
                dummy += txt[0];

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

        private void HuntForHoles(List<TextPolygon> pfigures)
        {
            bool rescan = false;
            while (!rescan)
            {
                rescan = true;

                for (int i = 0; i < pfigures.Count && rescan; i++)
                {
                    for (int j = 0; j < pfigures.Count && rescan; j++)
                    {
                        if (i != j)
                        {
                            if (pfigures[i].ContainsPoly(pfigures[j]))
                            {
                                pfigures[i].Holes.Add(pfigures[j]);
                                pfigures.RemoveAt(j);
                                rescan = false;
                            }
                        }
                    }
                }
            }
        }

        private void MakeWall(List<PointF> points, double thickness, bool invert)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                int p0 = AddVertice(points[i].X, 0, points[i].Y);
                int p1 = AddVertice(points[i].X, thickness, points[i].Y);
                int p2 = AddVertice(points[j].X, thickness, points[j].Y);
                int p3 = AddVertice(points[j].X, 0, points[j].Y);

                Faces.Add(p0);
                Faces.Add(p1);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p3);
            }
        }

        private void RemoveHoles(List<TextPolygon> pfigures)
        {
            foreach (TextPolygon tp in pfigures)
            {
                tp.RemoveHoles();
            }
        }

        private void TriangulateFigureWalls(List<TextPolygon> pfigures)
        {
            foreach (TextPolygon tp in pfigures)
            {
                MakeWall(tp.Points, thickness, false);
                foreach (TextPolygon hole in tp.Holes)
                {
                    MakeWall(hole.Points, thickness, true);
                }
            }
        }
    }
}