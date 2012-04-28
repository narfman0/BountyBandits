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
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using BountyBandits.Stats;
using BountyBandits.Animation;
using BountyBandits.Map;
using BountyBandits.Story;
using System.IO;
using BountyBandits.Network;
using System.Net;
using Lidgren.Network;
using BountyBandits.Inventory;
using BountyBandits.Character;

namespace BountyBandits
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        #region Fields
        public const float DEPTH_MULTIPLE = 42, DEPTH_X_OFFSET = 12, FORCE_AMOUNT = 10, DROP_ITEM_MAX_DISTANCE = 10000f;
        DifficultyEnum difficulty = DifficultyEnum.Normal;
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public Dictionary<Guid, Being> players = new Dictionary<Guid, Being>();
        public Dictionary<Guid, GameItem> activeItems;
        
        public StoryElement storyElement;
        public Dictionary<int, Being> storyBeings;
        public double timeStoryElementStarted;

        public SpriteFont vademecumFont24, vademecumFont12, vademecumFont18;
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
        public static MarkovNameGenerator nameGenerator;
        public NetworkManager network;
        public StateManager currentState;
        //choosing characters
        public List<String> characterOptions = new List<string>(SaveManager.getAvailableCharacterNames());
        public Dictionary<PlayerIndex, int> selectedMenuIndex = new Dictionary<PlayerIndex, int>();
        public List<Input> inputs = new List<Input>();
        public static Game instance;
        #endregion
        public Game()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            activeItems = new Dictionary<Guid, GameItem>();
            res = Resolution.Initialize(graphics);
#if XBOX
                graphics.IsFullScreen = true;
#endif

            spawnManager = new SpawnManager(this);
            rand = new Random();
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
                inputs.Add(new Input(playerIndex));
            inputs[0].useKeyboard = true;
            network = new NetworkManager(this);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            nameGenerator = new MarkovNameGenerator(MarkovNameGenerator.SAMPLES, 3, 5);
            texMan = new TextureManager(Content);
            vademecumFont12 = Content.Load<SpriteFont>(@"Fonts\vademecum12");
            vademecumFont18 = Content.Load<SpriteFont>(@"Fonts\vademecum18");
            vademecumFont24 = Content.Load<SpriteFont>(@"Fonts\vademecum24");

            easyLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "easyLevel");
            extremeLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "extremeLevel");
            hardLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "hardLevel");
            mediumLevel = Content.Load<Texture2D>(MapManager.CAMPAIGNS_PATH + "mediumLevel");
            animationManager = new AnimationManager(this);
            mapManager = new MapManager(this, MapManager.DEFAULT_CAMPAIGN_PATH);

            physicsSimulator = new PhysicsSimulator(new Vector2(0, -20));
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
                if (!selectedMenuIndex.ContainsKey(playerIndex))
                    selectedMenuIndex.Add(playerIndex, -1);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            currentState = new StateManager(this);
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            network.update(gameTime);
            #region fullscreen
#if WINDOWS
            if (Keyboard.GetState().IsKeyDown(Keys.LeftAlt) && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                res.Mode = res.BaseMode = !graphics.IsFullScreen ?
                    ScreenMode.XGA : Resolution.GetPreferredMode(graphics);
                res.SetResolution(graphics);
            }
