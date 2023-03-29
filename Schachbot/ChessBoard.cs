﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schachbot.Bot;
using Schachbot.Pieces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Schachbot;

public class ChessBoard
{
    public ChessField[][] Board;

    private List<Vector2> _hintPositions = new List<Vector2>();
    private Vector2 _hintPosition = new Vector2();
    private bool _whiteToMove = false;

    private Vector2 _whiteKingPosition = new Vector2();
    private Vector2 _blackKingPosition = new Vector2();

    private List<KeyValuePair<Vector2, Vector2>> _arrowList = new List<KeyValuePair<Vector2, Vector2>>();
    private int _arrowCount = 0;

    private double _elapsedMsBotMove = 0;
    private int _botMaxMoveDelay = 5000;

    private Evaluation evaluation = new Evaluation();

    public ChessBoard()
    {
        Random r = new Random();
        _whiteToMove = r.Next(0, 100) > 50;

    }

    public void HandleArrow(Vector2 start, Vector2 end, int width, int height)
    {
        start.X = (int)(start.X / (width / 8));
        start.Y = (int)(start.Y / (height / 8));

        end.X = (int)(end.X / (width / 8));
        end.Y = (int)(end.Y / (height / 8));

        _arrowList.Add(new KeyValuePair<Vector2, Vector2>(start, end));
        _arrowList = _arrowList.Distinct().ToList();
    }

    public ChessField GetField(int x, int y)
    {
        if (x < 0 || x >= 8) return null;
        if (y < 0 || y >= 8) return null;

        return Board[x][y];
    }

    public void InitializeField()
    {

        Board = new ChessField[8][];
        for (int x = 0; x < 8; x++)
        {
            Board[x] = new ChessField[8];
            for (int y = 0; y < 8; y++)
            {
                Board[x][y] = new ChessField((x + y) % 2 == 1, x, y);
            }
        }

        for (int x = 0; x < 8; x++)
        {
            Board[x][1].PlacePiece(new Pawn());
            Board[x][6].PlacePiece(new Pawn(true));
        }

        foreach(var y in new[] {0,7})
        {
            Board[0][y].PlacePiece(new Rook(y == 7));
            Board[1][y].PlacePiece(new Knight(y == 7));
            Board[2][y].PlacePiece(new Bishop(y == 7));
            Board[4][y].PlacePiece(new Queen(y == 7));
            Board[5][y].PlacePiece(new Bishop(y == 7));
            Board[6][y].PlacePiece(new Knight(y == 7));
            Board[7][y].PlacePiece(new Rook(y == 7));
        }

        King weiss = new King();
        King schwarz = new King(true);

        weiss.Moved += (fX, fY, tX, tY) => KingMoved(true, new Vector2(tX, tY));
        schwarz.Moved += (fX, fY, tX, tY) => KingMoved(false, new Vector2(tX, tY));

        Board[3][0].PlacePiece(weiss);
        Board[3][7].PlacePiece(schwarz);
    }

