using System;
using System.Collections.Generic;
using System.Drawing;

namespace PlacementLib
{
    public class Arranger
    {
        private double clearance;

        public double Clearance
        {
            get { return clearance; }
            set { clearance = value; }
        }

        private double width;

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                centre.X = width / 2;
            }
        }

        private double height;

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                centre.Y = height / 2;
            }
        }

        public List<Component> ComponentsToPlace;
        public List<Component> Results;

        private List<Workspace> workspaces;
        private System.Windows.Point centre;
        public double BestScore { get; set; }
        private double maxR;
        private List<PointF> placementPoints;
        private double plateWidth = 210;
        private double plateHeight = 210;

        public Arranger()
        {
            clearance = 3;
            width = 180;
            height = 180;

            ComponentsToPlace = new List<Component>();
            Results = new List<Component>();
        }

        public void Arrange()
        {
            BestScore = double.MaxValue;
            workspaces = new List<Workspace>();
            Workspace ws = new Workspace();
            centre.X = plateWidth / 2;
            centre.Y = plateHeight / 2;
            maxR = Math.Sqrt((centre.X * centre.X) + (centre.Y * centre.Y)) - clearance;
            // more components make the search time increase dramatically
            // increasing A reduces the time but also the quality
            double A = 1.75;
            placementPoints = GetSpiralPoints(new PointF((float)centre.X, (float)centre.Y), A, 0, maxR);
            ws.Width = width;
            ws.Height = height;
            ws.Place(ComponentsToPlace[0], centre.X - ComponentsToPlace[0].Width / 2, centre.Y - ComponentsToPlace[0].Height / 2, false);
            ProcessWorkspace(1, ws);
        }

        // Convert polar coordinates into Cartesian coordinates.
        private void PolarToCartesian(double r, double theta, out float x, out float y)
        {
            x = (float)(r * Math.Cos(theta));
            y = (float)(r * Math.Sin(theta));
        }

        private List<PointF> GetSpiralPoints(PointF center, double A, double angle_offset, double max_r)
        {
            // Get the points.
            List<PointF> points = new List<PointF>();
            float dtheta = (float)(2 * Math.PI / 180);
            double r = A;
            while (r < max_r)
            {
                for (float theta = 0; theta < 2 * Math.PI; theta += dtheta)
                {
                    // Convert to Cartesian coordinates.
                    float x, y;
                    PolarToCartesian(r, (float)(theta + angle_offset), out x, out y);

                    // Center.
                    x += center.X;
                    y += center.Y;

                    // Create the point.
                    points.Add(new PointF(x, y));
                }
                r += A;
            }
            return points;
        }

        private struct ScorePoint
        {
            public PointF pnt;
            public double score;
        }

        private void ProcessWorkspace(int v, Workspace parent)
        {
            if (v < ComponentsToPlace.Count)
            {
                Workspace ws = new Workspace();
                ws.Width = width;
                ws.Height = height;

                if (parent != null)
                {
                    foreach (Component c in parent.PlacedComponents)
                    {
                        ws.PlacedComponents.Add(c);
                    }
                }
                bool rotate = false;
                TryPlace(v, ws, rotate);
                // if foot print isn't square then it may be better to rotate the component
                //  if (ComponentsToPlace[v].CanBeRotated)
                //  {
                //      TryPlace(v, ws, rotate);
                //}
            }
            else
            {
                // parent workspace should all items placed somewhere
                // evaluate the the placement
                double score = parent.Evaluate();
                if (score < BestScore)
                {
                    BestScore = score;
                    Results = new List<Component>();
                    foreach (Component co in parent.PlacedComponents)
                    {
                        Component cn = new Component();
                        cn.Tag = co.Tag;
                        cn.Position = new System.Windows.Point(co.Position.X, co.Position.Y);
                        cn.OriginalPosition = new System.Windows.Point(co.OriginalPosition.X, co.OriginalPosition.Y);
                        cn.Rotated = co.Rotated;

                        Results.Add(cn);
                    }
                }
            }
        }

        private void TryPlace(int v, Workspace ws, bool rotate)
        {
            List<ScorePoint> scorePoints = new List<ScorePoint>();
            foreach (PointF pnt in placementPoints)
            {
                if (ws.CanPlace(ComponentsToPlace[v], pnt.X, pnt.Y, rotate))
                {
                    ws.Place(ComponentsToPlace[v], pnt.X, pnt.Y, rotate);
                    double score = ws.Evaluate();
                    ScorePoint sp = new ScorePoint();
                    sp.score = score;
                    sp.pnt = pnt;
                    scorePoints.Add(sp);
                    ws.Remove(ComponentsToPlace[v]);
                }
            }

            bool swapped;
            ScorePoint tmp;
            do
            {
                swapped = false;
                for (int i = 0; i < scorePoints.Count - 1; i++)
                {
                    if (scorePoints[i].score > scorePoints[i + 1].score)
                    {
                        tmp = scorePoints[i];
                        scorePoints[i] = scorePoints[i + 1];
                        scorePoints[i + 1] = tmp;
                        swapped = true;
                    }
                }
            } while (swapped);

            if (scorePoints[0].score < BestScore)
            {
                ws.Place(ComponentsToPlace[v], scorePoints[0].pnt.X, scorePoints[0].pnt.Y, rotate);
                if (ws.Evaluate() < BestScore)
                {
                    ProcessWorkspace(v + 1, ws);
                }
            }
        }

        public void AddComponent(object tag, System.Windows.Point lower, System.Windows.Point upper)
        {
            double lx = lower.X - clearance / 2;
            double ly = lower.Y - clearance / 2;
            double hx = upper.X + clearance / 2;
            double hy = upper.Y + clearance / 2;
            Component com = new Component(tag, new System.Windows.Point(lx, ly), new System.Windows.Point(hx, hy));
            if (ComponentsToPlace.Count == 0)
            {
                ComponentsToPlace.Add(com);
            }
            else
            {
                if (ComponentsToPlace[0].Area < com.Area)
                {
                    ComponentsToPlace.Insert(0, com);
                }
                else
                {
                    if (ComponentsToPlace[ComponentsToPlace.Count - 1].Area >= com.Area)
                    {
                        ComponentsToPlace.Add(com);
                    }
                    else
                    {
                        for (int i = 0; i < ComponentsToPlace.Count - 1; i++)
                        {
                            if ((ComponentsToPlace[i].Area >= com.Area) &&
                                 (ComponentsToPlace[i + 1].Area < com.Area))
                            {
                                ComponentsToPlace.Insert(i + 1, com);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}