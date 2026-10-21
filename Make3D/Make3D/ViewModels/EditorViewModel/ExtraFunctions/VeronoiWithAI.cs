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

using AiWpf3D;
using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal class VeronoiWithAI
    {
        public static bool VeronoiObject(Object3D ob, Point3DCollection seeds)
        {
            bool res = true;
            /*
             * ob.Remesh();

            MeshGeometry3D inputMesh = ob.Mesh;
            MeshGeometry3D outputMesh = WpfLoopSubdivision.ApplyLoopSubdivision(inputMesh);
            ob.AbsoluteObjectVertices.Clear();
            ob.TriangleIndices.Clear();
            foreach (Point3D p in outputMesh.Positions)
            {
                ob.AbsoluteObjectVertices.Add(p);
            }
            foreach (int f in outputMesh.TriangleIndices)
            {
                ob.TriangleIndices.Add(f);
            }
            ob.AbsoluteToRelative();
            ob.Remesh();
            */
            return res;
        }
    }
}