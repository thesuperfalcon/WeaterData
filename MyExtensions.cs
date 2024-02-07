using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaterData
{
    internal static class MyExtensions
    {
        public static void Cw(this string message)
        {
            Console.WriteLine(message);
        }
    }
}
