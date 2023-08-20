using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RectGrilleMaker : MakerBase
    {
        private double grillLength;
        private double grillHeight;
        private double grillWidth;
        private bool makeEdge;
        private double edgeThickness;
        private double verticalBars;
        private double verticalBarThickness;
        private double horizontalBars;
        private double horizontalBarThickness;

        public RectGrilleMaker(double grillLength, double grillHeight, double grillWidth, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grillLength = grillLength;
            this.grillHeight = grillHeight;
            this.grillWidth = grillWidth;
            this.makeEdge = makeEdge;
            this.edgeThickness = edgeThickness;
            this.verticalBars = verticalBars;
            this.verticalBarThickness = verticalBarThickness;
            this.horizontalBars = horizontalBars;
            this.horizontalBarThickness = horizontalBarThickness;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
           
            if (makeEdge)
            {
                GenerateWithEdge();
            }
            else
            {
                GenerateWithoutEdge();
            }
            
        }

        private void GenerateWithoutEdge()
        {

        }

        private void GenerateWithEdge()
        {
            double fl = edgeThickness;

            double innerLength = grillLength - (2 * fl);
            double vbarlength = verticalBars * verticalBarThickness;
            
            double vOff = (innerLength ) / (verticalBars);
            double gl = vOff - (verticalBarThickness / 2);

            double innerHeight = grillHeight - (2 * fl);
            double hbarlength =  horizontalBars * horizontalBarThickness;
            double hOff = innerHeight / (horizontalBars+1);
            
            double hl = hOff - (horizontalBarThickness/2) ;


            // bottom left corner
            double x = 0;
            double y = 0;
            Box(x, y, 0, fl, fl, grillWidth, true, false, false, true);
            Box(x+grillLength-fl, y, 0, fl, fl, grillWidth, false, true, false, true);
            Box(x, y+grillHeight-fl, 0, fl, fl, grillWidth, true, false, true, false);
            Box(x + grillLength - fl, y + grillHeight - fl, 0, fl, fl, grillWidth, false, true, true, false);

            for (int i = 1; i <= horizontalBars; i++)
            {
                y = fl + (i * hOff);
                Box(x, y- horizontalBarThickness / 2, 0, fl, horizontalBarThickness, grillWidth, true, false, false, false);
                Box(x+grillLength-fl, y - horizontalBarThickness / 2, 0, fl, horizontalBarThickness, grillWidth, false, true, false, false);

            }
            y = fl;
            for (int i = 0; i <= horizontalBars; i++)
            {
                y = fl + (i * hOff);
                if (i > 0)
                {
                    y = fl + (i *hOff+(horizontalBarThickness/2));
                }
                Box(x, y , 0, fl, hl, grillWidth, true, true, false, false);
                Box(x + grillLength - fl, y , 0, fl, hl, grillWidth, true, true, false, false);
  
            }




            /*
                        for ( int i = 1; i <= verticalBars; i ++)
                        {
                            x = fl + (i * vOff) - (verticalBarThickness/2);
                            Box(x, y, 0, verticalBarThickness, fl, grillWidth, false, false, false, true);
                         }
                        for (int i = 1; i <= verticalBars+1; i++)
                        {
                            x =  (i * vOff)-gl- verticalBarThickness / 2;
                            x += fl;
                            Box(x, y, 0, gl, fl, grillWidth, false, false, true, true);
                        }

                        x = grillLength - fl;
                        Box(x, y, 0, fl, fl, grillWidth, false, true, false, true);

                        // top of frame
                        x = 0;
                        y = grillHeight - fl;
                        Box(x, y, 0, fl, fl, grillWidth, true, false, true, false);

                        for (int i = 1; i <= verticalBars; i++)
                        {
                            x = fl + (i * vOff) - (verticalBarThickness / 2);
                            Box(x, y, 0, verticalBarThickness, fl, grillWidth, false, false, true, false);
                        }
                        for (int i = 1; i <= verticalBars + 1; i++)
                        {
                            x = (i * vOff) - gl - verticalBarThickness / 2;
                            x += fl;
                            Box(x, y, 0, gl, fl, grillWidth, false, false, true, true);
                        }

                        x = grillLength - fl;
                        Box(x, y, 0, fl, fl, grillWidth, false, true, true, false);

                        // left of frame
                        x = 0;
                        for (int i = 1; i <= horizontalBars; i++)
                        {
                            y = fl + (i * hOff) - (horizontalBarThickness / 2);
                            Box(x, y, 0, fl, horizontalBarThickness, grillWidth, true, false, false, false);
                        }
                        for (int i = 1; i <= verticalBars + 1; i++)
                        {
                            y = (i * hOff) - hl - horizontalBarThickness / 2;
                            y += fl;
                            Box(x, y, 0, fl, hl, grillWidth, true, true, false,false);
                        }
                        */
        }
        private void Box(double x, double y, double z, double l, double h, double w, bool left, bool right, bool top, bool bottom)
        {


            // back
            int v0 = AddVertice(x, y, z);
            int v1 = AddVertice(x, y+h, z );
            int v2 = AddVertice(x+l, y + h, z);
            int v3 = AddVertice(x+l, y, z);
            AddFace(v0, v1, v2);
            AddFace(v0, v2, v3);

            // front
            int v4 = AddVertice(x, y, z + w);
            int v5 = AddVertice(x, y + h, z + w);
            int v6 = AddVertice(x + l, y + h, z + w);
            int v7 = AddVertice(x + l, y, z + w);
            AddFace(v4, v6, v5);
            AddFace(v4, v7, v6);
            if (left)
            {
                AddFace(v0, v5, v1);
                AddFace(v0, v4, v5);
            }

            if (right)
            {
                AddFace(v3, v2, v6);
                AddFace(v3, v6, v7);
            }

            if (top)
            {
                AddFace(v1, v6, v2);
                AddFace(v1, v5, v6);
            }

            if (bottom)
            {
                AddFace(v0, v3, v7);
                AddFace(v0, v7, v4);
            }

        }
    }
}