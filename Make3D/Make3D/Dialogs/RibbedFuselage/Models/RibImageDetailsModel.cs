using Barnacle.LineLib;
using Barnacle.RibbedFuselage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    public class RibImageDetailsModel : ImageDetailsModel
    {
        public override void Load(XmlElement ele)
        {
            base.Load(ele);
        }

        private List<PointF> profilePoints;

        public List<PointF> ProfilePoints
        {
            get { return profilePoints; }
            set { profilePoints = value; }
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private int numDivisions;

        public int NumDivisions
        {
            get { return numDivisions; }
            set
            {
                numDivisions = value;
                Dirty = true;
            }
        }

        public void GenerateProfilePoints()
        {
            if (Dirty || profilePoints == null || profilePoints.Count == 0)
            {
                double tlx;
                double tly;
                double brx;
                double bry;
                double middleX;
                double middleY;
                double pathWidth;
                double pathHeight;
                FlexiPath fp = new FlexiPath();
                profilePoints = new List<PointF>();
                fp.FromString(FlexiPathText);
                List<PointF> pnts = fp.DisplayPointsF();
                if (pnts != null && pnts.Count > 0)
                {
                    pnts.Add(new PointF(pnts[0].X, pnts[0].Y));
                    tlx = double.MaxValue;
                    tly = double.MaxValue;
                    brx = double.MinValue;
                    bry = double.MinValue;

                    double pathLength = 0;
                    for (int i = 0; i < pnts.Count; i++)
                    {
                        if (i < pnts.Count - 1)
                        {
                            pathLength += Distance(pnts[i], pnts[i + 1]);
                        }
                        if (pnts[i].X < tlx)
                        {
                            tlx = pnts[i].X;
                        }

                        if (pnts[i].Y < tly)
                        {
                            tly = pnts[i].Y;
                        }

                        if (pnts[i].X > brx)
                        {
                            brx = pnts[i].X;
                        }

                        if (pnts[i].Y > bry)
                        {
                            bry = pnts[i].Y;
                        }
                    }

                    pathWidth = brx - tlx;
                    pathHeight = bry - tly;
                    middleX = tlx + pathWidth / 2.0;
                    middleY = tly + pathHeight / 2.0;
                    double minangle = double.MaxValue;
                    int minIndex = int.MaxValue;

                    double deltaT = 1.0 / (NumDivisions + 1);
                    List<double> angles = new List<double>();
                    for (int div = 0; div < NumDivisions; div++)
                    {
                        double t = div * deltaT;
                        double targetDistance = t * pathLength;

                        if (targetDistance < pathLength)
                        {
                            double runningDistance = 0;
                            int cp = 1;
                            bool found = false;

                            while (!found && cp < pnts.Count)
                            {
                                PointF p0 = pnts[cp - 1];
                                PointF p1 = pnts[cp];
                                double d = Distance(p0, p1);
                                if ((runningDistance <= targetDistance) &&
                                     (runningDistance + d >= targetDistance))
                                {
                                    found = true;
                                    double overhang = targetDistance - runningDistance;
                                    if (overhang < 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Distance error creating profile");
                                    }
                                    if (overhang != 0.0)
                                    {
                                        double delta = overhang / d;
                                        double nx = p0.X + (p1.X - p0.X) * delta;
                                        double ny = p0.Y + (p1.Y - p0.Y) * delta;

                                        double rx = nx - middleX;
                                        double ry = ny - middleY;
                                        double rd = Math.Atan2(ry, rx);
                                        angles.Add(rd);
                                        if (rx > 0 && rd < minangle)
                                        {
                                            minIndex = angles.Count - 1;
                                            minangle = rd;
                                        }

                                        nx = (nx - tlx) / pathWidth;
                                        ny = (ny - tly) / pathHeight;
                                        profilePoints.Add(new PointF((float)nx, (float)ny));
                                    }
                                    else
                                    {
                                        double nx = p0.X;
                                        double ny = p0.Y;

                                        double rx = nx - middleX;
                                        double ry = ny - middleY;
                                        double rd = Math.Atan2(ry, rx);
                                        angles.Add(rd);
                                        if (rx > 0 && rd < minangle)
                                        {
                                            minIndex = angles.Count - 1;
                                            minangle = rd;
                                        }

                                        nx = (nx - tlx) / pathWidth;
                                        ny = (ny - tly) / pathHeight;
                                        profilePoints.Add(new PointF((float)nx, (float)ny));
                                    }
                                }
                                runningDistance += d;
                                cp++;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Target distance exceeds length");
                        }
                    }

                    List<PointF> tmp = new List<PointF>();

                    for (int j = minIndex; j < profilePoints.Count; j++)
                    {
                        tmp.Add(profilePoints[j]);
                    }
                    if (minIndex > 0 && minIndex < profilePoints.Count)
                    {
                        for (int j = 0; j < minIndex; j++)
                        {
                            tmp.Add(profilePoints[j]);
                        }
                    }
                    profilePoints = tmp;

                    Dirty = false;
                }
            }
        }

        public override void Save(XmlElement ele, XmlDocument doc)
        {
            base.Save(ele, doc);
        }

        internal RibImageDetailsModel Clone()
        {
            RibImageDetailsModel cln = new RibImageDetailsModel();

            cln.ImageFilePath = ImageFilePath;
            cln.DisplayFileName = DisplayFileName;
            cln.Name = Name;
            cln.FlexiPathText = FlexiPathText;
            cln.NumDivisions = NumDivisions;
            cln.ProfilePoints = new List<PointF>();
            foreach (PointF p in profilePoints)
            {
                cln.ProfilePoints.Add(new PointF(p.X, p.Y));
            }
            cln.Dirty = Dirty;
            return cln;
        }

        internal RibImageDetailsModel()
        {
            NumDivisions = 100;
            Dirty = true;
        }
    }
}