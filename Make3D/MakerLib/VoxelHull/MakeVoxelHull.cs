using Barnacle.Object3DLib;
using HullLibrary;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class VoxelHullMaker : MakerBase
    {
        private List<Point3D> dataPoints;

        public VoxelHullMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
            dataPoints = new List<Point3D>();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            foreach (var p in dataPoints)
            {
                Vertices.Add(new Point3D(p.X, p.Y, p.Z));
            }

            ConvexHullCalculator calc = new ConvexHullCalculator();
            calc.GeneratePoint3DHull(Vertices, Faces);
        }

        public void SetValues(List<Point3D> sourcePoints)
        {
            foreach (var p in sourcePoints)
            {
                dataPoints.Add(p);
            }
        }

        private void SetLimits()
        {
        }
    }
}