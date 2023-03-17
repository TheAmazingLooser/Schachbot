using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Figuren;

public class Dame : ISchachfigur
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }
    public bool IstSchwarz { get; set; }

    public Dame(bool istSchwarz = false)
    {
        IstSchwarz = istSchwarz;
    }

    public void Draw(SpriteBatch sb, int x, int y, int width, int height, bool isBlackField)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Dame.png"));
        }

        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Dame_outline.png"));
        }

        sb.Draw(_texture, new Rectangle(x, y, width, height), IstSchwarz ? (isBlackField ? SchachGame.FigurSchwarz : SchachGame.FigurSchwarzWeiss) : SchachGame.FigurWeiss);
        sb.Draw(_outline, new Rectangle(x, y, width, height), isBlackField ? (IstSchwarz ? SchachGame.OutlineWeißSchwarz : SchachGame.OutlineWeiss) : SchachGame.OutlineWeißSchwarz);
    }

    // GetLegalMoves
    public List<Vector2> GetLegalMoves(Schachbot.Schachbrett schachbrett, int x, int y)
    {
        List<Vector2> toReturn = new List<Vector2>();

        bool AddIfNotSameTeamDiagonal(int f1, int f2)
        {
            if (schachbrett.GetFeld(x + f1, y + f2) is SchachbrettFeld feld && (feld.Figur is ISchachfigur sf && sf.IstSchwarz != IstSchwarz || feld.Figur == null))
            {
                toReturn.Add(new Vector2(x + f1, y + f2));
                return true;
            }

            return false;
        }

        bool AddIfNotSameTeamGerade(int f, bool isX)
        {
            if (schachbrett.GetFeld(x + (isX ? f : 0), y + (!isX ? f : 0)) is SchachbrettFeld feld && (feld.Figur is ISchachfigur sf && sf.IstSchwarz != IstSchwarz || feld.Figur == null))
            {
                toReturn.Add(new Vector2(x + (isX ? f : 0), y + (!isX ? f : 0)));

                if (feld.Figur == null)
                    return true;

                return false;
            }

            return false;
        }

        for (int f = 0; f < 7; f++)
        {
            if (!AddIfNotSameTeamDiagonal(f, f))
                break;
        }
        for (int f = 0; f < 7 && AddIfNotSameTeamDiagonal(-f, f); f++) ;
        for (int f = 0; f < 7 && AddIfNotSameTeamDiagonal(f, -f); f++) ;

        for (int f = 0; f < 7 && AddIfNotSameTeamGerade(f, true); f++) ;
        for (int f = 0; f < 7 && AddIfNotSameTeamGerade(f, false); f++) ;

        return toReturn;
    }
}