using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using BountyBandits.Stats;
using BountyBandits.Animation;
using BountyBandits.Map;

namespace BountyBandits
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public enum GameState
        {
            RootMenu,   //choose characters
            WorldMap,
            Gameplay
        };
        #region Fields
        public const float DEPTH_MULTIPLE = 42, DEPTH_X_OFFSET = 12, FORCE_AMOUNT = 10, DROP_ITEM_MAX_DISTANCE = 4000f;
        DifficultyEnum difficulty = DifficultyEnum.Normal;
        GraphicsDeviceManager graphics;
        GameTime previousGameTime;
        SpriteBatch spriteBatch;
        public List<Being> players = new List<Being>();
        List<GameItem> activeItems;
        StateManager currentState = new StateManager();
        private SpriteFont vademecumFont24, vademecumFont12, vademecumFont18;
        public MapManager mapManager;
        public AnimationManager animationManager;
        public SpawnManager spawnManager;
        public Texture2D easyLevel, extremeLevel, hardLevel, mediumLevel;
        public PhysicsSimulator physicsSimulator;
        public Resolution res;
        public Random rand;
        public Geom groundGeom;
        public TextureManager texMan;
        public XPManager xpManager = new XPManager();
        private Input input;
        #endregion
        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            input = new Input();
            res = new Resolution(graphics, ScreenMode.tv720p);
            spawnManager = new SpawnManager(this);
            rand = new Random();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            texMan = new TextureManager(Content);
            vademecumFont12 = Content.Load<SpriteFont>(@"Fonts\vademecum12");
            vademecumFont18 = Content.Load<SpriteFont>(@"Fonts\vademecum18");
            vademecumFont24 = Content.Load<SpriteFont>(@"Fonts\vademecum24");

            easyLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "easyLevel");
            extremeLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "extremeLevel");
            hardLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "hardLevel");
            mediumLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "mediumLevel");
            mapManager = new MapManager(this, MapManager.DEFAULT_CAMPAIGN_PATH);
            animationManager = new AnimationManager(this);

            physicsSimulator = new PhysicsSimulator(new Vector2(0, -20));
            players.Add(new Being("temp", 1, this, animationManager.getController("cowboy")));
            players[0].controllerIndex = PlayerIndex.One;

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            switch (currentState.getState())
            {
                #region gameplay
                case GameState.Gameplay:
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                        currentState.setState(GameState.RootMenu);
                    foreach (PlayerIndex index in System.Enum.GetValues(typeof(PlayerIndex)))
                        foreach (Being currentplayer in players)
                            if (currentplayer.controllerIndex == index)
                            {
                                currentplayer.update(gameTime);
                                #region Input
                                input.setCurrentInput(currentplayer.prevKeyboardState, GamePad.GetState(index), currentplayer.prevGamePadState);
                                if (input.getButtonDown(Buttons.LeftThumbstickLeft))
                                    currentplayer.move(new Vector2(-FORCE_AMOUNT, 0));
                                if (input.getButtonDown(Buttons.LeftThumbstickRight))
                                    currentplayer.move(new Vector2(FORCE_AMOUNT, 0));
                                if (input.getButtonHit(Buttons.A))
                                {
                                    if (currentplayer.menu.getMenuScreen() == Menu.MenuScreens.Data && currentplayer.unusedAttr > 0)
                                    {
                                        if (currentplayer.menu.getMenuItem() == 0) currentplayer.upgradeStat(StatType.Agility, 1);
                                        else if (currentplayer.menu.getMenuItem() == 1) currentplayer.upgradeStat(StatType.Magic, 1);
                                        else if (currentplayer.menu.getMenuItem() == 2) currentplayer.upgradeStat(StatType.Speed, 1);
                                        else if (currentplayer.menu.getMenuItem() == 3) currentplayer.upgradeStat(StatType.Strength, 1);
                                        currentplayer.unusedAttr--;
                                    }
                                    currentplayer.jump();
                                }
                                if (input.getButtonHit(Buttons.X))
                                    currentplayer.attack("attack1");
                                if (input.getButtonHit(Buttons.Back))
                                    currentplayer.menu.toggleMenu();
                                if (input.getButtonDown(Buttons.DPadDown))
                                    if (currentplayer.menu.getMenuActive())
                                        currentplayer.menu.changeMenuItem(false);
                                    else
                                        currentplayer.lane(false);
                                if (input.getButtonDown(Buttons.DPadUp))
                                    if (currentplayer.menu.getMenuActive())
                                        currentplayer.menu.changeMenuItem(true);
                                    else
                                        currentplayer.lane(true);

                                if (input.getButtonHit(Buttons.DPadRight))
                                    currentplayer.menu.changeMenuScreen(true);
                                if (input.getButtonHit(Buttons.DPadLeft))
                                    currentplayer.menu.changeMenuScreen(false);
                                if (input.getButtonHit(Buttons.RightShoulder))
                                {
                                    //pick up closest item and throw the equipped one on the ground
                                    DropItem dropItem = getClosestDropItem(currentplayer);
                                    if (dropItem != null && Vector2.DistanceSquared(dropItem.body.Position, currentplayer.body.Position) < DROP_ITEM_MAX_DISTANCE)
                                    {
                                        BountyBandits.Inventory.Item playerItem = currentplayer.getItemManager().getItem(dropItem.getItem().getItemType());
                                        currentplayer.getItemManager().putItem(dropItem.getItem());
                                        if (playerItem != null)
                                        {
                                            dropItem.setItem(playerItem);
                                            dropItem.body.LinearVelocity.Y += 100f;
                                            dropItem.body.ApplyTorque((float)rand.NextDouble() - .5f);
                                        }
                                        else
                                            activeItems.Remove(dropItem);
                                    }
                                }
#if DEBUG
                                if (Keyboard.GetState().IsKeyDown(Keys.F3) && currentplayer.prevKeyboardState.IsKeyUp(Keys.F3))
                                    spawnManager.spawnGroup("cowboy", 1, 1);
                                if (Keyboard.GetState().IsKeyDown(Keys.F4) && currentplayer.prevKeyboardState.IsKeyUp(Keys.F4))
                                    foreach (Being player in players)
                                        player.giveXP(xpManager.getXPToLevelUp(player.level - 1));
                                if (Keyboard.GetState().IsKeyDown(Keys.F5) && currentplayer.prevKeyboardState.IsKeyUp(Keys.F5)){
                                    DropItem item = new DropItem();
                                    item.body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, item.radius, item.radius, item.weight);
                                    item.body.Position = new Vector2(currentplayer.body.Position.X, currentplayer.body.Position.Y+25);
                                    item.body.Rotation = 100f * (float)(rand.NextDouble() - .5f);
                                    item.body.AngularVelocity = (float)(rand.NextDouble() - .5f);
                                    item.body.LinearVelocity = 50f * new Vector2((float)rand.NextDouble() - .5f, 1);
                                    #region geometry setup
                                    Geom geom = new Geom();
                                    geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, item.body, item.radius, item.radius);
                                    geom.FrictionCoefficient = .6f;
                                    geom.CollisionCategories = CollisionCategory.None;
                                    for (uint depth = item.startdepth; depth < item.width; depth++)
                                        geom.CollisionCategories = (CollisionCategory)(int)geom.CollisionCategories + ((int)Math.Pow(2, depth));
                                    geom.CollidesWith = geom.CollisionCategories;
                                    #endregion
                                    item.setItem(DropManager.generateItem(currentplayer));
                                    activeItems.Add(item);
                                }
