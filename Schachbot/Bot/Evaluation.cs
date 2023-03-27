using System;
using Schachbot;
using Microsoft.Xna.Framework;
using Schachbot.Pieces;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Content;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ComponentModel;

namespace Schachbot.Bot
{
    public class Evaluation
    {
        public ConcurrentBag<ChessMove> BestMoves { get; protected set; } = new ConcurrentBag<ChessMove>();
        public int Depth { get; protected set; }
        public int MaxDepth = -1;

        private List<Thread> threads = new List<Thread>();
        private ConcurrentDictionary<int, ConcurrentBag<KeyValuePair<ChessMove, ChessMove>>> BestMoveTree = new ConcurrentDictionary<int, ConcurrentBag<KeyValuePair<ChessMove, ChessMove>>>();
        private ConcurrentQueue<EvaluationThreadParameter> Actions = new ConcurrentQueue<EvaluationThreadParameter>();

        private bool stop = true;

        public int ThreadCount { get; protected set; }

        public Evaluation(int ThreadCount = 24)
        {
            this.ThreadCount = ThreadCount;
        }

        public void Start()
        {
            if (stop)
            {
                stop = false;
                for (int i = 0; i < ThreadCount; i++)
                {
                    Thread t = new Thread(() =>
                    {
                        while (!stop)
                        {
                            Thread.Sleep(10);
                            EvaluationThreadParameter Param = null;
                            if (Actions.Count > 0 && Actions.TryDequeue(out Param))
                            {
                                InternalEvaluate(Param.Board, Param.Depth, Param.IsWhite, Param.BestMove);
                            }
                        }
                    });
                    threads.Add(t);
                    t.Start();
                }
            }
        }

        public void Stop()
        {
            stop = true;
            threads.Clear();
            Actions.Clear();
        }

        public bool IsRunning()
        {
            return !stop;
        }

        public ChessMove GetBestMove(bool forWhite)
        {
            return BestMoves.First();
        }


        public void Evaluate(ChessBoard board, bool isWhite = true)
        {
            Actions.Enqueue(new EvaluationThreadParameter
            {
                Board = board.Copy(),
                Depth = 0,
                IsWhite = isWhite,
                BestMove = new ConcurrentBag<ChessMove>()
            });
        }

