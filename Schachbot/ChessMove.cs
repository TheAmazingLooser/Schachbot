using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot
{
    public class ChessMove
    {
        public int FromX { get; private set; }
        public int FromY { get; private set; }
        public int ToX { get; private set; }
        public int ToY { get; private set; }

        public double Score { get; private set; }

        public ChessMove(int fromX, int fromY, int toX, int toY, double score)
        {
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;

            Score = score;
        }

        // Copy Constructor
        public ChessMove(ChessMove move)
        {
            FromX = move.FromX;
            FromY = move.FromY;
            ToX = move.ToX;
            ToY = move.ToY;

            Score = move.Score;
        }

        public override string ToString()
        {
            return $"({"" + (char)(FromX + 'a')}|{8 - FromY}) -> ({(char)(ToX + 'a')}|{8 - ToY})";
        }
    }
}
