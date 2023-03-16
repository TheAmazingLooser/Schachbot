using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schachbot.Figuren;

namespace Schachbot;

public class Schachbrett
{
    public ISchachfigur[][] Brett;
    
    private Texture2D _texture;

    private MouseState lastMouseState;
    
    public Schachbrett()
    {
        Brett = new ISchachfigur[8][];
        for (int i = 0; i < 8; i++)
        {
            Brett[i] = new ISchachfigur[8];
        }
        
    }

    public void InitialisiereBrett()
    {
        Brett[0][0] = new Turm();
        Brett[1][0] = new Springer();
        Brett[2][0] = new Läufer();
        Brett[3][0] = new König();
        Brett[4][0] = new Dame();
        Brett[5][0] = new Läufer();
        Brett[6][0] = new Springer();
        Brett[7][0] = new Turm();

        for (int i = 0; i < 8; i++)
        {
            Brett[i][1] = new Bauer();
            Brett[i][6] = new Bauer(true);
        }
        
        Brett[0][7] = new Turm(true);
        Brett[1][7] = new Springer(true);
        Brett[2][7] = new Läufer(true);
        Brett[3][7] = new König(true);
        Brett[4][7] = new Dame(true);
        Brett[5][7] = new Läufer(true);
        Brett[6][7] = new Springer(true);
        Brett[7][7] = new Turm(true);
    }
    
    public void InitialisiereBrettRandom()
    {
        Random r = new();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Turm();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Springer();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Läufer();
        Brett[r.Next(0,8)][r.Next(0,8)] = new König();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Dame();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Läufer();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Springer();
        Brett[r.Next(0,8)][r.Next(0,8)] = new Turm();

        for (int i = 0; i < 8; i++)
        {
            Brett[r.Next(0,8)][r.Next(0,8)] = new Bauer();
            Brett[r.Next(0,8)][r.Next(0,8)] = new Bauer(true);
        }
        
        Brett[r.Next(0,8)][r.Next(0,8)] = new Turm(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Springer(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Läufer(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new König(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Dame(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Läufer(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Springer(true);
        Brett[r.Next(0,8)][r.Next(0,8)] = new Turm(true);
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
        if (_texture == null)
        {
            _texture = new Texture2D(sb.GraphicsDevice, 1, 1);
            
            Color[] data = new Color[1];
            data[0] = Color.White;
            _texture.SetData(data);
        }
        
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                // TODO: Rendern der Felder
                sb.Draw(_texture, new Rectangle(i * Feldbreite, j * Feldhöhe, Feldbreite, Feldhöhe), (i + j) % 2 == 0 ? SchachGame.FeldWeiss : SchachGame.FeldSchwarz);
                
                if (Brett[i][j] != null)
                {
                    Brett[i][j].Draw(sb, i * Feldbreite, j * Feldhöhe, Feldbreite, Feldhöhe, (i + j) % 2 == 1);
                }
            }
        }
    }
}