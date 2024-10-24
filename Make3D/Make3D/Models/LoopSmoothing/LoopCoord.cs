﻿namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopCoord
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public LoopCoord()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public LoopCoord(double v1, double v2, double v3)
        {
            X = v1;
            Y = v2;
            Z = v3;
        }

        public static LoopCoord operator +(LoopCoord a, LoopCoord b)
        {
            LoopCoord res = new LoopCoord();
            res.X = a.X + b.X;
            res.Y = a.Y + b.Y;
            res.Z = a.Z + b.Z;
            return res;
        }
    }
}