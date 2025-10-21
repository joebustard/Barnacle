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

using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Object3DLib
{
    public class ObjectDropper
    {
        public void DropItems(string dir, Object3D fixedObject, Object3D movingObject)
        {
            switch (dir.ToLower())
            {
                case "above":
                    {
                        DropFromAbove(fixedObject, movingObject);
                    }
                    break;

                case "below":
                    {
                        DropFromBelow(fixedObject, movingObject);
                    }
                    break;

                case "right":
                    {
                        DropFromRight(fixedObject, movingObject);
                    }
                    break;

                case "left":
                    {
                        DropFromLeft(fixedObject, movingObject);
                    }
                    break;

                case "front":
                    {
                        DropFromFront(fixedObject, movingObject);
                    }
                    break;

                case "back":
                    {
                        DropFromBack(fixedObject, movingObject);
                    }
                    break;
            }
        }

        private Vector3D BaryCentric(Vector3D v0, Vector3D v1, Vector3D v2, double u, double v, double w)
        {
            Vector3D res = new Vector3D();
            res.X = (u * v0.X) + (v * v1.X) + (w * v2.X);
            res.Y = (u * v0.Y) + (v * v1.Y) + (w * v2.Y);
            res.Z = (u * v0.Z) + (v * v1.Z) + (w * v2.Z);
            return res;
        }

        private void DropFromAbove(Object3D baseOb, Object3D ob)
        {
            double maxMove = double.MaxValue;
            Vector3D rayDirection = new Vector3D(0, 1, 0);

            baseOb.CalculateAbsoluteBounds();
            ob.CalculateAbsoluteBounds();

            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetHorizontalTriangleBounds2D(baseOb, baseTriBounds);

            // Move the object above the base one
            Point3D originalPos = ob.Position;
            double dAbsY = baseOb.AbsoluteBounds.Upper.Y + ob.AbsoluteBounds.Height + 10.0;

            ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetHorizontalTriangleBounds2D(ob, movBounds);

            double rayY = 0;
            rayY = Math.Min(rayY, ob.AbsoluteBounds.Lower.Y - 10);
            rayY = Math.Min(rayY, baseOb.AbsoluteBounds.Lower.Y - 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // so tri angle movIndex is above triangle baseIndex in the area given by overlap
                        // Shoot a ray up from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap

                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        // pass ray through triangle corners
                        Vector3D rayOrigin = new Vector3D(v0.X, rayY, v0.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v1.X, rayY, v1.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v2.X, rayY, v2.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }
            MoveToNewPos(ob, ob.Position.X, ob.Position.Y - maxMove - 0.1, ob.Position.Z, maxMove, originalPos);
        }

        private void DropFromBack(Object3D baseOb, Object3D ob)
        {
            Bounds3D bns = new Bounds3D(baseOb.AbsoluteBounds);
            Vector3D rayDirection = new Vector3D(0, 0, -1);
            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetVerticalTriangleBounds2D(baseOb, baseTriBounds);
            double maxMove = double.MaxValue;
            // Move the object above the base one
            Point3D originalPos = ob.Position;
            double zOff = bns.Lower.Z - ob.AbsoluteBounds.Depth - 10.0;

            ob.Position = new Point3D(ob.Position.X, ob.Position.Y, zOff);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetVerticalTriangleBounds2D(ob, movBounds);

            double rayZ = 0;
            rayZ = Math.Max(rayZ, ob.AbsoluteBounds.Upper.Z + 10);
            rayZ = Math.Max(rayZ, baseOb.AbsoluteBounds.Upper.Z + 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // so tri angle movIndex is above triangle baseIndex in the area given by overlap
                        // Shoot a ray up from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap

                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        // pass ray through triangle corners
                        Vector3D rayOrigin = new Vector3D(v0.X, v0.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v1.X, v1.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v2.X, v2.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }
            MoveToNewPos(ob, ob.Position.X, ob.Position.Y, ob.Position.Z + maxMove + 0.1, maxMove, originalPos);
        }

        private void DropFromBelow(Object3D baseOb, Object3D ob)
        {
            Bounds3D bns = new Bounds3D(baseOb.AbsoluteBounds);
            Vector3D rayDirection = new Vector3D(0, -1, 0);
            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetHorizontalTriangleBounds2D(baseOb, baseTriBounds);
            double maxMove = double.MaxValue;
            Point3D originalPos = ob.Position;

            // Move the object below the base one
            double dAbsY = bns.Lower.Y - ob.AbsoluteBounds.Height - 10.0;

            ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetHorizontalTriangleBounds2D(ob, movBounds);
            double rayY = ob.AbsoluteBounds.Upper.Y + 10;
            rayY = Math.Max(rayY, baseOb.AbsoluteBounds.Upper.Y + 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // so tri angle movIndex is above triangle baseIndex in the area given by overlap
                        // Shoot a ray up from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap
                        double obDistance = 0;
                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        Vector3D rayOrigin = new Vector3D(v0.X, rayY, v0.Z);
                        obDistance = ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v1.X, rayY, v1.Z);
                        obDistance = ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v2.X, rayY, v2.Z);
                        obDistance = ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(bc.X, rayY, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }

            MoveToNewPos(ob, ob.Position.X, ob.Position.Y + maxMove + 0.1, ob.Position.Z, maxMove, originalPos);
        }

        private void DropFromFront(Object3D baseOb, Object3D ob)
        {
            Bounds3D bns = new Bounds3D(baseOb.AbsoluteBounds);
            Vector3D rayDirection = new Vector3D(0, 0, 1);
            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetVerticalTriangleBounds2D(baseOb, baseTriBounds);
            double maxMove = double.MaxValue;
            // Move the object above the base one
            Point3D originalPos = ob.Position;
            double zOff = bns.Upper.Z + ob.AbsoluteBounds.Depth + 10.0;

            ob.Position = new Point3D(ob.Position.X, ob.Position.Y, zOff);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetVerticalTriangleBounds2D(ob, movBounds);

            double rayZ = 0;
            rayZ = Math.Min(rayZ, ob.AbsoluteBounds.Lower.Z - 10);
            rayZ = Math.Min(rayZ, baseOb.AbsoluteBounds.Lower.Z - 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // so tri angle movIndex is above triangle baseIndex in the area given by overlap
                        // Shoot a ray up from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap

                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        // pass ray through triangle corners
                        Vector3D rayOrigin = new Vector3D(v0.X, v0.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v1.X, v1.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(v2.X, v2.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(bc.X, bc.Y, rayZ);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }
            MoveToNewPos(ob, ob.Position.X, ob.Position.Y, ob.Position.Z - maxMove - 0.1, maxMove, originalPos);
        }

        private void DropFromLeft(Object3D baseOb, Object3D ob)
        {
            Bounds3D bns = new Bounds3D(baseOb.AbsoluteBounds);
            Vector3D rayDirection = new Vector3D(-1, 0, 0);
            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetDistalTriangleBounds2D(baseOb, baseTriBounds);

            double maxMove = double.MaxValue;
            // Move the object above the base one
            Point3D originalPos = ob.Position;
            double dAbsX = bns.Lower.X - ob.AbsoluteBounds.Width - 10.0;

            ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetDistalTriangleBounds2D(ob, movBounds);
            double rayX = 0;
            rayX = Math.Max(rayX, ob.AbsoluteBounds.Upper.X + 10);
            rayX = Math.Max(rayX, baseOb.AbsoluteBounds.Upper.X + 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // Shoot a ray left from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap
                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        // pass ray through triangle corners
                        Vector3D rayOrigin = new Vector3D(rayX, v0.Y, v0.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(rayX, v1.Y, v1.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(rayX, v2.Y, v2.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }
            MoveToNewPos(ob, ob.Position.X + maxMove + 0.1, ob.Position.Y, ob.Position.Z, maxMove, originalPos);
        }

        private void DropFromRight(Object3D baseOb, Object3D ob)
        {
            Vector3D rayDirection = new Vector3D(1, 0, 0);
            Bounds3D bns = new Bounds3D(baseOb.AbsoluteBounds);
            List<Bounds2D> baseTriBounds = new List<Bounds2D>();
            GetDistalTriangleBounds2D(baseOb, baseTriBounds);

            double maxMove = double.MaxValue;
            // Move the object clear of  the base one
            Point3D originalPos = ob.Position;
            double dAbsX = bns.Upper.X + ob.AbsoluteBounds.Width + 10.0;
            ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
            ob.Remesh();

            List<Bounds2D> movBounds = new List<Bounds2D>();
            GetDistalTriangleBounds2D(ob, movBounds);

            double rayX = 0;
            rayX = Math.Min(rayX, ob.AbsoluteBounds.Lower.X - 10);
            rayX = Math.Min(rayX, baseOb.AbsoluteBounds.Lower.X - 10);
            for (int movIndex = 0; movIndex < movBounds.Count; movIndex++)
            {
                for (int baseIndex = 0; baseIndex < baseTriBounds.Count; baseIndex++)
                {
                    Bounds2D overlap = baseTriBounds[baseIndex].Overlap(movBounds[movIndex]);
                    if (overlap != null)
                    {
                        // Shoot a ray left from each corner of the over lap
                        // WE expect the ray should hit both tri angles at at least three of the corners of the overlap
                        double baseDistance = 0;

                        int faceIndex = movIndex * 3;
                        int f0 = ob.TriangleIndices[faceIndex];
                        int f1 = ob.TriangleIndices[faceIndex + 1];
                        int f2 = ob.TriangleIndices[faceIndex + 2];

                        Vector3D v0 = new Vector3D(ob.AbsoluteObjectVertices[f0].X, ob.AbsoluteObjectVertices[f0].Y, ob.AbsoluteObjectVertices[f0].Z);
                        Vector3D v1 = new Vector3D(ob.AbsoluteObjectVertices[f1].X, ob.AbsoluteObjectVertices[f1].Y, ob.AbsoluteObjectVertices[f1].Z);
                        Vector3D v2 = new Vector3D(ob.AbsoluteObjectVertices[f2].X, ob.AbsoluteObjectVertices[f2].Y, ob.AbsoluteObjectVertices[f2].Z);

                        faceIndex = baseIndex * 3;
                        int f3 = baseOb.TriangleIndices[faceIndex];
                        int f4 = baseOb.TriangleIndices[faceIndex + 1];
                        int f5 = baseOb.TriangleIndices[faceIndex + 2];

                        Vector3D v3 = new Vector3D(baseOb.AbsoluteObjectVertices[f3].X, baseOb.AbsoluteObjectVertices[f3].Y, baseOb.AbsoluteObjectVertices[f3].Z);
                        Vector3D v4 = new Vector3D(baseOb.AbsoluteObjectVertices[f4].X, baseOb.AbsoluteObjectVertices[f4].Y, baseOb.AbsoluteObjectVertices[f4].Z);
                        Vector3D v5 = new Vector3D(baseOb.AbsoluteObjectVertices[f5].X, baseOb.AbsoluteObjectVertices[f5].Y, baseOb.AbsoluteObjectVertices[f5].Z);

                        // pass ray through triangle corners
                        Vector3D rayOrigin = new Vector3D(rayX, v0.Y, v0.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(rayX, v1.Y, v1.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                        rayOrigin = new Vector3D(rayX, v2.Y, v2.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through barycentre
                        Vector3D bc = BaryCentric(v0, v1, v2, 0.333333, 0.333333, 0.333333);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        // pass ray through three other sample points
                        bc = BaryCentric(v0, v1, v2, 0.2, 0.4, 0.4);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.2, 0.4);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);

                        bc = BaryCentric(v0, v1, v2, 0.4, 0.4, 0.2);
                        rayOrigin = new Vector3D(rayX, bc.Y, bc.Z);
                        ObjectDropGap(ref maxMove, ref baseDistance, rayDirection, v0, v1, v2, v3, v4, v5, rayOrigin);
                    }
                }
            }

            MoveToNewPos(ob, ob.Position.X - maxMove - 0.1, ob.Position.Y, ob.Position.Z, maxMove, originalPos);
        }

        private void GetDistalTriangleBounds2D(Object3D object3D, List<Bounds2D> bounds)
        {
            bounds.Clear();
            int i = 0;
            while (i + 2 < object3D.TriangleIndices.Count)
            {
                Bounds2D bounds2D = new Bounds2D();

                for (int j = 0; j < 3; j++)
                {
                    int f = object3D.TriangleIndices[i + j];
                    bounds2D.Adjust(object3D.AbsoluteObjectVertices[f].Z, object3D.AbsoluteObjectVertices[f].Y);
                }
                bounds.Add(bounds2D);
                i += 3;
            }
        }

        private void GetHorizontalTriangleBounds2D(Object3D object3D, List<Bounds2D> bounds)
        {
            bounds.Clear();
            int i = 0;
            while (i + 2 < object3D.TriangleIndices.Count)
            {
                Bounds2D bounds2D = new Bounds2D();

                for (int j = 0; j < 3; j++)
                {
                    int f = object3D.TriangleIndices[i + j];
                    bounds2D.Adjust(object3D.AbsoluteObjectVertices[f].X, object3D.AbsoluteObjectVertices[f].Z);
                }
                bounds.Add(bounds2D);
                i += 3;
            }
        }

        private void GetVerticalTriangleBounds2D(Object3D object3D, List<Bounds2D> bounds)
        {
            bounds.Clear();
            int i = 0;
            while (i + 2 < object3D.TriangleIndices.Count)
            {
                Bounds2D bounds2D = new Bounds2D();

                for (int j = 0; j < 3; j++)
                {
                    int f = object3D.TriangleIndices[i + j];
                    bounds2D.Adjust(object3D.AbsoluteObjectVertices[f].X, object3D.AbsoluteObjectVertices[f].Y);
                }
                bounds.Add(bounds2D);
                i += 3;
            }
        }

        private void MoveToNewPos(Object3D ob, double x, double y, double z, double maxMove, Point3D originalPos)
        {
            if (maxMove < double.MaxValue)
            {
                ob.Position = new Point3D(x, y, z);
                ob.Remesh();
            }
            else
            {
                // Cant drop so put back in place
                ob.Position = originalPos;
                ob.Remesh();
            }
        }

        private double ObjectDropGap(ref double maxMove, ref double baseDistance, Vector3D rayDirection, Vector3D v0, Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4, Vector3D v5, Vector3D rayOrigin)
        {
            double obDistance;
            if (Utils.RayTriangleIntersect(rayOrigin, rayDirection,
                                                              v0, v1, v2, out obDistance))
            {
                if (Utils.RayTriangleIntersect(rayOrigin, rayDirection,
                                 v3, v4, v5, out baseDistance))
                {
                    if (obDistance >= baseDistance)
                    {
                        double canMove = obDistance - baseDistance;
                        if (maxMove > canMove)
                        {
                            maxMove = canMove;
                        }
                    }
                }
            }

            return obDistance;
        }
    }
}