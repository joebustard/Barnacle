/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

//using OpenToolkit.Mathematics;

using System.Windows.Media.Media3D;

namespace CSGLib
{
    public class Plane
    {
        public double DistanceToPlaneFromOrigin;
        public Vector3D PlaneNormal;
        private const double TreatAsZero = .0000000001;

        public Plane(Vector3D planeNormal, double distanceFromOrigin)
        {
            PlaneNormal = Normalized(planeNormal);
            DistanceToPlaneFromOrigin = distanceFromOrigin;
        }

        private Vector3D Normalized(Vector3D v)
        {
            Vector3D v2 = new Vector3D(v.X, v.Y, v.Z);
            v2.Normalize();
            return v2;
        }

        public Plane(Vector3D point0, Vector3D point1, Vector3D point2)
        {
            PlaneNormal = Normalized(Vector3D.CrossProduct(point1 - point0, point2 - point0));
            DistanceToPlaneFromOrigin = Vector3D.DotProduct(PlaneNormal, point0);
        }

        public Plane(Vector3D planeNormal, Vector3D pointOnPlane)
        {
            PlaneNormal = Normalized(planeNormal);
            DistanceToPlaneFromOrigin = Vector3D.DotProduct(planeNormal, pointOnPlane);
        }

        public double GetDistanceFromPlane(Vector3D positionToCheck)
        {
            double distanceToPointFromOrigin = Vector3D.DotProduct(positionToCheck, PlaneNormal);
            return distanceToPointFromOrigin - DistanceToPlaneFromOrigin;
        }

        public double GetDistanceToIntersection(Ray ray, out bool inFront)
        {
            inFront = false;
            double normalDotRayDirection = Vector3D.DotProduct(PlaneNormal, ray.DirectionNormal);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return double.PositiveInfinity;
            }

            if (normalDotRayDirection < 0)
            {
                inFront = true;
            }

            return (DistanceToPlaneFromOrigin - Vector3D.DotProduct(PlaneNormal, ray.Origin)) / normalDotRayDirection;
        }

        public double GetDistanceToIntersection(Vector3D pointOnLine, Vector3D lineDirection)
        {
            double normalDotRayDirection = Vector3D.DotProduct(PlaneNormal, lineDirection);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return double.PositiveInfinity;
            }

            double planeNormalDotPointOnLine = Vector3D.DotProduct(PlaneNormal, pointOnLine);
            return (DistanceToPlaneFromOrigin - planeNormalDotPointOnLine) / normalDotRayDirection;
        }

        public bool RayHitPlane(Ray ray, out double distanceToHit, out bool hitFrontOfPlane)
        {
            distanceToHit = double.PositiveInfinity;
            hitFrontOfPlane = false;

            double normalDotRayDirection = Vector3D.DotProduct(PlaneNormal, ray.DirectionNormal);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return false;
            }

            if (normalDotRayDirection < 0)
            {
                hitFrontOfPlane = true;
            }

            double distanceToRayOriginFromOrigin = Vector3D.DotProduct(PlaneNormal, ray.Origin);

            double distanceToPlaneFromRayOrigin = DistanceToPlaneFromOrigin - distanceToRayOriginFromOrigin;

            bool originInFrontOfPlane = distanceToPlaneFromRayOrigin < 0;

            bool originAndHitAreOnSameSide = originInFrontOfPlane == hitFrontOfPlane;
            if (!originAndHitAreOnSameSide)
            {
                return false;
            }

            distanceToHit = distanceToPlaneFromRayOrigin / normalDotRayDirection;
            return true;
        }

        public bool LineHitPlane(Vector3D start, Vector3D end, out Vector3D intersectionPosition)
        {
            double distanceToStartFromOrigin = Vector3D.DotProduct(PlaneNormal, start);
            if (distanceToStartFromOrigin == 0)
            {
                intersectionPosition = start;
                return true;
            }

            double distanceToEndFromOrigin = Vector3D.DotProduct(PlaneNormal, end);
            if (distanceToEndFromOrigin == 0)
            {
                intersectionPosition = end;
                return true;
            }

            if ((distanceToStartFromOrigin < 0 && distanceToEndFromOrigin > 0)
                || (distanceToStartFromOrigin > 0 && distanceToEndFromOrigin < 0))
            {
                Vector3D direction = Normalized((end - start));

                double startDistanceFromPlane = distanceToStartFromOrigin - DistanceToPlaneFromOrigin;
                double endDistanceFromPlane = distanceToEndFromOrigin - DistanceToPlaneFromOrigin;
                double lengthAlongPlanNormal = endDistanceFromPlane - startDistanceFromPlane;

                double ratioToPlanFromStart = startDistanceFromPlane / lengthAlongPlanNormal;
                intersectionPosition = start + direction * ratioToPlanFromStart;

                return true;
            }

            intersectionPosition = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            return false;
        }
    }
}