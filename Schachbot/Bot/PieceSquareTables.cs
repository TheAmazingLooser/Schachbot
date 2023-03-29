using Schachbot.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot.Bot
{
    public static class PieceSquareTables
    {
        static int doubledPawnsPenalty = 50;
        static int tripledAndMorePenalty = 150;
        static int pawnIslandPenalty = 25;
        static int defendedBonus = 5;

        public static readonly int[] pst_pawn_white = {
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 20, 20, 0, 0, 0,
          0, 0, 0, 20, 20, 0, 0, 0,
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


                    if(chessBoard.Board[x][y].Piece is Pawn pawn)
                    {
                        if (chessBoard.Board[x][y].Piece.IsWhite)
                        {
                            whitePawnsInFile++;

                            if ((chessBoard.GetField(x + 1, y - 1) is ChessField f1 && f1.Piece is Pawn pawn1 && pawn1.IsWhite) ||
                                (chessBoard.GetField(x - 1, y - 1) is ChessField f2 && f2.Piece is Pawn pawn2 && pawn2.IsWhite))
                            {
                                structureVal += defendedBonus;
                            } else if ((chessBoard.GetField(x + 1, y - 1) is ChessField f1_1 && f1_1.Piece is Pawn pawn1_1 && pawn1_1.IsWhite) &&
                                      (chessBoard.GetField(x - 1, y - 1) is ChessField f2_2 && f2_2.Piece is Pawn pawn2_1 && pawn2_1.IsWhite))
                            {
                                structureVal += 2 * defendedBonus;
                            }
                        }
                        else if (chessBoard.Board[x][y].Piece.IsBlack)
                        {
                            blackPawnsInFile++;

                            if ((chessBoard.GetField(x + 1, y - 1) is ChessField f1 && f1.Piece is Pawn pawn1 && !pawn1.IsWhite) ||
                                (chessBoard.GetField(x - 1, y - 1) is ChessField f2 && f2.Piece is Pawn pawn2 && !pawn2.IsWhite))
                            {
                                structureVal -= defendedBonus;
                            }
                            else if ((chessBoard.GetField(x + 1, y - 1) is ChessField f1_1 && f1_1.Piece is Pawn pawn1_1 && !pawn1_1.IsWhite) &&
                                     (chessBoard.GetField(x - 1, y - 1) is ChessField f2_2 && f2_2.Piece is Pawn pawn2_1 && !pawn2_1.IsWhite))
                            {
                                structureVal -= 2 * defendedBonus;
                            }
                        }
                    }
                }

                if(whitePawnsInFile == 2)
                {
                    structureVal -= doubledPawnsPenalty;
                    whiteLastFileEmpty = false;
                }
                else if(whitePawnsInFile >= 3)
                {
                    structureVal -= tripledAndMorePenalty; 
                    whiteLastFileEmpty = false;
                }
                else if (whitePawnsInFile == 1)
                {
                    whiteLastFileEmpty = false;
                }
                else if(whitePawnsInFile == 0)
                {
                    if(!whiteLastFileEmpty)
                    {
                        whitePawnIslands++;
                    }
                    
                    whiteLastFileEmpty = true;
                }

                if (blackPawnsInFile == 2)
                {
                    structureVal += doubledPawnsPenalty;
                    blackLastFileEmpty = false;
                }
                else if (blackPawnsInFile >= 3)
                {
                    structureVal += tripledAndMorePenalty;
                    blackLastFileEmpty = false;
                }
                else if (blackPawnsInFile == 1)
                {
                    blackLastFileEmpty = false;
                }
                else if (blackPawnsInFile == 0)
                {
                    if (!blackLastFileEmpty)
                    {
                        blackPawnIslands++;
                    }

                    blackLastFileEmpty = true;
                }

                whitePawnsInFile = 0;
                blackPawnsInFile = 0;
            }

            structureVal += (whitePawnIslands * pawnIslandPenalty) + (blackPawnIslands * -pawnIslandPenalty);

            return structureVal;
        }
    }
}
