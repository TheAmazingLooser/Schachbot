using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot.Figuren;

public class König : ISchachfigur
{
    private static Texture2D _texture { get; set; }
    private static Texture2D _outline { get; set; }
    public bool IstSchwarz { get; set; }

    public König(bool istSchwarz = false)
    {
        IstSchwarz = istSchwarz;
    }

    public void Draw(SpriteBatch sb, int x, int y, int width, int height, bool isBlackField)
    {
        if (_texture == null)
        {
            _texture = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/König.png"));
        }

        if (_outline == null)
        {
            _outline = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead("Figuren/König_outline.png"));
        }
        
        sb.Draw(_texture, new Rectangle(x,y, width, height), IstSchwarz ? (isBlackField ? SchachGame.FigurSchwarz : SchachGame.FigurSchwarzWeiss) : SchachGame.FigurWeiss);
        sb.Draw(_outline, new Rectangle(x,y, width, height), isBlackField ? (IstSchwarz ? SchachGame.OutlineWeißSchwarz : SchachGame.OutlineWeiss) : SchachGame.OutlineWeißSchwarz);
    }
}