#endif
            #endregion
            currentState.getScreen().Update(gameTime);
            Thread.Sleep(5);
            base.Update(gameTime);
        }
        public void dropItem(Vector2 dropPosition, Being killedBeing)
        {
            DropItem item = new DropItem();
            item.body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, item.radius, item.radius, item.weight);
            item.body.Position = new Vector2(dropPosition.X, dropPosition.Y + 25);
            item.body.Rotation = 100f * (float)(rand.NextDouble() - .5f);
            item.body.AngularVelocity = (float)(rand.NextDouble() - .5f);
            item.body.LinearVelocity = 50f * new Vector2((float)rand.NextDouble() - .5f, 1);
            #region geometry setup
            Geom geom = new Geom();
            geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, item.body, item.radius, item.radius);
            geom.FrictionCoefficient = .6f;
            geom.CollisionCategories = killedBeing.geom.CollisionCategories;
            geom.CollidesWith = geom.CollisionCategories;
            #endregion
            item.setItem(DropManager.generateItem(killedBeing));
            item.startdepth = (uint)PhysicsHelper.collisionCategoryToDepth(geom.CollisionCategories);
            activeItems.Add(item.guid, item);
            network.sendFullObjectsUpdate();
        }
        public bool isUnlocked(int level)
        {
            foreach (Being player in players.Values)
                if (!player.unlocked.isUnlocked(mapManager.guid, difficulty, level))
                    return false;
            return true;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            currentState.getScreen().Draw(gameTime);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void drawGameplay(Vector2 avePosition)
        {
            Level currentLevel = mapManager.getCurrentLevel();
            #region Gameworld
            if (mapManager.getCurrentLevel().horizon != null)
            {
                Vector2 currentResolution = new Vector2(res.ScreenWidth, res.ScreenHeight),
                    origin = new Vector2(mapManager.getCurrentLevel().horizon.Width / 2f, mapManager.getCurrentLevel().horizon.Height / 2f),
                    position = currentResolution / 2f - new Vector2(0, avePosition.Y - res.ScreenHeight / 2);
                spriteBatch.Draw(mapManager.getCurrentLevel().horizon, new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            }
            foreach (BackgroundItemStruct item in mapManager.getCurrentLevel().backgroundItems)
            {
                Vector2 position = item.location - new Vector2(avePosition.X - res.ScreenWidth / 2, avePosition.Y - res.ScreenHeight / 2);
                Texture2D tex = texMan.getTex(item.texturePath);
                drawItem(tex, position, item.rotation, 0f, new Vector2(item.scale), SpriteEffects.None, new Vector2(tex.Width / 2, tex.Height / 2));
            }
            for (int currentDepth = 0; currentDepth < 4; currentDepth++)
            {
                foreach (GameItem gameItem in activeItems.Values)
                    if (//gameItem.width - 1 + gameItem.startdepth == currentDepth) //single lowest depth drawing
                        
                        currentDepth >= gameItem.startdepth && //multidepth drawing
                        currentDepth < gameItem.startdepth + gameItem.width)
                    {
                        Vector2 scale = Vector2.One, pos = new Vector2(gameItem.body.Position.X - avePosition.X + res.ScreenWidth / 2, gameItem.body.Position.Y - avePosition.Y + res.ScreenHeight / 2);
                        Texture2D tex = !(gameItem is DropItem) ? texMan.getTex(gameItem.name) :
                            texMan.getTexColored(((DropItem)gameItem).getItem().getTextureName(), ((DropItem)gameItem).getItem().getPrimaryColor(), ((DropItem)gameItem).getItem().getSecondaryColor(), this.graphics.GraphicsDevice);
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
                            }
                        drawGameItem(tex, pos, rotation, currentDepth, scale, SpriteEffects.None, origin);
                    }
                foreach (Being enemy in spawnManager.enemies.Values)
                    if (currentDepth == enemy.getDepth())
                        enemy.draw();
                foreach (Being player in players.Values)
                    if (currentDepth == player.getDepth())
                    {
                        player.draw();
                        DropItem item = getClosestDropItem(player);
                        if (item != null && Vector2.DistanceSquared(item.body.Position, player.body.Position) < DROP_ITEM_MAX_DISTANCE)
                            drawItemDescription(item);
                    }
            }
            #endregion
            #region HUD
            int pIndex = 0;
            foreach (Being currPlayer in players.Values)
            {
                if (currPlayer.menu.isMenuActive())
                {
                    spriteBatch.Draw(texMan.getTex("portraitBackground"), new Vector2(24 + 16 + pIndex * 288 + 32 * pIndex, 63), new Color(255, 255, 255, 192));
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Data)
                    {
                        drawTextBorder(vademecumFont18, "Level:   " + currPlayer.level, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);
                        drawTextBorder(vademecumFont18, "Current XP:    " + currPlayer.xp, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 93), Color.Black, Color.White, 0);
                        drawTextBorder(vademecumFont18, "XP to Level:   " + currPlayer.xpOfNextLevel, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 119), Color.Black, Color.White, 0);
                        drawTextBorder(vademecumFont18, "Agility:   " + currPlayer.getStat(BountyBandits.Stats.StatType.Agility), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 145), currPlayer.menu.getMenuColor(0), Color.White, 0);
                        drawTextBorder(vademecumFont18, "Magic:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Magic), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 171), currPlayer.menu.getMenuColor(1), Color.White, 0);
                        drawTextBorder(vademecumFont18, "Speed:     " + currPlayer.getStat(BountyBandits.Stats.StatType.Speed), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 197), currPlayer.menu.getMenuColor(2), Color.White, 0);
                        drawTextBorder(vademecumFont18, "Strength:  " + currPlayer.getStat(BountyBandits.Stats.StatType.Strength), new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 223), currPlayer.menu.getMenuColor(3), Color.White, 0);
                        drawTextBorder(vademecumFont18, "Available: " + currPlayer.unusedAttr, new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 249), currPlayer.menu.getMenuColor(4), Color.White, 0);
                    }
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Inv)
                        drawTextBorder(vademecumFont18, "Inventory Screen", new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);
                    if (currPlayer.menu.getMenuScreen() == Menu.MenuScreens.Stats)
                        drawTextBorder(vademecumFont18, "Data Screen", new Vector2(48 + pIndex * 320, res.ScreenHeight - 128 - 67), Color.Black, Color.White, 0);


                }
                if (currPlayer.controller.portrait != null)
                {
                    int xLoc = 42 - currPlayer.controller.portrait.Width / 2 + pIndex * 288 + 32 * pIndex,
                        yLoc = 43 - currPlayer.controller.portrait.Height / 2;
                    spriteBatch.Draw(currPlayer.controller.portrait, new Vector2(xLoc, yLoc), Color.White);
                }
                spriteBatch.Draw(texMan.getTex("portrait"), new Vector2(16 + pIndex * 288 + 32 * pIndex, 16), Color.White);

                for (int healthIndex = 0; healthIndex < (int)currPlayer.CurrentHealth; ++healthIndex)
                    spriteBatch.Draw(texMan.getTex("redBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * healthIndex, 16), Color.White);
                int currentHP = currPlayer.CurrentHealth > 0f && (int)currPlayer.CurrentHealth == 0 ? 1 : (int)currPlayer.CurrentHealth;
                drawTextBorder(vademecumFont12, currentHP + "/" + currPlayer.getStat(BountyBandits.Stats.StatType.Life), new Vector2(86 + pIndex * 288 + 32 * pIndex, res.ScreenHeight - 140), Color.Black, Color.DarkGray, 0);

                for (int specialIndex = 0; specialIndex < (int)currPlayer.currentspecial; ++specialIndex)
                    spriteBatch.Draw(texMan.getTex("yellowBar"), new Vector2(66 + pIndex * 288 + 32 * pIndex + 8 * specialIndex, 40), Color.White);
                drawTextBorder(vademecumFont12, (int)currPlayer.currentspecial + "/" + currPlayer.getStat(StatType.Special), new Vector2(86 + pIndex * 288 + 32 * pIndex, res.ScreenHeight - 164), Color.Black, Color.DarkGray, 0);

                pIndex++;
            }
            #endregion
        }
        public void drawGameItem(Texture2D tex, Vector2 pos, float rot, float depth, Vector2 scale, SpriteEffects effects, Vector2 origin)
        {
            drawItem(tex, new Vector2(pos.X - DEPTH_X_OFFSET * depth, pos.Y + (DEPTH_MULTIPLE * (3 - depth))), rot, depth / 10f, scale, effects, origin);
        }
        public void drawItem(Texture2D tex, Vector2 pos, float rot, float depth, Vector2 scale, SpriteEffects effects, Vector2 origin)
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
        public void drawText(SpriteFont font, String text, Vector2 pos, Color color, int depth)
        {
            spriteBatch.DrawString(font, text, new Vector2(pos.X - DEPTH_X_OFFSET * depth, res.ScreenHeight - (pos.Y + (DEPTH_MULTIPLE * (3 - depth)))), color);
        }
        public void drawItemDescription(DropItem item)
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
            Vector2 avePosition = getAvePosition() - new Vector2(res.ScreenWidth / 2, res.ScreenHeight / 2);
            int backgroundDrawHeight = (20 * numNewLines < (texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3) ? (texMan.getTex(item.getItem().getTextureName()).Height * 2) / 3 : 20 * numNewLines;
            spriteBatch.Draw(texMan.getTex("portraitBackground"), new Vector2(item.body.Position.X - 46f - avePosition.X, res.ScreenHeight + avePosition.Y - (item.body.Position.Y + 60f + (DEPTH_MULTIPLE * (3 - item.startdepth)))), new Rectangle(0, 0, maxWidth, backgroundDrawHeight), new Color(255, 255, 255, 192));
            drawGameItem(texMan.getTexColored(item.getItem().getTextureName(), item.getItem().getPrimaryColor(), item.getItem().getSecondaryColor(), this.graphics.GraphicsDevice), new Vector2(item.body.Position.X - 25f - avePosition.X, item.body.Position.Y + 15f - avePosition.Y), 0f, (int)item.startdepth, Vector2.One, SpriteEffects.None, new Vector2(texMan.getTex(item.getItem().getTextureName()).Width / 2, texMan.getTex(item.getItem().getTextureName()).Height / 2));
            drawTextBorder(vademecumFont12, name, new Vector2(item.body.Position.X - 10f - avePosition.X, (item.body.Position.Y + -64f + (DEPTH_MULTIPLE * (3 - item.startdepth)) - avePosition.Y)), nameColor, Color.Black, 0);
            drawTextBorder(vademecumFont12, stats, new Vector2(item.body.Position.X - 10f - avePosition.X, (item.body.Position.Y + -79f - (20f * (name.Length / 20)) + (DEPTH_MULTIPLE * (3 - item.startdepth)) - avePosition.Y)), Color.White, Color.Black, 0);
            #endregion
        }
        public void endLevel(bool increment)
        {
            foreach (Being player in players.Values)
            {
                if (increment)
                    player.unlocked.add(mapManager, difficulty);
                SaveManager.saveCharacter(player);
            }
            bool autoProgress = mapManager.getCurrentLevel().autoProgress;
            mapManager.incrementCurrentLevel(increment);
            if (autoProgress)
                newLevel();
            else
                currentState.setState(GameState.WorldMap);
        }
        public Vector2 getAvePosition()
        {
            Vector2 ave = Vector2.Zero;
            foreach (Being player in players.Values)
                ave += player.getPos();
            ave /= players.Count;
            if (ave.Y < res.ScreenHeight / 2)
                ave.Y = res.ScreenHeight / 2;
            if (ave.X < res.ScreenWidth / 2)
                ave.X = res.ScreenWidth / 2;
            else if (ave.X > mapManager.getCurrentLevel().levelLength - res.ScreenWidth)
                ave.X = mapManager.getCurrentLevel().levelLength - res.ScreenWidth;
            return ave;
        }
        public DropItem getClosestDropItem(Being player)
        {
            DropItem closest = null;
            float closestDist = float.MaxValue;
            foreach (GameItem item in activeItems.Values)
            {
                if (item is DropItem && player.getDepth() == item.startdepth)
                {
                    if (closest == null)
                        closest = (DropItem)item;
                    float distanceToPlayer = Vector2.DistanceSquared(item.body.Position, player.body.Position);
                    if (distanceToPlayer < closestDist)
                    {
                        closestDist = distanceToPlayer;
                        closest = (DropItem)item;
                    }
                }
            }
            return closest;
        }
        public bool isEndLevel()
        {
            foreach (Being player in players.Values)
                if (player.getPos().X >= mapManager.getCurrentLevel().levelLength - res.ScreenWidth / 2)
                    return true;
            return false;
        }
        public void newLevel()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, -10));
            foreach (Being player in players.Values)
                if (player.isLocal)
                    player.newLevel();
            #region add gameitems
            activeItems.Clear();
            if (!network.isClient())
            {
                foreach (SpawnPoint spawn in mapManager.getCurrentLevel().spawns)
                    if (spawn.type != null)
                        animationManager.getController(spawn.type);
                foreach (GameItem item in mapManager.getCurrentLevel().items)
                    addGameItem(item);
            }
            #endregion
            spawnManager.newLevel(mapManager.getCurrentLevel());
            mapManager.getCurrentLevel().resetStoryElements();
            #region physics - add ground and side wall
            const int GROUND_WIDTH = 10000;
            const int GROUND_HEIGHT = 100;
            Body ground = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, GROUND_WIDTH, GROUND_HEIGHT, 100);
            groundGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, ground, GROUND_WIDTH, GROUND_HEIGHT);
            groundGeom.FrictionCoefficient = 1;
            ground.Position = new Vector2(GROUND_WIDTH / 2 - 32, -GROUND_HEIGHT / 2);
            ground.IsStatic = true;
            #endregion
            currentState.setState(GameState.Gameplay);
        }
        public void addGameItem(GameItem item)
        {
            Geom geom = new Geom();
            switch (item.polygonType)
            {
                case PhysicsPolygonType.Circle:
                    item.body = BodyFactory.Instance.CreateCircleBody(physicsSimulator, item.radius, item.weight);
                    geom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, item.body, item.radius, 12);
                    break;
                case PhysicsPolygonType.Rectangle:
                    item.body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, item.sideLengths.X, item.sideLengths.Y, item.weight);
                    geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, item.body, item.sideLengths.X, item.sideLengths.Y);
                    break;
                case PhysicsPolygonType.Polygon:
                    item.body = BodyFactory.Instance.CreatePolygonBody(item.vertices, item.weight);
                    geom = GeomFactory.Instance.CreatePolygonGeom(item.body, item.vertices, item.radius);
                    break;
            }
            if (item.immovable)
                item.body.IsStatic = item.immovable;
            geom.FrictionCoefficient = .6f;
            #region Collision Categories
            geom.CollisionCategories = CollisionCategory.None;
            for (int depth = (int)item.startdepth; depth < item.width + item.startdepth; depth++)
                geom.CollisionCategories |= (CollisionCategory)PhysicsHelper.depthToCollisionCategory(depth);
            geom.CollidesWith = geom.CollisionCategories;
            #endregion
            item.body.Position = item.loc;
            item.body.Rotation = item.rotation;
            activeItems.Add(item.guid, item);
            network.sendFullObjectsUpdate();
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
