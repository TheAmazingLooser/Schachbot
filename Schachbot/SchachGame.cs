using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Schachbot;

public class SchachGame : Game
{

    private const int WIDTH = 800;
    private const int HEIGHT = 800;

    private MouseState lastMouseState;

    #region Farben

    public static Color FigurWeiss = Color.White;
    public static Color FigurSchwarz = new Color(0.2f, 0.2f, 0.2f);
    public static Color FigurSchwarzWeiss = new Color(0.15f, 0.15f, 0.15f);
    public static Color FeldWeiss = Color.White;
    public static Color FeldSchwarz = new Color(0.1f, 0.1f, 0.1f);
    public static Color FeldHighlight = new Color(0.1f, 0.7f, 0.7f);
    public static Color FeldSchach = new Color(1f, 0.3f, 0.3f);
    public static Color OutlineWeiss = new Color(0.3f, 0.3f, 0.3f);
    public static Color OutlineSchwarz = new Color(0.01f, 0.1f, 0.1f);
    public static Color OutlineWeißSchwarz = new Color(0.02f, 0.02f, 0.02f);
    #endregion

    private SpriteBatch _sb;
    public Schachbrett Brett { get; set; }

    private GraphicsDeviceManager _gdm;

    public SchachGame()
    {
        _gdm = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.Title = "Schachbot";
        Window.AllowUserResizing = false;

        // Setze die Auflösung auf 800x800
        _gdm.PreferredBackBufferWidth = WIDTH;
        _gdm.PreferredBackBufferHeight = HEIGHT;
    }

    protected override void LoadContent()
    {
        // Erstelle ein neues SpriteBatch
        _sb = new SpriteBatch(GraphicsDevice);
        
        
        // Erstelle ein neues Schachbrett
        Brett = new Schachbrett();
        Brett.InitialisiereBrett();
        //Brett.Randomize();
        //Brett.InitialisiereBrettRandom();

        Mouse.WindowHandle = Window.Handle;
    }

    protected override void Draw(GameTime gameTime)
    {        
        _sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        
        Brett.Draw(_sb, WIDTH,HEIGHT);
        
        _sb.End();
    }

    private bool IstMausInFenster(MouseState mouseState)
    {
        return mouseState.X >= 0 && mouseState.X < _gdm.PreferredBackBufferWidth && mouseState.Y >= 0 && mouseState.Y < _gdm.PreferredBackBufferHeight;
    }

    protected override void Update(GameTime gameTime)
    {

        var mouseState = Mouse.GetState();

        // Auf klicks überprüfen
        if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed && IstMausInFenster(mouseState))
        {
            Console.WriteLine("Klick auf " + mouseState.X + " " + mouseState.Y);
            Brett.Klick(WIDTH, HEIGHT);
        }

        lastMouseState = mouseState;

        Brett.Update(gameTime);
        base.Update(gameTime);
    }
}