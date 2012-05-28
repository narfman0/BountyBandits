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
        private GameTime previousGameTime;

        public override void Update(GameTime gameTime)
        {
            if (Game.instance.isEndLevel())
                Game.instance.endLevel(true);
            foreach (Being currentplayer in Game.instance.players.Values)
            {
                if (currentplayer.getPos().X < 0)
                    Game.instance.endLevel(false);
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
                            currentplayer.menu.getMenuScreen() == Menu.MenuScreens.Data)
                        {
                            if (currentplayer.menu.getMenuItem() == 4)
                                Game.instance.currentState.setState(GameState.WorldMap);
                            else if (currentplayer.unusedAttr > 0)
                            {
                                if (currentplayer.menu.getMenuItem() == 0) currentplayer.upgradeStat(StatType.Agility, 1);
                                else if (currentplayer.menu.getMenuItem() == 1) currentplayer.upgradeStat(StatType.Magic, 1);
                                else if (currentplayer.menu.getMenuItem() == 2) currentplayer.upgradeStat(StatType.Speed, 1);
                                else if (currentplayer.menu.getMenuItem() == 3) currentplayer.upgradeStat(StatType.Strength, 1);
                                currentplayer.unusedAttr--;
                            }
                        }
                        currentplayer.jump();
                    }
                    #region attacks
                    if (currentplayer.input.getButtonHit(Buttons.X))
                    {
                        if (currentplayer.input.getButtonDown(Buttons.RightTrigger))
                            currentplayer.attack("attack2");
                        else if (currentplayer.input.getButtonDown(Buttons.LeftTrigger))
                            currentplayer.attack("attack3");
                        else if (!pickupClosestItem(currentplayer))
                            currentplayer.attack("attack1");
                    }
                    if (currentplayer.input.getButtonHit(Buttons.Y)){
                        if (currentplayer.input.getButtonDown(Buttons.RightTrigger))
                            currentplayer.attack("attackCC");
                        else if (currentplayer.input.getButtonDown(Buttons.LeftTrigger))
                            currentplayer.attack("attackRanged");
                        else
                            currentplayer.attack("attackMove");
                    }
#if WINDOWS
                    if (currentplayer.input.useKeyboard)
                    {
                        if (currentplayer.input.isKeyHit(Keys.Q))
                            currentplayer.attack("attack1");
                        else if (currentplayer.input.isKeyHit(Keys.W))
                            currentplayer.attack("attack2");
                        else if (currentplayer.input.isKeyHit(Keys.E))
                            currentplayer.attack("attack3");
                        else if (currentplayer.input.isKeyHit(Keys.A))
                            currentplayer.attack("attackMove");
                        else if (currentplayer.input.isKeyHit(Keys.S))
                            currentplayer.attack("attackRanged");
                        else if (currentplayer.input.isKeyHit(Keys.D))
                            currentplayer.attack("attackCC");
                    }
#endif
                    #endregion
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
                    //if (currentplayer.input.getButtonHit(Buttons.DPadRight))
                    //    if (currentplayer.menu.isMenuActive())
                    //        currentplayer.menu.changeMenuScreen(true);
                    //if (currentplayer.input.getButtonHit(Buttons.DPadLeft))
                    //    if (currentplayer.menu.isMenuActive())
                    //        currentplayer.menu.changeMenuScreen(false);
#if WINDOWS
                    if (Keyboard.GetState().IsKeyDown(Keys.F2))
                        Game.instance.endLevel(false);
