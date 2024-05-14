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
    public class FieldPoint
    {
        public float x;
        public float y;
        public float z;
        public float v;

        public FieldPoint()
        {
            x = 0;
            y = 0;
            z = 0;
            v = 0;
        }

        public FieldPoint(float x, float y, float z, float v)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.v = v;
        }

        public static FieldPoint operator +(FieldPoint v1, FieldPoint v2)
        {
            return new FieldPoint(v1.x + v2.x,
                                   v1.y + v2.y,
                                   v1.z + v2.z,
                                   v1.v + v2.v);
        }

        public static FieldPoint operator -(FieldPoint v1, FieldPoint v2)
        {
            return new FieldPoint(v1.x - v2.x,
                                   v1.y - v2.y,
                                   v1.z - v2.z,
                                   v1.v - v2.v);
        }

        public const float tol = 0.000001F;

        public static bool operator ==(FieldPoint v1, FieldPoint v2)
        {
            bool result = false;
            if (Math.Abs(v1.x - v2.x) < tol)
            {
                if (Math.Abs(v1.y - v2.y) < tol)
                {
                    if (Math.Abs(v1.z - v2.z) < tol)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public static bool operator !=(FieldPoint v1, FieldPoint v2)
        {
            bool result = false;
            if ((Math.Abs(v1.x - v2.x) >= tol) ||
                 (Math.Abs(v1.y - v2.y) >= tol) ||
                 (Math.Abs(v1.z - v2.z) >= tol))
            {
                result = true;
            }

            return result;
        }

        public override bool Equals(object fp)
        {
            if (fp == null)
                return false;
            if (GetType() != fp.GetType())
                return false;
            var fp1 = (FieldPoint)fp;
            return (x == fp1.x) && (y == fp1.y) && (z == fp1.z);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool At(float x, float y, float z)
        {
            bool result = false;
            if (Math.Abs(this.x - x) < tol)
            {
                if (Math.Abs(this.y - y) < tol)
                {
                    if (Math.Abs(this.z - z) < tol)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public void Dump(string insert = "")
        {
            System.Diagnostics.Debug.WriteLine($"{insert} x {x} ,y {y} ,z {z}, v {v}");
        }
    }
}