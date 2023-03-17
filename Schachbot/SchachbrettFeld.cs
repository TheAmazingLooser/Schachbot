using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schachbot
{
    public class SchachbrettFeld
    {
        private bool _redraw = false;
        private static Texture2D _texture;

        public bool IstSchwarz { get; }
        public bool IstWeiss => !IstSchwarz;
        public ISchachfigur Figur { get; private set; }

        public SchachbrettFeld(bool istSchwarz)
        {
            IstSchwarz = istSchwarz;
            _redraw = true;
        }

        // Funktion zum setzen der Figur
        public void SetzeFigur(ISchachfigur figur)
        {
            _redraw = true;
            Figur = figur;
        }

        public void LöscheFigur()
        {
            _redraw = true;
            Figur = null;
        }

        // Funktion zum Zeichnen des Feldes
        public void Draw(SpriteBatch sb, int x, int y, int width, int height)
        {
            if (_redraw || _texture == null)
            {
                if (_texture == null)
                {
                    _texture = new Texture2D(sb.GraphicsDevice, 1, 1);
                    
                    // Farbe aus Weiss setzen, sonst ist es alpha = 0 (meh...)
                    Color[] data = new Color[1];
                    data[0] = Color.White;
                    _texture.SetData(data);
                }
                _redraw = false;
                sb.Draw(_texture, new Rectangle(x, y, width, height), IstSchwarz ? SchachGame.FeldSchwarz : SchachGame.FeldWeiss);
                if (Figur != null)
                {
                    Figur.Draw(sb, x, y, width, height, IstSchwarz);
                }
            }
        }
    }
}
