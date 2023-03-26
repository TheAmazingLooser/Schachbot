using System;
using Schachbot;
using Microsoft.Xna.Framework;
using Schachbot.Pieces;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Content;
using System.Collections.Concurrent;

namespace Schachbot.Bot
{
    public static class Evaluation
    {
        private const int CHECKMATE_ADVANTAGE = 500;

        public static int MaxDepth = 5;
        public static KeyValuePair<Vector2, KeyValuePair<Vector2, int>>? BestMove = null;
        public static int HighestScore = 0;
        public static int Depth = 0;
        public static string FieldString = "";

        public static bool Stop = false;

        private static ConcurrentDictionary<Thread, int> runningThreads = new ConcurrentDictionary<Thread, int>();

        public static void GetBestMove(ChessBoard board, int depth, bool isWhitesTurn, KeyValuePair<Vector2, KeyValuePair<Vector2, int>>? bestMove = null)
        {
            Thread t = new Thread(() =>
            {
                if (Stop || depth >= MaxDepth || (((HighestScore > CHECKMATE_ADVANTAGE && isWhitesTurn) || (HighestScore < -1 * CHECKMATE_ADVANTAGE && !isWhitesTurn)) && depth >= Depth))
                {
                    runningThreads.TryRemove(new KeyValuePair<Thread, int>(Thread.CurrentThread, depth));
                    return;
                }
                List<Vector2> currentMove;

                int biggestMaterialAdvantage = GetMaterialCount(board.Board);
                int currentMaterialAdvantage = biggestMaterialAdvantage;

                if (depth == 0)
                {
                    HighestScore = currentMaterialAdvantage;
                }

                if ((isWhitesTurn && HighestScore < currentMaterialAdvantage) || (!isWhitesTurn && HighestScore > currentMaterialAdvantage))
                {
                    BestMove = bestMove;
                    HighestScore = currentMaterialAdvantage;
                    Depth = depth;
                    FieldString = board.ToString();
                }

                Dictionary<Vector2, List<Vector2>> AntiCheckMoves = board.GetNonCheckingMoves(isWhitesTurn);

                if (AntiCheckMoves.Count == 0)
                {
                    runningThreads.TryRemove(new KeyValuePair<Thread, int>(Thread.CurrentThread, depth));
                    return;
                }
                try
                {
                    while (runningThreads.Values.Min() < depth && !Stop)
                    {
                        Thread.Sleep(10);
                    }
                } catch
                {
                    runningThreads.TryRemove(new KeyValuePair<Thread, int>(Thread.CurrentThread, depth));
                    return;
                }

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (Stop || (((HighestScore > CHECKMATE_ADVANTAGE && isWhitesTurn) || (HighestScore < -1 * CHECKMATE_ADVANTAGE && !isWhitesTurn)) && depth >= Depth))
                        {
                            runningThreads.TryRemove(new KeyValuePair<Thread, int>(Thread.CurrentThread, depth));
                            return;
                        }
                        Vector2 Pos = new Vector2(x, y);
                        if (AntiCheckMoves.ContainsKey(Pos))
                        {
                            foreach (Vector2 move in AntiCheckMoves[Pos])
                            {
                                ChessBoard boardCopy = CreateBoardCopy(board);
                                currentMove = new List<Vector2>
                                {
                                    Pos,
                                    move
                                };

                                boardCopy.DoMove(currentMove);

                                if (depth == 0)
                                {
                                    bestMove = new KeyValuePair<Vector2, KeyValuePair<Vector2, int>>(Pos, new KeyValuePair<Vector2, int>(move, currentMaterialAdvantage));
                                    currentMaterialAdvantage = GetMaterialCount(boardCopy.Board);

                                    if ((currentMaterialAdvantage > HighestScore && isWhitesTurn) || (currentMaterialAdvantage < HighestScore && !isWhitesTurn))
                                    {
                                        BestMove = bestMove;
                                        HighestScore = currentMaterialAdvantage;
                                        Depth = depth;
                                        FieldString = board.ToString();
                                    }
                                }

                                if (UpdateBestMove(boardCopy, !isWhitesTurn))
                                {
                                    if (BestMove == null || (currentMaterialAdvantage >= HighestScore && isWhitesTurn) || (currentMaterialAdvantage <= HighestScore && !isWhitesTurn))
                                    {
                                        BestMove = bestMove;
                                        HighestScore = currentMaterialAdvantage;
                                        Depth = depth;
                                        FieldString = board.ToString();
                                    }
                                    GetBestMove(boardCopy, depth + 1, isWhitesTurn, bestMove);
                                } else
                                {
                                    if (BestMove == null || (currentMaterialAdvantage >= HighestScore && isWhitesTurn) || (currentMaterialAdvantage <= HighestScore && !isWhitesTurn))
                                    {
                                        BestMove = bestMove;
                                        HighestScore = currentMaterialAdvantage;
                                        Depth = depth;
                                        FieldString = board.ToString();
                                    }
                                }
                            }
                        }
                    }
                }

                if (depth == 0 && BestMove == null)
                    BestMove = bestMove;

                runningThreads.TryRemove(new KeyValuePair<Thread, int>(Thread.CurrentThread, depth));
            });
            runningThreads.TryAdd(t, depth);
            t.Start();
        }

        public static bool IsStillRunning()
        {
            try
            {
                int c = Evaluation.runningThreads.Count();
                return c > 0;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static void Init()
        {
            runningThreads.Clear();
            BestMove = null;
            HighestScore = 0;
            Stop = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="isWhitesTurn"></param>
        /// <returns>A Boolean represents if a move was made. If true a move was made, if false, none was made (checkmate)</returns>
        private static bool UpdateBestMove(ChessBoard board, bool isWhitesTurn)
        {
            List<Vector2> currentMove;

            int biggestMaterialAdvantage = GetMaterialCount(board.Board);
            int currentMaterialAdvantage = 0;

            KeyValuePair<Vector2, KeyValuePair<Vector2, int>>? bestMove = null;


            Dictionary<Vector2, List<Vector2>> AntiCheckMoves = board.GetNonCheckingMoves(isWhitesTurn);

            if (AntiCheckMoves.Count == 0) return false;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector2 Pos = new Vector2(x, y);
                    if (AntiCheckMoves.ContainsKey(Pos))
                    {
                        foreach (Vector2 move in AntiCheckMoves[Pos])
                        {
                            var boardCopy = CreateBoardCopy(board);
                            currentMove = new List<Vector2>
                            {
                                Pos,
                                move
                            };

                            boardCopy.DoMove(currentMove);
                            currentMaterialAdvantage = GetMaterialCount(boardCopy.Board);

                            if (bestMove == null || (currentMaterialAdvantage > biggestMaterialAdvantage && isWhitesTurn) || (currentMaterialAdvantage < biggestMaterialAdvantage && !isWhitesTurn))
                            {
                                bestMove = new KeyValuePair<Vector2, KeyValuePair<Vector2, int>>(Pos, new KeyValuePair<Vector2, int>(move, currentMaterialAdvantage));
                                biggestMaterialAdvantage = currentMaterialAdvantage;
                            }
                        }
                    }
                }
            }

            if (bestMove != null)
            {
                board.DoBotMove(new List<Vector2>
                {
                    bestMove.Value.Key,
                    bestMove.Value.Value.Key
                });
                return true;
            }
            return false;
        }

        private static bool UpdateBestMoveFutureMoves(ChessBoard board, bool isWhitesTurn)
        {
            List<Vector2> currentMove;

            KeyValuePair<Vector2, KeyValuePair<Vector2, int>>? bestMove = null;


            Dictionary<Vector2, List<Vector2>> AntiCheckMoves = board.GetNonCheckingMoves(isWhitesTurn);
            int MaxNonCheckingMoves = 9000;

            if (AntiCheckMoves.Count == 0) return false;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector2 Pos = new Vector2(x, y);
                    if (AntiCheckMoves.ContainsKey(Pos))
                    {
                        foreach (Vector2 move in AntiCheckMoves[Pos])
                        {
                            var boardCopy = CreateBoardCopy(board);
                            currentMove = new List<Vector2>
                            {
                                Pos,
                                move
                            };

                            boardCopy.DoMove(currentMove);
                            int newNonCheckingMoves = board.GetNonCheckingMoves(!isWhitesTurn).Count;

                            if (bestMove == null || newNonCheckingMoves < MaxNonCheckingMoves)
                            {
                                bestMove = new KeyValuePair<Vector2, KeyValuePair<Vector2, int>>(Pos, new KeyValuePair<Vector2, int>(move, GetMaterialCount(board.Board)));
                                MaxNonCheckingMoves = newNonCheckingMoves;
                            }
                        }
                    }
                }
            }

            if (bestMove != null)
            {
                board.DoBotMove(new List<Vector2>
                {
                    bestMove.Value.Key,
                    bestMove.Value.Value.Key
                });
                return true;
            }
            return false;
        }


        public static int GetMaterialCount(ChessField[][] board)
        {
            int materialCount = 0;

            foreach (ChessField[] rank in board)
            {
                foreach (ChessField field in rank)
                {
                    if (field.Piece != null)
                    {
                        if (field.Piece.IsBlack)
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
            if (field.Piece is BasePiece bp)
            {
                return bp.MaterialValue;
            }

            return -1;
        }

        private static ChessBoard CreateBoardCopy(ChessBoard original)
        {

            ChessBoard copy = new ChessBoard();
            copy.Board = new ChessField[8][];

            for (int x = 0; x < 8; x++)
            {
                copy.Board[x] = new ChessField[8];
                for (int y = 0; y < 8; y++)
                {
                    copy.Board[x][y] = new ChessField(original.Board[x][y].IsBlack, x, y);
                    BasePiece bp = null;
                    if (original.Board[x][y].Piece is Pawn pawn)
                        bp = new Pawn(pawn);
                    else if (original.Board[x][y].Piece is Rook rook)
                        bp = new Rook(rook);
                    else if (original.Board[x][y].Piece is Knight knight)
                        bp = new Knight(knight);
                    else if (original.Board[x][y].Piece is Bishop bishop)
                        bp = new Bishop(bishop);
                    else if (original.Board[x][y].Piece is Queen queen)
                        bp = new Queen(queen);
                    else if (original.Board[x][y].Piece is King king)
                        bp = new King(king);

                    copy.Board[x][y].PlacePiece(bp);
                }
            }

            return copy;
        }
    }
}