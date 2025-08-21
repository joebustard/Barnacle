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

namespace Barnacle.Models
{
    public class Bounds2D
    {
        public double Bottom;
        public double Left;
        public double Right;
        public double Top;

        public Bounds2D()
        {
            Bottom = double.MaxValue;
            Left = double.MaxValue;
            Right = double.MinValue;
            Top = double.MinValue;
        }

        public void Adjust(double x, double y)
        {
            if (x < Left) { Left = x; }
            if (x > Right) { Right = x; }
            if (y > Top) { Top = y; }
            if (y < Bottom) { Bottom = y; }
        }

        public double Height()
        {
            return Top - Bottom;
        }

        public double Width()
        {
            return Right - Left;
        }

        internal Bounds2D Overlap(Bounds2D b)
        {
            Bounds2D result = null;

            bool over = false;
            if (Right > b.Left && Right < b.Right)
            {
                if (Top >= b.Bottom && Top <= b.Top)
                {
                    over = true;
                }
                else
                if (Bottom >= b.Bottom && Bottom <= b.Top)
                {
                    over = true;
                }
                else
                if (Bottom >= b.Bottom && Top <= b.Top)
                {
                    over = true;
                }
                else
                if (Bottom < b.Bottom && Top > b.Top)
                {
                    over = true;
                }
            }
            else if (Left > b.Left && Left < b.Right)
            {
                if (Top > b.Bottom && Top < b.Top)
                {
                    over = true;
                }
                else
                if (Bottom > b.Bottom && Bottom < b.Top)
                {
                    over = true;
                }
                else
                if (Bottom >= b.Bottom && Top <= b.Top)
                {
                    over = true;
                }
                else
                if (Bottom < b.Bottom && Top > b.Top)
                {
                    over = true;
                }
            }
            if (over)
            {
                result = new Bounds2D();
                result.Left = Math.Max(Left, b.Left);
                result.Right = Math.Min(Right, b.Right);
                result.Top = Math.Min(Top, b.Top);
                result.Bottom = Math.Max(Bottom, b.Bottom);
            }
            return result;
        }
    }
}