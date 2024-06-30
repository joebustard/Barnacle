using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MakerLib.TextureUtils
{
    public class TextureManager
    {
        private SortedList<string, string> textureFiles;
        private int textureImageHeight;
        private int textureImageWidth;
        private TextureCell[,] textureMap;
        private System.Drawing.Bitmap workingImage;
        private TextureCell outOfRangeCell;

        public double PatternHeight
        {
            get
            {
                if (workingImage != null)
                {
                    return textureImageHeight;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double PatternWidth
        {
            get
            {
                if (workingImage != null)
                {
                    return textureImageWidth;
                }
                else
                {
                    return 0;
                }
            }
        }

        public SortedList<string, string> TextureFiles
        {
            get { return textureFiles; }
        }

        public List<string> TextureNames
        {
            get
            {
                List<String> res = textureFiles.Keys.ToList<string>();
                return res;
            }
        }

        private string loadedImageName;

        public void LoadTextureImage(string selectedTexture)
        {
            if (selectedTexture != "")
            {
                if (selectedTexture != loadedImageName)
                {
                    string imagePath = textureFiles[selectedTexture];
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            byte neighbour = 0;
                            byte diff = 0;
                            workingImage = new System.Drawing.Bitmap(imagePath);
                            loadedImageName = selectedTexture;
                            textureImageWidth = workingImage.Width;
                            textureImageHeight = workingImage.Height;
                            textureMap = new TextureCell[workingImage.Width, workingImage.Height];
                            for (int x = 0; x < workingImage.Width; x++)
                            {
                                for (int y = 0; y < workingImage.Height; y++)
                                {
                                    System.Drawing.Color col = workingImage.GetPixel(x, y);
                                    textureMap[x, y] = new TextureCell((byte)(255 - col.R));
                                }
                            }

                            //
                            for (int x = 0; x < workingImage.Width; x++)
                            {
                                for (int y = 0; y < workingImage.Height; y++)
                                {
                                    byte w = textureMap[x, y].Width;
                                    if (w > 0)
                                    {
                                        // WEST
                                        // if cell on the left edge it will need a west wall
                                        if (x == 0)
                                        {
                                            textureMap[x, y].WestWall = textureMap[x, y].Width;
                                        }
                                        if (x > 0)
                                        {
                                            neighbour = textureMap[x - 1, y].Width;
                                            if (neighbour < w)
                                            {
                                                textureMap[x, y].WestWall = (byte)(textureMap[x, y].Width - neighbour);
                                            }
                                        }

                                        // EAST
                                        if (x == workingImage.Width - 1)
                                        {
                                            textureMap[x, y].EastWall = textureMap[x, y].Width;
                                        }
                                        if (x < workingImage.Width - 1)
                                        {
                                            neighbour = textureMap[x + 1, y].Width;
                                            if (neighbour < w)
                                            {
                                                textureMap[x, y].EastWall = (byte)(textureMap[x, y].Width - neighbour);
                                            }
                                        }
                                        // South
                                        if (y == 0)
                                        {
                                            textureMap[x, y].SouthWall = textureMap[x, y].Width;
                                        }
                                        if (y > 0)
                                        {
                                            neighbour = textureMap[x, y - 1].Width;
                                            if (neighbour < w)
                                            {
                                                textureMap[x, y].SouthWall = (byte)(textureMap[x, y].Width - neighbour);
                                            }
                                        }

                                        // North
                                        if (y == workingImage.Height - 1)
                                        {
                                            textureMap[x, y].NorthWall = textureMap[x, y].Width;
                                        }
                                        if (y < workingImage.Height - 1)
                                        {
                                            neighbour = textureMap[x, y + 1].Width;
                                            if (neighbour < w)
                                            {
                                                textureMap[x, y].NorthWall = (byte)(textureMap[x, y].Width - neighbour);
                                            }
                                        }
                                    }
                                }
                            }
                            /*
                            System.Diagnostics.Debug.WriteLine("=======================================");
                            for (int x = 0; x < workingImage.Width; x++)
                            {
                                for (int y = 0; y < workingImage.Height; y++)
                                {
                                    System.Diagnostics.Debug.WriteLine($"{x},{y} = {textureMap[x, y].Width} > {textureMap[x, y].NorthWall},{textureMap[x, y].SouthWall},{textureMap[x, y].EastWall},{textureMap[x, y].WestWall}");
                                }
                            }
                            */
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }
        }

        public enum MapMode
        {
            ClippedTile,
            FittedTile,
            ClippedSingle,
            FittedSingle
        }

        public MapMode Mode { get; set; }

        public TextureCell GetCell(int tx, int ty)
        {
            TextureCell res = null;
            if (workingImage != null)
            {
                switch (Mode)
                {
                    case MapMode.ClippedTile:
                        {
                            tx = tx % workingImage.Width;
                            ty = ty % workingImage.Height;
                            ty = workingImage.Height - ty - 1;
                            res = textureMap[tx, ty];
                        }
                        break;

                    case MapMode.FittedTile:
                        {
                            tx = tx % workingImage.Width;
                            ty = ty % workingImage.Height;
                            ty = workingImage.Height - ty - 1;
                            res = textureMap[tx, ty];
                        }
                        break;

                    case MapMode.ClippedSingle:
                    case MapMode.FittedSingle: // Fitted relies on the caller changing the resolution
                        {
                            if ((tx < workingImage.Width) && (ty < workingImage.Height))
                            {
                                ty = workingImage.Height - ty - 1;
                                res = textureMap[tx, ty];
                            }
                            else
                            {
                                res = outOfRangeCell;
                            }
                        }
                        break;
                }
            }
            return res;
        }

        private TextureManager()
        {
            textureFiles = new SortedList<string, string>();
            outOfRangeCell = new TextureCell(0);
        }

        private static TextureManager instance = null;

        public static TextureManager Instance()
        {
            if (instance == null)
            {
                instance = new TextureManager();
            }
            return instance;
        }

        public void LoadTextureNames()
        {
            try
            {
                String appFolder = AppDomain.CurrentDomain.BaseDirectory + "Data\\Textures";
                GetTexturesFromFolder(appFolder);

                appFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Barnacle\\Textures";
                GetTexturesFromFolder(appFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public string GetTextureFolderName()
        {
        return System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Barnacle\\Textures";
        }
        private void GetTexturesFromFolder(string appFolder)
        {
            if (Directory.Exists(appFolder))
            {
                string[] txtFiles = Directory.GetFiles(appFolder, "*.png");
                foreach (string s in txtFiles)
                {
                    string fName = System.IO.Path.GetFileNameWithoutExtension(s);
                    textureFiles[fName] = s;
                }
            }
        }
    }
}