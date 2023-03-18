using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot
{
    public class ChessField
    {
        private bool _redraw = true;
        private bool _redrawHighlight = false;
        private bool _redrawSchach = false;
        private static Texture2D _texture;

        public int x { get; private set; }
        public int y { get; private set; }

        public bool IsBlack { get; }
        public bool IsWhite => !IsBlack;
        public IChessPiece Piece { get; private set; }

        public ChessField(bool black, int x, int y)
        {
            IsBlack = black;
            _redraw = true;
            this.x = x;
            this.y = y;
        }

        // Funktion zum setzen der Figur
        public void PlacePiece(IChessPiece piece)
        {
            _redraw = true;
            Piece = piece;
            if (Piece != null)
                Piece.MoveTo(x, y);
        }

        // Funktion zum Zeichnen des Feldes
        public void Draw(SpriteBatch sb, int width, int height, bool highlight = false, bool schach = false)
        {
            int drawX = x * width;
            int drawY = y * height;
            if (_redraw || _texture == null || (_redrawHighlight != highlight) || _redrawSchach != schach)
            {
                _redrawSchach = schach;
                _redrawHighlight = highlight;
                if (_texture == null)
                {
                    _texture = new Texture2D(sb.GraphicsDevice, 1, 1);
                    
                    // Farbe aus Weiss setzen, sonst ist es alpha = 0 (meh...)
                    Color[] data = new Color[1];
                    data[0] = Color.White;
                    _texture.SetData(data);
                }
                _redraw = false;
                sb.Draw(_texture, new Rectangle(drawX, drawY, width, height), schach ? ChessGame.FieldCheck : (IsBlack ? ChessGame.FieldBlack : ChessGame.FieldWhite));
                // Wenn das Filg ein highlight hat, dann zeichne es
                if (highlight)
                {
                    sb.Draw(_texture, new Rectangle(drawX + width / 4, drawY + height / 4, width / 2, height / 2), ChessGame.FieldHighlight);
                }
                
                if (Piece != null)
                {
                    Piece.Draw(sb, width, height, IsBlack);
                }
            }
        }
    }
}
