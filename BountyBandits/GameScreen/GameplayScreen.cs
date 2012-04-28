using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BountyBandits.Character;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Stats;
using BountyBandits.Inventory;

namespace BountyBandits.GameScreen
{
    public class GameplayScreen : BaseGameScreen
    {
        public GameplayScreen(Game game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            if (game.isEndLevel())
                game.endLevel(true);
            foreach (Being currentplayer in game.players.Values)
            {
                if (currentplayer.getPos().X < 0)
                    game.endLevel(false);
                currentplayer.update(gameTime);
                #region Input
                if (currentplayer.isLocal)
                {
                    currentplayer.input.update();
                    if (currentplayer.input.getButtonDown(Buttons.LeftThumbstickLeft))
                        currentplayer.move(new Vector2(-Game.FORCE_AMOUNT, 0));
                    if (currentplayer.input.getButtonDown(Buttons.LeftThumbstickRight))
                        currentplayer.move(new Vector2(Game.FORCE_AMOUNT, 0));
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
                        game.endLevel(false);
#if WINDOWS
                    if (Keyboard.GetState().IsKeyDown(Keys.F2))
                        game.endLevel(false);
#endif
                    if (currentplayer.input.getButtonHit(Buttons.RightShoulder))
                    {
                        //pick up closest item and throw the equipped one on the ground
                        DropItem dropItem = game.getClosestDropItem(currentplayer);
                        if (dropItem != null && Vector2.DistanceSquared(dropItem.body.Position, currentplayer.body.Position) < Game.DROP_ITEM_MAX_DISTANCE)
                        {
                            Item playerItem = currentplayer.getItemManager().getItem(dropItem.getItem().getItemType());
                            currentplayer.getItemManager().putItem(dropItem.getItem());
                            if (playerItem != null)
                            {
                                dropItem.setItem(playerItem);
                                dropItem.body.LinearVelocity.Y += 25f;
                                dropItem.body.ApplyTorque((float)game.rand.NextDouble() * .25f - .125f);
                            }
                            else
                                game.activeItems.Remove(dropItem.guid);
                        }
                    }
#if DEBUG
                    if (game.inputs[0].keyPreviousState.IsKeyUp(Keys.F3) && Keyboard.GetState().IsKeyDown(Keys.F3))
                        game.spawnManager.spawnGroup("sumo", 1, 1);
                    if (Keyboard.GetState().IsKeyDown(Keys.F4))
                        foreach (Being player in game.players.Values)
                            player.giveXP(game.xpManager.getXPToLevelUp(player.level - 1));
                    if (game.inputs[0].keyPreviousState.IsKeyUp(Keys.F5) && Keyboard.GetState().IsKeyDown(Keys.F5))
                    {
                        game.dropItem(1 * currentplayer.body.Position, currentplayer);
                    }
                    if (game.inputs[0].keyPreviousState.IsKeyUp(Keys.F6) && Keyboard.GetState().IsKeyDown(Keys.F6))
                    {
                        string[] chartypes = {"amish","buddhistmonk","cow","cowboy","frenchman","godzilla",
                                                         "governator","hippie","hitler","kimjongil","mexican","mountie",
                                                     "nerd","obama","panda","pedobear","pirate","seal","shakespeare","sloth",
                                                     "stalin","sumo","tikiSmile","tikiTeeth"};
                        game.spawnManager.spawnGroup(chartypes[game.rand.Next(chartypes.Length)], 1, 1);
                    }
                    if (game.inputs[0].keyPreviousState.IsKeyUp(Keys.F7) && Keyboard.GetState().IsKeyDown(Keys.F7))
                    {
                        GameItem gameItem = new GameItem();
                        gameItem.loc = game.getAvePosition() + new Vector2(32, res.ScreenHeight);
                        gameItem.polygonType = PhysicsPolygonType.Rectangle;
                        gameItem.sideLengths = new Vector2((float)game.rand.NextDouble() * 32f + 32f, (float)game.rand.NextDouble() * 32f + 32f);
                        gameItem.weight = 1;
                        gameItem.name = "box";
                        gameItem.startdepth = (uint)game.rand.Next(4);
                        game.addGameItem(gameItem);
                    }
#endif
                }
                #endregion
            }
            if (!game.network.isClient())
                game.spawnManager.update(gameTime);
            else
                game.spawnManager.updateEnemies(gameTime);

            float timeElapsed = (float)gameTime.ElapsedGameTime.Milliseconds;
            game.physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
            #region initiate cutscene
            game.storyElement = game.mapManager.getCurrentLevel().popStoryElement(game.getAvePosition().X);
            if (game.storyElement != null)
            {
                game.timeStoryElementStarted = gameTime.TotalGameTime.TotalMilliseconds;
                game.currentState.setState(GameState.Cutscene);
                game.storyBeings = new Dictionary<int, Being>();
            }
            #endregion
        }

        public override void Draw(GameTime gameTime)
        {
            game.drawGameplay(game.getAvePosition());
        }
    }
}