#endif
#if DEBUG
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F3) && Keyboard.GetState().IsKeyDown(Keys.F3))
                        Game.instance.spawnManager.spawnGroup("hitler", 1, 1);
                    if (Keyboard.GetState().IsKeyDown(Keys.F4))
                        foreach (Being player in Game.instance.players.Values)
                            player.giveXP(Game.instance.xpManager.getXPToLevelUp(player.level - 1));
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F5) && Keyboard.GetState().IsKeyDown(Keys.F5))
                    {
                        Game.instance.dropItem(1 * currentplayer.body.Position, currentplayer);
                    }
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F6) && Keyboard.GetState().IsKeyDown(Keys.F6))
                    {
                        int num = Game.instance.rand.Next(Enum.GetValues(typeof(BeingTypes)).Length);
                        BeingTypes spawnType = (BeingTypes)Enum.GetValues(typeof(BeingTypes)).GetValue(num);
                        Game.instance.spawnManager.spawnGroup(spawnType.ToString(), 1, 1);
                    }
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F7) && Keyboard.GetState().IsKeyDown(Keys.F7))
                    {
                        GameItem gameItem = new GameItem();
                        gameItem.loc = Game.instance.getAvePosition() + new Vector2(32, res.ScreenHeight);
                        gameItem.polygonType = PhysicsPolygonType.Rectangle;
                        gameItem.sideLengths = new Vector2((float)Game.instance.rand.NextDouble() * 32f + 32f, (float)Game.instance.rand.NextDouble() * 32f + 32f);
                        gameItem.weight = 1;
                        gameItem.name = "box";
                        gameItem.startdepth = (uint)Game.instance.rand.Next(4);
                        Game.instance.addGameItem(gameItem);
                    }
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F8) && Keyboard.GetState().IsKeyDown(Keys.F8))
                        Game.instance.spawnManager.spawnGroup("panda", 1, 1);
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F9) && Keyboard.GetState().IsKeyDown(Keys.F9))
                        Game.instance.spawnManager.spawnGroup("sloth", 1, 1);
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F10) && Keyboard.GetState().IsKeyDown(Keys.F10))
                        Game.instance.spawnManager.spawnGroup("seal", 1, 1);
                    if (Game.instance.inputs[0].keyPreviousState.IsKeyUp(Keys.F11) && Keyboard.GetState().IsKeyDown(Keys.F11))
                        Game.instance.spawnManager.spawnGroup("pedobear", 1, 1);
#endif
                }
                #endregion
            }
            if (!Game.instance.network.isClient())
                Game.instance.spawnManager.update(gameTime);
            else
                Game.instance.spawnManager.updateEnemies(gameTime);
            #region Physics
            if (previousGameTime == null)
                previousGameTime = gameTime;
            float timeElapsed = (float)gameTime.TotalGameTime.TotalMilliseconds - (float)previousGameTime.TotalGameTime.TotalMilliseconds;
            previousGameTime = gameTime;
            Game.instance.physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
            #endregion
            #region initiate cutscene
            Game.instance.storyElement = Game.instance.mapManager.getCurrentLevel().popStoryElement(Game.instance.getAvePosition().X);
            if (Game.instance.storyElement != null)
            {
                Game.instance.currentState.setState(GameState.Cutscene);
            }
            #endregion
        }

        public override void Draw(GameTime gameTime)
        {
            drawGameplay(Game.instance.getAvePosition());
        }

        /// <summary>
        /// pick up closest item and throw the equipped one on the ground
        /// </summary>
        /// <param name="player">player to pick up item</param>
        /// <returns>true if item picked up</returns>
        private bool pickupClosestItem(Being player)
        {
            DropItem dropItem = Game.instance.getClosestDropItem(player);
            if (dropItem != null && Vector2.DistanceSquared(dropItem.body.Position, player.body.Position) < Game.DROP_ITEM_MAX_DISTANCE)
            {
                Item playerItem = player.getItemManager().getItem(dropItem.getItem().getItemType());
                player.getItemManager().putItem(dropItem.getItem());
                if (playerItem != null)
                {
                    dropItem.setItem(playerItem);
                    dropItem.body.LinearVelocity.Y += 25f;
                    dropItem.body.ApplyTorque((float)Game.instance.rand.NextDouble() * .25f - .125f);
                }
                else
                {
                    Game.instance.activeItems.Remove(dropItem.guid);
                    Game.instance.physicsSimulator.BodyList.Remove(dropItem.body);
                }
            }
            else
                return false;
            return true;
        }
    }
}
