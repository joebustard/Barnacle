using Barnacle.Object3DLib;
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
        private bool bold;
        private string fontName;
        private double fontsize;
        private double height;
        private double holeOverlap = 10;
        private bool italic;
        private bool superSmooth;
        private string text;
        private double thickness;

        public TextMaker(string t, string fn, double fs, double h, bool ss, bool bold, bool italic)
        {
            text = t;
            fontName = fn;
            fontsize = fs;
            thickness = h;
            superSmooth = ss;
            this.bold = bold;
            this.italic = italic;
        }

        public string Generate(Point3DCollection pnts, Int32Collection faces)
        {
            string res = "";
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            PathGeometry p = TextHelper.PathFrom(text, "", true, fontName, fontsize, superSmooth, bold, italic);
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

            Object3D bodyShape = new Object3D();
            bodyShape.Position = new Point3D(0, 0, 0);
            Object3D holeShape = new Object3D();
            holeShape.Position = new Point3D(0, 0, 0);
            TriangulateFigureWallsBody(pfigures, bodyShape.AbsoluteObjectVertices, bodyShape.TriangleIndices);
            TriangulateFigureWallsHoles(pfigures, holeShape.AbsoluteObjectVertices, holeShape.TriangleIndices);

            //add front and back of body
            foreach (TextPolygon pf in pfigures)
            {
                bool reverse = true;
                for (float py = 0; py <= thickness; py += (float)thickness)
                {
                    EarClipping earClipping = new EarClipping();
                    List<Vector3m> rootPoints = new List<Vector3m>();

                    foreach (PointF rp in pf.Points)
                    {
                        rootPoints.Insert(0, new Vector3m(rp.X, py, rp.Y));
                    }

                    earClipping.SetPoints(rootPoints);

                    earClipping.Triangulate();
                    var surface = earClipping.Result;
                    for (int i = 0; i < surface.Count; i += 3)
                    {
                        int v1 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i].X, surface[i].Y, surface[i].Z);
                        int v2 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                        int v3 = AddVertice(bodyShape.AbsoluteObjectVertices, surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);
                        if (reverse)
                        {
                            bodyShape.TriangleIndices.Add(v1);
                            bodyShape.TriangleIndices.Add(v3);
                            bodyShape.TriangleIndices.Add(v2);
                        }
                        else
                        {
                            bodyShape.TriangleIndices.Add(v1);
                            bodyShape.TriangleIndices.Add(v2);
                            bodyShape.TriangleIndices.Add(v3);
                        }
                    }
                    reverse = !reverse;
                }
            }

            bodyShape.AbsoluteToRelative();


            bool holesAdded = false;



            // Go through all the letter shapes
            foreach (TextPolygon pf in pfigures)
            {
               // does it have any holes
                if (pf.Holes.Count > 0)
                {
                    holesAdded = true;
                    
                    for (float py = (float)-holeOverlap / 2; py <= (float)(thickness + holeOverlap / 2); py += (float)(thickness + holeOverlap))
                    {
                        EarClipping earClipping = new EarClipping();

                        for (int holeIndex = 0; holeIndex < pf.Holes.Count; holeIndex++)
                        {
                            TextPolygon hole = pf.Holes[holeIndex];

                            List<Vector3m> rootPoints = new List<Vector3m>();
                     
                                
                                // turns out that if there are more than one holes, their orientaion alternates
                                if (Math.Abs(py - (float)-holeOverlap / 2) <0.0001)
                                {
                                    foreach (PointF rp in hole.Points)
                                    {
                                        rootPoints.Insert(0, new Vector3m(rp.X, py, rp.Y));
                                    }
                                    
                                }
                                else
                                {
                                    foreach (PointF rp in hole.Points)
                                    {
                                        rootPoints.Add(new Vector3m(rp.X, py, rp.Y));
                                    }
                                }
                                
            
                            

                            earClipping.SetPoints(rootPoints);

                            earClipping.Triangulate();
                            var surface = earClipping.Result;
                            for (int i = 0; i < surface.Count; i += 3)
                            {
                                int v1 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i].X, surface[i].Y, surface[i].Z);
                                int v2 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                                int v3 = AddVertice(holeShape.AbsoluteObjectVertices, surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);
                                
                                    holeShape.TriangleIndices.Add(v1);
                                    holeShape.TriangleIndices.Add(v2);
                                    holeShape.TriangleIndices.Add(v3);
                                
                            }

                        }
                    }
                }
            }
            if (holesAdded)
            {


                holeShape.AbsoluteToRelative();
                Group3D merged = new Group3D();

                merged.LeftObject = bodyShape;
                merged.RightObject = holeShape;
                merged.Position = new Point3D(0, 0, 0);
                merged.PrimType = "groupdifference";

                //merged.PrimType = "groupunion";
                merged.Init();
                merged.Remesh();
                CopyShape(merged, pnts, faces);
            }
            else
            {
                CopyShape(bodyShape, pnts, faces);
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

        private void MakeWall(List<PointF> points, double thickness, bool invert, double offset = 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                int p0 = AddVertice(points[i].X, -offset, points[i].Y);
                int p1 = AddVertice(points[i].X, thickness - offset, points[i].Y);
                int p2 = AddVertice(points[j].X, thickness - offset, points[j].Y);
                int p3 = AddVertice(points[j].X, -offset, points[j].Y);

                Faces.Add(p0);
                Faces.Add(p1);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p3);
            }
        }

        private void MakeWall(List<PointF> points, double thickness, bool invert, Point3DCollection pnts, Int32Collection tris, double offset = 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                int p0 = AddVertice(pnts, points[i].X, -offset, points[i].Y);
                int p1 = AddVertice(pnts, points[i].X, thickness - offset, points[i].Y);
                int p2 = AddVertice(pnts, points[j].X, thickness - offset, points[j].Y);
                int p3 = AddVertice(pnts, points[j].X, -offset, points[j].Y);

                if (!invert)
                {
                    tris.Add(p0);
                    tris.Add(p1);
                    tris.Add(p2);

                    tris.Add(p0);
                    tris.Add(p2);
                    tris.Add(p3);
                }
                else
                {
                    tris.Add(p0);
                    tris.Add(p2);
                    tris.Add(p1);

                    tris.Add(p0);
                    tris.Add(p3);
                    tris.Add(p2);
                }
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

        private void TriangulateFigureWallsBody(List<TextPolygon> pfigures, Point3DCollection pnts, Int32Collection tris)
        {
            foreach (TextPolygon tp in pfigures)
            {
                MakeWall(tp.Points, thickness, false, pnts, tris);
            }
        }

        private void TriangulateFigureWallsHoles(List<TextPolygon> pfigures, Point3DCollection pnts, Int32Collection tris)
        {
            foreach (TextPolygon tp in pfigures)
            {
                foreach (TextPolygon hole in tp.Holes)
                {
                    MakeWall(hole.Points, thickness + holeOverlap, true, pnts, tris, holeOverlap / 2);
                }
            }
        }
    }
}