    public void InitializeField(string FEN)
    {
        Board = new ChessField[8][];
        for (int x = 0; x < 8; x++)
        {
            Board[x] = new ChessField[8];
            for (int y = 0; y < 8; y++)
            {
                Board[x][y] = new ChessField((x + y) % 2 == 1, x, y);
            }
        }

        try
        {
            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            int Line = 0;
            int Col = 0;

            string enPassant = "";
            foreach (var chr in FEN)
            {
                if (chr == '/' || chr == ' ')
                {
                    Col = 0;
                    Line++;


                    if (Line == 11 && enPassant != "")
                    {
                        int Column = enPassant.ToLower().First() - 'a';
                        int Row = enPassant.ToLower().Last() - '1';

                        if (Row == 5)
                        {
                            Row = 3;
                        } else if (Row == 2)
                        {
                            Row = 4;
                        }

                        if (Row != 4 && Row != 3)
                        {
                            throw new Exception("En Passant \"" + enPassant + "\"ist falsch.");
                        }

                        if (Board[Column][Row].Piece is Pawn p)
                        {
                            p.HasMoved2 = true;
                        }
                    }

                    continue;
                }

                if (Line == 8)
                {
                    var str = chr.ToString().ToLower();
                    _whiteToMove = str == "w";
                    continue;
                }

                if (Line == 9)
                {

                    continue;
                }

                if (Line == 10)
                {
                    if (chr != '-')
                    {
                        enPassant += chr;
                    }
                    continue;
                }

                bool isBlack = chr >= 'a' && chr <= 'z';

                switch (chr.ToString().ToLower().First())
                {
                    case 'r':
                        Board[Col][Line].PlacePiece(new Rook(isBlack));
                        Col++;
                        break;
                    case 'n':
                        Board[Col][Line].PlacePiece(new Knight(isBlack));
                        Col++;
                        break;
                    case 'b':
                        Board[Col][Line].PlacePiece(new Bishop(isBlack));
                        Col++;
                        break;
                    case 'q':
                        Board[Col][Line].PlacePiece(new Queen(isBlack));
                        Col++;
                        break;
                    case 'k':
                        King k = new King(isBlack);
                        if (isBlack)
                            k.Moved += (fX, fY, tX, tY) => KingMoved(false, new Vector2(tX, tY));
                        else
                            k.Moved += (fX, fY, tX, tY) => KingMoved(true, new Vector2(tX, tY));
                        Board[Col][Line].PlacePiece(k);
                        Col++;
                        break;
                    case 'p':
                        Board[Col][Line].PlacePiece(new Pawn(isBlack));
                        Col++;
                        break;
                    default:
                        Col += chr - '0';
                        break;
                }
            }
        } catch(Exception ex)
        {
            Console.Error.WriteLine("Fehler bei der FEN-Notation: " + FEN);
        }
    }

