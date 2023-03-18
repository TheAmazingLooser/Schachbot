﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Pieces;

public class Pawn : BasePiece, IChessPiece
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }

    public bool HasMoved2 { get; set; }

    public Pawn(bool isBlack = false)
    {
        IsBlack = isBlack;
        HasMoved2 = false;
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

        void AddIfPossible(int yO)
        {
            ChessField feld = chessBoard.GetField(x, y + yO);
            if (feld != null && feld.Piece == null)
                toReturn.Add(new Vector2(x, y + yO));
        }

        void AddIfPoissibleCatch(int xO, int yO)
        {
            ChessField feld = chessBoard.GetField(x + xO, y + yO);
            if (feld != null && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        if (!isCapturingOnly)
        {
            AddIfPossible(IsWhite ? 1 : -1);
            if ((IsBlack && y == 6) || (IsWhite && y == 1))
            {
                AddIfPossible(IsWhite ? 2 : -2);
            }
        }

        AddIfPoissibleCatch(1, IsWhite ? 1 : -1);
        AddIfPoissibleCatch(-1, IsWhite ? 1 : -1);

        return toReturn;
    }
}