        private void InternalEvaluate(ChessBoard board, int depth, bool isWhite, ConcurrentBag<ChessMove> bestMove)
        {
            if (depth > MaxDepth)
            {
                MaxDepth = depth;
            }

            if (bestMove.Count > 0 && (BestMoves.Count == 0 || (bestMove.Last().Score > BestMoves.Last().Score && isWhite) || (bestMove.Last().Score < BestMoves.Last().Score && !isWhite)))
            {
                BestMoves.Add(bestMove.Last());
                Depth = depth;
            }

            /*
            double HighestFutureScore = board.GetMaterialCount();
            List<ChessMove> bestFutureMoves = new List<ChessMove>();
            ChessMove bestFutureMove = null;

            var nonCheckMoves = board.GetNonCheckingMoves(isWhite);
            foreach (var move in nonCheckMoves)
            {
                int count = 0;
                int score = 0;
                foreach (var toMove in move.Value)
                {
                    var CopyBoard = board.Copy();
                    var tempMove = new ChessMove((int)move.Key.X, (int)move.Key.Y, (int)toMove.X, (int)toMove.Y, HighestFutureScore);
                    CopyBoard.DoMove(tempMove);
                    count++;
                    score += CopyBoard.GetMaterialCount();
                    int enemyScore = score / count;
                    foreach (var enemyMove in CopyBoard.GetNonCheckingMoves(!isWhite))
                    {
                        int tEnemyScore = 0;
                        int tEnemyCount = 0;
                        var CopyBoardEnemy = CopyBoard.Copy();
                        foreach (var toMoveEnemy in enemyMove.Value)
                        {
                            tEnemyCount++;
                            CopyBoardEnemy.DoMove(new ChessMove((int)enemyMove.Key.X, (int)enemyMove.Key.Y, (int)toMove.X, (int)toMove.Y, CopyBoardEnemy.GetMaterialCount()));
                            tEnemyScore += CopyBoardEnemy.GetMaterialCount();
                        }

                        if ((tEnemyScore / tEnemyCount > enemyScore && isWhite) || (tEnemyScore / tEnemyCount < enemyScore && !isWhite))
                        {
                            count += tEnemyCount;
                            score += tEnemyScore;
                        }
                    }

                    if (count > 0)
                    {
                        double nextScore = score / (double)count;
                        if (((nextScore >= HighestFutureScore && isWhite) || (nextScore <= HighestFutureScore && !isWhite)) || bestFutureMoves.Count == 0)
                        {
                            HighestFutureScore = nextScore;
                            bestFutureMoves.Add(tempMove);
                            try
                            {
                                bestFutureMoves.Remove(bestFutureMove);
                            } catch
                            {
                                
                            }
                        }

                        if (bestFutureMove == null || (bestFutureMove.Score <= tempMove.Score && isWhite) || (bestFutureMove.Score >= tempMove.Score && !isWhite))
                        {
                            bestFutureMove = tempMove;
                        }
                    } else if (count == 0)
                    {
                        return;
                    }
                }
            }

            if (bestFutureMoves.Count > 0)
            {
                foreach (var move in bestFutureMoves)
                {
                    ChessMove futureBestMove = bestMove == null ? new(move) : new(bestMove);
                    if (BestMoveTree.ContainsKey(depth))
                    {
                        ConcurrentBag<KeyValuePair<ChessMove, ChessMove>> outMoves = null;
                        if (BestMoveTree.TryGetValue(depth, out outMoves))
                        {
                            outMoves.Add(new KeyValuePair<ChessMove, ChessMove>(futureBestMove, move));
                        }
                    }
                    else
                    {
                        BestMoveTree.TryAdd(depth, new ConcurrentBag<KeyValuePair<ChessMove, ChessMove>>()
                        {
                            new KeyValuePair<ChessMove, ChessMove>(futureBestMove, move)
                        });
                    }
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(move);
                    Actions.Enqueue(new EvaluationThreadParameter()
                    {
                        Board = copyBoard,
                        BestMove = futureBestMove,
                        Depth = depth + 1,
                        IsWhite = !isWhite
                    });
                }
            } else
            {
                if (BestMoveTree.ContainsKey(depth))
                {
                    ConcurrentBag<KeyValuePair<ChessMove, ChessMove>> outMoves = null;
                    if (BestMoveTree.TryGetValue(depth, out outMoves))
                    {
                        outMoves.Add(new KeyValuePair<ChessMove, ChessMove>(bestMove, bestFutureMove));
                    }
                }
                else
                {
                    BestMoveTree.TryAdd(depth, new ConcurrentBag<KeyValuePair<ChessMove, ChessMove>>()
                    {
                        new KeyValuePair<ChessMove, ChessMove>(bestMove, bestFutureMove)
                    });
                }
                var copyBoard = board.Copy();
                if (bestFutureMove != null)
                {
                    copyBoard.DoMove(bestFutureMove);
                    Actions.Enqueue(new EvaluationThreadParameter()
                    {
                        Board = copyBoard,
                        BestMove = bestMove == null ? new(bestFutureMove) : new(bestMove),
                        Depth = depth + 1,
                        IsWhite = !isWhite
                    });
                }
            }
            */

            // Get all Moves for us
            var ourMoves = GetAllMoves(board, isWhite);

            // Rank all of the moves desc by the average score of the moves
            var rankedMoveSets = ourMoves.OrderByDescending(x => x.Value.Average(y => y.Score)).ToList();

            var currentScore = board.GetMaterialCount();

            List<ChessMove> futureMove = new List<ChessMove>();

            // Take 3 of the best move sets
            for (int i = 0; i < rankedMoveSets.Count; i++)
            {
                var Score = rankedMoveSets[i].Value.Average(x => x.Score);
                var moveSet = rankedMoveSets[i];
                double averageEnemyScore = Score;
                ChessMove bestFutMove = null;
                foreach (var pos in moveSet.Value)
                {
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(pos);
                    var enemyMoves = GetAllMoves(copyBoard, !isWhite);

                    double avgScore = Score;
                    if (enemyMoves.Count > 0)
                        avgScore = enemyMoves.Max(x => x.Value.Average(y => y.Score));

                    // Get the average enemy score
                    if (bestMove.Count == 0 || enemyMoves.Count == 0 || (averageEnemyScore > avgScore && isWhite) || (averageEnemyScore < avgScore && !isWhite))
                    {
                        averageEnemyScore = avgScore;
                        bestFutMove = pos;
                    }
                }

                if (bestFutMove != null)
                {
                    futureMove.Add(bestFutMove);
                }
            }

            // Add the best move to the Queue
            foreach (var move in futureMove)
            {
                var copyBoard = board.Copy();
                copyBoard.DoMove(move);
                bestMove.Add(move);
                Actions.Enqueue(new EvaluationThreadParameter()
                {
                    Board = copyBoard,
                    BestMove = bestMove,
                    Depth = depth + 1,
                    IsWhite = !isWhite
                });
            }
        }

