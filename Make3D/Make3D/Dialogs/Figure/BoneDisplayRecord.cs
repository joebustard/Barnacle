﻿/**************************************************************************
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
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.Figure
{
    internal class BoneDisplayRecord
    {
        private Point3DCollection endJointPoints;

        private Point3DCollection startJointPoints;

        public BoneDisplayRecord()
        {
            ModelName = "bone";
            startJointPoints = new Point3DCollection();
            endJointPoints = new Point3DCollection();
        }

        public Bone Bone { get; set; }
        public Object3D DisplayObject { get; set; }

        public Point3DCollection EndJointPoints
        {
            get { return endJointPoints; }
        }

        public Point3D MarkerPosition { get; set; }

        public string ModelName { get; internal set; }

        public string Name { get; internal set; }

        // Absolute, Midpoint of the associated bone (i.e. after all rotations etc)
        public Point3D Position { get; set; }

        // accumulated rotations of all parents etc
        public Point3D Rotation { get; set; }

        public Scale3D Scale { get; set; }

        public Point3DCollection StartJointPoints
        {
            get { return startJointPoints; }
        }

        public List<BoneDisplayRecord> SubDisplayBones { get; set; }
        /*
        internal void SetJointPoints(Point3DCollection absoluteObjectVertices)
        {
            StartJointPoints.Clear();
            EndJointPoints.Clear();
            double l = Bone.Length;

            if (Bone.Height > l)
            {
                l = Bone.Height;
            }
            if (Bone.Width > l)
            {
                l = Bone.Width;
            }

            double minDist = l / 8;
            foreach (Point3D pnt in absoluteObjectVertices)
            {
                double dist = BaseModellerDialog.Distance3D(pnt, Bone.StartPosition);
                if (dist < minDist)
                {
                    StartJointPoints.Add(new Point3D(pnt.X, pnt.Y, pnt.Z));
                }

                dist = BaseModellerDialog.Distance3D(pnt, Bone.EndPosition);
                if (dist < minDist)
                {
                    EndJointPoints.Add(new Point3D(pnt.X, pnt.Y, pnt.Z));
                }
            }
        }
        */

        internal void SetJointEndPoints(Point3DCollection absoluteObjectVertices)
        {
            StartJointPoints.Clear();
            EndJointPoints.Clear();
            double l = Bone.Length;

            if (Bone.Height > l)
            {
                l = Bone.Height;
            }
            if (Bone.Width > l)
            {
                l = Bone.Width;
            }

            double halfDistance = BaseModellerDialog.Distance3D(Bone.StartPosition, Bone.EndPosition);
            double minDist = halfDistance / 5;
            foreach (Point3D pnt in absoluteObjectVertices)
            {
                /*
                double dist = BaseModellerDialog.Distance3D(pnt, Bone.StartPosition);
                if (dist < minDist)
                {
                    StartJointPoints.Add(new Point3D(pnt.X, pnt.Y, pnt.Z));
                }
                */

                double dist = BaseModellerDialog.Distance3D(pnt, Bone.EndPosition);
                if (dist < minDist)
                {
                    EndJointPoints.Add(new Point3D(pnt.X, pnt.Y, pnt.Z));
                }
            }
        }

        internal void SetJointStartPoints(Point3D org, Point3DCollection absoluteObjectVertices)
        {
            StartJointPoints.Clear();

            double halfDistance = BaseModellerDialog.Distance3D(Bone.StartPosition, Bone.EndPosition);
            double minDist = halfDistance / 4;
            foreach (Point3D pnt in absoluteObjectVertices)
            {
                double dist = BaseModellerDialog.Distance3D(pnt, org);
                if (dist < minDist)
                {
                    StartJointPoints.Add(new Point3D(pnt.X, pnt.Y, pnt.Z));
                }
            }
        }
    }
}