﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Schachbot;

public class ChessGame : Game
{

    private const int WIDTH = 800;
    private const int HEIGHT = 800;

    private MouseState lastMouseState;

    private MouseState drawingStartPosition;
    private MouseState drawingEndPosition;

    #region Farben

    public static Color PieceWhite = Color.MintCream;
    public static Color PieceBlack = new Color(0.2f, 0.2f, 0.2f);
    public static Color PieceBlackWhite = new Color(0.15f, 0.15f, 0.15f);
    public static Color FieldWhite = Color.White;
    public static Color FieldBlack = new Color(0.1f, 0.1f, 0.1f);
    public static Color FieldHighlight = new Color(0.1f, 0.7f, 0.7f);
    public static Color FieldCheck = new Color(1f, 0.3f, 0.3f);
    public static Color OutlineWhite = new Color(0.3f, 0.3f, 0.3f);
    public static Color OutlineBlack = new Color(0.01f, 0.1f, 0.1f);
    public static Color OutlineBlackWhite = new Color(0.02f, 0.02f, 0.02f);
    #endregion

    private SpriteBatch _sb;
    public ChessBoard Board { get; set; }

    private GraphicsDeviceManager _gdm;

    public ChessGame()
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
        Board = new ChessBoard();
        Board.InitializeField("rnbqkbnr/pp1ppppp/8/8/2pP3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");
        //Brett.Randomize();
        //Brett.InitialisiereBrettRandom();

        Mouse.WindowHandle = Window.Handle;
    }

    protected override void Draw(GameTime gameTime)
    {        
        _sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        
        Board.Draw(_sb, WIDTH,HEIGHT);
        
        _sb.End();
    }

    private bool IsMouseInsideGameWindow(MouseState mouseState)
    {
        return mouseState.X >= 0 && mouseState.X < _gdm.PreferredBackBufferWidth && mouseState.Y >= 0 && mouseState.Y < _gdm.PreferredBackBufferHeight;
    }

    protected override void Update(GameTime gameTime)
    {

        var mouseState = Mouse.GetState();

        // Auf klicks überprüfen
        if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed && IsMouseInsideGameWindow(mouseState))
        {
            Console.WriteLine("Klick auf " + mouseState.X + " " + mouseState.Y);
            Board.DoPlayerMove(WIDTH, HEIGHT);
        }

        if (lastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed && IsMouseInsideGameWindow(mouseState))
        {
            Console.WriteLine("Rechtsklick auf " + mouseState.X + " " + mouseState.Y);
            drawingStartPosition = mouseState;
        } else if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Pressed && IsMouseInsideGameWindow(mouseState))
        {
            if (lastMouseState.X != mouseState.X || lastMouseState.Y != mouseState.Y)
            {
                drawingEndPosition = mouseState;
            }
        }
        else if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
        {
            Board.HandleArrow(new Vector2(drawingStartPosition.X, drawingStartPosition.Y), new Vector2(drawingEndPosition.X, drawingEndPosition.Y), WIDTH, HEIGHT);
        }

        lastMouseState = mouseState;

        Board.Update(gameTime);
        base.Update(gameTime);
    }
}