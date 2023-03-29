using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Schachbot.Bot
{
    public class Evaluation
    {
        private bool CheckMateAsWhite = false;
        private ChessMove CheckMateMove = null;
        private const bool UseSquareTable = true;
        public ConcurrentBag<KeyValuePair<ChessMove, List<ChessMove>>> BestMoves { get; protected set; } = new ConcurrentBag<KeyValuePair<ChessMove, List<ChessMove>>>();
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
                                InternalEvaluate(Param.Board, Param.Depth, Param.IsWhite, Param.BestMove, Param.MoveList);
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

        public ChessMove GetBestMove(bool forWhite, out double Score)
        {
            Score = 0;
            ChessMove ret = null;

            if (CheckMateMove != null && CheckMateAsWhite == forWhite && BestMoves.Count(x => 
                x.Key.ToX == CheckMateMove.ToX &&
                x.Key.FromX == CheckMateMove.FromX &&
                x.Key.ToY == CheckMateMove.ToY &&
                x.Key.ToX == CheckMateMove.ToX
            ) == 1)
            {
                if (forWhite)
                    Score = double.MaxValue;
                else
                    Score = double.MinValue;
                return CheckMateMove;
            }

            List<ChessMove> movesToHere = null;
            if (BestMoves.Count > 0)
            {
                // Convert the BestMoves to a Dictionary
                Dictionary<ChessMove, List<List<ChessMove>>> ConvertedMoves = new Dictionary<ChessMove, List<List<ChessMove>>>();
                foreach (var kv in BestMoves)
                {
                    if (!ConvertedMoves.ContainsKey(kv.Key))
                        ConvertedMoves.Add(kv.Key, new List<List<ChessMove>>());
                    ConvertedMoves[kv.Key].Add(kv.Value);
                }

                // Get the best move
                List<ChessMove> PossibleMoves = new List<ChessMove>();
                foreach(var kv in ConvertedMoves)
                {
                    // Calculate the Average of every move in the list of submoves
                    Score = kv.Key.Score;

                    // TODO: Elias: Hier die List<List<...>> in einen auswertbaren baum umbauen...
                    // GetBestMove -> GetWorstMove -> GetBestMove -> GetWorstMove
                    List<List<ChessMove>> GetBestMove(List<List<ChessMove>> currentMoveSet, bool forWhite)
                    {
                        // Create a dictionary containing the first element of the 2nd list as keys and as a value a list of lists containing every other element of the 2nd list
                        Dictionary<ChessMove, List<List<ChessMove>>> ConvertedSubMoves = new Dictionary<ChessMove, List<List<ChessMove>>>();
                        foreach (var subMove in currentMoveSet)
                        {
                            if (subMove.Count == 0) continue;
                            
                            if (!ConvertedSubMoves.ContainsKey(subMove[0]))
                                ConvertedSubMoves.Add(subMove[0], new List<List<ChessMove>>());
                            ConvertedSubMoves[subMove[0]].Add(subMove.Skip(1).ToList());
                        }

                        // Get the best moves for white (max Score in key) or black (min Score in key)
                        List<ChessMove> BestMoves = new List<ChessMove>();
                        if (forWhite)
                            BestMoves = ConvertedSubMoves.OrderByDescending(x => x.Key.Score).Select(x => x.Key).ToList();
                        else
                            BestMoves = ConvertedSubMoves.OrderBy(x => x.Key.Score).Select(x => x.Key).ToList();

                        // Clear all moves from BestMoves where the score is not equal to the first one.
                        if (BestMoves.Count > 0)
                        {
                            double BestScore = BestMoves[0].Score;
                            BestMoves.RemoveAll(x => x.Score != BestScore);
                        }


                        // Remove every empty list from ConvertedSubMoves[BestMoves.First()]
                        if (BestMoves.Count > 0)
                            ConvertedSubMoves[BestMoves.First()].RemoveAll(x => x.Count == 0);

                        if (BestMoves.Count > 0)                        
                            return ConvertedSubMoves[BestMoves.First()];
                        return new List<List<ChessMove>>();

                    }

                    var BestMove = kv.Value;
                    bool forWhitWhile = forWhite;

                    do {
                        var newBestMoveFutGood = GetBestMove(BestMove, forWhitWhile);
                        if (newBestMoveFutGood.Count > 0)
                        {
                            // Get the best Scoring move
                            if (forWhitWhile)
                                BestMove = newBestMoveFutGood.OrderByDescending(x => x.Average(y => y.Score)).ToList();
                            else
                                BestMove = newBestMoveFutGood.OrderBy(x => x.Average(y => y.Score)).ToList();
                        } else
                        {
                            break;
                        }
                        forWhitWhile = !forWhitWhile;
                    } while (BestMove.Count > 0);

                    kv.Key.Score = BestMove.First().First().Score;
                    PossibleMoves.Add(kv.Key);
                    // Get the best Scoring move

                    /*
                    int maxSubMoves = kv.Value.Max(x => x.Count);

                    for(int f = 0; f < maxSubMoves; f++)
                    {
                        bool isWhiteMove = forWhite && (f == 0 || f % 2 == 0);
                        bool isBlackMove = !forWhite && (f == 0 || f % 2 == 0);
                        Score = kv.Key.Score;

                        // Convert the values to a dictionary where the key is the first element of the list in the list and the value a list of a list of objects contains every other object left in that list.
                        Dictionary<ChessMove, List<List<ChessMove>>> ConvertedSubMoves = new Dictionary<ChessMove, List<List<ChessMove>>>();

                        foreach (var subMove in kv.Value)
                        {
                            if (subMove.Count <= f) break;
                            if (!ConvertedSubMoves.ContainsKey(subMove[f]))
                                ConvertedSubMoves.Add(subMove[f], new List<List<ChessMove>>());
                            ConvertedSubMoves[subMove[f]].Add(subMove.Skip(f + 1).ToList());
                        }

                        // Figure out the worst move according to isWhite or isBlacks move
                        List<ChessMove> IgnoreI = new List<ChessMove>();

                        foreach (var subMove in ConvertedSubMoves)
                        {
                            if (IgnoreI.Contains(subMove.Key)) continue;
                            if (subMove.Value.Count <= f) break;
                            if ((isWhiteMove && subMove.Key.Score > Score) || (isBlackMove && subMove.Key.Score < Score))
                            {
                                Score = subMove.Key.Score;
                            }
                            else
                            {
                                IgnoreI.Add(subMove.Key);
                            }
                        }
                    }

                    kv.Key.Score = Score;
                    PossibleMoves.Add(kv.Key);
                    */
                }

                var orderedMoves = PossibleMoves.OrderByDescending(x => x.Score);
                if (!forWhite)
                    orderedMoves = PossibleMoves.OrderBy(x => x.Score);

                ret = orderedMoves.First();
                Score = ret.Score;
            }
            
            return ret;
        }


        public void Evaluate(ChessBoard board, bool isWhite = true)
        {
            Actions.Enqueue(new EvaluationThreadParameter
            {
                Board = board.Copy(),
                Depth = 0,
                IsWhite = isWhite,
                BestMove = null
            });
        }

        private void InternalEvaluate(ChessBoard board, int depth, bool isWhite, ChessMove bestMove, List<ChessMove> moveList)
        {
            if (CheckMateMove != null) return;
            
            if (depth > MaxDepth)
            {
                MaxDepth = depth;
                Depth = depth;
            }
            
            // Get all Moves for us
            var ourMoves = GetAllMoves(board, isWhite);
            if (ourMoves.Count == 0)
            {
                if (moveList.Count > 0)
                    moveList.Last().Score = isWhite ? double.MinValue : double.MaxValue;
                return;
            }

            // Rank all of the moves desc by the average score of the moves
            var rankedMoveSets = ourMoves.OrderByDescending(x => x.Value.Max(y => y.Score)).ToList();
            
            if (!isWhite)
                rankedMoveSets = ourMoves.OrderBy(x => x.Value.Min(y => y.Score)).ToList();

            var currentScore = board.GetMaterialCount(UseSquareTable);

            List<ChessMove> futureMove = new List<ChessMove>();

            // Get the best move for us
            
            for(int i = 0; i < rankedMoveSets.Count; i++)
            {
                var moveFrom = rankedMoveSets[i].Key;
                // Predict the enemy moves
                for (int f = 0; f < rankedMoveSets[i].Value.Count; f++)
                {
                    var Score = rankedMoveSets[i].Value[f].Score;

                    // if the Score is not better than the current Score, skip it. Always keep 1 move (i > 0)
                    //if (i > 0 && !(isWhite && Score > currentScore) && !(!isWhite && Score < currentScore))
                    //    continue;

                    var move = rankedMoveSets[i].Value[f];
                    
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(move);

                    ChessMove bestFutureMove = new(move);

                    var newList = new List<ChessMove>(moveList);
                    newList.Add(move);
                    BestMoves.Add(new KeyValuePair<ChessMove, List<ChessMove>>(bestMove == null ? bestFutureMove : bestMove, newList));
                    Actions.Enqueue(new EvaluationThreadParameter()
                    {
                        Board = copyBoard,
                        BestMove = bestMove == null ? bestFutureMove : bestMove,
                        Depth = depth + 1,
                        IsWhite = !isWhite,
                        MoveList = newList
                    });
                }
            }







            /*
            var usedMoveSets = rankedMoveSets.First().Value;
            if (isWhite)
                usedMoveSets = usedMoveSets.OrderByDescending(x => x.Score).ToList();
            else
                usedMoveSets = usedMoveSets.OrderBy(x => x.Score).ToList();

            for (int i = 0; i < usedMoveSets.Count; i++)
            {
                var Score = usedMoveSets[i].Score;
                if (!(isWhite && Score >= currentScore) && !(!isWhite && Score <= currentScore))
                    continue;
                ChessMove bestFutMove = null;
                foreach (var pos in usedMoveSets)
                {
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(pos);
                    var enemyMoves = GetAllMoves(copyBoard, !isWhite);

                    if (enemyMoves.Count > 0)
                    {
                        double enemyScore = Score;

                        if (!isWhite) enemyScore = enemyMoves.OrderByDescending(x => x.Value.Average(y => y.Score)).First().Value.Min(y => y.Score);
                        else enemyScore = enemyMoves.OrderBy(x => x.Value.Average(y => y.Score)).First().Value.Max(y => y.Score);

                        if ((enemyScore <= Score && isWhite) || (enemyScore >= Score && !isWhite))
                        {
                            Score = enemyScore;
                            bestFutMove = pos;
                            futureMove.Add(pos);
                        }
                    } else
                    {
                        Score = isWhite ? -100000 : 100000;
                        bestFutMove = pos;
                    }
                }
            }

            // Add the best move to the Queue
            List<ChessMove> orderedMoves = futureMove.OrderByDescending(x => x.Score).ToList();
            if(!isWhite)
                orderedMoves = futureMove.OrderBy(x => x.Score).ToList();

            for (int i = 0; i < orderedMoves.Count; i++)
            {
                var move = orderedMoves[i];
                if (i == 0 || (move.Score > currentScore && isWhite) || (move.Score < currentScore && !isWhite))
                {
                    var copyBoard = board.Copy();
                    copyBoard.DoMove(move);
                    if (bestMove == null)
                        bestMove = new(move);
                    var newList = new List<ChessMove>(moveList);
                    newList.Add(move);
                    BestMoves.Add(new KeyValuePair<ChessMove, List<ChessMove>>(bestMove, newList));
                    Actions.Enqueue(new EvaluationThreadParameter()
                    {
                        Board = copyBoard,
                        BestMove = bestMove,
                        Depth = depth + 1,
                        IsWhite = !isWhite,
                        MoveList = newList
                    });
                }
            }
            */
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
                    ChessMove move = new ChessMove((int)tMoveVec.Key.X, (int)tMoveVec.Key.Y, (int)tMoveVecTo.X, (int)tMoveVecTo.Y, copyBoard.GetMaterialCount(UseSquareTable));
                
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
        public ChessMove BestMove;
        public List<ChessMove> MoveList = new List<ChessMove>();
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