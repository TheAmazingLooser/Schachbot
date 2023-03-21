using System;
using Schachbot;

namespace Schachbot.Bot
{
    public static class Evaluation
    {
        /*
        public static Dictionary<Vector2, Vector2> GetBestMove(ChessField[][] board, int depth, bool isWhitesTurn)
        {
            List<List<Vector2>> possibleMoves = new List<List<Vector2>>();
            List<Vector2> possiblePieceMoves;

            if(isWhitesTurn)
            {
                foreach(ChessField field in board)
                {
                    possiblePieceMoves = new List<Vector2>();

                    if(field.Piece.IsWhite)
                    {
                        field.Piece.GetLegalMoves(out possiblePieceMoves);
                    }

                    possibleMoves.Add(possiblePieceMoves);
                }
            }
            else
            {
                foreach(ChessField field in board)
                {
                    possiblePieceMoves = new List<Vector2>();

                    if(field.Piece.IsBlack)
                    {
                        field.Piece.GetLegalMoves(out possiblePieceMoves);
                    }

                    possibleMoves.Add(possiblePieceMoves);
                }
            }
        }
        */
    }
}