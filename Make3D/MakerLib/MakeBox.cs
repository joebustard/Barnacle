using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BoxMaker : MakerBase
    {
        private double backThickness;
        private double baseThickness;
        private double boxHeight;
        private double boxLength;
        private double boxWidth;
        private double frontThickness;
        private double leftThickness;
        private double rightThickness;

        public BoxMaker(double boxLength, double boxHeight, double boxWidth, double baseThickness, double leftThickness, double rightThickness, double frontThickness, double backThickness)
        {
            this.boxLength = boxLength;
            this.boxHeight = boxHeight;
            this.boxWidth = boxWidth;
            this.baseThickness = baseThickness;
            this.leftThickness = leftThickness;
            this.rightThickness = rightThickness;
            this.frontThickness = frontThickness;
            this.backThickness = backThickness;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double bl = boxLength;
            double bh = boxHeight;
            double bw = boxWidth;
            double ft = frontThickness;
            double lt = leftThickness;
            double rt = rightThickness;
            double bs = baseThickness;
            double bk = backThickness;
            int p0 = AddVertice(0, 0, 0);
            int p1 = AddVertice(0, 0, bw);
            int p2 = AddVertice(bl, 0, bw);
            int p3 = AddVertice(bl, 0, 0);

            // bottom
            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);

            Faces.Add(p0);
            Faces.Add(p3);
            Faces.Add(p2);

            int p4 = AddVertice(0, bh, 0);
            int p5 = AddVertice(0, bh, bw);
            int p6 = AddVertice(bl, bh, bw);
            int p7 = AddVertice(bl, bh, 0);

            //left
            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p5);

            Faces.Add(p0);
            Faces.Add(p5);
            Faces.Add(p4);

            //right
            Faces.Add(p2);
            Faces.Add(p3);
            Faces.Add(p7);

            Faces.Add(p2);
            Faces.Add(p7);
            Faces.Add(p6);

            // back
            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p4);

            Faces.Add(p3);
            Faces.Add(p4);
            Faces.Add(p7);

            // front
            Faces.Add(p1);
            Faces.Add(p6);
            Faces.Add(p5);

            Faces.Add(p1);
            Faces.Add(p2);
            Faces.Add(p6);

            int p8 = AddVertice(lt, bs, bk);
            int p9 = AddVertice(lt, bs, bw - ft);
            int p10 = AddVertice(bl - rt, bs, bw - ft);
            int p11 = AddVertice(bl - rt, bs, bk);
            Faces.Add(p8);
            Faces.Add(p9);
            Faces.Add(p10);

            Faces.Add(p8);
            Faces.Add(p10);
            Faces.Add(p11);

            int p12 = AddVertice(lt, bh, bk);
            int p13 = AddVertice(lt, bh, bw - ft);
            int p14 = AddVertice(bl - rt, bh, bw - ft);
            int p15 = AddVertice(bl - rt, bh, bk);

            // left
            Faces.Add(p8);
            Faces.Add(p13);
            Faces.Add(p9);

            Faces.Add(p8);
            Faces.Add(p12);
            Faces.Add(p13);

            // right
            Faces.Add(p10);
            Faces.Add(p15);
            Faces.Add(p11);

            Faces.Add(p10);
            Faces.Add(p14);
            Faces.Add(p15);

            // back
            Faces.Add(p11);
            Faces.Add(p12);
            Faces.Add(p8);

            Faces.Add(p11);
            Faces.Add(p15);
            Faces.Add(p12);

            // front
            Faces.Add(p9);
            Faces.Add(p14);
            Faces.Add(p10);

            Faces.Add(p9);
            Faces.Add(p13);
            Faces.Add(p14);

            // close top
            // left
            Faces.Add(p4);
            Faces.Add(p5);
            Faces.Add(p13);

            Faces.Add(p4);
            Faces.Add(p13);
            Faces.Add(p12);

            //right
            Faces.Add(p7);
            Faces.Add(p15);
            Faces.Add(p14);

            Faces.Add(p7);
            Faces.Add(p14);
            Faces.Add(p6);

            // back
            Faces.Add(p4);
            Faces.Add(p12);
            Faces.Add(p15);

            Faces.Add(p4);
            Faces.Add(p15);
            Faces.Add(p7);

            Faces.Add(p5);
            Faces.Add(p6);
            Faces.Add(p14);

            Faces.Add(p5);
            Faces.Add(p14);
            Faces.Add(p13);
        }
    }
}