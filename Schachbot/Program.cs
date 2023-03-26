using System;
using Microsoft.Xna.Framework;
using Schachbot.Pieces;

namespace Schachbot
{
    class Program
    {
        static void Main(string[] args)
        {
            List<BasePiece> list = new List<BasePiece>();
            using (ChessGame g = new ChessGame())
            {
                g.Run();
            }
        }
    }
}