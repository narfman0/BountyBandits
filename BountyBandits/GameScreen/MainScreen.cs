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
        public override void Update(GameTime gameTime)
        {
            foreach (Input input in Game.instance.inputs)
            {
                input.update();
                updateMenu(input, Enum.GetValues(typeof(RootMenuOptions)).Length);
                if (input.getButtonHit(Buttons.A))
                {
                    switch (selectedMenuItem)
                    {
                        case 0:
                            Game.instance.currentState.setState(GameState.CharacterSelection);
                            break;
                        case 1:
                            Game.instance.currentState.setState(GameState.Multiplayer);
                            break;
                        case 2:
                            Game.instance.Exit();
                            break;
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(Game.instance.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            drawTextBorder(Game.instance.vademecumFont24, "Single Player", new Vector2(128, res.ScreenHeight / 2), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
            drawTextBorder(Game.instance.vademecumFont24, "Multiplayer", new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
            drawTextBorder(Game.instance.vademecumFont24, "Exit", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 2 ? Color.Yellow : Color.White, Color.Black, 0);
        }
    }
}
