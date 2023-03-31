using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Schachbot.Pieces;

public class King : BasePiece, IChessPiece
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }

    public bool HasMoved { get; private set; } = false;
    public bool SuppressMoveEvent { get; set; } = false;

    public void ResetMoved()
    {
        HasMoved = false;
    }

    public King(bool isBlack = false)
    {
        MaterialValue = 9999;
        IsBlack = isBlack;
    }

    public override void MoveTo(int x, int y, bool initialMove = false)
    {
        if (!initialMove && !SuppressMoveEvent)
            HasMoved = true;
        base.MoveTo(x, y, initialMove);
    }

    public King(King p)
    {
        y = p.y;
        x = p.x;
        IsBlack = p.IsBlack;
        MaterialValue = p.MaterialValue;
    }

    public override Texture2D GetTexture(SpriteBatch sb)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/König.png"));
        }
        return _texture;
    }
    public override Texture2D GetOutline(SpriteBatch sb)
    {
        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/König_outline.png"));
        }
        return _outline;
    }
    
    /// <summary>
    /// A function which calculates a list of legal moves possible with that piece.
    /// </summary>
    /// <param name="chessBoard">The current chess board of the game.</param>
    /// <param name="x">x position of the piece</param>
    /// <param name="y">y position of the piece</param>
    /// <param name="isCapturingOnly">Parameter to determine wheter the list should only contain moves which are possible capturing moves (only affects a <see cref="Pawn"/>)</param>
    /// <returns>A list of possible moves which can be played.</returns>
    public override List<Vector2> GetLegalMoves(ChessBoard chessBoard, bool isCapturingOnly = false)
    {
        List<Vector2> toReturn = new List<Vector2>();

        void AddIfPossible(int xO, int yO)
        {
            ChessField feld = chessBoard.GetField(x + xO, y + yO);
            if (feld != null && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack || feld.Piece == null))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        void AddCastleIfPossible()
        {
            // Keine Rochade wenn King sich bereits bewegt hat!
            if (HasMoved) return;

            int y = 0;
            if (IsWhite)
                y = 7;

            // Wenn der Turm sich noch an der Anfangsposition befindet und sich noch nicht bewegt hat, dann ist eine Rochade mit diesen potentiell möglich!
            if (chessBoard.GetField(7, y).Piece is Rook r && !r.HasMoved)
            {
                var blockingmoves = chessBoard.GetCastleBlockingMoves(IsWhite);
                bool CanCastle = true;
                for (int i = x + 1; i < 7; i++)
                {
                    if (chessBoard.GetField(i, y).Piece != null || blockingmoves.Contains(new Vector2(i, y)))
                    {
                        CanCastle = false;
                        break;
                    }
                }

                if (CanCastle)
                {
                    toReturn.Add(new Vector2(7, y));
                }

                CanCastle = true;
                for (int i = x - 1; i > 0; i--)
                {
                    if (chessBoard.GetField(i, y).Piece != null || (blockingmoves.Contains(new Vector2(i, y)) && i != 1))
                    {
                        CanCastle = false;
                        break;
                    }
                }

                if (CanCastle)
                {
                    toReturn.Add(new Vector2(0, y));
                }
            }
        }

        AddIfPossible(-1, 1);
        AddIfPossible(-1, 0);
        AddIfPossible(-1, -1);
        AddIfPossible(1, 1);
        AddIfPossible(1, 0);
        AddIfPossible(1, -1);
        AddIfPossible(0, -1);
        AddIfPossible(0, 1);

        AddCastleIfPossible();
        //AddShortCastleIfPossible();

        return toReturn;
    }

    public string FEN_Name()
    {
        if (IsBlack)
            return "k";
        else
            return "K";
    }

}