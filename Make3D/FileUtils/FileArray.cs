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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileUtils
{
    public class FileArray
    {
        private int rows;
        private int columns;
        private int ranks;
        private int rowByCols;
        private FileStream fileStream;


        public FileArray()
        {
            rows = -1;
            columns = -1;
            ranks = -1;
            fileStream = null;
        }
        /// <summary>
        /// Create and new file and then open it for read/ write
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="nrows"></param>
        /// <param name="ncols"></param>
        /// <param name="nranks"></param>
        /// <param name="defValue"></param>
        public void Create(String filePath, int nrows, int ncols, int nranks, double defValue)
        {
            String rootPath = System.IO.Path.GetFullPath(filePath);

            // make sure the folder exists
            PathManager.CreateIfNeeded(rootPath);

            // create the binary file
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(nrows);
                writer.Write(ncols);
                writer.Write(nranks);

                for (int r = 0; r < nrows; r++)
                {
                    for (int c = 0; c < ncols; c++)
                    {
                        for (int k = 0; k < nranks; k++)
                        {
                            writer.Write(defValue);
                        }
                    }
                }
                writer.Close();
            }
            Open(filePath, out rows, out cols, out ranks);
        }


        /// <summary>
        /// Open an existing array file for erad/write
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="ranks"></param>
        public void Open(String filePath, out int rows, out int cols, out int ranks)
        {

            rows = -1;
            cols = -1;
            ranks = -1;
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    fileStream = new FileStream(filePath, FileMode.Open);
                    rows = ReadInt32(res);
                    cols = ReadInt32(res);
                    ranks = ReadInt32(res);
                    rowByCols = rows * cols;
                }
                catch
                {

                }
            }
        }

        private int ReadInt32()
        {
            byte[] buffer = new byte[4]; // Size of int
            fileStream.Read(buffer, 0, 4);
            int intValue = BitConverter.ToInt32(buffer, 0);
            return intValue;
        }

        private void Write(int v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        private void Write(double d)
        {
            byte[] buffer = BitConverter.GetBytes(d);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        private double ReadDouble()
        {
            int l = sizeof(double);
            byte[] buffer = new byte[l];
            fileStream.Read(buffer, 0, l);
            double value = BitConverter.ToDouble(buffer, 0);
            return value;
        }
        private const long hdrOff = 3 * sizeof(int);
        private void SeekToElement(int r, int c, int k)
        {
            long pos = (k * rowByCols) + (r * columns) + c;
            pos * sizeof(double);
            pos += hdrOff;
            fileStream.Seek(pos);
        }

        public void Set(int r, int c, int k, double v)
        {
            SeekToElement(r, c, k);
            Write(v);
        }

        public double Get(int r, int c, int k)
        {
            SeekToElement(r, c, k);
            return ReadDouble();
        }


        public double this[int r, int c, int k]
        {
            get { return Get(r, c, k); }
            set { Set(r, c, k, value); }
        }
    }
}