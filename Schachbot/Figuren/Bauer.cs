using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Figuren;

public class Bauer : ISchachfigur
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }
    public bool IstSchwarz { get; set; }
    public bool IstWeiss => !IstSchwarz;

    public bool HasMoved2 { get; set; }

    
    public event BewegtHandler Bewegt;

    public Bauer(bool istSchwarz = false)
    {
        IstSchwarz = istSchwarz;
        HasMoved2 = false;
    }


    public void Draw(SpriteBatch sb, int x, int y, int width, int height, bool isBlackField)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Bauer.png"));
        }

        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/Bauer_outline.png"));
        }
        
        sb.Draw(_texture, new Rectangle(x,y, width, height), IstSchwarz ? (isBlackField ? SchachGame.FigurSchwarz : SchachGame.FigurSchwarzWeiss) : SchachGame.FigurWeiss);
        sb.Draw(_outline, new Rectangle(x,y, width, height), isBlackField ? (IstSchwarz ? SchachGame.OutlineWeißSchwarz : SchachGame.OutlineWeiss) : SchachGame.OutlineWeißSchwarz);
    }

    public List<Vector2> GetLegalMoves(Schachbot.Schachbrett schachbrett, int x, int y, bool isCapturingOnly = false)
    {
        List<Vector2> toReturn = new List<Vector2>();

        void AddIfPossible(int yO)
        {
            SchachbrettFeld feld = schachbrett.GetFeld(x, y + yO);
            if (feld != null && feld.Figur == null)
                toReturn.Add(new Vector2(x, y + yO));
        }

        void AddIfPoissibleCatch(int xO, int yO)
        {
            SchachbrettFeld feld = schachbrett.GetFeld(x + xO, y + yO);
            if (feld != null && (feld.Figur is ISchachfigur sf && sf.IstSchwarz != IstSchwarz))
                toReturn.Add(new Vector2(x + xO, y + yO));
        }

        if (!isCapturingOnly)
        {
            AddIfPossible(IstWeiss ? 1 : -1);
            if ((IstSchwarz && y == 6) || (IstWeiss && y == 1))
            {
                AddIfPossible(IstWeiss ? 2 : -2);
            }
        }

        AddIfPoissibleCatch(1, IstWeiss ? 1 : -1);
        AddIfPoissibleCatch(-1, IstWeiss ? 1 : -1);

        return toReturn;
    }

    public void Bewege(int x, int y)
    {
        Bewegt?.Invoke(x, y);
    }
}