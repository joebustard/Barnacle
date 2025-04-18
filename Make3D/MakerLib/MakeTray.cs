using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TrayMaker : MakerBase
    {
        private double baseLength;
        private double baseWidth;
        private double topLength;
        private double topWidth;
        private double trayHeight;
        private double wallThickness;

        public TrayMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            int[] pointIds = new int[16];
            double bl = baseLength / 2;
            double bw = baseWidth / 2;
            double tl = topLength / 2;
            double tw = topWidth / 2;
            Bounds3D bounds = new Bounds3D();
            bounds.Adjust(new Point3D(-bl, -1, -bw));
            bounds.Adjust(new Point3D(bl, -1, bw));
            bounds.Adjust(new Point3D(-tl, trayHeight + 1, -tw));
            bounds.Adjust(new Point3D(tl, trayHeight + 1, tw));

            CreateOctree(bounds.Lower, bounds.Upper);

            pointIds[0] = AddVerticeOctTree(-bl, 0, bw);
            pointIds[1] = AddVerticeOctTree(-bl, 0, -bw);
            pointIds[2] = AddVerticeOctTree(bl, 0, -bw);
            pointIds[3] = AddVerticeOctTree(bl, 0, bw);

            pointIds[4] = AddVerticeOctTree(-tl, trayHeight, tw);
            pointIds[5] = AddVerticeOctTree(-tl, trayHeight, -tw);
            pointIds[6] = AddVerticeOctTree(tl, trayHeight, -tw);
            pointIds[7] = AddVerticeOctTree(tl, trayHeight, tw);

            // bottom of base
            AddFace(0, 1, 2);
            AddFace(0, 2, 3);

            // left outside
            AddFace(0, 4, 1);
            AddFace(1, 4, 5);

            // back outside
            AddFace(2, 1, 5);
            AddFace(2, 5, 6);

            // right outside
            AddFace(2, 6, 7);
            AddFace(2, 7, 3);

            // front outside
            AddFace(0, 3, 7);
            AddFace(0, 7, 4);

            tl = tl - wallThickness;
            tw = tw - wallThickness;
            bl = bl - wallThickness;
            bw = bw - wallThickness;

            pointIds[8] = AddVerticeOctTree(-bl, wallThickness, bw);
            pointIds[9] = AddVerticeOctTree(-bl, wallThickness, -bw);
            pointIds[10] = AddVerticeOctTree(bl, wallThickness, -bw);
            pointIds[11] = AddVerticeOctTree(bl, wallThickness, bw);

            pointIds[12] = AddVerticeOctTree(-tl, trayHeight, tw);
            pointIds[13] = AddVerticeOctTree(-tl, trayHeight, -tw);
            pointIds[14] = AddVerticeOctTree(tl, trayHeight, -tw);
            pointIds[15] = AddVerticeOctTree(tl, trayHeight, tw);

            // top of base
            AddInverseFace(8, 9, 10);
            AddInverseFace(8, 10, 11);

            // left inside
            AddInverseFace(8, 12, 9);
            AddInverseFace(9, 12, 13);

            // back inside
            AddInverseFace(9, 13, 10);
            AddInverseFace(10, 13, 14);

            // right inside
            AddInverseFace(10, 14, 15);
            AddInverseFace(10, 15, 11);

            // front inside
            AddInverseFace(8, 11, 15);
            AddInverseFace(8, 15, 12);

            // close top
            AddFace(5, 4, 12);
            AddFace(5, 12, 13);

            AddFace(6, 5, 13);
            AddFace(6, 13, 14);

            AddFace(7, 6, 14);
            AddFace(7, 14, 15);

            AddFace(4, 7, 15);
            AddFace(4, 15, 12);
        }

        public void SetValues(double topLength, double topWidth, double baseLength, double baseWidth, double trayHeight, double wallThickness
                                      )
        {
            this.topLength = topLength;
            this.topWidth = topWidth;
            this.baseLength = baseLength;
            this.baseWidth = baseWidth;
            this.trayHeight = trayHeight;
            this.wallThickness = wallThickness;
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("TopLength", 0.1, 200);
            paramLimits.AddLimit("TopWidth", 0.1, 200);
            paramLimits.AddLimit("BaseLength", 0.1, 200);
            paramLimits.AddLimit("BaseWidth", 0.1, 200);
            paramLimits.AddLimit("TrayHeight", 0.1, 200);
            paramLimits.AddLimit("WallThickness", 0.1, 100);
        }
    }
}