using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Pieces;

public class Knight : BasePiece, IChessPiece
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }

    
    public Knight(bool isBlack = false)
    {
        IsBlack = isBlack;
    }

    public override Texture2D GetTexture(SpriteBatch sb)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Springer.png"));
        }
        return _texture;
    }
    public override Texture2D GetOutline(SpriteBatch sb)
    {
        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Springer_outline.png"));
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

        AddIfNotSameTeam(chessBoard, 1, 2, toReturn);
        AddIfNotSameTeam(chessBoard, -1, 2, toReturn);
        AddIfNotSameTeam(chessBoard, 2, 1, toReturn);
        AddIfNotSameTeam(chessBoard, 2, -1, toReturn);
        AddIfNotSameTeam(chessBoard, 1, -2, toReturn);
        AddIfNotSameTeam(chessBoard, -1, -2, toReturn);
        AddIfNotSameTeam(chessBoard, -2, 1, toReturn);
        AddIfNotSameTeam(chessBoard, - 2, -1, toReturn);

        return toReturn;
    }

    public string FEN_Name()
    {
        if (IsBlack)
            return "n";
        else
            return "N";
    }

}