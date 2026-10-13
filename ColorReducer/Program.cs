using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColorReducer
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = "../coins.jpeg";
            string outputPath = "../coins_flattened.png";

            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                Color[,] pixels = new Color[bitmap.Width, bitmap.Height];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        // Convert to grayscale and threshold
                        int brightness = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                        pixels[x, y] = brightness > 150 ? Color.White : Color.Red;
                    }
                }

                Color[,] newPixels = new Color[bitmap.Width, bitmap.Height];
                int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
                int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color current = pixels[x, y];
                        newPixels[x, y] = current;
                        
                        if (current == Color.White) // check if adjacent to red
                        {
                            bool adjacentToCoin = false;
                            for (int i = 0; i < 8; i++)
                            {
                                int nx = x + dx[i];
                                int ny = y + dy[i];
                                if (nx >= 0 && nx < bitmap.Width && ny >= 0 && ny < bitmap.Height)
                                {
                                    if (pixels[nx, ny] == Color.Red)
                                    {
                                        adjacentToCoin = true;
                                        break;
                                    }
                                }
                            }
                            if (adjacentToCoin)
                            {
                                newPixels[x, y] = Color.Red;
                            }
                        }
                    }
                }

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        bitmap.SetPixel(x, y, newPixels[x, y]);
                    }
                }

                bitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine("Processing complete. Output saved to " + outputPath);
            }
        }
    }
}
