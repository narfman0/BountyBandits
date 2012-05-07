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
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(Game.instance.mapManager.worldBackground, new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            foreach (Level level in Game.instance.mapManager.getLevels())
                spriteBatch.Draw(easyLevel, level.loc, Color.White);
            const string chooseLevelStr = "Choose level, A to start game";
            drawTextBorder(Game.instance.vademecumFont24, chooseLevelStr, new Vector2(res.ScreenWidth / 2 - Game.instance.vademecumFont24.MeasureString(chooseLevelStr).X / 2, 1f), Color.Black, Color.DarkGray, 0);
            spriteBatch.Draw(Game.instance.texMan.getTex("mapInfo"), new Vector2(res.ScreenWidth - Game.instance.texMan.getTex("mapInfo").Width, res.ScreenHeight / 2 - Game.instance.texMan.getTex("mapInfo").Height / 2), Color.White);
            drawTextBorder(Game.instance.vademecumFont24, "Level name:", new Vector2(res.ScreenWidth - Game.instance.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - Game.instance.texMan.getTex("mapInfo").Height / 2 + 168), Color.Black, Color.DarkGray, 0);
            drawTextBorder(Game.instance.vademecumFont24, Game.instance.mapManager.getCurrentLevel().name, new Vector2(res.ScreenWidth - Game.instance.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - Game.instance.texMan.getTex("mapInfo").Height / 2 + 168 - 32), Color.Black, Color.DarkGray, 0);
            drawTextBorder(Game.instance.vademecumFont24, "Level length:", new Vector2(res.ScreenWidth - Game.instance.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - Game.instance.texMan.getTex("mapInfo").Height / 2 + 96), Color.Black, Color.DarkGray, 0);
            drawTextBorder(Game.instance.vademecumFont24, Game.instance.mapManager.getCurrentLevel().levelLength.ToString(), new Vector2(res.ScreenWidth - Game.instance.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - Game.instance.texMan.getTex("mapInfo").Height / 2 + 96 - 32), Color.Black, Color.DarkGray, 0);
        }
    }
}
