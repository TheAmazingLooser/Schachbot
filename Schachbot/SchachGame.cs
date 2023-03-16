using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Schachbot;

public class SchachGame : Game
{

    private const int WIDTH = 800;
    private const int HEIGHT = 800;
    
    #region Farben

    public static Color FigurWeiss = Color.White;
    public static Color FigurSchwarz = new Color(0.2f, 0.2f, 0.2f);
    public static Color FigurSchwarzWeiss = new Color(0.15f, 0.15f, 0.15f);
    public static Color FeldWeiss = Color.White;
    public static Color FeldSchwarz = new Color(0.1f, 0.1f, 0.1f);
    public static Color OutlineWeiss = new Color(0.3f, 0.3f, 0.3f);
    public static Color OutlineSchwarz = new Color(0.1f, 0.1f, 0.1f);
    public static Color OutlineWeißSchwarz = new Color(0.02f, 0.02f, 0.02f);
    #endregion
    
    private SpriteBatch _sb;
    public Schachbrett Brett { get; set; }
    
    public SchachGame()
    {
        GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.Title = "Schachbot";
        Window.AllowUserResizing = false;
        
        // Setze die Auflösung auf 800x800
        gdm.PreferredBackBufferWidth = WIDTH;
        gdm.PreferredBackBufferHeight = HEIGHT;
    }

    protected override void LoadContent()
    {
        // Erstelle ein neues SpriteBatch
        _sb = new SpriteBatch(GraphicsDevice);
        
        
        // Erstelle ein neues Schachbrett
        Brett = new Schachbrett();
        Brett.InitialisiereBrett();
        //Brett.InitialisiereBrettRandom();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        
        Brett.Draw(_sb, WIDTH,HEIGHT);
        
        _sb.End();
    }

    protected override void Update(GameTime gameTime)
    {
        Brett.Update(gameTime);
        base.Update(gameTime);
    }
}