using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BountyBandits.Character;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Map;

namespace BountyBandits.GameScreen
{
    public class WorldMapScreen : BaseGameScreen
    {
        public WorldMapScreen(Game game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            foreach (Being player in game.players.Values)
            {
                if (player.isLocal)
                {
                    player.input.update();
                    if (player.input.getButtonHit(Buttons.Back))
                        game.currentState.setState(GameState.CharacterSelection);
                    if (player.input.getButtonHit(Buttons.A) && !game.network.isClient())
                        game.newLevel();
                    if (player.input.getButtonHit(Buttons.DPadRight))
                    {
                        int newLevelIndex = game.mapManager.getCurrentLevelIndex() + 1;
                        if (game.isUnlocked(newLevelIndex))
                        {
                            if (game.network.isClient())
                                game.network.sendIncrementLevelRequest(true);
                            else
                            {
                                game.mapManager.incrementCurrentLevel(true);
                                if (game.network.isServer())
                                    game.network.sendLevelIndexChange(newLevelIndex);
                            }
                        }
                    }
                    if (player.input.getButtonHit(Buttons.DPadLeft))
                    {
                        int newLevelIndex = game.mapManager.getCurrentLevelIndex() - 1;
                        if (game.isUnlocked(newLevelIndex))
                        {
                            if (game.network.isClient())
                                game.network.sendIncrementLevelRequest(false);
                            else
                            {
                                game.mapManager.incrementCurrentLevel(false);
                                if (game.network.isServer())
                                    game.network.sendLevelIndexChange(newLevelIndex);
                            }
                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(game.mapManager.worldBackground, new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            foreach (Level level in game.mapManager.getLevels())
                spriteBatch.Draw(game.easyLevel, level.loc, Color.White);
            const string chooseLevelStr = "Choose level, A to start game";
            game.drawTextBorder(game.vademecumFont24, chooseLevelStr, new Vector2(res.ScreenWidth / 2 - game.vademecumFont24.MeasureString(chooseLevelStr).X / 2, 1f), Color.Black, Color.DarkGray, 0);
            spriteBatch.Draw(game.texMan.getTex("mapInfo"), new Vector2(res.ScreenWidth - game.texMan.getTex("mapInfo").Width, res.ScreenHeight / 2 - game.texMan.getTex("mapInfo").Height / 2), Color.White);
            game.drawTextBorder(game.vademecumFont24, "Level name:", new Vector2(res.ScreenWidth - game.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - game.texMan.getTex("mapInfo").Height / 2 + 168), Color.Black, Color.DarkGray, 0);
            game.drawTextBorder(game.vademecumFont24, game.mapManager.getCurrentLevel().name, new Vector2(res.ScreenWidth - game.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - game.texMan.getTex("mapInfo").Height / 2 + 168 - 32), Color.Black, Color.DarkGray, 0);
            game.drawTextBorder(game.vademecumFont24, "Level length:", new Vector2(res.ScreenWidth - game.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - game.texMan.getTex("mapInfo").Height / 2 + 96), Color.Black, Color.DarkGray, 0);
            game.drawTextBorder(game.vademecumFont24, game.mapManager.getCurrentLevel().levelLength.ToString(), new Vector2(res.ScreenWidth - game.texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - game.texMan.getTex("mapInfo").Height / 2 + 96 - 32), Color.Black, Color.DarkGray, 0);
        }
    }
}
