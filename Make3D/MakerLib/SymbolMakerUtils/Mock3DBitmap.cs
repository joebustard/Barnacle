using System.Windows.Media.Imaging;

namespace MakerLib
{
    public class Mock3DBitmap
    {
        private int width;

        private int depth;
        private int numberOfLayers;
        private object[] layers;

        public Mock3DBitmap(int width, int height, int depth)
        {
            this.width = width;

            this.depth = depth;
            this.numberOfLayers = height;
            layers = new object[height];
            for (int i = 0; i < height; i++)
            {
                layers[i] = null;
            }
        }

        /// <summary>
        /// Set the actual bitmap for a specific layer
        /// The same bitmap can be used for multiple layers
        /// Its also ok for layers to be null
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="layer"></param>
        public void SetLayerImage(WriteableBitmap bitmap, int layer)
        {
            if (layer >= 0 && layer < numberOfLayers)
            {
                layers[layer] = bitmap;
            }
        }

        /// <summary>
        /// Return the colour of a pixel on a particular layer
        /// If the layer doesn't exist, return white
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public System.Windows.Media.Color GetPixel(int x, int y, int z)
        {
            if (x >= 0 && x < this.width && y >= 0 && y < this.numberOfLayers && z >= 0 && z < this.depth)
            {
                if (layers[y] != null)
                {
                    return (layers[y] as WriteableBitmap).GetPixel(x, z);
                }
            }
            return System.Windows.Media.Colors.White;
        }
    }
}