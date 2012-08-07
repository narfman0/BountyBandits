using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Character;
using BountyBandits.Map;
using BountyBandits.GameScreen.Controls;

namespace BountyBandits.GameScreen
{
    public class MapEditorScreen : BaseGameScreen
    {
        private GameTime previousGameTime;
        public Level level;
        private Control control;
        public Vector2 cameraOffset;
        private MouseState previousMouseState;
        public bool physicsEnabled = false;
        private IMovableItem movingItem;

        public MapEditorScreen()
            : base()
        {
            cameraOffset = Game.instance.getAvePosition();
            previousMouseState = Mouse.GetState();
            level = Game.instance.mapManager.getCurrentLevel();
            control = new Control(this);
            control.Visible = true;
            Game.instance.IsMouseVisible = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.instance.isEndLevel())
                Game.instance.endLevel(true);
            foreach (Being currentplayer in Game.instance.players.Values)
            {
                currentplayer.update(gameTime);
                if (currentplayer.isLocal)
                {
                    currentplayer.input.update();
                    if (currentplayer.input.isKeyHit(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                        exitMapEditor();
                    if (currentplayer.input.getButtonHit(Buttons.LeftThumbstickUp))
                        cameraOffset.Y++;
                    if (currentplayer.input.getButtonHit(Buttons.LeftThumbstickDown))
                        cameraOffset.Y--;
                    if (currentplayer.input.getButtonHit(Buttons.LeftThumbstickLeft))
                        cameraOffset.X--;
                    if (currentplayer.input.getButtonHit(Buttons.LeftThumbstickRight))
                        cameraOffset.X++;
                    if (currentplayer.input.isKeyHit(Keys.P))
                        control.setPhysicsEnabled(physicsEnabled = !physicsEnabled);
                    if (previousMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Game.instance.res.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                        cameraOffset += new Vector2(Mouse.GetState().X, Game.instance.res.ScreenHeight - Mouse.GetState().Y) - getResolution() / 2;
                    if (currentplayer.input.isKeyHit(Keys.Delete) && movingItem != null)
                    {
                        level.remove(movingItem);
                        control.setGuiControls(movingItem);
                        movingItem = null;
                    }
                    if (Mouse.GetState().RightButton == ButtonState.Pressed && Game.instance.res.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                    {
                        Vector2 currentLocation = getCurrentLocation();
                        control.setCurrentPosition(currentLocation);
                        #region begin move items
                        if (previousMouseState.RightButton == ButtonState.Released)
                        {
                            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                                movingItem = level.getGameItemAtLocation(currentLocation);
                            else if (Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
                                movingItem = level.getSpawnAtLocation(currentLocation.X, currentLocation.Y);
                            else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                                movingItem = level.getBackgroundItemAtLocation(currentLocation.X, currentLocation.Y);
                        }
                        #endregion
                    }
                    #region end move items
                    if (ButtonState.Pressed == Mouse.GetState().RightButton && Game.instance.res.Contains(Mouse.GetState().X, Mouse.GetState().Y) && movingItem != null)
                        movingItem.setPosition(getCurrentLocation());
                    if (ButtonState.Released == Mouse.GetState().RightButton && ButtonState.Pressed == previousMouseState.RightButton)
                        movingItem = null;
                    #endregion

                    previousMouseState = Mouse.GetState();
                }
            }
            if (!Game.instance.network.isClient())
                Game.instance.spawnManager.update(gameTime);
            #region Physics
            if (previousGameTime == null)
                previousGameTime = gameTime;
            float timeElapsed = (float)gameTime.TotalGameTime.TotalMilliseconds - (float)previousGameTime.TotalGameTime.TotalMilliseconds;
            previousGameTime = gameTime;
            if(physicsEnabled)
                Game.instance.physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
            #endregion
        }

        private Vector2 getCurrentLocation()
        {
            return cameraOffset - getResolution() / 2 + new Vector2(Mouse.GetState().X, Game.instance.res.ScreenHeight - Mouse.GetState().Y);
        }

        private Vector2 getResolution()
        {
            return new Vector2(Game.instance.res.ScreenWidth, Game.instance.res.ScreenHeight);
        }

        public override void Draw(GameTime gameTime)
        {
            drawGameplay(cameraOffset, level);
        }

        public void exitMapEditor()
        {
            Game.instance.currentState.setState(GameState.Gameplay);
            control.Dispose();
            Game.instance.IsMouseVisible = false;
        }
    }
}
