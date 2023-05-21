using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ConstructionStripMaker : MakerBase
    {

        private double holeRadius;
        private int numberOfHoles;
        private double stripHeight;
        private int stripRepeats;
        private double stripWidth;
        public ConstructionStripMaker(double stripHeight, double stripWidth, int stripRepeats, double holeRadius, int numberOfHoles)
        {
            this.stripHeight = stripHeight;
            this.stripWidth = stripWidth;
            this.stripRepeats = stripRepeats;
            this.holeRadius = holeRadius;
            this.numberOfHoles = numberOfHoles;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (stripRepeats == 1)
            {
                GenerateStrip();
            }
            else
            {
                GeneratePlate();
            }
        }

        private void Dump(byte[,] pattern)
        {
            for (int r = 0; r < pattern.GetLength(0); r++)
            {
                for (int c = 0; c < pattern.GetLength(1); c++)
                {
                    System.Diagnostics.Debug.Write($"{pattern[r, c]}, ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        private void GeneratePlate()
        {

            if (numberOfHoles > 2)
            {
                byte[,] pattern = new byte[numberOfHoles + 1, stripRepeats + 1];
                // sorry, you need the documentation to understand these numbers
                pattern[0, 0] = 1;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 0] = 2;
                }
                pattern[numberOfHoles, 0] = 3;

                for (int j = 1; j < stripRepeats; j++)
                {
                    pattern[0, j] = 4;
                    for (int i = 1; i < numberOfHoles; i++)
                    {
                        pattern[i, j] = 5;
                    }

                    pattern[numberOfHoles, j] = 6;
                }

                pattern[0, stripRepeats] = 7;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, stripRepeats] = 8;
                }
                pattern[numberOfHoles, stripRepeats] = 9;
                //Dump(pattern);
            }
        }

        private void GenerateStrip()
        {
            if (numberOfHoles > 2)
            {
                byte[,] pattern = new byte[numberOfHoles + 1, 2];
                // sorry, you need the documentation to understand these numbers
                pattern[0, 0] = 1;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 0] = 2;
                }
                pattern[numberOfHoles, 0] = 3;

                pattern[0, 1] = 7;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 1] = 8;
                }
                pattern[numberOfHoles, 1] = 9;
                //Dump(pattern);
            }
        }
    }
}
