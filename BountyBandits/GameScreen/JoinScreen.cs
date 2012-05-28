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
        public override void Update(GameTime gameTime)
        {
            foreach (Input input in Game.instance.inputs)
            {
                input.update();
                updateMenu(input, Enum.GetValues(typeof(JoinMenuOptions)).Length);
                if (input.getButtonHit(Buttons.A))
                {
                    switch (selectedMenuItem)
                    {
                        case 0://join
                            if (Game.instance.network.startClient())
                                Game.instance.currentState.setState(GameState.CharacterSelection);
                            break;
                        case 1:
                            Game.instance.currentState.setState(GameState.Multiplayer);
                            break;
                    }
                }
                if (input.getButtonHit(Buttons.Back))
                    Game.instance.currentState.setState(GameState.Multiplayer);
#if WINDOWS
                foreach (Keys key in input.getKeysHit())
                {
                    try
                    {
                        if (key == Keys.Back)
                            NetworkManager.joinString = NetworkManager.joinString.Substring(0, NetworkManager.joinString.Length - 1);
                        else if ((key >= Keys.A && key <= Keys.Z) || (key >= Keys.D0 && key <= Keys.D9))
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
            spriteBatch.Draw(Game.instance.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            drawTextBorder(Game.instance.vademecumFont24, NetworkManager.joinString, new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
            drawTextBorder(Game.instance.vademecumFont24, "Back", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
        }
    }
}
