using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Dialogs.TextureUtils
{
    public class TextureManager
    {
        private SortedList<string, string> textureFiles;
        private int textureImageHeight;
        private int textureImageWidth;
        private TextureCell[,] textureMap;
        private System.Drawing.Bitmap workingImage;
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
                                    textureMap[x, y] = new TextureCell((byte)(255-col.R));
                                }
                            }

                            //
                            for (int x = 0; x < workingImage.Width; x++)
                            {
                                for (int y = 0; y < workingImage.Height; y++)
                                {
                                    if (textureMap[x, y].Width > 0)
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
                                            textureMap[x, y].WestWall = (byte)(textureMap[x, y].Width - neighbour);
                                        }

                                        // EAST
                                        if (x == workingImage.Width - 1)
                                        {
                                            textureMap[x, y].EastWall = textureMap[x, y].Width;
                                        }
                                        if (x < workingImage.Width - 1)
                                        {

                                            neighbour = textureMap[x + 1, y].Width;
                                            textureMap[x, y].EastWall = (byte)(textureMap[x, y].Width - neighbour);
                                        }
                                        // NORTH
                                        if (y == 0)
                                        {
                                            textureMap[x, y].NorthWall = textureMap[x, y].Width;
                                        }
                                        if (y > 0)
                                        {
                                            neighbour = textureMap[x, y - 1].Width;
                                            textureMap[x, y].NorthWall = (byte)(textureMap[x, y].Width - neighbour);
                                        }

                                        // SOUTH
                                        if (y == workingImage.Height - 1)
                                        {
                                            textureMap[x, y].SouthWall = textureMap[x, y].Width;
                                        }
                                        if (y < workingImage.Height - 1)
                                        {
                                            neighbour = textureMap[x, y + 1].Width;
                                            textureMap[x, y].SouthWall = (byte)(textureMap[x, y].Width - neighbour);
                                        }
                                    }
                                }
                            }

                            /*
                            for (int x = 0; x < workingImage.Width; x++)
                            {
                                for (int y = 0; y < workingImage.Height; y++)
                                {
                                    System.Diagnostics.Debug.WriteLine($"{x},{y} = {textureMap[x, y].Width} > {textureMap[x, y].NorthWall},{textureMap[x, y].SouthWall},{textureMap[x, y].EastWall},{textureMap[x, y].WestWall}");                                    
                                }
                            }
                            */
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }
        }

        public TextureManager()
        {
            textureFiles = new SortedList<string, string>();
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
