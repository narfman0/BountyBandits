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
using BountyBandits.GameScreen;

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
        public SpriteFont vademecumFont24, vademecumFont12, vademecumFont18;
        public MapManager mapManager;
        public AnimationManager animationManager;
        public SpawnManager spawnManager;
        public PhysicsSimulator physicsSimulator;
        public Resolution res;
        public Random rand;
        public Geom groundGeom;
        public TextureManager texMan;
        public XPManager xpManager = new XPManager();
        public NetworkManager network;
        public StateManager currentState;
        public List<Input> inputs = new List<Input>();
        public static Game instance;
        #endregion
        public Game()
        {
            instance = this;
            rand = new Random();
            activeItems = new Dictionary<Guid, GameItem>();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            res = Resolution.Initialize(graphics);
#if XBOX
                graphics.IsFullScreen = true;
#endif
            spawnManager = new SpawnManager(this);
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
                inputs.Add(new Input(playerIndex));
            inputs[0].useKeyboard = true;
            network = new NetworkManager(this);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            texMan = new TextureManager(Content);
            vademecumFont12 = Content.Load<SpriteFont>(@"Fonts\vademecum12");
            vademecumFont18 = Content.Load<SpriteFont>(@"Fonts\vademecum18");
            vademecumFont24 = Content.Load<SpriteFont>(@"Fonts\vademecum24");
            animationManager = new AnimationManager(this);
            mapManager = new MapManager(this, MapManager.DEFAULT_CAMPAIGN_PATH);
            physicsSimulator = new PhysicsSimulator(new Vector2(0, -20));
            spriteBatch = new SpriteBatch(GraphicsDevice);
            currentState = new StateManager();
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
            //if the level is supposed to go seemlessly to the next stage 
            //without worldmap, and the user hasn't quit early, go to next level
            if (autoProgress && increment)
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
            resetPhysics();
            foreach (Being player in players.Values)
                if (player.isLocal)
                    player.newLevel();
            #region add gameitems
            activeItems.Clear();
            if (network != null && !network.isClient())
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
            currentState.setState(GameState.Gameplay);
        }
        public void resetPhysics()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, -10));
            const int GROUND_WIDTH = 20000, GROUND_HEIGHT = 100;
            Body ground = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, GROUND_WIDTH, GROUND_HEIGHT, 100);
            groundGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, ground, GROUND_WIDTH, GROUND_HEIGHT);
            groundGeom.FrictionCoefficient = 1;
            ground.Position = new Vector2(GROUND_WIDTH / 2 - 32, -GROUND_HEIGHT / 2);
            ground.IsStatic = true;
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
                    geom = PhysicsHelper.textureToGeom(physicsSimulator, texMan.getTex(item.name), item.weight);
                    item.body = geom.Body;
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
