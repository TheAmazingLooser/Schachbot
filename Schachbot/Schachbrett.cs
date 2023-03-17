using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schachbot.Figuren;

namespace Schachbot;

public class Schachbrett
{
    public SchachbrettFeld[][] Brett;

    private List<Vector2> _hinweisPositionen = new List<Vector2>();
    private Vector2 _hinweisPosition = new Vector2();
    private bool _weissAmZug = false;

    private Vector2 _weisserKönig = new Vector2();
    private Vector2 _schwarzerKönig = new Vector2();
    
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
        
        Random r = new Random();
        _weissAmZug = r.Next(0, 100) > 50;

    }

    public SchachbrettFeld GetFeld(int x, int y)
    {
        if (x < 0 || x >= 8) return null;
        if (y < 0 || y >= 8) return null;

        return Brett[x][y];
    }

    public void InitialisiereBrett()
    {

        for (int x = 0; x < 8; x++)
        {
            Brett[x][1].SetzeFigur(new Bauer(), x, 1);
            Brett[x][6].SetzeFigur(new Bauer(true), x, 6);
        }

        foreach(var y in new[] {0,7})
        {
            Brett[0][y].SetzeFigur(new Turm(y == 7), 0, y);
            Brett[1][y].SetzeFigur(new Springer(y == 7), 1, y);
            Brett[2][y].SetzeFigur(new Läufer(y == 7), 2, y);
            Brett[4][y].SetzeFigur(new Dame(y == 7), 4, y);
            Brett[5][y].SetzeFigur(new Läufer(y == 7), 5, y);
            Brett[6][y].SetzeFigur(new Springer(y == 7), 6, y);
            Brett[7][y].SetzeFigur(new Turm(y == 7), 7, y);
        }

        König weiss = new König();
        König schwarz = new König(true);

        weiss.Bewegt += (x, y) => KönigBewegt(true, new Vector2(x, y));
        schwarz.Bewegt += (x, y) => KönigBewegt(false, new Vector2(x, y));

        Brett[3][0].SetzeFigur(weiss, 3, 0);
        Brett[3][7].SetzeFigur(schwarz, 3, 7);
    }

    private void KönigBewegt(bool weiss, Vector2 pos)
    {
        if (weiss) _weisserKönig = pos;
        else _schwarzerKönig = pos;
    }
    public void Randomize()
    {
        Random r = new Random();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Brett[x][y].Figur is ISchachfigur figur)
                {
                    // Zufallsposition bekommen:
                    int xRand = r.Next(0, 8);
                    int yRand = r.Next(0, 8);

                    Brett[x][y].SetzeFigur(Brett[xRand][yRand].Figur,x,y);
                    Brett[xRand][yRand].SetzeFigur(figur, xRand, yRand);
                }
            }
        }
    }

    public bool ImSchach(bool weiss)
    {
        Vector2 königPosition = weiss ? _weisserKönig : _schwarzerKönig;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Brett[x][y].Figur is ISchachfigur figur && figur.IstWeiss != weiss)
                {
                    List<Vector2> moves = figur.GetLegalMoves(this, x, y, true);
                    if (moves.Contains(königPosition))
                        return true;
                }
            }
        }

        return false;
    }

    public void Update(GameTime gameTime)
    {
        if (_weissAmZug)
        {
            Dictionary<Vector2, List<Vector2>> botZüge = new Dictionary<Vector2, List<Vector2>>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Brett[x][y].Figur is ISchachfigur figur && figur.IstWeiss)
                    {
                        List<Vector2> moves = figur.GetLegalMoves(this, x, y);
                        if (moves.Count > 0)
                            botZüge.Add(new Vector2(x, y), moves);
                    }
                }
            }

            if (botZüge.Count > 0)
            {
                // Zufälligen Zug auswählen
                Random r = new Random();
                int figurPosition = r.Next(0, botZüge.Count);
                int xStart = (int)botZüge.ElementAt(figurPosition).Key.X;
                int yStart = (int)botZüge.ElementAt(figurPosition).Key.Y;

                int zug = r.Next(0, botZüge.ElementAt(figurPosition).Value.Count);
                int xZiel = (int)botZüge.ElementAt(figurPosition).Value.ElementAt(zug).X;
                int yZiel = (int)botZüge.ElementAt(figurPosition).Value.ElementAt(zug).Y;

                Brett[xZiel][yZiel].SetzeFigur(Brett[xStart][yStart].Figur, xZiel, yZiel);
                Brett[xStart][yStart].SetzeFigur(null, xStart, yStart);
            }
            _weissAmZug = false;
        }
    }

    public void Klick(int width, int height)
    {
        MouseState ms = Mouse.GetState();
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        int x = ms.X / Feldbreite;
        int y = ms.Y / Feldhöhe;

        if (_hinweisPositionen.Contains(new Vector2(x,y)) && !_weissAmZug)
        {
            Brett[x][y].SetzeFigur(Brett[(int)_hinweisPosition.X][(int)_hinweisPosition.Y].Figur, x,y);
            Brett[(int)_hinweisPosition.X][(int)_hinweisPosition.Y].SetzeFigur(null, (int)_hinweisPosition.X, (int)_hinweisPosition.Y);

            _weissAmZug = true;
            _hinweisPositionen.Clear();
            return;
        }

        if (Brett[x][y].Figur is ISchachfigur figur && !figur.IstWeiss)
        {
            Dictionary<Vector2, List<Vector2>> AntiSchachMoves = new Dictionary<Vector2, List<Vector2>>();
            if (ImSchach(false))
            {
                Vector2 altePos = new Vector2(_schwarzerKönig.X, _schwarzerKönig.Y);
                for (int xS = 0; xS < 8; xS++)
                {
                    for (int yS = 0; yS < 8; yS++)
                    {
                        if (Brett[xS][yS].Figur is ISchachfigur figurS && !figurS.IstWeiss)
                        {
                            List<Vector2> moves = figurS.GetLegalMoves(this, xS, yS);
                            // Simulieren von einem Move
                            foreach(var move in moves)
                            {
                                var oldFigur = Brett[(int)move.X][(int)move.Y].Figur;
                                Brett[(int)move.X][(int)move.Y].SetzeFigur(Brett[xS][yS].Figur, (int)move.X, (int)move.Y);
                                Brett[xS][yS].SetzeFigur(null, xS, yS);

                                if (!ImSchach(false))
                                {
                                    if (AntiSchachMoves.ContainsKey(new Vector2(xS, yS)))
                                        AntiSchachMoves[new Vector2(xS, yS)].Add(move);
                                    else
                                        AntiSchachMoves.Add(new Vector2(xS, yS), new List<Vector2>() { move });
                                }
                                Brett[xS][yS].SetzeFigur(figurS, xS, yS);
                                Brett[(int)move.X][(int)move.Y].SetzeFigur(oldFigur, (int)move.X,(int)move.Y);
                                _schwarzerKönig = altePos;
                            }
                        }
                    }
                }
            }

            var curPos = new Vector2(x, y);
            if (AntiSchachMoves.Count == 0 && ImSchach(false))
            {
                // Wir haben verloren == Schachmatt
            } else if (AntiSchachMoves.Count > 0 && ImSchach(false) && AntiSchachMoves.ContainsKey(curPos))
            {
                _hinweisPositionen.Clear();
                List<Vector2> allPos = figur.GetLegalMoves(this, x, y);
                foreach(var pos in allPos)
                {
                    if (AntiSchachMoves[curPos].Contains(pos))
                        _hinweisPositionen.Add(pos);

                }
                _hinweisPosition = new Vector2(x, y);
            } else if (!ImSchach(false))
            {
                _hinweisPositionen = figur.GetLegalMoves(this, x, y);
                _hinweisPosition = new Vector2(x, y);
            } else
            {
                _hinweisPositionen.Clear();
            }
        } else
        {
            _hinweisPositionen.Clear();
        }
    }
    
    public void Draw(SpriteBatch sb, int width, int height)
    {
        int Feldbreite = width / 8;
        int Feldhöhe = height / 8;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Überprüfen ob die Position die eines Königs ist.
                Vector2 pos = new Vector2(x, y);

                bool imSchach = false;
                if (pos == _weisserKönig)
                {
                    imSchach = ImSchach(true);
                }
                if (pos == _schwarzerKönig)
                {
                    imSchach = ImSchach(false);
                }

                Brett[x][y].Draw(sb, x * Feldbreite, y * Feldhöhe, Feldbreite, Feldhöhe, _hinweisPositionen.Contains(new Vector2(x, y)), imSchach);
            }
        }
    }
}