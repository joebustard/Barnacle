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

using HalfEdgeLib;

using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.ClaySculpt
{
    internal class SculptingTool
    {
        private double piByTwo = Math.PI / 2.0;

        public SculptingTool()
        {
            Radius = 5;
            Scaler = 2;
            Inverse = false;
        }

        public bool Inverse
        {
            get; set;
        }

        public double Radius
        {
            get; set;
        }

        public double Scaler
        {
            get; set;
        }

        public bool ApplyTool(ToolSelectionContent content, Point3D centre)
        {
            bool res = true;
            content.SubdivideSelectedFaces();

            foreach (int vid in content.SelectedVertices)
            {
                Vertex xyz = content.GetVertex(vid);
                Vector3D pxyz = new Vector3D(xyz.X, xyz.Y, xyz.Z);
                Vector3D normal = content.GetVertexNormal(vid);
                // System.Diagnostics.Debug.WriteLine($" x={xyz.X},y={xyz.Y},z={xyz.Z}");
                // System.Diagnostics.Debug.WriteLine($" nx={normal.X},ny={normal.Y},nz={normal.Z}");
                pxyz.Normalize();

                double dot = Vector3D.DotProduct(pxyz, normal);
                //float sign = Math.Sign(dot);
                float sign = -1;
                if (Inverse)
                {
                    sign = -sign;
                }
                // System.Diagnostics.Debug.WriteLine($"dot={dot}");
                double d = content.Distance(centre, xyz);
                float force = sign * (float)Force(d);

                normal.X *= force;
                normal.Y *= force;
                normal.Z *= force;
                // System.Diagnostics.Debug.WriteLine($" offx={normal.X},offy={normal.Y},offz={normal.Z}");
                content.MoveVertex(normal, vid);
            }
            return res;
        }

        public virtual double Force(double v)
        {
            double res = 0;
            if (v > 0 && v <= Radius)
            {
                res = Math.Cos((v / Radius) * piByTwo) * Scaler;
            }
            return res;
        }
    }
}