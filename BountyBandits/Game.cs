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
        GameTime previousGameTime = new GameTime();
        SpriteBatch spriteBatch;
        public Dictionary<Guid, Being> players = new Dictionary<Guid, Being>();
        public Dictionary<Guid, GameItem> activeItems;
        StoryElement storyElement; double timeStoryElementStarted; Dictionary<int, Being> storyBeings;
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
        List<String> characterOptions = new List<string>(SaveManager.getAvailableCharacterNames());
        Dictionary<PlayerIndex, int> selectedMenuIndex = new Dictionary<PlayerIndex, int>(); int selectedMenuItem = 0;
        List<Input> inputs = new List<Input>();
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
            currentState = new StateManager(this);
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
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            float timeElapsed = (float)(gameTime.ElapsedGameTime.Milliseconds - previousGameTime.ElapsedGameTime.Milliseconds);
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
            switch (currentState.getState())
            {
                #region RootMenu
                case GameState.RootMenu:
                    foreach (Input input in inputs)
                    {
                        input.update();
                        updateMenu(input, Enum.GetValues(typeof(RootMenuOptions)).Length);
                        if (input.getButtonHit(Buttons.A))
                        {
                            switch (selectedMenuItem)
                            {
                                case 0:
                                    currentState.setState(GameState.CharacterSelection);
                                    break;
                                case 1:
                                    currentState.setState(GameState.Multiplayer);
                                    break;
                                case 2:
                                    Exit();
                                    break;
                            }
                        }
                    }
                    break;
                #endregion
                #region Multiplayer
                case GameState.Multiplayer:
                    foreach (Input input in inputs)
                    {
                        input.update();
                        updateMenu(input, Enum.GetValues(typeof(MultiplayerMenuOptions)).Length);
                        if (input.getButtonHit(Buttons.A))
                        {
                            switch (selectedMenuItem)
                            {
                                case 0:
                                    network.startServer();
                                    currentState.setState(GameState.CharacterSelection);
                                    break;
                                case 1:
                                    currentState.setState(GameState.JoinScreen);
                                    break;
                                case 2:
                                    currentState.setState(GameState.RootMenu);
                                    break;
                            }
                        }
                    }
                    break;
                #endregion
                #region JoinScreen
                case GameState.JoinScreen:
                    foreach (Input input in inputs)
                    {
                        input.update();
                        updateMenu(input, Enum.GetValues(typeof(JoinMenuOptions)).Length);
                        if (input.getButtonHit(Buttons.A))
                        {
                            switch (selectedMenuItem)
                            {
                                case 0://join
                                    if (network.startClient())
                                        currentState.setState(GameState.CharacterSelection);
                                    break;
                                case 1:
                                    currentState.setState(GameState.Multiplayer);
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
                    break;
                #endregion
                #region cutscene
                case GameState.Cutscene:
                    double elapsedCutsceneTime = gameTime.TotalGameTime.TotalMilliseconds - timeStoryElementStarted;
                    #region audio
                    AudioElement audio = storyElement.popAudioElement(elapsedCutsceneTime);
                    if (audio != null)
                        Content.Load<SoundEffect>(mapManager.currentCampaignPath + audio.audioPath).Play();
                    #endregion
                    #region characters
                    foreach (BeingController controller in storyElement.beingControllers)
                    {
                        if (!storyBeings.ContainsKey(controller.entranceMS) &&
                            controller.entranceMS >= elapsedCutsceneTime)
                        {
                            Being being = new Being(controller.entranceMS + "", 1, this, controller.animationController, null, false, true);
                            being.body.Position = controller.startLocation;
                            being.changeAnimation(controller.animations[0].animationName);
                            being.setDepth(controller.startDepth);
                            storyBeings.Add(controller.entranceMS, being);
                        }
                        if (storyBeings.ContainsKey(controller.entranceMS))
                        {
                            Being being = storyBeings[controller.entranceMS];
                            being.changeAnimation(controller.getCurrentAnimation(elapsedCutsceneTime));
                            ActionStruct currentAction = controller.getCurrentAction(elapsedCutsceneTime);
                            if (currentAction != null)
                            {
                                switch (currentAction.action)
                                {
                                    case ActionEnum.Jump:
                                        being.jump();
                                        break;
                                    case ActionEnum.Stop:
                                        being.body.ApplyForce(-being.body.Force);
                                        break;
                                    case ActionEnum.Move:
                                        being.move(new Vector2(currentAction.intensity, 0));
                                        break;
                                }
                            }
                        }
                    }
                    foreach (Being being in storyBeings.Values)
                        being.update(gameTime);
                    #endregion
                    #region quit cutscene
                    bool startPressed = false;
                    foreach (Being player in players.Values)
                        if (player.isLocal && player.input.getButtonHit(Buttons.Start))
                            startPressed = true;
                    double msTotal = gameTime.TotalGameTime.TotalMilliseconds;
                    if (startPressed || storyElement.cutsceneLength + 500 < msTotal - timeStoryElementStarted)
                    {
                        currentState.setState(GameState.Gameplay);
                        storyElement = null;
                        storyBeings.Clear();
                    }
                    #endregion
                    physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
                    break;
                #endregion
                #region gameplay
                case GameState.Gameplay:
                    if (isEndLevel())
                        endLevel(true);
                    foreach (Being currentplayer in players.Values)
                    {
                        if (currentplayer.getPos().X < 0)
                            endLevel(false);
                        currentplayer.update(gameTime);
                        #region Input
                        if (currentplayer.isLocal)
                        {
                            currentplayer.input.update();
                            if (currentplayer.input.getButtonDown(Buttons.LeftThumbstickLeft))
                                currentplayer.move(new Vector2(-FORCE_AMOUNT, 0));
                            if (currentplayer.input.getButtonDown(Buttons.LeftThumbstickRight))
                                currentplayer.move(new Vector2(FORCE_AMOUNT, 0));
                            if (currentplayer.input.getButtonHit(Buttons.A))
                            {
                                if (currentplayer.menu.isMenuActive() && 
                                    currentplayer.menu.getMenuScreen() == Menu.MenuScreens.Data && 
                                    currentplayer.unusedAttr > 0)
                                {
                                    if (currentplayer.menu.getMenuItem() == 0) currentplayer.upgradeStat(StatType.Agility, 1);
                                    else if (currentplayer.menu.getMenuItem() == 1) currentplayer.upgradeStat(StatType.Magic, 1);
                                    else if (currentplayer.menu.getMenuItem() == 2) currentplayer.upgradeStat(StatType.Speed, 1);
                                    else if (currentplayer.menu.getMenuItem() == 3) currentplayer.upgradeStat(StatType.Strength, 1);
                                    currentplayer.unusedAttr--;
                                }
                                currentplayer.jump();
                            }
                            if (currentplayer.input.getButtonHit(Buttons.X))
                                currentplayer.attack("attack1");
                            if (currentplayer.input.getButtonHit(Buttons.Back))
                                currentplayer.menu.toggleMenu();
                            if (currentplayer.input.getButtonDown(Buttons.DPadDown))
                                if (currentplayer.menu.isMenuActive())
                                    currentplayer.menu.changeMenuItem(false);
                                else
                                    currentplayer.lane(false);
                            if (currentplayer.input.getButtonDown(Buttons.DPadUp))
                                if (currentplayer.menu.isMenuActive())
                                    currentplayer.menu.changeMenuItem(true);
                                else
                                    currentplayer.lane(true);
                            if (currentplayer.input.getButtonHit(Buttons.DPadRight))
                                currentplayer.menu.changeMenuScreen(true);
                            if (currentplayer.input.getButtonHit(Buttons.DPadLeft))
                                currentplayer.menu.changeMenuScreen(false);
                            if (currentplayer.input.getButtonHit(Buttons.Start))
                                endLevel(false);
#if WINDOWS
                            if (Keyboard.GetState().IsKeyDown(Keys.F2))
                                endLevel(false);
#endif
                            if (currentplayer.input.getButtonHit(Buttons.RightShoulder))
                            {
                                //pick up closest item and throw the equipped one on the ground
                                DropItem dropItem = getClosestDropItem(currentplayer);
                                if (dropItem != null && Vector2.DistanceSquared(dropItem.body.Position, currentplayer.body.Position) < DROP_ITEM_MAX_DISTANCE)
                                {
                                    Item playerItem = currentplayer.getItemManager().getItem(dropItem.getItem().getItemType());
                                    currentplayer.getItemManager().putItem(dropItem.getItem());
                                    if (playerItem != null)
                                    {
                                        dropItem.setItem(playerItem);
                                        dropItem.body.LinearVelocity.Y += 25f;
                                        dropItem.body.ApplyTorque((float)rand.NextDouble() * .25f - .125f);
                                    }
                                    else
                                        activeItems.Remove(dropItem.guid);
                                }
                            }
#if DEBUG
                            if (inputs[0].keyPreviousState.IsKeyUp(Keys.F3) && Keyboard.GetState().IsKeyDown(Keys.F3))
                                spawnManager.spawnGroup("sumo", 1, 1);
                            if (Keyboard.GetState().IsKeyDown(Keys.F4))
                                foreach (Being player in players.Values)
                                    player.giveXP(xpManager.getXPToLevelUp(player.level - 1));
                            if (inputs[0].keyPreviousState.IsKeyUp(Keys.F5) && Keyboard.GetState().IsKeyDown(Keys.F5))
                            {
                                dropItem(1 * currentplayer.body.Position, currentplayer);
                            }
                            if (inputs[0].keyPreviousState.IsKeyUp(Keys.F6) && Keyboard.GetState().IsKeyDown(Keys.F6))
                            {
                                string[] chartypes = {"amish","buddhistmonk","cow","cowboy","frenchman","godzilla",
                                                         "governator","hippie","hitler","kimjongil","mexican","mountie",
                                                     "nerd","obama","panda","pedobear","pirate","seal","shakespeare","sloth",
                                                     "stalin","sumo","tikiSmile","tikiTeeth"};
                                spawnManager.spawnGroup(chartypes[rand.Next(chartypes.Length)], 1, 1);
                            }
                            if (inputs[0].keyPreviousState.IsKeyUp(Keys.F7) && Keyboard.GetState().IsKeyDown(Keys.F7))
                            {
                                GameItem gameItem = new GameItem();
                                gameItem.loc = getAvePosition() + new Vector2(32, res.ScreenHeight);
                                gameItem.polygonType = PhysicsPolygonType.Rectangle;
                                gameItem.sideLengths = new Vector2((float)rand.NextDouble() * 32f + 32f, (float)rand.NextDouble() * 32f + 32f);
                                gameItem.weight = 1;
                                gameItem.name = "box";
                                gameItem.startdepth = (uint)rand.Next(4);
                                addGameItem(gameItem);
                            }
#endif
                        }
                        #endregion
                    }
                    if (!network.isClient())
                        spawnManager.update(gameTime);
                    else
                        spawnManager.updateEnemies(gameTime);
                    physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
                    #region initiate cutscene
                    storyElement = mapManager.getCurrentLevel().popStoryElement(getAvePosition().X);
                    if (storyElement != null)
                    {
                        timeStoryElementStarted = gameTime.TotalGameTime.TotalMilliseconds;
                        currentState.setState(GameState.Cutscene);
                        storyBeings = new Dictionary<int, Being>();
                    }
                    #endregion
                    break;
                #endregion
                #region CharacterSelection
                case GameState.CharacterSelection:
                    foreach (Input input in inputs)
                    {
                        input.update();

                        if (input.getButtonHit(Buttons.A))
                        {
                            bool isPlayerOneAdded = false;
                            foreach (Being player in players.Values)
                                if (player.input.useKeyboard)
                                    isPlayerOneAdded = true;

                            if (selectedMenuIndex[input.getPlayerIndex()] == -1)
                                selectedMenuIndex[input.getPlayerIndex()] = 0;
                            else
                            {
                                Being player = new Being(nameGenerator.NextName, 1, this, animationManager.getController("pirate"), input, true, true);
                                if (selectedMenuIndex[input.getPlayerIndex()] != 0)
                                {
                                    int charindex = selectedMenuIndex[input.getPlayerIndex()] - 1;
                                    String characterName = characterOptions[charindex];
                                    player = SaveManager.loadCharacter(characterName, this);
                                    player.isLocal = true;
                                    player.isPlayer = true;
                                    player.input = input;
                                }
                                List<Guid> killGuids = new List<Guid>();
                                foreach (Being extraPlayer in players.Values)
                                    if (extraPlayer.input.getPlayerIndex() == input.getPlayerIndex())
                                        killGuids.Add(extraPlayer.guid);
                                foreach (Guid kill in killGuids)
                                    players.Remove(kill);
                                players.Add(player.guid, player);
                            }
                            //go to worldmap if player one hits a
                            if (isPlayerOneAdded && input.getPlayerIndex() == PlayerIndex.One)
                            {
                                if (network.isClient())
                                {
                                    network.sendFullPlayersUpdateClient();
                                    currentState.setState(GameState.WorldMap);
                                }
                                else
                                    foreach (Being player in players.Values)
                                        if (player.input.getButtonHit(Buttons.A))
                                            currentState.setState(GameState.WorldMap);
                            }
                        }
                        if (input.getButtonHit(Buttons.DPadDown) || input.getButtonHit(Buttons.LeftThumbstickDown))
                        {
                            int selected = selectedMenuIndex[input.getPlayerIndex()] + 1;
                            PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                            for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                            {
                                if (input.getPlayerIndex() != indices[playerIndex] && //same player
                                    selectedMenuIndex[indices[playerIndex]] == selected)
                                {
                                    selected++;
                                    playerIndex = 0;
                                }
                            }
                            selectedMenuIndex[input.getPlayerIndex()] = selected;
                            if (characterOptions.Count < selected)
                                selectedMenuIndex[input.getPlayerIndex()] = 0;
                        }
                        if (input.getButtonHit(Buttons.DPadUp) || input.getButtonHit(Buttons.LeftThumbstickUp))
                        {
                            int selected = selectedMenuIndex[input.getPlayerIndex()] - 1;
                            PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                            for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                            {
                                if (indices[playerIndex] != input.getPlayerIndex() && //same player
                                    selected != 0 && selectedMenuIndex[indices[playerIndex]] == selected)
                                {
                                    selected--;
                                    playerIndex = 0;
                                }
                            }
                            if (selected <= 0)
                                selected = 0;
                            selectedMenuIndex[input.getPlayerIndex()] = selected;
                        }
                    }
                    break;
                #endregion
                #region world map
                case GameState.WorldMap:
                    foreach (Being player in players.Values)
                    {
                        if (player.isLocal)
                        {
                            player.input.update();
                            if (player.input.getButtonHit(Buttons.Back))
                                currentState.setState(GameState.CharacterSelection);
                            if (player.input.getButtonHit(Buttons.A) && !network.isClient())
                                newLevel();
                            if (player.input.getButtonHit(Buttons.DPadRight))
                            {
                                int newLevelIndex = mapManager.getCurrentLevelIndex() + 1;
                                if (isUnlocked(newLevelIndex))
                                {
                                    if (network.isClient())
                                        network.sendIncrementLevelRequest(true);
                                    else
                                    {
                                        mapManager.incrementCurrentLevel(true);
                                        if (network.isServer())
                                            network.sendLevelIndexChange(newLevelIndex);
                                    }
                                }
                            }
                            if (player.input.getButtonHit(Buttons.DPadLeft))
                            {
                                int newLevelIndex = mapManager.getCurrentLevelIndex() - 1;
                                if (isUnlocked(newLevelIndex))
                                {
                                    if (network.isClient())
                                        network.sendIncrementLevelRequest(false);
                                    else
                                    {
                                        mapManager.incrementCurrentLevel(false);
                                        if (network.isServer())
                                            network.sendLevelIndexChange(newLevelIndex);
                                    }
                                }
                            }
                        }
                    }
                    break;
                #endregion
            }
            previousGameTime = gameTime;
            Thread.Sleep(5);
            base.Update(gameTime);
        }
        public void updateMenu(Input input, int menuItemCount)
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
        private bool isUnlocked(int level)
        {
            foreach (Being player in players.Values)
                if (!player.unlocked.isUnlocked(mapManager.guid, difficulty, level))
                    return false;
            return true;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Vector2 fontPos;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            switch (currentState.getState())
            {
                case GameState.RootMenu:
                    spriteBatch.Draw(texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
                    drawTextBorder(vademecumFont24, "Single Player", new Vector2(128, res.ScreenHeight / 2), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
                    drawTextBorder(vademecumFont24, "Multiplayer", new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
                    drawTextBorder(vademecumFont24, "Exit", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 2 ? Color.Yellow : Color.White, Color.Black, 0);
                    break;
                case GameState.Multiplayer:
                    spriteBatch.Draw(texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
                    drawTextBorder(vademecumFont24, "Host", new Vector2(128, res.ScreenHeight / 2), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
                    drawTextBorder(vademecumFont24, "Join", new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
                    drawTextBorder(vademecumFont24, "Back", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 2 ? Color.Yellow : Color.White, Color.Black, 0);
                    break;
                case GameState.JoinScreen:
                    spriteBatch.Draw(texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
                    drawTextBorder(vademecumFont24, NetworkManager.joinString, new Vector2(128, res.ScreenHeight / 2 - 32), selectedMenuItem == 0 ? Color.Yellow : Color.White, Color.Black, 0);
                    drawTextBorder(vademecumFont24, "Back", new Vector2(128, res.ScreenHeight / 2 - 64), selectedMenuItem == 1 ? Color.Yellow : Color.White, Color.Black, 0);
                    break;
                #region Cutscene
                case GameState.Cutscene:
                    try
                    {
                        drawGameplay(getAvePosition() + new Vector2(storyElement.getCameraOffset(gameTime).X, 0f));
                        foreach (Being storyBeing in storyBeings.Values)
                            storyBeing.draw();
                    }
                    catch (Exception e) { System.Console.WriteLine(e.StackTrace); }
                    drawTextBorder(vademecumFont18, "Press Start to skip cutscene", new Vector2(2, res.ScreenHeight - 40), Color.Black, Color.White, 0);
                    break;
                #endregion
                #region Gameplay
                case GameState.Gameplay:
                    drawGameplay(getAvePosition());
                    break;
                #endregion
                #region CharacterSelection
                case GameState.CharacterSelection:
                    spriteBatch.Draw(texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
                    fontPos = new Vector2(1.0f, 1.0f);
                    if (players.Count > 0)
                        drawTextBorder(vademecumFont24, "Press " + Input.AFFIRM_KEY + " to start game", fontPos, Color.White, Color.Black, 0);
                    fontPos = new Vector2(1.0f, res.ScreenHeight / 2);
                    foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
                    {
                        if (selectedMenuIndex[playerIndex] == -1)
                            drawTextBorder(vademecumFont24, "Press " + Input.AFFIRM_KEY + "\nto join", fontPos, Color.White, Color.Black, 0);
                        else
                        {
                            List<String> saves = new List<string>();
                            saves.Add("Create New...");
                            saves.AddRange(SaveManager.getAvailableCharacterNames());
                            for (int saveIndex = 0; saveIndex < saves.Count; saveIndex++)
                            {
                                Color color = selectedMenuIndex[playerIndex] == saveIndex ? Color.Yellow : Color.White;
                                drawTextBorder(vademecumFont24, saves[saveIndex], fontPos, color, Color.Black, 0);
                                fontPos.Y -= 28f;
                            }
                            fontPos.Y = res.ScreenHeight / 2;
                        }
                        fontPos.X += res.ScreenWidth / 4;
                    }
                    break;
                #endregion
                #region worldmap
                case GameState.WorldMap:
                    spriteBatch.Draw(mapManager.worldBackground, new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
                    foreach (Level level in mapManager.getLevels())
                        spriteBatch.Draw(easyLevel, level.loc, Color.White);
                    const string chooseLevelStr = "Choose level, A to start game";
                    drawTextBorder(vademecumFont24, chooseLevelStr, new Vector2(res.ScreenWidth / 2 - vademecumFont24.MeasureString(chooseLevelStr).X / 2, 1f), Color.Black, Color.DarkGray, 0);
                    spriteBatch.Draw(texMan.getTex("mapInfo"), new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2), Color.White);
                    drawTextBorder(vademecumFont24, "Level name:", new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 168), Color.Black, Color.DarkGray, 0);
                    drawTextBorder(vademecumFont24, mapManager.getCurrentLevel().name, new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 168 - 32), Color.Black, Color.DarkGray, 0);
                    drawTextBorder(vademecumFont24, "Level length:", new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 96), Color.Black, Color.DarkGray, 0);
                    drawTextBorder(vademecumFont24, mapManager.getCurrentLevel().levelLength.ToString(), new Vector2(res.ScreenWidth - texMan.getTex("mapInfo").Width + 64, res.ScreenHeight / 2 - texMan.getTex("mapInfo").Height / 2 + 96 - 32), Color.Black, Color.DarkGray, 0);
                    break;
                #endregion
            }
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
