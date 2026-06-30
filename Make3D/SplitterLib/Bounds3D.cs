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
using System.Windows.Media.Media3D;

namespace SplitterLib
{
    internal class Bounds3D
    {
        private Point3D lower;

        private Point3D upper;

        public Bounds3D()
        {
            lower.X = double.MaxValue;
            lower.Y = double.MaxValue;
            lower.Z = double.MaxValue;

            upper.X = double.MinValue;
            upper.Y = double.MinValue;
            upper.Z = double.MinValue;
        }

        public Bounds3D(Bounds3D b)
        {
            lower.X = b.Lower.X;
            lower.Y = b.Lower.Y;
            lower.Z = b.Lower.Z;

            upper.X = b.Upper.X;
            upper.Y = b.Upper.Y;
            upper.Z = b.Upper.Z;
        }

        public double Depth
        {
            get
            {
                return Upper.Z - Lower.Z;
            }
        }

        public double Height
        {
            get
            {
                return Upper.Y - Lower.Y;
            }
        }

        public Point3D Lower
        {
            get
            {
                return lower;
            }
            set
            {
                lower = value;
            }
        }

        public Point3D Upper
        {
            get
            {
                return upper;
            }
            set
            {
                upper = value;
            }
        }

        public double Width
        {
            get
            {
                return Upper.X - Lower.X;
            }
        }

        public static Bounds3D operator +(Bounds3D a, Bounds3D b)
        {
            Bounds3D res = a;
            if (b != null)
            {
                if (b.lower.X != double.MaxValue)
                {
                    res.Adjust(b.Lower);
                    res.Adjust(b.Upper);
                }
            }
            return res;
        }

        public void Add(Bounds3D b)
        {
            if (b != null)
            {
                Adjust(b.Lower);
                Adjust(b.Upper);
            }
        }

        public void Adjust(Point3D ap)
        {
            if (ap.X < lower.X)
            {
                lower.X = ap.X;
            }
            if (ap.Y < lower.Y)
            {
                lower.Y = ap.Y;
            }
            if (ap.Z < lower.Z)
            {
                lower.Z = ap.Z;
            }

            if (ap.X > upper.X)
            {
                upper.X = ap.X;
            }
            if (ap.Y > upper.Y)
            {
                upper.Y = ap.Y;
            }
            if (ap.Z > upper.Z)
            {
                upper.Z = ap.Z;
            }
        }

        public void Expand(Point3D diff)
        {
            lower.X = lower.X - diff.X;
            lower.Y = lower.Y - diff.Y;
            lower.Z = lower.Z - diff.Z;

            upper.X = upper.X + diff.X;
            upper.Y = upper.Y + diff.Y;
            upper.Z = upper.Z + diff.Z;
        }

        public Point3D MidPoint()
        {
            Point3D res = new Point3D();
            res.X = lower.X + (upper.X - lower.X) / 2;
            res.Y = lower.Y + (upper.Y - lower.Y) / 2;
            res.Z = lower.Z + (upper.Z - lower.Z) / 2;
            return res;
        }

        public Point3D Size()
        {
            Point3D res = new Point3D();
            res.X = upper.X - lower.X;
            res.Y = upper.Y - lower.Y;
            res.Z = upper.Z - lower.Z;
            return res;
        }

        public void Zero()
        {
            lower.X = 0;
            lower.Y = 0;
            lower.Z = 0;

            upper.X = 0;
            upper.Y = 0;
            upper.Z = 0;
        }
    }
}