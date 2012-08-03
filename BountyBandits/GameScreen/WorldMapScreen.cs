using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BountyBandits.Character;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Map;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.GameScreen
{
    public class WorldMapScreen : BaseGameScreen
    {
        private Texture2D easyLevel, extremeLevel, hardLevel, mediumLevel;

        public WorldMapScreen()
            : base()
        {
            easyLevel = Game.instance.Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "easyLevel");
            extremeLevel = Game.instance.Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "extremeLevel");
            hardLevel = Game.instance.Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "hardLevel");
            mediumLevel = Game.instance.Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "mediumLevel");
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Being player in Game.instance.players.Values)
            {
                if (player.isLocal)
                {
                    player.input.update();
                    if (player.input.getButtonHit(Buttons.Back))
                        Game.instance.currentState.setState(GameState.CharacterSelection);
                    if (player.input.getButtonHit(Buttons.A) && !Game.instance.network.isClient())
                        Game.instance.newLevel();
                    if (player.input.getButtonHit(Buttons.DPadRight))
                    {
                        int newLevelIndex = Game.instance.mapManager.getCurrentLevelIndex() + 1;
                        if (Game.instance.isUnlocked(newLevelIndex))
                        {
                            if (Game.instance.network.isClient())
                                Game.instance.network.sendIncrementLevelRequest(true);
                            else
                            {
                                Game.instance.mapManager.incrementCurrentLevel(true);
                                if (Game.instance.network.isServer())
                                    Game.instance.network.sendLevelIndexChange(newLevelIndex);
                            }
                        }
                    }
                    if (player.input.getButtonHit(Buttons.DPadLeft))
                    {
                        int newLevelIndex = Game.instance.mapManager.getCurrentLevelIndex() - 1;
                        if (Game.instance.isUnlocked(newLevelIndex))
                        {
                            if (Game.instance.network.isClient())
                                Game.instance.network.sendIncrementLevelRequest(false);
                            else
                            {
                                Game.instance.mapManager.incrementCurrentLevel(false);
                                if (Game.instance.network.isServer())
                                    Game.instance.network.sendLevelIndexChange(newLevelIndex);
                            }
                        }
                    }
                }
                if (player.input.isKeyHit(Keys.F3))
                    Game.instance.currentState.setState(GameState.WorldEditor);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            drawWorldBackground();
        }
    }
}
