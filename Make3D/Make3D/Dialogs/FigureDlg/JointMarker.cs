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

using Barnacle.Object3DLib;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.Figure
{
    internal class JointMarker : Object3D
    {
        public delegate void BoneRotate(Bone bn);

        public BoneRotate OnBoneRotated { get; set; }

        public JointMarker()
        {
            Bone = null;
            Dirty = false;
        }

        public Bone Bone { get; set; }
        public bool Dirty { get; set; }

        public override void Rotate(Point3D rot)
        {
            System.Diagnostics.Debug.WriteLine($"ad rot {rot.X},{rot.Y},{rot.Z}");

            if (Bone != null)
            {
                Bone.XRot += rot.X;
                Bone.YRot += rot.Y;
                Bone.ZRot += rot.Z;
                if (OnBoneRotated != null)
                {
                    OnBoneRotated(Bone);
                }
                Dirty = true;
            }
        }
    }
}