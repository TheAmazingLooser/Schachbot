using System;
using Microsoft.Xna.Framework;

namespace Schachbot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ChessGame g = new ChessGame())
            {
                g.Run();
            }
        }
    }
}