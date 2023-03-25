using System;
using Schachbot;
using Microsoft.Xna.Framework;

namespace Schachbot.Bot
{
    public static class Evaluation
    {
        public static List<Vector2> GetBestMove(ChessBoard board, int depth, bool isWhitesTurn)
        {
            ChessBoard boardCopy = null;
            CreateBoardCopy(board, ref boardCopy);

            List<Vector2> bestMove = null;
            List<Vector2> currentMove;

            List<Vector2> currentLegalMoves = new List<Vector2>();

            int biggestMaterialAdvantage = GetMaterialCount(board.Board);
            int currentMaterialAdvantage;

            if(isWhitesTurn)
            {
                foreach(ChessField[] rank in boardCopy.Board)
                {
                    foreach(ChessField field in rank)
                    {
                        if (field.Piece != null && field.Piece.IsWhite)
                        {
                            currentMove = new List<Vector2>
                            {
                                new Vector2(field.x, field.y)
                            };

                            currentLegalMoves = field.Piece.GetLegalMoves(new ChessBoard() { Board = boardCopy.Board });

                            foreach(Vector2 move in currentLegalMoves)
                            {
                                currentMove.Add(move);
                                boardCopy.DoBotMove(currentMove);

                                currentMove.RemoveAt(currentMove.Count - 1);

                                currentMaterialAdvantage = GetMaterialCount(boardCopy.Board);

                                if (currentMaterialAdvantage == biggestMaterialAdvantage && bestMove == null)
                                {
                                    bestMove = new List<Vector2>();
                                    bestMove.Add(new Vector2(field.x, field.y));
                                    bestMove.Add(move);
                                }

                                if (currentMaterialAdvantage > biggestMaterialAdvantage)
                                {
                                    biggestMaterialAdvantage = currentMaterialAdvantage;
                                    bestMove = new List<Vector2>();
                                    bestMove.Add(new Vector2(field.x, field.y));
                                    bestMove.Add(move);
                                }

                                CreateBoardCopy(board, ref boardCopy);
                            }
                        }

                    }
                    
                }

            }
            else
            {
                foreach (ChessField[] rank in boardCopy.Board)
                {
                    foreach (ChessField field in rank)
                    {
                        if (field.Piece != null && field.Piece.IsBlack)
                        {
                            currentMove = new List<Vector2>
                            {
                                new Vector2(field.x, field.y)
                            };

                            currentLegalMoves = field.Piece.GetLegalMoves(new ChessBoard() { Board = boardCopy.Board });

                            foreach (Vector2 move in currentLegalMoves)
                            {
                                currentMove.Add(move);
                                boardCopy.DoBotMove(currentMove);

                                currentMove.RemoveAt(currentMove.Count - 1);

                                currentMaterialAdvantage = GetMaterialCount(boardCopy.Board);

                                if (currentMaterialAdvantage == biggestMaterialAdvantage && bestMove == null)
                                {
                                    bestMove = new List<Vector2>();
                                    bestMove.Add(new Vector2(field.x, field.y));
                                    bestMove.Add(move);
                                }

                                if (currentMaterialAdvantage < biggestMaterialAdvantage)
                                {
                                    biggestMaterialAdvantage = currentMaterialAdvantage;
                                    bestMove = new List<Vector2>();
                                    bestMove.Add(new Vector2(field.x, field.y));
                                    bestMove.Add(move);
                                }

                                CreateBoardCopy(board, ref boardCopy);
                            }
                        }

                    }

                }

            }

            Console.WriteLine(bestMove[0].X.ToString() + " " + bestMove[0].Y.ToString());

            return bestMove;
        }

        private static int GetMaterialCount(ChessField[][] board)
        {
            int materialCount = 0;

            foreach (ChessField[] rank in board)
            {
                foreach(ChessField field in rank)
                {
                    if(field.Piece != null)
                    {
                        if(field.Piece.IsBlack)
                        {
                            materialCount -= GetPieceValue(field);
                        }
                        else
                        {
                            materialCount += GetPieceValue(field);
                        }
                    }
                }
            }

            return materialCount;
        }

        private static int GetPieceValue(ChessField field)
        {
            if(field.Piece != null)
            {
                if (field.Piece.GetType() == typeof(Pieces.Rook))
                    return 50;
                if (field.Piece.GetType() == typeof(Pieces.Bishop))
                    return 30;
                if (field.Piece.GetType() == typeof(Pieces.King))
                    return 999;
                if (field.Piece.GetType() == typeof(Pieces.Knight))
                    return 30;
                if (field.Piece.GetType() == typeof(Pieces.Pawn))
                    return 10;
                if (field.Piece.GetType() == typeof(Pieces.Queen))
                    return 80;
                else
                    return -1;
            }

            return -1;
        }

        private static ChessBoard CreateBoardCopy(ChessBoard original, ref ChessBoard copy)
        {
            copy = new ChessBoard();
            copy.Board = new ChessField[8][];

            for (int x = 0; x < 8; x++)
            {
                copy.Board[x] = new ChessField[8];
                for (int y = 0; y < 8; y++)
                {
                    copy.Board[x][y] = new ChessField(original.Board[x][y].IsBlack, x, y);
                    copy.Board[x][y].PlacePiece(original.Board[x][y].Piece);
                }
            }

            return copy;
        }
    }
}