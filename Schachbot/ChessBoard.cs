using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schachbot.Pieces;

namespace Schachbot;

public class ChessBoard
{
    public ChessField[][] Board;

    private List<Vector2> _hintPositions = new List<Vector2>();
    private Vector2 _hintPosition = new Vector2();
    private bool _whiteToMove = false;

    private Vector2 _whiteKingPosition = new Vector2();
    private Vector2 _blackKingPosition = new Vector2();
    
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

        weiss.Moved += (x, y) => KingMoved(true, new Vector2(x, y));
        schwarz.Moved += (x, y) => KingMoved(false, new Vector2(x, y));

        Board[3][0].PlacePiece(weiss);
        Board[3][7].PlacePiece(schwarz);
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
            Dictionary<Vector2, List<Vector2>> botZüge = new Dictionary<Vector2, List<Vector2>>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Board[x][y].Piece is IChessPiece figur && figur.IsWhite)
                    {
                        List<Vector2> moves = figur.GetLegalMoves(this);
                        if (moves.Count > 0)
                            botZüge.Add(new Vector2(x, y), moves);
                    }
                }
            }

            if (botZüge.Count > 0)
            {
                // Zufälligen Zug auswählen
                Random r = new Random();
                int figurPosition = r.Next(0, botZüge.Count);
                int xStart = (int)botZüge.ElementAt(figurPosition).Key.X;
                int yStart = (int)botZüge.ElementAt(figurPosition).Key.Y;

                int zug = r.Next(0, botZüge.ElementAt(figurPosition).Value.Count);
                int xZiel = (int)botZüge.ElementAt(figurPosition).Value.ElementAt(zug).X;
                int yZiel = (int)botZüge.ElementAt(figurPosition).Value.ElementAt(zug).Y;

                Board[xZiel][yZiel].PlacePiece(Board[xStart][yStart].Piece);
                Board[xStart][yStart].PlacePiece(null);
            }
            _whiteToMove = false;
        }
    }

    public void Click(int width, int height)
    {
        MouseState ms = Mouse.GetState();
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        int x = ms.X / Feldbreite;
        int y = ms.Y / Feldhöhe;

        if (_hintPositions.Contains(new Vector2(x,y)) && !_whiteToMove)
        {
            Board[x][y].PlacePiece(Board[(int)_hintPosition.X][(int)_hintPosition.Y].Piece);
            Board[(int)_hintPosition.X][(int)_hintPosition.Y].PlacePiece(null);

            _whiteToMove = true;
            _hintPositions.Clear();
            return;
        }

        if (Board[x][y].Piece is IChessPiece figur && !figur.IsWhite)
        {
            Dictionary<Vector2, List<Vector2>> AntiSchachMoves = new Dictionary<Vector2, List<Vector2>>();
            if (IsChecked(false))
            {
                Vector2 altePos = new Vector2(_blackKingPosition.X, _blackKingPosition.Y);
                for (int xS = 0; xS < 8; xS++)
                {
                    for (int yS = 0; yS < 8; yS++)
                    {
                        if (Board[xS][yS].Piece is IChessPiece figurS && !figurS.IsWhite)
                        {
                            List<Vector2> moves = figurS.GetLegalMoves(this);
                            // Simulieren von einem Move
                            foreach(var move in moves)
                            {
                                var oldFigur = Board[(int)move.X][(int)move.Y].Piece;
                                Board[(int)move.X][(int)move.Y].PlacePiece(Board[xS][yS].Piece);
                                Board[xS][yS].PlacePiece(null);

                                if (!IsChecked(false))
                                {
                                    if (AntiSchachMoves.ContainsKey(new Vector2(xS, yS)))
                                        AntiSchachMoves[new Vector2(xS, yS)].Add(move);
                                    else
                                        AntiSchachMoves.Add(new Vector2(xS, yS), new List<Vector2>() { move });
                                }
                                Board[xS][yS].PlacePiece(figurS);
                                Board[(int)move.X][(int)move.Y].PlacePiece(oldFigur);
                                _blackKingPosition = altePos;
                            }
                        }
                    }
                }
            }

            var curPos = new Vector2(x, y);
            if (AntiSchachMoves.Count == 0 && IsChecked(false))
            {
                // Wir haben verloren == Schachmatt
            } else if (AntiSchachMoves.Count > 0 && IsChecked(false) && AntiSchachMoves.ContainsKey(curPos))
            {
                _hintPositions.Clear();
                List<Vector2> allPos = figur.GetLegalMoves(this);
                foreach(var pos in allPos)
                {
                    if (AntiSchachMoves[curPos].Contains(pos))
                        _hintPositions.Add(pos);

                }
                _hintPosition = new Vector2(x, y);
            } else if (!IsChecked(false))
            {
                _hintPositions = figur.GetLegalMoves(this);
                _hintPosition = new Vector2(x, y);
            } else
            {
                _hintPositions.Clear();
            }
        } else
        {
            _hintPositions.Clear();
        }
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
    }
}