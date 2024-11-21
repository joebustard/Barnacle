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

using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    internal class VectorUtils
    {
        // Return vectors along the coordinate axes.
        public static Vector3D XVector(double length = 1)
        {
            return new Vector3D(length, 0, 0);
        }

        public static Vector3D YVector(double length = 1)
        {
            return new Vector3D(0, length, 0);
        }

        public static Vector3D ZVector(double length = 1)
        {
            return new Vector3D(0, 0, length);
        }
    }
}