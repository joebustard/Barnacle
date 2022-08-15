using System.Collections.Generic;

namespace asdflib
{
    public class AdaptiveSignedDistanceField
    {
        // caller must provide a way of calculating the distance to the solid
        public delegate float CalculateDistance(float x, float y, float z);

        public static CalculateDistance OnCalculateDistance;

        public static  List<FieldPoint> allFieldPoints;
        private FieldNode rootNode;

        public AdaptiveSignedDistanceField()
        {
            allFieldPoints = new List<FieldPoint>();
            rootNode = new FieldNode();
        }
        public static int AddFieldPoint(float x, float y, float z)
        {
            int res = -1;
            if (OnCalculateDistance != null)
            {
                FieldPoint p0 = new FieldPoint(x, y, z, OnCalculateDistance(x, y, z));
                allFieldPoints.Add(p0);
                res = allFieldPoints.Count - 1;
            }
            return res;
        }
        public void SetDimensions(float lowx, float lowy, float lowz, float highx, float highy, float highz)
        {
            if (OnCalculateDistance != null)
            {
                // set the coordinates of the root node from the bounds passed in
                // bottom
                FieldPoint p0 = new FieldPoint(lowx, lowy, lowz, OnCalculateDistance(lowx, lowy, lowz));
                FieldPoint p1 = new FieldPoint(lowx, lowy, highz, OnCalculateDistance(lowx, lowy, highz));
                FieldPoint p2 = new FieldPoint(highx, lowy, highz, OnCalculateDistance(lowx, lowy, highz));
                FieldPoint p3 = new FieldPoint(highx, lowy, lowz, OnCalculateDistance(lowx, lowy, highz));

                // top
                FieldPoint p4 = new FieldPoint(lowx, highy, lowz, OnCalculateDistance(lowx, lowy, lowz));
                FieldPoint p5 = new FieldPoint(lowx, highy, highz, OnCalculateDistance(lowx, lowy, highz));
                FieldPoint p6 = new FieldPoint(highx, highy, highz, OnCalculateDistance(lowx, lowy, highz));
                FieldPoint p7 = new FieldPoint(highx, highy, lowz, OnCalculateDistance(lowx, lowy, highz));

                float cx = lowx + (highx - lowx) / 2.0F;
                float cy = lowy + (highy - lowy) / 2.0F;
                float cz = lowz + (highz - lowz) / 2.0F;

                FieldPoint p8 = new FieldPoint(cx, cy, cz, OnCalculateDistance(cx, cy, cz));

                // just add these field points, no need to search for them
                allFieldPoints.Add(p0);
                allFieldPoints.Add(p1);
                allFieldPoints.Add(p2);
                allFieldPoints.Add(p3);
                allFieldPoints.Add(p4);
                allFieldPoints.Add(p5);
                allFieldPoints.Add(p6);
                allFieldPoints.Add(p7);
                allFieldPoints.Add(p8);

                rootNode.SetCorner(0, 0);
                rootNode.SetCorner(1, 1);
                rootNode.SetCorner(2, 2);
                rootNode.SetCorner(3, 3);
                rootNode.SetCorner(4, 4);
                rootNode.SetCorner(5, 5);
                rootNode.SetCorner(6, 6);
                rootNode.SetCorner(7, 7);

                rootNode.SetCentre(8);
            }
        }
    }
}