        private Dictionary<Vector2, List<ChessMove>> GetAllMoves(ChessBoard board, bool white)
        {
            Dictionary<Vector2, List<ChessMove>> ret = new Dictionary<Vector2, List<ChessMove>>();
            var nonCheckMoves = board.GetNonCheckingMoves(white);
            foreach (var tMoveVec in nonCheckMoves)
            {
                foreach(var tMoveVecTo in tMoveVec.Value)
                {
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(new ChessMove((int)tMoveVec.Key.X, (int)tMoveVec.Key.Y, (int)tMoveVecTo.X, (int)tMoveVecTo.Y, 0));
                    ChessMove move = new ChessMove((int)tMoveVec.Key.X, (int)tMoveVec.Key.Y, (int)tMoveVecTo.X, (int)tMoveVecTo.Y, copyBoard.GetMaterialCount(false));
                
                    if (!ret.ContainsKey(tMoveVec.Key))
                    {
                        ret.Add(tMoveVec.Key, new List<ChessMove>());
                    }

                    ret[tMoveVec.Key].Add(move); 
                }
            }

            return ret;
        }

        private double GetAverageScore(List<ChessMove> moves)
        {
            return moves.Average(x => x.Score);
        }
    }

    public class EvaluationThreadParameter
    {
        public ChessBoard Board;
        public int Depth;
        public bool IsWhite;
        public ConcurrentBag<ChessMove> BestMove;
    }











    /*
    public static class Evaluation_Old
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
                int c = Evaluation_Old.runningThreads.Count();
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


        public static int GetMaterialCount(ChessField[][] board, bool includePawnStructure)
        {
            int materialCount = 0;


            int val = 0;

            foreach (ChessField[] rank in board)
            {
                foreach (ChessField field in rank)
                {
                    if (field.Piece != null)
                    {
                        val = GetPieceValue(field);

                        if (field.Piece.IsBlack)
                        {
                            if (field.Piece.GetType() == typeof(Pieces.Rook))
                                materialCount -= val + PieceSquareTables.pst_rook_black[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Bishop))
                                materialCount -= val + PieceSquareTables.pst_bishop_black[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.King))
                                materialCount -= val + PieceSquareTables.pst_king_black[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Knight))
                                materialCount -= val + PieceSquareTables.pst_knight_black[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Pawn))
                                materialCount -= val + PieceSquareTables.pst_pawn_black[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Queen))
                                materialCount -= val;
                        }
                        else
                        {
                            if (field.Piece.GetType() == typeof(Pieces.Rook))
                                materialCount += val + PieceSquareTables.pst_rook_white[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Bishop))
                                materialCount += val + PieceSquareTables.pst_bishop_white[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.King))
                                materialCount += val + PieceSquareTables.pst_king_white[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Knight))
                                materialCount += val + PieceSquareTables.pst_knight_white[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Pawn))
                                materialCount += val + PieceSquareTables.pst_pawn_white[(field.y * 8) + field.x] / 5;
                            if (field.Piece.GetType() == typeof(Pieces.Queen))
                                materialCount += val;
                        }
                    }
                }
            }

            if(includePawnStructure)
                materialCount += PieceSquareTables.EvaluatePawnStructure(board);

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
    */
}