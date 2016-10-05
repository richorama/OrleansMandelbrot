using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new DevSilo())
            {
                Console.WriteLine("Press and key to continue...");
                Console.ReadKey();
            }

        }
    }
}
