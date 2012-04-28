using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Network;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.GameScreen
{
    public class JoinScreen : BaseGameScreen
    {
        public JoinScreen(Game game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            foreach (Input input in game.inputs)
            {
                input.update();
                updateMenu(input, Enum.GetValues(typeof(JoinMenuOptions)).Length);
                if (input.getButtonHit(Buttons.A))
                {
                    switch (selectedMenuItem)
                    {
                        case 0://join
                            if (game.network.startClient())
                                game.currentState.setState(GameState.CharacterSelection);
                            break;
                        case 1:
                            game.currentState.setState(GameState.Multiplayer);
                            break;
                    }
                }
#if WINDOWS
                foreach (Keys key in input.getKeysHit())
                {
                    try
                    {
                        if (key == Keys.Back)
                            NetworkManager.joinString = NetworkManager.joinString.Substring(0, NetworkManager.joinString.Length - 1);
                        else if (key >= Keys.A && key <= Keys.Z)
                            NetworkManager.joinString += Convert.ToChar(key);
                        else if (key == Keys.OemPeriod)
                            NetworkManager.joinString += ".";
                        else if (key == Keys.OemMinus)
                            NetworkManager.joinString += "-";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        NetworkManager.joinString = "";
                    }
                }
#else
                        throw new NotImplementedException();
#endif
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(game.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            game.drawTextBorder(game.vademecumFont24, NetworkManager.joinString, new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
            game.drawTextBorder(game.vademecumFont24, "Back", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
        }
    }
}