#endif
                                currentplayer.prevGamePadState = GamePad.GetState(index);
                                currentplayer.prevKeyboardState = Keyboard.GetState();
                                #endregion
                            }
                    spawnManager.update(gameTime);
                    float timeElapsed = (float)(gameTime.ElapsedRealTime.Milliseconds - previousGameTime.ElapsedRealTime.Milliseconds);
                    physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
                    break;
                #endregion
                #region root menu
                case GameState.RootMenu:
                    foreach(PlayerIndex index in System.Enum.GetValues(typeof(PlayerIndex)))
                        if (GamePad.GetState(index).Buttons.Start == ButtonState.Pressed){
                            Boolean found = false;
                            foreach(Being player in players)
                                if(player.controllerIndex == index)
                                    found = true;
                            if (!found)
                            {
                                players.Add(new Being("newName" + index.ToString(), 1, this, animationManager.getController("cow")));
                                players[players.Count - 1].controllerIndex = index;
                            }
                        }

                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                        Exit();
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
                        currentState.setState(GameState.WorldMap);
                    break;
                #endregion
                #region world map
                case GameState.WorldMap:
                    if (input.getButtonHit(Buttons.Back))
                        currentState.setState(GameState.RootMenu);
                    if (input.getButtonHit(Buttons.A))
                        newLevel();
                    if (input.getButtonHit(Buttons.DPadRight))
                    {
                        bool isUnlocked = true;
                        foreach (Being player in players)
                            if (!player.unlocked.isUnlocked(mapManager.guid, difficulty, mapManager.currentLevelIndex + 1))
                                isUnlocked = false;
                        if(isUnlocked)
                            mapManager.currentLevelIndex++;
                    }if (input.getButtonHit(Buttons.DPadLeft))
                        if (mapManager.currentLevelIndex > 0)
                            mapManager.currentLevelIndex--;
                    break;
                #endregion
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            previousGameTime = gameTime;
            Thread.Sleep(5);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            StringBuilder buffer;
            Vector2 fontPos;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            switch (currentState.getState())
            {
                #region Gameplay
                case GameState.Gameplay:
                    if (isEndLevel())
                        endLevel();
                    else
                    {
                        Level currentLevel = mapManager.getCurrentLevel();
                        float aveX = getAveX();
                        #region Gameworld
                        if(mapManager.getCurrentLevel().horizon != null) 
                            spriteBatch.Draw(mapManager.getCurrentLevel().horizon, Vector2.Zero, Color.White);
                        spriteBatch.Draw(mapManager.getCurrentLevel().background, new Vector2(-aveX, 0), Color.White);
                        for (int currentDepth = 0; currentDepth < 4; currentDepth++)
                        {
                            foreach (GameItem gameItem in activeItems)
                                if (currentDepth >= gameItem.startdepth &&
                                    currentDepth < gameItem.width)
                                {
                                    Vector2 pos = new Vector2(gameItem.body.Position.X - aveX + res.ScreenWidth / 2, gameItem.body.Position.Y);
                                    if (gameItem.name.Contains("log") || gameItem.name.Contains("circle"))
                                    {
                                        Vector2 scale = new Vector2((float)gameItem.radius * 2 / (float)texMan.getTex("log").Width, (float)gameItem.radius * 2 / (float)texMan.getTex("log").Width);
                                        drawItem(texMan.getTex("log"), pos, -gameItem.body.Rotation, currentDepth, scale, SpriteEffects.None, new Vector2(texMan.getTex("log").Width / 2, texMan.getTex("log").Height / 2));
                                    }
                                    else if (gameItem.name.Contains("box") || gameItem.name.Contains("crate"))
                                        drawItem(texMan.getTex("box"), pos, -gameItem.body.Rotation, currentDepth, new Vector2((float)gameItem.radius / (float)texMan.getTex("box").Width, (float)gameItem.radius / (float)texMan.getTex("box").Width), SpriteEffects.None, new Vector2(texMan.getTex("box").Width / 2, texMan.getTex("box").Height / 2));
                                    if (gameItem is DropItem)
                                        drawItem(texMan.getTexColored(((DropItem)gameItem).getItem().getTextureName(), ((DropItem)gameItem).getItem().getPrimaryColor(), ((DropItem)gameItem).getItem().getSecondaryColor(), this.graphics.GraphicsDevice), pos, gameItem.body.Rotation, currentDepth, Vector2.One, SpriteEffects.None, new Vector2(texMan.getTex(((DropItem)gameItem).getItem().getTextureName()).Width / 2, texMan.getTex(((DropItem)gameItem).getItem().getTextureName()).Height / 2));
                                }
                            foreach (Being enemy in spawnManager.enemies)
                                if (currentDepth == enemy.getDepth())
                                    enemy.draw(currentDepth);
                            foreach (Being player in players)
                                if (currentDepth == player.getDepth())
                                {
                                    player.draw(currentDepth);
                                    DropItem item = getClosestDropItem(player);
                                    if(item != null && Vector2.DistanceSquared(item.body.Position, player.body.Position) < DROP_ITEM_MAX_DISTANCE)
                                        drawItemDescription(item);
                                }
                        }
                        #endregion
                        #region HUD
                        for (int pIndex = 0; pIndex < players.Count; ++pIndex)
                        {
                            Being currPlayer = players[pIndex];
                            if (currPlayer.menu.getMenuActive())
                            {
                                spriteBatch.Draw(texMan.getTex("portraitBackground"), new Vector2(24 + 16 + pIndex * 288 + 32 * pIndex, 63), new Color(255, 255, 255, 192));
                                if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Data)
                                {
                                    spriteBatch.DrawString(vademecumFont18, "Level:   " + currPlayer.level, new Vector2(48 + pIndex * 320, 67), Color.Black);
                                    spriteBatch.DrawString(vademecumFont18, "Current XP:     " + currPlayer.xp, new Vector2(48 + pIndex * 320, 93), Color.Black);
                                    spriteBatch.DrawString(vademecumFont18, "XP to Level:     " + currPlayer.xpOfNextLevel, new Vector2(48 + pIndex * 320, 119), Color.Black);
                                    spriteBatch.DrawString(vademecumFont18, "Agility:   " + currPlayer.getStat(BountyBandits.Stats.StatType.Agility), new Vector2(48 + pIndex * 320, 145), currPlayer.menu.getMenuColor(0));
                                    spriteBatch.DrawString(vademecumFont18, "Magic:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Magic), new Vector2(48 + pIndex * 320, 171), currPlayer.menu.getMenuColor(1));
                                    spriteBatch.DrawString(vademecumFont18, "Speed:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Speed), new Vector2(48 + pIndex * 320, 197), currPlayer.menu.getMenuColor(2));
                                    spriteBatch.DrawString(vademecumFont18, "Strength:  " + currPlayer.getStat(BountyBandits.Stats.StatType.Strength), new Vector2(48 + pIndex * 320, 223), currPlayer.menu.getMenuColor(3));
                                    spriteBatch.DrawString(vademecumFont18, "Available: " + currPlayer.unusedAttr, new Vector2(48 + pIndex * 320, 249), currPlayer.menu.getMenuColor(4));
                                }
                                if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Inv)
                                    spriteBatch.DrawString(vademecumFont18, "Inventory Screen", new Vector2(48 + pIndex * 320, 67), Color.Black);
                                if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Stats)
                                    spriteBatch.DrawString(vademecumFont18, "Data Screen", new Vector2(48 + pIndex * 320, 67), Color.Black);


                            }
                            if (currPlayer.controller.portrait != null)
                            {
                                int xLoc = 42 - currPlayer.controller.portrait.Width / 2 + pIndex * 288 + 32 * pIndex,
                                    yLoc = 43 - currPlayer.controller.portrait.Height / 2;
                                spriteBatch.Draw(currPlayer.controller.portrait, new Vector2(xLoc, yLoc), Color.White);
                            }
                            spriteBatch.Draw(texMan.getTex("portrait"), new Vector2(16 + pIndex * 288 + 32 * pIndex, 16), Color.White);

                            for (int healthIndex = 0; healthIndex < currPlayer.currenthealth; ++healthIndex)
                                spriteBatch.Draw(texMan.getTex("redBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * healthIndex, 16), Color.White);
                            spriteBatch.DrawString(vademecumFont12, currPlayer.currenthealth + "/" + currPlayer.getStat(BountyBandits.Stats.StatType.Life), new Vector2(86 + pIndex * 288 + 32 * pIndex, 14), Color.Black);

                            for (int specialIndex = 0; specialIndex < currPlayer.currentspecial; ++specialIndex)
                                spriteBatch.Draw(texMan.getTex("yellowBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * specialIndex, 40), Color.White);
                            spriteBatch.DrawString(vademecumFont12, currPlayer.currentspecial + "/" + currPlayer.maxspecial, new Vector2(86 + pIndex * 288 + 32 * pIndex, 40), Color.Black);

                        }
                        #endregion
                    }
                    break;
                #endregion
                #region root menu
                case GameState.RootMenu:
                    buffer = new StringBuilder();
                    fontPos = new Vector2(1.0f, 1.0f);
                    buffer.AppendLine("Choose character, A to start game");
                    spriteBatch.DrawString(vademecumFont24, buffer.ToString(), fontPos, Color.Black);
                    break;
                #endregion
                #region worldmap
                case GameState.WorldMap:
                    spriteBatch.Draw(mapManager.worldBackground, Vector2.Zero, Color.White);
                    foreach (Level level in mapManager.getLevels())
                        spriteBatch.Draw(easyLevel, level.loc, Color.White);
                    const string chooseLevelStr = "Choose level, A to start game";
                    spriteBatch.DrawString(vademecumFont24, chooseLevelStr, new Vector2(res.ScreenWidth / 2 - chooseLevelStr.Length*12, 1.0f), Color.Black);
                    spriteBatch.Draw(texMan.getTex("mapInfo"), new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2), Color.White);
                    spriteBatch.DrawString(vademecumFont24, "Level name:", new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 128), Color.Black);
                    spriteBatch.DrawString(vademecumFont24, mapManager.getCurrentLevel().name, new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 128 + 32), Color.Black);
                    spriteBatch.DrawString(vademecumFont24, "Level length:", new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 200), Color.Black);
                    spriteBatch.DrawString(vademecumFont24, mapManager.getCurrentLevel().background.Width.ToString(), new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 200 + 32), Color.Black);
                    break;
                #endregion
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void drawItem(Texture2D tex, Vector2 pos, float rot, int depth, Vector2 scale, SpriteEffects effects, Vector2 origin)
        {
            spriteBatch.Draw(tex, new Vector2(pos.X - DEPTH_X_OFFSET * depth, res.ScreenHeight - pos.Y - (DEPTH_MULTIPLE * (3 - depth))), null, Color.White, rot, origin, scale, effects, ((float)depth) / 10.0f);
        }
        public void drawItemDescription(DropItem item)
        {
            const int FONT_WIDTH = 10;
            const int BUFFER_WIDTH = 36;
            #region Get color for item class
            Color nameColor = Color.White;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Magic)   nameColor = Color.Yellow;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Rare)    nameColor = Color.Orange;
            if (item.getItem().getItemClass() == BountyBandits.Inventory.ItemClass.Unique)  nameColor = Color.Blue;
            #endregion
            #region Get name string
            String name = item.getItem().getName();
            int numNewLines = 1, maxWidth = BUFFER_WIDTH + ((name.Length > 20) ? 20 : name.Length) * FONT_WIDTH;
            for (int insertIndex = 20; insertIndex < name.Length; insertIndex += 20, numNewLines++)
                name = name.Substring(0, insertIndex) + "-\n" + name.Substring(insertIndex+1);
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
            Texture2D tex = texMan.getTex(item.getItem().getTextureName());
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
            float aveX = getAveX() - res.ScreenWidth / 2;
            int backgroundDrawHeight = (20 * numNewLines < (texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3) ? (texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3 : 20 * numNewLines;
            spriteBatch.Draw(texMan.getTex("portraitBackground"), new Vector2(item.body.Position.X - 46f - aveX, res.ScreenHeight - (item.body.Position.Y + 60f + (DEPTH_MULTIPLE * (3 - item.startdepth)))), new Rectangle(0, 0, maxWidth, backgroundDrawHeight), new Color(255, 255, 255, 192));
            drawItem(texMan.getTexColored(item.getItem().getTextureName(), item.getItem().getPrimaryColor(), item.getItem().getSecondaryColor(), this.graphics.GraphicsDevice), new Vector2(item.body.Position.X - 25f - aveX, item.body.Position.Y + 15f), 0f, (int)item.startdepth, Vector2.One, SpriteEffects.None, new Vector2(texMan.getTex(item.getItem().getTextureName()).Width / 2, texMan.getTex(item.getItem().getTextureName()).Height / 2));
            spriteBatch.DrawString(vademecumFont12, name, new Vector2(item.body.Position.X - 10f - aveX, res.ScreenHeight - (item.body.Position.Y + 60f + (DEPTH_MULTIPLE * (3 - item.startdepth)))), nameColor);
            spriteBatch.DrawString(vademecumFont12, stats, new Vector2(item.body.Position.X - 10f - aveX, res.ScreenHeight - (item.body.Position.Y + 45f - (20f*(name.Length/20)) + (DEPTH_MULTIPLE * (3 - item.startdepth)))), Color.White);
            #endregion
        }
        public void endLevel()
        {
            foreach (Being player in players)
                player.unlocked.add(mapManager, difficulty);
            mapManager.currentLevelIndex++;
            currentState.setState(GameState.WorldMap);
        }
        public float getAveX()
        {
            float aveX = 0;
            foreach (Being player in players) aveX += player.getPos().X;
            aveX /= players.Count;
            if (aveX < res.ScreenWidth / 2)
                aveX = res.ScreenWidth / 2;
            else if (aveX > mapManager.getCurrentLevel().background.Width - res.ScreenWidth)
                aveX = mapManager.getCurrentLevel().background.Width - res.ScreenWidth;
            return aveX;
        }
        public DropItem getClosestDropItem(Being player)
        {
            DropItem closest = null;
            float closestDist = float.MaxValue;
            foreach (GameItem item in activeItems)
            {
                if (item is DropItem)
                {
                    if (closest != null)
                    {
                        float distanceToPlayer = Vector2.DistanceSquared(item.body.Position, player.body.Position);
                        if (distanceToPlayer < closestDist)
                        {
                            closestDist = distanceToPlayer;
                            closest = (DropItem)item;
                        }
                    }
                    else
                        closest = (DropItem)item;
                }
            }
            return closest;
        }
        public bool isEndLevel()
        {
            bool endLevel = false;
            foreach (Being player in players)
                if (player.getPos().X >= mapManager.getCurrentLevel().background.Width - res.ScreenWidth/2)
                    endLevel = true;
            return endLevel;
        }
        public void newLevel()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, -20));
            #region add gameitems
            activeItems = new List<GameItem>();
            foreach (SpawnPoint spawn in mapManager.getCurrentLevel().spawns)
                if (spawn.type != null)
                    animationManager.getController(spawn.type);
            foreach (Being player in players)
                player.newLevel();
            foreach (GameItem item in mapManager.getCurrentLevel().items)
            {
                Geom geom = new Geom();
                if (item.name.Contains("box"))
                {
                    item.body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, item.radius, item.radius, item.weight);
                    geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, item.body, item.radius, item.radius);
                }
                else if (item.name.Contains("circle") || item.name.Contains("log"))
                {
                    item.body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, item.radius, item.weight);
                    geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, item.body, item.radius, 8);
                }
                geom.FrictionCoefficient = .6f;
                #region Collision Categories
                geom.CollisionCategories = CollisionCategory.None;
                for (uint depth = item.startdepth; depth < item.width; depth++)
                    geom.CollisionCategories = (CollisionCategory)(int)geom.CollisionCategories + ((int)Math.Pow(2, depth));
                geom.CollidesWith = geom.CollisionCategories;
                #endregion
                item.body.Position = item.loc;
                activeItems.Add(item);
            }
            #endregion
            #region physics - add ground and side wall
            const int GROUND_WIDTH = 10000;
            const int GROUND_HEIGHT = 100;
            Body ground = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, GROUND_WIDTH, GROUND_HEIGHT, 100);
            groundGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, ground, GROUND_WIDTH, GROUND_HEIGHT);
            groundGeom.FrictionCoefficient = 1;
            ground.Position = new Vector2(GROUND_WIDTH / 2 - 32, -GROUND_HEIGHT/2);
            ground.IsStatic = true;
            Body sideWall = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 100, 1000, 100);
            Geom sideWallGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, sideWall, 100, 1000);
            sideWall.Position = new Vector2(-42, 500);
            sideWall.IsStatic = true;
            #endregion
            spawnManager.newLevel(mapManager.getCurrentLevel());
            currentState.setState(GameState.Gameplay);
        }
    }
    static class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
}
