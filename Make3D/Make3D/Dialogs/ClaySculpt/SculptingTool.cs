﻿using Plankton;
using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.ClaySculpt
{
    internal class SculptingTool
    {
        private double piByTwo = Math.PI / 2.0;

        public SculptingTool()
        {
            Radius = 10;
            Scaler = 1;
            Inverse = false;
        }

        public bool Inverse { get; set; }
        public double Radius { get; set; }
        public double Scaler { get; set; }

        public virtual double Force(double v)
        {
            double res = 0;
            if (v > 0 && v <= Radius)
            {
                res = Math.Cos((v / Radius) * piByTwo) * Scaler;
            }
            return res;
        }

        public bool ApplyTool(ToolSelectionContent content, Point3D centre)
        {
            bool res = true;
             content.SubdivideSelectedFaces();
            foreach (int vid in content.SelectedVertices)
            {
                PlanktonVertex xyz = content.GetVertex(vid);
                PlanktonXYZ pxyz = new PlanktonXYZ(xyz.X, xyz.Y, xyz.Z);

                PlanktonXYZ normal = content.GetVertexNormal(vid);
                System.Diagnostics.Debug.WriteLine($" x={xyz.X},y={xyz.Y},z={xyz.Z}");
                System.Diagnostics.Debug.WriteLine($" nx={normal.X},ny={normal.Y},nz={normal.Z}");
                pxyz.Normalize();

                double dot = (double)PlanktonXYZ.DotProduct(pxyz, normal);
                float sign = Math.Sign(dot);
                if (Inverse)
                {
                    sign = -sign;
                }
                System.Diagnostics.Debug.WriteLine($"dot={dot}");
                double d = content.Distance(centre, xyz);
                float force = sign * (float)Force(d);
                normal.X *= force;
                normal.Y *= force;
                normal.Z *= force;
                System.Diagnostics.Debug.WriteLine($" offx={normal.X},offy={normal.Y},offz={normal.Z}");
                content.MoveVertex(normal, vid);
            }
            return res;
        }
    }
}