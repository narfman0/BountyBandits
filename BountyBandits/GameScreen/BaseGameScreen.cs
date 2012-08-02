using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Map;
using BountyBandits.Character;
using BountyBandits.Stats;

namespace BountyBandits.GameScreen
{
    public abstract class BaseGameScreen
    {
        protected SpriteBatch spriteBatch;
        protected Resolution res;
        protected int selectedMenuItem = 0;

        public BaseGameScreen()
        {
            this.spriteBatch = Game.instance.spriteBatch;
            this.res = Game.instance.res;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

        protected void updateMenu(Input input, int menuItemCount)
        {
            if (input.getButtonHit(Buttons.DPadDown) || input.getButtonHit(Buttons.LeftThumbstickDown))
            {
                selectedMenuItem++;
                if (selectedMenuItem >= menuItemCount)
                    selectedMenuItem = menuItemCount - 1;
            }
            if (input.getButtonHit(Buttons.DPadUp) || input.getButtonHit(Buttons.LeftThumbstickUp))
            {
                selectedMenuItem--;
                if (selectedMenuItem <= 0)
                    selectedMenuItem = 0;
            }
        }

        protected void drawGameplay(Vector2 avePosition, Level currentLevel)
        {
            #region Gameworld
            if (currentLevel.horizon != null)
            {
                Vector2 currentResolution = new Vector2(res.ScreenWidth, res.ScreenHeight),
                    origin = new Vector2(currentLevel.horizon.Width / 2f, currentLevel.horizon.Height / 2f),
                    position = currentResolution / 2f - new Vector2(0, avePosition.Y - res.ScreenHeight / 2);
                spriteBatch.Draw(currentLevel.horizon, new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            }
            foreach (BackgroundItemStruct item in currentLevel.backgroundItems)
            {
                Vector2 position = item.location - item.layer * new Vector2(avePosition.X - res.ScreenWidth / 2, avePosition.Y - res.ScreenHeight / 2);
                Texture2D tex = Game.instance.texMan.getTex(item.texturePath);
                drawItem(tex, position, item.rotation, 0f, new Vector2(item.scale), SpriteEffects.None, new Vector2(tex.Width / 2, tex.Height / 2));
            }
            for (int currentDepth = 0; currentDepth < 4; currentDepth++)
            {
                foreach (GameItem gameItem in Game.instance.activeItems.Values)
                    if (//gameItem.width - 1 + gameItem.startdepth == currentDepth) //single lowest depth drawing

                        currentDepth >= gameItem.startdepth && //multidepth drawing
                        currentDepth < gameItem.startdepth + gameItem.width)
                    {
                        Vector2 scale = Vector2.One, pos = new Vector2(gameItem.body.Position.X - avePosition.X + res.ScreenWidth / 2, gameItem.body.Position.Y - avePosition.Y + res.ScreenHeight / 2);
                        Texture2D tex = !(gameItem is DropItem) ? Game.instance.texMan.getTex(gameItem.name) :
                            Game.instance.texMan.getTexColored(((DropItem)gameItem).getItem().getTextureName(), ((DropItem)gameItem).getItem().getPrimaryColor(), ((DropItem)gameItem).getItem().getSecondaryColor(), Game.instance.GraphicsDevice);
                        Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
                        float rotation = gameItem.body.Rotation;
                        if (!(gameItem is DropItem))
                            switch (gameItem.polygonType)
                            {
                                case PhysicsPolygonType.Circle:
                                    scale = new Vector2((float)gameItem.radius * 2 / (float)tex.Width, (float)gameItem.radius * 2 / (float)tex.Height);
                                    rotation *= -1;
                                    break;
                                case PhysicsPolygonType.Rectangle:
                                    scale = new Vector2((float)gameItem.sideLengths.X / (float)tex.Width, (float)gameItem.sideLengths.Y / (float)tex.Height);
                                    rotation *= -1;
                                    break;
                                case PhysicsPolygonType.Polygon:
                                    rotation *= -1;
                                    break;
                            }
                        drawGameItem(tex, pos, rotation, currentDepth, scale, SpriteEffects.None, origin);
                    }
                foreach (Being enemy in Game.instance.spawnManager.enemies.Values)
                    if (currentDepth == enemy.getDepth())
                        enemy.draw(avePosition);
                foreach (Being player in Game.instance.players.Values)
                    if (currentDepth == player.getDepth())
                    {
                        player.draw(avePosition);
                        DropItem item = Game.instance.getClosestDropItem(player);
                        if (item != null && Vector2.DistanceSquared(item.body.Position, player.body.Position) < Game.DROP_ITEM_MAX_DISTANCE)
                            drawItemDescription(item);
                    }
            }
            #endregion
            #region HUD
            int pIndex = 0;
            foreach (Being currPlayer in Game.instance.players.Values)
            {
                if (currPlayer.menu.isMenuActive())
                {
                    spriteBatch.Draw(Game.instance.texMan.getTex("portraitBackground"), new Vector2(24 + 16 + pIndex * 288 + 32 * pIndex, 63), new Color(255, 255, 255, 192));
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Data)
                    {
                        drawTextBorder(Game.instance.vademecumFont18, "Level:   " + currPlayer.level, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Current XP:    " + currPlayer.xp, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 93), Color.Black, Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "XP to Level:   " + currPlayer.xpOfNextLevel, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 119), Color.Black, Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Agility:   " + currPlayer.getStat(BountyBandits.Stats.StatType.Agility), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 145), currPlayer.menu.getMenuColor(0), Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Magic:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Magic), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 171), currPlayer.menu.getMenuColor(1), Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Speed:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Speed), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 197), currPlayer.menu.getMenuColor(2), Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Strength:  " + currPlayer.getStat(BountyBandits.Stats.StatType.Strength), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 223), currPlayer.menu.getMenuColor(3), Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Available: " + currPlayer.unusedAttr, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 249), Color.Black, Color.White, 0);
                        drawTextBorder(Game.instance.vademecumFont18, "Quit", new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 275), currPlayer.menu.getMenuColor(4), Color.White, 0);
                    }
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Inv)
                        drawTextBorder(Game.instance.vademecumFont18, "Inventory Screen", new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Stats)
                        drawTextBorder(Game.instance.vademecumFont18, "Data Screen", new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);
                }
                if (currPlayer.controller.portrait != null)
                {
                    int xLoc = 42 - currPlayer.controller.portrait.Width / 2 + pIndex * 288 + 32 * pIndex,
                        yLoc = 43 - currPlayer.controller.portrait.Height / 2;
                    spriteBatch.Draw(currPlayer.controller.portrait, new Vector2(xLoc, yLoc), Color.White);
                }
                spriteBatch.Draw(Game.instance.texMan.getTex("portrait"), new Vector2(16 + pIndex * 288 + 32 * pIndex, 16), Color.White);

                for (int healthIndex = 0; healthIndex < (int)currPlayer.CurrentHealth; ++healthIndex)
                    spriteBatch.Draw(Game.instance.texMan.getTex("redBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * healthIndex, 16), Color.White);
                int currentHP = currPlayer.CurrentHealth > 0f && (int)currPlayer.CurrentHealth == 0 ? 1 : (int)currPlayer.CurrentHealth;
                drawTextBorder(Game.instance.vademecumFont12, currentHP + "/" + currPlayer.getStat(BountyBandits.Stats.StatType.Life), new Vector2(86 + pIndex * 288 + 32 * pIndex, res.ScreenHeight - 140), Color.Black, Color.DarkGray, 0);

                //for (int specialIndex = 0; specialIndex < (int)currPlayer.currentspecial; ++specialIndex)
                //    spriteBatch.Draw(Game.instance.texMan.getTex("yellowBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * specialIndex, 40), Color.White);
                //drawTextBorder(Game.instance.vademecumFont12, (int)currPlayer.currentspecial + "/" + currPlayer.getStat(StatType.Special), new Vector2(86 + pIndex * 288 + 32 * pIndex, res.ScreenHeight - 164), Color.Black, Color.DarkGray, 0);

                pIndex++;
            }
            #endregion
        }

        public void drawGameItem(Texture2D tex, Vector2 pos, float rot, float depth, Vector2 scale, SpriteEffects effects, Vector2 origin)
        {
            drawItem(tex, new Vector2(pos.X - Game.DEPTH_X_OFFSET * depth, pos.Y + (Game.DEPTH_MULTIPLE * (3 - depth))), rot, depth / 10f, scale, effects, origin);
        }

        protected void drawItem(Texture2D tex, Vector2 pos, float rot, float depth, Vector2 scale, SpriteEffects effects, Vector2 origin)
        {
            spriteBatch.Draw(tex, new Vector2(pos.X, res.ScreenHeight - pos.Y), null, Color.White, rot, origin, scale, effects, depth);
        }

        public void drawTextBorder(SpriteFont font, String text, Vector2 pos, Color color, Color borderColor, int depth)
        {
            drawText(font, text, pos + new Vector2(0, 1), borderColor, depth);
            drawText(font, text, pos + new Vector2(0, -1), borderColor, depth);
            drawText(font, text, pos + new Vector2(1, 0), borderColor, depth);
            drawText(font, text, pos + new Vector2(-1, 0), borderColor, depth);
            drawText(font, text, pos, color, depth);
        }

        protected void drawText(SpriteFont font, String text, Vector2 pos, Color color, int depth)
        {
            spriteBatch.DrawString(font, text, new Vector2(pos.X - Game.DEPTH_X_OFFSET * depth, res.ScreenHeight - (pos.Y + (Game.DEPTH_MULTIPLE * (3 - depth)))), color);
        }

        protected void drawItemDescription(DropItem item)
        {
            const int FONT_WIDTH = 10;
            const int BUFFER_WIDTH = 36;
            #region Get color for item class
            Color nameColor = Color.White;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Magic) nameColor = Color.Yellow;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Rare) nameColor = Color.Orange;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Unique) nameColor = Color.Blue;
            #endregion
            #region Get name string
            String name = item.getItem().getName();
            int numNewLines = 1, maxWidth = BUFFER_WIDTH + ((name.Length > 20) ? 20 : name.Length) * FONT_WIDTH;
            for (int insertIndex = 20; insertIndex < name.Length; insertIndex += 20, numNewLines++)
                name = name.Substring(0, insertIndex) + "-\n" + name.Substring(insertIndex + 1);
            #endregion
            #region Get stats string
            int i = 0;
            String stats = "";
            foreach (BountyBandits.Stats.StatType type in Enum.GetValues(typeof(BountyBandits.Stats.StatType)))
            {
                if (item.getItem().getStats().getStat(type).getValue() > 0)
                {
                    stats += Enum.GetNames(typeof(BountyBandits.Stats.StatType))[i] + " " + item.getItem().getStats().getStat(type).getValue() + "\n";
                    numNewLines++;
                    if (maxWidth < BUFFER_WIDTH + FONT_WIDTH * (Enum.GetNames(typeof(BountyBandits.Stats.StatType))[i] + " " + item.getItem().getStats().getStat(type).getValue()).Length)
                        maxWidth = BUFFER_WIDTH + FONT_WIDTH * (Enum.GetNames(typeof(BountyBandits.Stats.StatType))[i] + " " + item.getItem().getStats().getStat(type).getValue()).Length;
                }
                i++;
            }
            #endregion
            #region Modify Texture to be correct color
            Texture2D tex = Game.instance.texMan.getTex(item.getItem().getTextureName());
            /*byte[] textArr = new byte[tex.Width*tex.Height*4];
            tex.GetData(textArr);
            for (int texel = 0; texel < textArr.Length; texel++)
            {
                //if it is all alpha, quit
                if (textArr[texel + 3] == 0)
                {
                    if (textArr[texel] == 255)
                    {
                        textArr[texel++] = item.getItem().getPrimaryColor().R;
                        textArr[texel++] = item.getItem().getPrimaryColor().G;
                        textArr[texel++] = item.getItem().getPrimaryColor().B;
                        textArr[texel] = item.getItem().getPrimaryColor().A;
                    }
                    else if (textArr[i] == 0)
                    {
                        textArr[texel++] = item.getItem().getSecondaryColor().R;
                        textArr[texel++] = item.getItem().getSecondaryColor().G;
                        textArr[texel++] = item.getItem().getSecondaryColor().B;
                        textArr[texel] = item.getItem().getSecondaryColor().A;
                    }
                }
                else
                    texel += 3;
            }*/
            #endregion
            #region Draw
            Vector2 avePosition = Game.instance.getAvePosition() - new Vector2(res.ScreenWidth / 2, res.ScreenHeight / 2);
            int backgroundDrawHeight = (20 * numNewLines < (Game.instance.texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3) ? (Game.instance.texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3 : 20 * numNewLines;
            spriteBatch.Draw(Game.instance.texMan.getTex("portraitBackground"), new Vector2(item.body.Position.X - 46f - avePosition.X, res.ScreenHeight + avePosition.Y - (item.body.Position.Y + 60f + (Game.DEPTH_MULTIPLE * (3 - item.startdepth)))), new Rectangle(0, 0, maxWidth, backgroundDrawHeight), new Color(255, 255, 255, 192));
            drawGameItem(Game.instance.texMan.getTexColored(item.getItem().getTextureName(), item.getItem().getPrimaryColor(), item.getItem().getSecondaryColor(), Game.instance.GraphicsDevice), new Vector2(item.body.Position.X - 25f - avePosition.X, item.body.Position.Y + 15f - avePosition.Y), 0f, (int)item.startdepth, Vector2.One, SpriteEffects.None, new Vector2(Game.instance.texMan.getTex(item.getItem().getTextureName()).Width / 2, Game.instance.texMan.getTex(item.getItem().getTextureName()).Height / 2));
            drawTextBorder(Game.instance.vademecumFont12, name, new Vector2(item.body.Position.X - 10f - avePosition.X, (item.body.Position.Y + -64f + (Game.DEPTH_MULTIPLE * (3 - item.startdepth)) - avePosition.Y)), nameColor, Color.Black, 0);
            drawTextBorder(Game.instance.vademecumFont12, stats, new Vector2(item.body.Position.X - 10f - avePosition.X, (item.body.Position.Y + -79f - (20f * (name.Length / 20)) + (Game.DEPTH_MULTIPLE * (3 - item.startdepth)) - avePosition.Y)), Color.White, Color.Black, 0);
            #endregion
        }
    }
}
