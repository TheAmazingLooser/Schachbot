using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schachbot.Figuren;

namespace Schachbot;

public class Schachbrett
{
    public SchachbrettFeld[][] Brett;

    private MouseState lastMouseState;
    
    public Schachbrett()
    {
        Brett = new SchachbrettFeld[8][];
        for (int x = 0; x < 8; x++)
        {
            Brett[x] = new SchachbrettFeld[8];
            for (int y = 0; y < 8; y++)
            {
                Brett[x][y] = new SchachbrettFeld((x + y) % 2 == 1);
            }
        }
        
    }

    public SchachbrettFeld GetFeld(int x, int y)
    {
        if (x < 0 || x >= 8) return null;
        if (y < 0 || y >= 8) return null;

        return Brett[x][y];
    }

    public void InitialisiereBrett()
    {

        for (int i = 0; i < 8; i++)
        {
            Brett[i][1].SetzeFigur(new Bauer());
            Brett[i][6].SetzeFigur(new Bauer(true));
        }

        foreach(var y in new[] {0,7})
        {
            Brett[0][y].SetzeFigur(new Turm(y == 7));
            Brett[1][y].SetzeFigur(new Springer(y == 7));
            Brett[2][y].SetzeFigur(new Läufer(y == 7));
            Brett[3][y].SetzeFigur(new König(y == 7));
            Brett[4][y].SetzeFigur(new Dame(y == 7));
            Brett[5][y].SetzeFigur(new Läufer(y == 7));
            Brett[6][y].SetzeFigur(new Springer(y == 7));
            Brett[7][y].SetzeFigur(new Turm(y == 7));
        }
    }
    
    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();

        // Auf klicks überprüfen
        if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
        {
            Console.WriteLine("Click");
        }

        lastMouseState = mouseState;
    }
    
    public void Draw(SpriteBatch sb, int width, int height)
    {
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Brett[x][y].Draw(sb, x * Feldbreite, y * Feldhöhe, Feldbreite, Feldhöhe);
            }
        }
    }
}