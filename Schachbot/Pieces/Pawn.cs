using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Pieces;

public class Pawn : BasePiece, IChessPiece
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }

    public bool HasMoved2 { get; set; }

    public bool SuppressMoveEvent { get; set; } = false;

    public Pawn(bool isBlack = false)
    {
        MaterialValue = 100;
        IsBlack = isBlack;
        HasMoved2 = false;

        Moved += (fX, fY, tX, tY) =>
        {
            if(!SuppressMoveEvent)
            {
                HasMoved2 = Math.Abs(fY - tY) == 2;
            }
        };
    }

    public Pawn(Pawn p)
    {
        y = p.y;
        x = p.x;
        IsBlack = p.IsBlack;
        MaterialValue = p.MaterialValue;
        HasMoved2 = p.HasMoved2;
        Moved += (fX, fY, tX, tY) =>
        {
            if (!SuppressMoveEvent)
            {
                if (Math.Abs(fY - tY) == 1)
                    HasMoved2 = false;
            }
        };
    }

    public override Texture2D GetTexture(SpriteBatch sb)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Bauer.png"));
        }
        return _texture;
    }
    public override Texture2D GetOutline(SpriteBatch sb)
    {
        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Bauer_outline.png"));
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
    public override List<Vector2> GetLegalMoves(Schachbot.ChessBoard chessBoard, bool isCapturingOnly = false)
    {
        List<Vector2> toReturn = new List<Vector2>();

        bool AddIfPossible(int yO)
        {
            ChessField feld = chessBoard.GetField(x, y + yO);
            if (feld != null && feld.Piece == null)
            {
                toReturn.Add(new Vector2(x, y + yO));
                return true;
            }

            return false;
        }

        void AddIfPoissibleCatch(int xO, int yO)
        {
            ChessField feld = chessBoard.GetField(x + xO, y + yO);
            if (feld != null && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        void AddIfPoissibleEnPassant(int xO, int yO)
        {
            ChessField feld = chessBoard.GetField(x + xO, y);
            if (feld != null && (feld.Piece is Pawn p && p.IsBlack != IsBlack && p.HasMoved2))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        if (!isCapturingOnly)
        {
            var added = AddIfPossible(IsWhite ? -1 : 1);
            if (added && (IsBlack && y == 1) || (IsWhite && y == 6))
            {
                AddIfPossible(IsWhite ? -2 : 2);
            }
        }

        AddIfPoissibleCatch(1, IsWhite ? -1 : 1);
        AddIfPoissibleCatch(-1, IsWhite ? -1 : 1);

        AddIfPoissibleEnPassant(1, IsWhite ? -1 : 1);
        AddIfPoissibleEnPassant(-1, IsWhite ? -1 : 1);

        return toReturn;
    }

    public string FEN_Name()
    {
        if (IsBlack)
            return "p";
        else
            return "P";
    }

    
}