using Microsoft.Xna.Framework;
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
    
    public ChessBoard()
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
        Vector2 königPosition = white ? _whiteKingPosition : _blackKingPosition;
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
            DoBotMove(Evaluation.GetBestMove(this, 0, true));

            _whiteToMove = false;
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

    public void DoBotMove(List<Vector2> move)
    {
        int xStart = (int)move[0].X;

        int yStart = (int)move[0].Y;
        int xEnd = (int)move[1].X;
        int yEnd = (int)move[1].Y;

        

        Board[xEnd][yEnd].PlacePiece(Board[xStart][yStart].Piece);
        Board[xStart][yStart].PlacePiece(null);
        
        ConvertPawnsToQueens();
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
            if ((AntiCheckMoves.Count > 0 && IsChecked(false) && AntiCheckMoves.ContainsKey(curPos)) || !IsChecked(false))
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

    private Dictionary<Vector2, List<Vector2>> GetNonCheckingMoves(bool isWhite)
    {
        Dictionary<Vector2, List<Vector2>> AntiCheckMoves = new Dictionary<Vector2, List<Vector2>>();
        Vector2 altePos = new Vector2(_blackKingPosition.X, _blackKingPosition.Y);
        if (isWhite)
            altePos = new Vector2(_whiteKingPosition.X, _whiteKingPosition.Y);
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
                if (pos == _whiteKingPosition)
                {
                    imSchach = IsChecked(true);
                }
                if (pos == _blackKingPosition)
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
}