using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.GameScreen
{
    public class MainScreen : BaseGameScreen
    {
        public MainScreen(Game game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            foreach (Input input in game.inputs)
            {
                input.update();
                updateMenu(input, Enum.GetValues(typeof(RootMenuOptions)).Length);
                if (input.getButtonHit(Buttons.A))
                {
                    switch (selectedMenuItem)
                    {
                        case 0:
                            game.currentState.setState(GameState.CharacterSelection);
                            break;
                        case 1:
                            game.currentState.setState(GameState.Multiplayer);
                            break;
                        case 2:
                            game.Exit();
                            break;
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(game.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            game.drawTextBorder(game.vademecumFont24, "Single Player", new Vector2(128, res.ScreenHeight / 2), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
            game.drawTextBorder(game.vademecumFont24, "Multiplayer", new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
            game.drawTextBorder(game.vademecumFont24, "Exit", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 2 ? Color.Yellow : Color.White, Color.Black, 0);
        }
    }
}
