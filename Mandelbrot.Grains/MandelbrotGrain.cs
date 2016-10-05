using Orleans;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Grains
{

    public interface IMandelbrotGrain : IGrainWithStringKey
    {
        Task<byte[]> Get();
    }


    public class MandelbrotGrain : Grain, IMandelbrotGrain
    {
        // coordinates should be a square to avoid distortion
        const double MIN_X = -2;
        const double MAX_X = 2;
        const double MIN_Y = -2;
        const double MAX_Y = 2;
        const int TILE_SIZE = 256;

        byte[] bytes;

        public override Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            Console.WriteLine(key);
            var parts = key.Split('_').Select(Int64.Parse).ToArray();

            var x = parts[0];
            var y = parts[1];
            var z = parts[2];
            var numberOfTiles = Math.Pow(2, z);

            var x1 = (MAX_X - MIN_X) * (x - (numberOfTiles / 2)) / numberOfTiles;
            var y1 = (MAX_Y - MIN_Y) * (y - (numberOfTiles / 2)) / numberOfTiles;
            var pixelSize = (MAX_X - MIN_X) / (numberOfTiles * TILE_SIZE);

            // consider moving bitmap rendering out to a stateless worker to reduce allocations
            using (var bitmap = new Bitmap(TILE_SIZE, TILE_SIZE))
            {
                var bData = bitmap.LockBits(new Rectangle(0,0, TILE_SIZE, TILE_SIZE),ImageLockMode.WriteOnly,PixelFormat.Format32bppPArgb);

                // allocate buffer for image
                var data = new byte[bData.Stride * bData.Height];

                for (var dx = 0; dx < TILE_SIZE; dx++)
                {
                    for (var dy = 0; dy < TILE_SIZE; dy++)
                    {
                        // calculate pixel value
                        var tx = x1 + (dx * pixelSize);
                        var ty = y1 + (dy * pixelSize);

                        data[((dx * 4) + (dy * bData.Stride)) + 3] = 255;
                        data[((dx * 4) + (dy * bData.Stride)) + 2] = (byte) GetColour(tx, ty);
                        //bitmap.SetPixel(dx, dy, GetColour(tx, ty));
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, data.Length);
                bitmap.UnlockBits(bData);

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    bytes = stream.ToArray();
                }
            }

            return base.OnActivateAsync();
        }

        int GetColour(double re, double im)
        {
            double zRe = 0;
            double zIm = 0;

            //Variables to store the squares of the real and imaginary part.
            double multZre = 0;
            double multZim = 0;

            //Start iterating the with the complex number to determine it's escape time (mandelValue)
            int mandelValue = 0;
            while (multZre + multZim < 4 && mandelValue < 255)
            {
                /*The new real part equals re(z)^2 - im(z)^2 + re(c), we store it in a temp variable
                tempRe because we still need re(z) in the next calculation
                    */
                double tempRe = multZre - multZim + re;

                /*The new imaginary part is equal to 2*re(z)*im(z) + im(c)
                    * Instead of multiplying these by 2 I add re(z) to itself and then multiply by im(z), which
                    * means I just do 1 multiplication instead of 2.
                    */
                zRe += zRe;
                zIm = zRe * zIm + im;

                zRe = tempRe; // We can now put the temp value in its place.

                // Do the squaring now, they will be used in the next calculation.
                multZre = zRe * zRe;
                multZim = zIm * zIm;

                //Increase the mandelValue by one, because the iteration is now finished.
                mandelValue++;
            }
            return 255 - mandelValue;
        }

        public Task<byte[]> Get()
        {
            return Task.FromResult(bytes);
        }
    }
}
