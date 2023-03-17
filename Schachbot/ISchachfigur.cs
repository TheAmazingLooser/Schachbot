using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot;

public interface ISchachfigur
{
    public bool IstSchwarz { get; set; }
    public bool IstWeiss => !IstSchwarz;
    
    public void Draw(SpriteBatch sb, int x, int y, int width, int height, bool isBlackField);

    public List<Vector2> GetLegalMoves(Schachbot.Schachbrett schachbrett, int x, int y);
}