    private void KingMoved(bool white, Vector2 pos)
    {
        if (white) _whiteKingPosition = pos;
        else _blackKingPosition = pos;
    }
    public void Randomize()
    {
        Random r = new Random();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Board[x][y].Piece is IChessPiece figur)
                {
                    // Zufallsposition bekommen:
                    int xRand = r.Next(0, 8);
                    int yRand = r.Next(0, 8);

                    Board[x][y].PlacePiece(Board[xRand][yRand].Piece);
                    Board[xRand][yRand].PlacePiece(figur);
                }
            }
        }
    }

    public bool IsChecked(bool white)
    {
        Vector2 königPosition = GetKingPosition(white);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Board[x][y].Piece is IChessPiece figur && figur.IsWhite != white)
                {
                    List<Vector2> moves = figur.GetLegalMoves(this, true);
                    if (moves.Contains(königPosition))
                        return true;
                }
            }
        }

        return false;
    }

    public void Update(GameTime gameTime)
    {
        if (_whiteToMove)
        {
            /*
            if (_elapsedMsBotMove == 0 && !Evaluation_Old.IsStillRunning())
            {
                _elapsedMsBotMove = gameTime.TotalGameTime.TotalMilliseconds;
                Evaluation_Old.Init();
                Evaluation_Old.GetBestMove(this, 0, true, null);
            } else
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _elapsedMsBotMove >= _botMaxMoveDelay || gameTime.IsRunningSlowly)
                {
                    Evaluation_Old.Stop = true;
                }

                if (!Evaluation_Old.IsStillRunning())
                {
                    if (Evaluation_Old.BestMove == null)
                    {
                        Console.WriteLine("Player WON by checkmate!");
                    } else
                    {
                        Console.WriteLine("Best move: " + Evaluation_Old.BestMove.Value.Key + " -> " + Evaluation_Old.BestMove.Value.Value.Key + " (" + Evaluation_Old.GetMaterialCount(Board) + " -> " + Evaluation_Old.HighestScore + "). Thought for " + _elapsedMsBotMove + " ms. Depth -> " + Evaluation_Old.Depth + " == " + Evaluation_Old.FieldString);

                        DoBotMove(new List<Vector2>{
                        Evaluation_Old.BestMove.Value.Key,
                        Evaluation_Old.BestMove.Value.Value.Key
                        });
                    }

                    _elapsedMsBotMove = 0;
                    _whiteToMove = false;
                }
            }
            */
            if (_elapsedMsBotMove == 0 && !evaluation.IsRunning())
            {
                _elapsedMsBotMove = gameTime.ElapsedGameTime.TotalMilliseconds;
                evaluation = new Evaluation();
                evaluation.Start();
                evaluation.Evaluate(this, true);
            } else
            {
                _elapsedMsBotMove += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_elapsedMsBotMove >= _botMaxMoveDelay && evaluation.BestMoves.Count > 0)
                {
                    evaluation.Stop();
                    double Score = 0;
                    var bestMove = evaluation.GetBestMove(true, out Score);
                    Console.WriteLine("[WHT] Best move: " + bestMove + " (" + GetMaterialCount(false) + " -> " + Score + "). Thought for " + _elapsedMsBotMove + " ms. Depth -> " + evaluation.Depth);


                    DoMove(bestMove);

                    _elapsedMsBotMove = 0;
                    _whiteToMove = false;
                }
            }
        } else if (ChessGame.IsBotVsBot)
        {
            if (_elapsedMsBotMove == 0 && !evaluation.IsRunning())
            {
                _elapsedMsBotMove = gameTime.ElapsedGameTime.TotalMilliseconds;
                evaluation = new Evaluation();
                evaluation.Start();
                evaluation.Evaluate(this, false);
            }
            else
            {
                _elapsedMsBotMove += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_elapsedMsBotMove >= _botMaxMoveDelay && evaluation.BestMoves.Count > 0)
                {
                    evaluation.Stop();
                    double Score = 0;
                    var bestMove = evaluation.GetBestMove(false, out Score);
                    Console.WriteLine("[BLK] Best move: " + bestMove + " (" + GetMaterialCount() + " -> " + Score + "). Thought for " + _elapsedMsBotMove + " ms. Depth -> " + evaluation.Depth);


                    DoMove(bestMove);

                    _elapsedMsBotMove = 0;
                    _whiteToMove = true;
                }
            }
        }
    }

    private void ConvertPawnsToQueens()
    {
        for (int x = 0; x < 8; x++)
        {
            foreach(int y in new[] {0,7})
            {
                if (Board[x][y].Piece is Pawn figur)
                {
                    Board[x][y].PlacePiece(new Queen(figur.IsBlack));
                }
            }
        }
    }

    public void DoMove(List<Vector2> move)
    {
        int xStart = (int)move[0].X;
        int yStart = (int)move[0].Y;
        int xEnd = (int)move[1].X;
        int yEnd = (int)move[1].Y;

        Board[xEnd][yEnd].PlacePiece(Board[xStart][yStart].Piece);
        Board[xStart][yStart].PlacePiece(null);

        ConvertPawnsToQueens();
    }
    public void DoMove(ChessMove move)
    {
        Board[move.ToX][move.ToY].PlacePiece(Board[move.FromX][move.FromY].Piece);
        Board[move.FromX][move.FromY].PlacePiece(null);

        ConvertPawnsToQueens();
    }

    public void DoBotMove(List<Vector2> move)
    {
        DoMove(move);   
        //UpdatePawn2Move(false);
        
        _whiteToMove = false;
    }

    private void UpdatePawn2Move(bool white)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Board[x][y].Piece is Pawn figur && figur.IsWhite == white)
                {
                    figur.HasMoved2 = false;
                }
            }
        }
    }

    public void DoPlayerMove(int width, int height)
    {
        MouseState ms = Mouse.GetState();
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        int x = ms.X / Feldbreite;
        int y = ms.Y / Feldhöhe;

        if (_hintPositions.Contains(new Vector2(x,y)) && !_whiteToMove)
        {
            // En Passant abhandeln
            if (Board[(int)_hintPosition.X][(int)_hintPosition.Y].Piece is Pawn p && Board[x][y].Piece is null && x != (int)_hintPosition.X)
            {
                Board[x][(int)_hintPosition.Y].PlacePiece(null);
            }

            Board[x][y].PlacePiece(Board[(int)_hintPosition.X][(int)_hintPosition.Y].Piece);
            Board[(int)_hintPosition.X][(int)_hintPosition.Y].PlacePiece(null);

            _whiteToMove = true;
            _hintPositions.Clear();

            ConvertPawnsToQueens();
            UpdatePawn2Move(true);
            return;
        }

        Dictionary<Vector2, List<Vector2>> AntiCheckMoves = GetNonCheckingMoves(false);
        if (Board[x][y].Piece is IChessPiece figur && !figur.IsWhite)
        {
            var curPos = new Vector2(x, y);
            if ((AntiCheckMoves.Count > 0 && AntiCheckMoves.ContainsKey(curPos)))
            {
                _hintPositions.Clear();
                List<Vector2> allPos = figur.GetLegalMoves(this);
                foreach(var pos in allPos)
                {
                    if (AntiCheckMoves[curPos].Contains(pos))
                        _hintPositions.Add(pos);

                }
                _hintPosition = new Vector2(x, y);
            } else if (AntiCheckMoves.Count == 0 && IsChecked(false))
            {
                // Wir haben verloren == Schachmatt
            } else if (AntiCheckMoves.Count == 0)
            {
                // Patt
            }
        } else
        {
            _hintPositions.Clear();
        }
    }

    public Vector2 GetKingPosition(bool isWhite)
    {
        Vector2 altePos = new Vector2(_blackKingPosition.X, _blackKingPosition.Y);
        if (isWhite)
            altePos = new Vector2(_whiteKingPosition.X, _whiteKingPosition.Y);

        if (GetField((int)altePos.X, (int)altePos.Y).Piece is King k && k.IsWhite == isWhite)
        {
            return altePos;
        }

        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                if (GetField(x, y).Piece is King king && king.IsWhite == isWhite)
                    return new Vector2(x, y);
            }
        }

        return altePos;
    }

    public Dictionary<Vector2, List<Vector2>> GetNonCheckingMoves(bool isWhite)
    {
        Dictionary<Vector2, List<Vector2>> AntiCheckMoves = new Dictionary<Vector2, List<Vector2>>();
        try
        {
            Vector2 altePos = GetKingPosition(isWhite);
            for (int xS = 0; xS < 8; xS++)
            {
                for (int yS = 0; yS < 8; yS++)
                {
                    if (Board[xS][yS].Piece is IChessPiece figurS && isWhite == figurS.IsWhite)
                    {
                        if (figurS is Pawn p)
                        {
                            p.SuppressMoveEvent = true;
                        }
                        List<Vector2> moves = figurS.GetLegalMoves(this);
                        // Simulieren von einem Move
                        foreach (var move in moves)
                        {
                            var oldFigur = Board[(int)move.X][(int)move.Y].Piece;
                            Board[(int)move.X][(int)move.Y].PlacePiece(Board[xS][yS].Piece);
                            Board[xS][yS].PlacePiece(null);

                            if (!IsChecked(isWhite))
                            {
                                if (AntiCheckMoves.ContainsKey(new Vector2(xS, yS)))
                                    AntiCheckMoves[new Vector2(xS, yS)].Add(move);
                                else
                                    AntiCheckMoves.Add(new Vector2(xS, yS), new List<Vector2>() { move });
                            }
                            Board[xS][yS].PlacePiece(figurS);
                            Board[(int)move.X][(int)move.Y].PlacePiece(oldFigur);
                            if (isWhite)
                                _whiteKingPosition = altePos;
                            else
                                _blackKingPosition = altePos;
                        }
                        if (figurS is Pawn p2)
                        {
                            p2.SuppressMoveEvent = false;
                        }
                    }
                }
            }

        } finally
        {
            // Ignore
        }
        return AntiCheckMoves;
    }
    
    public void Draw(SpriteBatch sb, int width, int height)
    {
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Überprüfen ob die Position die eines Königs ist.
                Vector2 pos = new Vector2(x, y);

                bool imSchach = false;
                if (pos == GetKingPosition(true))
                {
                    imSchach = IsChecked(true);
                }
                if (pos == GetKingPosition(false))
                {
                    imSchach = IsChecked(false);
                }

                Board[x][y].Draw(sb, Feldbreite, Feldhöhe, _hintPositions.Contains(new Vector2(x, y)), imSchach);
            }
        }

        if (_arrowCount != _arrowList.Count)
        {
            _arrowCount = _arrowList.Count;
        }
    }

    public override string ToString()
    {
        string s = "";
        for (int y = 0; y < 8; y++)
        {
            int i = 0;
            for (int x = 0; x < 8; x++)
            {
                if (Board[x][y].Piece is null)
                {
                    i++;
                }
                else
                {
                    if (i > 0)
                    {
                        s += i.ToString();
                        i = 0;
                    }
                    s += Board[x][y].Piece.FEN_Name();
                }
            }
            if (i > 0)
            {
                s += i.ToString();
                i = 0;
            }
            s += " / ";
        }
        return s;
    }

    public ChessBoard Copy()
    {

        ChessBoard copy = new ChessBoard();
        copy.Board = new ChessField[8][];

        for (int x = 0; x < 8; x++)
        {
            copy.Board[x] = new ChessField[8];
            for (int y = 0; y < 8; y++)
            {
                copy.Board[x][y] = new ChessField(Board[x][y].IsBlack, x, y);
                BasePiece bp = null;
                if (Board[x][y].Piece is Pawn pawn)
                    bp = new Pawn(pawn);
                else if (Board[x][y].Piece is Rook rook)
                    bp = new Rook(rook);
                else if (Board[x][y].Piece is Knight knight)
                    bp = new Knight(knight);
                else if (Board[x][y].Piece is Bishop bishop)
                    bp = new Bishop(bishop);
                else if (Board[x][y].Piece is Queen queen)
                    bp = new Queen(queen);
                else if (Board[x][y].Piece is King king)
                {
                    bp = new King(king);
                    if (bp.IsBlack)
                        bp.Moved += (fX, fY, tX, tY) => KingMoved(false, new Vector2(tX, tY));
                    else
                        bp.Moved += (fX, fY, tX, tY) => KingMoved(true, new Vector2(tX, tY));
                }

                copy.Board[x][y].PlacePiece(bp);
            }
        }

        return copy;
    }

    public double GetMaterialCount(bool IncludeSquareTable = true, bool IncludePawnStructure = true)
    {
        double materialCount = IncludePawnStructure ? PieceSquareTables.EvaluatePawnStructure(this) : 0;

        double div = 1.0;

        foreach (ChessField[] rank in Board)
        {
            foreach (ChessField field in rank)
            {
                if (field.Piece is BasePiece bp)
                {
                    double val = bp.MaterialValue;

                    if (field.Piece.IsBlack)
                    {
                        materialCount -= val;
                        if (IncludeSquareTable)
                        {
                            if (field.Piece.GetType() == typeof(Pieces.Rook))
                                materialCount -= PieceSquareTables.pst_rook_black[(field.y * 8) + field.x] * -1 / div;
                            if (field.Piece.GetType() == typeof(Pieces.Bishop))
                                materialCount -= PieceSquareTables.pst_bishop_black[(field.y * 8) + field.x] * -1 / div;
                            if (field.Piece.GetType() == typeof(Pieces.King))
                                materialCount -= PieceSquareTables.pst_king_black[(field.y * 8) + field.x] * -1 / div;
                            if (field.Piece.GetType() == typeof(Pieces.Knight))
                                materialCount -= PieceSquareTables.pst_knight_black[(field.y * 8) + field.x] * -1 / div;
                            if (field.Piece.GetType() == typeof(Pieces.Pawn))
                                materialCount -= PieceSquareTables.pst_pawn_black[(field.y * 8) + field.x] * -1 / div;
                        }
                    }
                    else
                    {
                        materialCount += val;
                        if (IncludeSquareTable)
                        {
                            if (field.Piece.GetType() == typeof(Pieces.Rook))
                                materialCount += PieceSquareTables.pst_rook_white[(field.y * 8) + field.x] / div;
                            if (field.Piece.GetType() == typeof(Pieces.Bishop))
                                materialCount += PieceSquareTables.pst_bishop_white[(field.y * 8) + field.x] / div;
                            if (field.Piece.GetType() == typeof(Pieces.King))
                                materialCount += PieceSquareTables.pst_king_white[(field.y * 8) + field.x] / div;
                            if (field.Piece.GetType() == typeof(Pieces.Knight))
                                materialCount += PieceSquareTables.pst_knight_white[(field.y * 8) + field.x] / div;
                            if (field.Piece.GetType() == typeof(Pieces.Pawn))
                                materialCount += PieceSquareTables.pst_pawn_white[(field.y * 8) + field.x] / div;
                        }
                    }
                }
            }
        }

        return materialCount;
    }
}