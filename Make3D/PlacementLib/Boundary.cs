﻿using System.Windows;

namespace PlacementLib
{
    public class Boundary
    {
        public Point Lower { get; set; }
        public Point Upper { get; set; }
        private double area;

        public double Area
        {
            get { return area; }
        }

        public Boundary(Point l, Point u)
        {
            Lower = l;
            Upper = u;

            area = 0;
            if (u != null && l != null)
            {
                area = (Upper.X - Lower.X) * (Upper.Y - Lower.Y);
            }
        }
    }
}