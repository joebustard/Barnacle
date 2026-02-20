/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

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
        private int numDivisions;

        private List<PointF> profilePoints;

        internal RibImageDetailsModel()
        {
            NumDivisions = 100;
            Dirty = true;
        }

        public PointF BottomPoint
        {
            get; set;
        }

        public int BottomPointIndex
        {
            get; set;
        }

        public int NumDivisions
        {
            get
            {
                return numDivisions;
            }

            set
            {
                numDivisions = value;
                Dirty = true;
            }
        }

        public List<PointF> ProfilePoints
        {
            get
            {
                return profilePoints;
            }
            set
            {
                profilePoints = value;
            }
        }

        public PointF TopPoint
        {
            get; set;
        }

        public int TopPointIndex
        {
            get; set;
        }

        public void GenerateProfilePoints()
        {
            int ribDivisions = NumDivisions;
            // we need the number of divisions used in the calcs to be an even number
            if (ribDivisions % 2 == 1)
            {
                ribDivisions++;
            }
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

                // screen points will always be arranged in the clockwise direction
                List<PointF> screenPoints = fp.DisplayPointsF();
                if (screenPoints != null && screenPoints.Count > 0)
                {
                    // if necessary close the polygon
                    if ((screenPoints[0].X != screenPoints[screenPoints.Count - 1].X) ||
                         (screenPoints[0].Y != screenPoints[screenPoints.Count - 1].Y))
                    {
                        screenPoints.Add(new PointF(screenPoints[0].X, screenPoints[0].Y));
                    }

                    // Calculate the total length of the path in screen units (mm)
                    // Also find the bounds
                    tlx = double.MaxValue;
                    tly = double.MaxValue;
                    brx = double.MinValue;
                    bry = double.MinValue;

                    double pathLength = 0;
                    for (int i = 0; i < screenPoints.Count; i++)
                    {
                        if (i < screenPoints.Count - 1)
                        {
                            pathLength += Distance(screenPoints[i], screenPoints[i + 1]);
                        }
                        if (screenPoints[i].X < tlx)
                        {
                            tlx = screenPoints[i].X;
                        }

                        if (screenPoints[i].Y < tly)
                        {
                            tly = screenPoints[i].Y;
                        }

                        if (screenPoints[i].X > brx)
                        {
                            brx = screenPoints[i].X;
                        }

                        if (screenPoints[i].Y > bry)
                        {
                            bry = screenPoints[i].Y;
                        }
                    }

                    // Calculate the dimensions and center point
                    pathWidth = brx - tlx;
                    pathHeight = bry - tly;
                    middleX = tlx + pathWidth / 2.0;
                    middleY = tly + pathHeight / 2.0;
                    double minangle = double.MaxValue;
                    int minIndex = int.MaxValue;

                    // split this path up into the required number of subdivisions
                    double deltaT = 1.0 / (ribDivisions - 1);
                    List<double> angles = new List<double>();
                    for (int div = 0; div < ribDivisions; div++)
                    {
                        double t = div * deltaT;
                        double targetDistance = t * pathLength;

                        if (targetDistance < pathLength)
                        {
                            double runningDistance = 0;
                            int cp = 1;
                            bool found = false;

                            while (!found && cp < screenPoints.Count)
                            {
                                PointF p0 = screenPoints[cp - 1];
                                PointF p1 = screenPoints[cp];
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
                                        // get the coordinates of the point for the current subdivision
                                        double nx = p0.X + (p1.X - p0.X) * delta;
                                        double ny = p0.Y + (p1.Y - p0.Y) * delta;

                                        // get the angle from the centre to this point
                                        // just so we can use the one closest to angle 0
                                        // to align all the ribs
                                        double rx = nx - middleX;
                                        double ry = ny - middleY;
                                        double rd = Math.Atan2(ry, rx);

                                        if (rd < 0)
                                        {
                                            rd += (2 * Math.PI);
                                        }
                                        angles.Add(rd);
                                        if (rx > 0 && rd < minangle)
                                        {
                                            minIndex = angles.Count - 1;
                                            minangle = rd;
                                        }

                                        // scale the division point t0 the range 0 -> 1
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
                                        if (rd < 0)
                                        {
                                            rd += (2 * Math.PI);
                                        }
                                        angles.Add(rd);
                                        if (rd < minangle)
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
                    for (int i = 0; i < profilePoints.Count - 1; i++)
                    {
                        int j = i + 1;
                        if (j == profilePoints.Count)
                        {
                            j = 0;
                        }
                        System.Diagnostics.Debug.WriteLine($"i = {i} d= {Distance(tmp[i], tmp[j])}");
                    }
                    double cx = 0.5;
                    // insert specific centre points at midx, top and bottom
                    // As the points are clockwise we should find the bottom first
                    for (int i = 0; i < profilePoints.Count; i++)
                    {
                        int j = i + 1;
                        if (j == profilePoints.Count)
                        {
                            j = 0;
                        }
                        if ((profilePoints[i].X > cx) && (profilePoints[j].X < cx))
                        {
                            double dx = profilePoints[i].X - cx;
                            double t = dx / (profilePoints[i].X - profilePoints[j].X);
                            double dy = t * (profilePoints[i].Y - profilePoints[j].Y);
                            BottomPoint = new PointF((float)cx, profilePoints[i].Y - (float)dy);
                            BottomPointIndex = j;
                            profilePoints.Insert(j, BottomPoint);
                        }
                        else
                        if ((profilePoints[i].X < cx) && (profilePoints[j].X > cx))
                        {
                            double dx = cx - profilePoints[i].X;
                            double t = dx / (profilePoints[j].X - profilePoints[i].X);
                            double dy = t * (profilePoints[j].Y - profilePoints[i].Y);
                            TopPoint = new PointF((float)cx, profilePoints[i].Y + (float)dy);
                            TopPointIndex = j;
                            profilePoints.Insert(j, TopPoint);
                        }
                    }
                    Dirty = false;
                }
            }
        }

        public override void Load(XmlElement ele)
        {
            base.Load(ele);
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
            cln.TopPoint = TopPoint;
            cln.BottomPoint = BottomPoint;

            cln.TopPointIndex = TopPointIndex;
            cln.BottomPointIndex = BottomPointIndex;
            cln.Dirty = Dirty;
            return cln;
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }
    }
}