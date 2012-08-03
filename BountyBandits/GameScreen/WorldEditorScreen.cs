using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BountyBandits.GameScreen.Controls;
using Microsoft.Xna.Framework.Input;

namespace BountyBandits.GameScreen
{
    public class WorldEditorScreen : BaseGameScreen
    {
        private WorldControl control;
        private MouseState previousMouseState;

        public WorldEditorScreen() : base()
        {
            control = new WorldControl(this);
            control.Visible = true;
            Game.instance.IsMouseVisible = true;
            previousMouseState = Mouse.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 resolution = new Vector2(Game.instance.res.ScreenWidth, Game.instance.res.ScreenHeight);
            if (previousMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Game.instance.res.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                control.setLocation(new Vector2(Mouse.GetState().X, Game.instance.res.ScreenHeight - Mouse.GetState().Y));
            previousMouseState = Mouse.GetState();
        }

        public override void Draw(GameTime gameTime)
        {
            drawWorldBackground();
        }

        public void exitMapEditor()
        {
            Game.instance.currentState.setState(GameState.Gameplay);
            control.Dispose();
            Game.instance.IsMouseVisible = false;
        }

    }
}
