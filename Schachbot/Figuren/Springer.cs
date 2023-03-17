﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Figuren;

public class Springer : ISchachfigur
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }
    public bool IstSchwarz { get; set; }

    public Springer(bool istSchwarz = false)
    {
        IstSchwarz = istSchwarz;
    }

    public void Draw(SpriteBatch sb, int x, int y, int width, int height, bool isBlackField)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Springer.png"));
        }

        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Springer_outline.png"));
        }
        
        sb.Draw(_texture, new Rectangle(x,y, width, height), IstSchwarz ? (isBlackField ? SchachGame.FigurSchwarz : SchachGame.FigurSchwarzWeiss) : SchachGame.FigurWeiss);
        sb.Draw(_outline, new Rectangle(x,y, width, height), isBlackField ? (IstSchwarz ? SchachGame.OutlineWeißSchwarz : SchachGame.OutlineWeiss) : SchachGame.OutlineWeißSchwarz);
    }

    public List<Vector2> GetLegalMoves(Schachbot.Schachbrett schachbrett, int x, int y)
    {
        List<Vector2> toReturn = new List<Vector2>();

        void AddIfPossible(int xO, int yO)
        {
            SchachbrettFeld feld = schachbrett.GetFeld(x + xO, y + yO);
            if (feld != null && (feld.Figur is ISchachfigur sf && sf.IstSchwarz != IstSchwarz || feld.Figur == null))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        AddIfPossible(1, 2);
        AddIfPossible(-1, 2);
        AddIfPossible(2, 1);
        AddIfPossible(2, -1);
        AddIfPossible(1, -2);
        AddIfPossible(-1, -2);
        AddIfPossible(-2, 1);
        AddIfPossible(-2, -1);

        return toReturn;
    }
}