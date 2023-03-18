using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot;

public delegate void MovedHandler(int x, int y);

public interface IChessPiece
{
    public bool IsBlack { get; }
    public bool IsWhite => !IsBlack;

    public int x { get; }
    public int y { get; }

    public void Draw(SpriteBatch sb, int width, int height, bool isBlackField);

    public List<Vector2> GetLegalMoves(Schachbot.ChessBoard chessBoard, bool isCapturingOnly = false);

    public event MovedHandler Moved;
    public void MoveTo(int x, int y);
}