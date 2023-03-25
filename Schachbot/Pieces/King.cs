using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Pieces;

public class King : BasePiece, IChessPiece
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }


    public King(bool isBlack = false)
    {
        IsBlack = isBlack;
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

        AddIfPossible(-1, 1);
        AddIfPossible(-1, 0);
        AddIfPossible(-1, -1);
        AddIfPossible(1, 1);
        AddIfPossible(1, 0);
        AddIfPossible(1, -1);
        AddIfPossible(0, -1);
        AddIfPossible(0, 1);

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