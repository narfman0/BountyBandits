using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.GameScreen
{
    public abstract class BaseGameScreen
    {
        protected Game game;
        protected SpriteBatch spriteBatch;
        protected Resolution res;
        protected int selectedMenuItem = 0;

        public BaseGameScreen(Game game)
        {
            this.game = game;
            this.spriteBatch = game.spriteBatch;
            this.res = game.res;
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
    }
}
