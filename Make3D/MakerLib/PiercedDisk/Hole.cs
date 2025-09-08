// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MakerLib
{
    public class Hole
    {
        public Hole()
        {
        }

        public double AngleFromDiskCentre
        {
            get; set;
        }

        public Point Centre
        {
            get; set;
        }

        public List<Point> InnerPoints
        {
            get;
            set;
        }

        public List<Point> OuterPoints
        {
            get;
            set;
        }

        public static Point Rotate(Point p1, double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            // Rotate point
            double xnew = p1.X * cos - p1.Y * sin;
            double ynew = p1.X * sin + p1.Y * cos;

            // Translate point back
            Point newPoint = new Point(xnew, ynew);
            return newPoint;
        }

        public void SetPoints(List<Point> sourceOuter, List<Point> sourceInner)
        {
            // make a fresh copy of the source points
            OuterPoints = new List<Point>();
            foreach (Point sp in sourceOuter)
            {
                System.Windows.Point rp = Rotate(sp, AngleFromDiskCentre);
                OuterPoints.Add(new Point(rp.X + Centre.X, rp.Y + Centre.Y));
            }
            InnerPoints = new List<Point>();
            foreach (Point sp in sourceInner)
            {
                System.Windows.Point rp = Rotate(sp, AngleFromDiskCentre);

                InnerPoints.Insert(0, new Point(rp.X + Centre.X, rp.Y + Centre.Y));
            }
        }
    }
}