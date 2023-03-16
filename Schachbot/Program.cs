using System;
using Microsoft.Xna.Framework;

namespace Schachbot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SchachGame g = new SchachGame())
            {
                g.Run();
            }
        }
    }
}