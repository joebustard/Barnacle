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

using System;

namespace asdflibrary
{
    public class Functions
    {
        public static float SphereRadius { get; set; }

        public static float Sphere(float x, float y, float z)
        {
            float v = (x * x) + (y * y) + (z * z);
            float d = (float)Math.Sqrt(v) - SphereRadius;
            return d;
        }

        public static float Sphere(double x, double y, double z)
        {
            return Sphere((float)x, (float)y, (float)z);
        }
    }
}