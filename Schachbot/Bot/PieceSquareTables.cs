using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot.Bot
{
    public static class PieceSquareTables
    {
        public static readonly int[] pst_pawn_white = {
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 15, 15, 0, 0, 0,
          0, 0, 0, 10, 10, 0, 0, 0,
          0, 0, 0, 5, 5, 0, 0, 0,
          0, 0, 0, -25, -25, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0
        };

        public static readonly int[] pst_pawn_black = {
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, -25, -25, 0, 0, 0,
          0, 0, 0, 5, 5, 0, 0, 0,
          0, 0, 0, 10, 10, 0, 0, 0,
          0, 0, 0, 15, 15, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0
        };

        public static readonly int[] pst_knight_white = {
          -40, -25, -25, -25, -25, -25, -25, -40,
          -30, 0, 0, 0, 0, 0, 0, -30,
          -30, 0, 0, 0, 0, 0, 0, -30,
          -30, 0, 0, 15, 15, 0, 0, -30,
          -30, 0, 0, 15, 15, 0, 0, -30,
          -30, 0, 10, 0, 0, 10, 0, -30,
          -30, 0, 0, 5, 5, 0, 0, -30,
          -40, -30, -25, -25, -25, -25, -30, -40
        };

        public static readonly int[] pst_knight_black = {
          -40, -25, -25, -25, -25, -25, -25, -40,
          -30, 0, 0, 0, 0, 0, 0, -30,
          -30, 0, 10, 0, 0, 10, 0, -30,
          -30, 0, 0, 15, 15, 0, 0, -30,
          -30, 0, 0, 15, 15, 0, 0, -30,
          -30, 0, 0, 0, 0, 0, 0, -30,
          -30, 0, 0, 5, 5, 0, 0, -30,
          -40, -30, -25, -25, -25, -25, -30, -40
        };

        public static readonly int[] pst_bishop_white = {
          -10, 0, 0, 0, 0, 0, 0, -10,
          -10, 5, 0, 0, 0, 0, 5, -10,
          -10, 0, 5, 0, 0, 5, 0, -10,
          -10, 0, 0, 10, 10, 0, 0, -10,
          -10, 0, 5, 10, 10, 5, 0, -10,
          -10, 0, 5, 0, 0, 5, 0, -10,
          -10, 5, 0, 0, 0, 0, 5, -10,
          -10, -20, -20, -20, -20, -20, -20, -10
        };

        public static readonly int[] pst_bishop_black = {
          -10, -20, -20, -20, -20, -20, -20, -10,
          -10,   5,   0,   0,   0,   0,   5, -10,
          -10,   0,   5,   0,   0,   5,   0, -10,
          -10,   0,   5,  10,  10,   5,   0, -10,
          -10,   0,   0,  10,  10,   0,   0, -10,
          -10,   0,   5,   0,   0,   5,   0, -10,
          -10,   5,   0,   0,   0,   0,   5, -10,
          -10,   0,   0,   0,   0,   0,   0, -10
        };

        public static readonly int[] pst_rook_white = {
          0, 0, 0, 0, 0, 0, 0, 0,
          10, 10, 10, 10, 10, 10, 10, 10,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 5, 5, 0, 0, 0
        };

        public static readonly int[] pst_rook_black = {
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          10, 10, 10, 10, 10, 10, 10, 10,
          0, 0, 0, 5, 5, 0, 0, 0
        };


        public static readonly int[] pst_king_white = {
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          10, 15, -15, -15, -15, -15, 15, 10
        };

        public static readonly int[] pst_king_black = {
           10,  15, -15, -15, -15, -15,  15,  10,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25,
          -25, -25, -25, -25, -25, -25, -25, -25
        };

        public static int EvaluatePawnStructure(ChessBoard chessBoard)
        {
            int structureVal = 0;

            int blackPawnsInFile = 0;
            int whitePawnsInFile = 0;

            int blackPawnIslands = 0;
            int whitePawnIslands = 0;
            bool blackLastFileEmpty = false;
            bool whiteLastFileEmpty = false;


            for (int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    if(chessBoard.Board[x][y].Piece.GetType() == typeof(Pieces.Pawn))
                    {
                        if (chessBoard.Board[x][y].Piece.IsWhite)
                        {
                            whitePawnsInFile++;
                        }
                        else if (chessBoard.Board[x][y].Piece.IsBlack)
                        {
                            blackPawnsInFile++;
                        }
                    }
                }

                if(whitePawnsInFile > 1)
                {
                    structureVal += 10 * whitePawnsInFile;
                }
                else if(whitePawnsInFile == 0)
                {
                    if(!whiteLastFileEmpty)
                    {
                        whitePawnIslands++;
                    }

                }

                if (blackPawnsInFile > 1)
                {
                    structureVal -= 10 * blackPawnsInFile;
                }

                whitePawnsInFile = 0;
                blackPawnsInFile = 0;
            }

            return structureVal;
        }
    }
}
