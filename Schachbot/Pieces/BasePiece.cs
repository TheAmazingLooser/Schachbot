using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot.Pieces
{
    public class BasePiece : IChessPiece
    {
        /// <summary>
        /// Gets the color of the piece. If it is black, this returns true.
        /// </summary>
        public bool IsBlack { get; protected set; }

        public int x { get; protected set; }
        public int y { get; protected set; }

        /// <summary>
        /// Always return the inverse of IsBlack (!IsBlack)!
        /// </summary>
        public bool IsWhite => !IsBlack;

        /// <summary>
        /// The moved event fires if that piece was moved.
        /// Only fires if <see cref="MoveTo(int, int)"/> was called.
        /// If you want to use that, make sure to invoke <see cref="MoveTo(int, int)"/> in your moving-logic
        /// </summary>
        public event MovedHandler Moved;
        
        /// <summary>
        /// The default ctor of every Chess Piece.
        /// </summary>
        /// <param name="isBlack">Parameter to determine wheter the piece is black (<paramref name="isBlack"/> = true). Defaults to false.</param>
        public BasePiece(bool isBlack = false)
        {
            IsBlack = isBlack;
        }

        /// <summary>
        /// A function which provides a list of legal moves possible with that piece.
        /// </summary>
        /// <param name="chessBoard">The current chess board of the game.</param>
        /// <param name="x">x position of the piece</param>
        /// <param name="y">y position of the piece</param>
        /// <param name="isCapturingOnly">Parameter to determine wheter the list should only contain moves which are possible capturing moves (only affects a <see cref="Pawn"/>)</param>
        /// <returns></returns>
        public virtual List<Vector2> GetLegalMoves(ChessBoard chessBoard, bool isCapturingOnly = false)
        {
            List<Vector2> toReturn = new List<Vector2>();

            AddIfNotSameTeam(chessBoard, -1, 1, toReturn);
            AddIfNotSameTeam(chessBoard, -1, 0, toReturn);
            AddIfNotSameTeam(chessBoard, -1, -1, toReturn);
            AddIfNotSameTeam(chessBoard, 1, 1, toReturn);
            AddIfNotSameTeam(chessBoard, 1, 0, toReturn);
            AddIfNotSameTeam(chessBoard, 1, -1, toReturn);
            AddIfNotSameTeam(chessBoard, 0, -1, toReturn);
            AddIfNotSameTeam(chessBoard, 0, 1, toReturn);

            return toReturn;
        }

        public virtual void MoveTo(int x, int y)
        {
            int oldX = this.x;
            int oldY = this.y;
            this.x = x;
            this.y = y;
            Moved?.Invoke(oldX, oldY, x, y);
        }
        
        public virtual Texture2D GetTexture(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }
        public virtual Texture2D GetOutline(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }
        public virtual void Draw(SpriteBatch sb, int width, int height, bool isBlackField)
        {
            sb.Draw(GetTexture(sb), new Rectangle(x * width, y * height, width, height), IsBlack ? (isBlackField ? ChessGame.PieceBlack : ChessGame.PieceBlackWhite) : ChessGame.PieceWhite);
            sb.Draw(GetOutline(sb), new Rectangle(x * width, y * height, width, height), isBlackField ? (IsBlack ? ChessGame.OutlineBlackWhite : ChessGame.OutlineWhite) : ChessGame.OutlineBlackWhite);
        }

        protected bool AddIfNotSameTeamDiagonal(ChessBoard chessBoard, int f1, int f2, List<Vector2> toReturn)
        {
            if (chessBoard.GetField(x + f1, y + f2) is ChessField feld && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack || feld.Piece == null))
            {
                toReturn.Add(new Vector2(x + f1, y + f2));
                if (feld.Piece == null)
                    return true;

                return false;
            }

            return false;
        }

        protected bool AddIfNotSameTeamStraight(ChessBoard chessBoard, int f, bool isX, List<Vector2> toReturn)
        {
            if (chessBoard.GetField(x + (isX ? f : 0), y + (!isX ? f : 0)) is ChessField feld && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack || feld.Piece == null))
            {
                toReturn.Add(new Vector2(x + (isX ? f : 0), y + (!isX ? f : 0)));

                if (feld.Piece == null)
                    return true;

                return false;
            }

            return false;
        }

        protected bool AddIfNotSameTeam(ChessBoard chessBoard, int xO, int yO, List<Vector2> toReturn)
        {
            ChessField feld = chessBoard.GetField(x + xO, y + yO);
            if (feld != null && (feld.Piece is IChessPiece sf && sf.IsBlack != IsBlack || feld.Piece == null))
                toReturn.Add(new Vector2(x + xO, y + yO));

            return true;
        }

        public string FEN_Name()
        {
            throw new NotImplementedException();
        }
    }
}
