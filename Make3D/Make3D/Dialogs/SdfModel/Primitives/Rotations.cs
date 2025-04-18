using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel.Primitives
{
    internal class Rotations
    {
        public static Point3D Multiply(double[,] matrix1, Point3D p)
        {
            double[,] matrix2 = new double[1, 3];
            matrix2[0, 0] = p.X;
            matrix2[0, 1] = p.Y;
            matrix2[0, 2] = p.Z;
            double[,] m = Multiply(matrix2, matrix1);
            return new Point3D(m[0, 0], m[0, 1], m[0, 2]);
        }

        public static double[,] Multiply(double[,] matrix1, double[,] matrix2)
        {
            // cahing matrix lengths for better performance
            var matrix1Rows = matrix1.GetLength(0);
            var matrix1Cols = matrix1.GetLength(1);
            var matrix2Rows = matrix2.GetLength(0);
            var matrix2Cols = matrix2.GetLength(1);

            // checking if product is defined
            if (matrix1Cols != matrix2Rows)
                throw new InvalidOperationException
                  ("Product is undefined. n columns of first matrix must equal to n rows of second matrix");

            // creating the final product matrix
            double[,] product = new double[matrix1Rows, matrix2Cols];

            // looping through matrix 1 rows
            for (int matrix1_row = 0; matrix1_row < matrix1Rows; matrix1_row++)
            {
                // for each matrix 1 row, loop through matrix 2 columns
                for (int matrix2_col = 0; matrix2_col < matrix2Cols; matrix2_col++)
                {
                    // loop through matrix 1 columns to calculate the dot product
                    for (int matrix1_col = 0; matrix1_col < matrix1Cols; matrix1_col++)
                    {
                        product[matrix1_row, matrix2_col] +=
                          matrix1[matrix1_row, matrix1_col] *
                          matrix2[matrix1_col, matrix2_col];
                    }
                }
            }
            return product;
        }

        public static double[,] RotateRoundX(double theta)
        {
            double[,] res = new double[3, 3];
            res[0, 0] = 1;
            res[0, 1] = 0;
            res[0, 2] = 0;

            res[1, 0] = 0;
            res[1, 1] = Cos(theta);
            res[1, 2] = -Sin(theta);

            res[2, 0] = 0;
            res[2, 1] = Sin(theta);
            res[2, 2] = Cos(theta);

            return res;
        }

        public static double[,] RotateRoundY(double theta)
        {
            double[,] res = new double[3, 3];
            res[0, 0] = Cos(theta);
            res[0, 1] = 0;
            res[0, 2] = Sin(theta);

            res[1, 0] = 0;
            res[1, 1] = 1;
            res[1, 2] = 0;

            res[2, 0] = -Sin(theta);
            res[2, 1] = 0;
            res[2, 2] = Cos(theta);

            return res;
        }

        public static double[,] RotateRoundZ(double theta)
        {
            double[,] res = new double[3, 3];
            res[0, 0] = Cos(theta);
            res[0, 1] = -Sin(theta);
            res[0, 2] = 0;

            res[1, 0] = Sin(theta);
            res[1, 1] = Cos(theta);
            res[1, 2] = 0;

            res[2, 0] = 0;
            res[2, 1] = 0;
            res[2, 2] = 1;

            return res;
        }

        public static double[,] RotateXYZ(double rx, double ry, double rz)
        {
            double[,] res = RotateRoundX(rx);
            res = Multiply(res, RotateRoundY(ry));
            res = Multiply(res, RotateRoundZ(rz));
            return res;
        }

        private static double Cos(double t)
        {
            t = DegsToRad(t);
            return Math.Cos(t);
        }

        private static double DegsToRad(double t)
        {
            return (t * Math.PI) / 180.0;
        }

        private static double Sin(double t)
        {
            t = DegsToRad(t);
            return Math.Sin(t);
        }
    }
}