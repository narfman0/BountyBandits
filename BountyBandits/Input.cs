using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BountyBandits
{
    public class Input
    {
#if WINDOWS 
            public static string AFFIRM_KEY = "Enter";
#else
            public static string AFFIRM_KEY = "A";
#endif
        private Dictionary<Buttons, Keys> xboxButtonToKeyboardKey = new Dictionary<Buttons, Keys>(), 
            xboxButtonToKeyboardKeySecondary = new Dictionary<Buttons, Keys>();
        public KeyboardState keyState, keyPreviousState;
        public GamePadState padState, padPreviousState;
        private PlayerIndex index;
        public bool useKeyboard = false;
        public Input(PlayerIndex index)
        {
            this.index = index;
            xboxButtonToKeyboardKey.Add(Buttons.A, Keys.Space);
            xboxButtonToKeyboardKey.Add(Buttons.X, Keys.Q);
            xboxButtonToKeyboardKey.Add(Buttons.Y, Keys.A);
            xboxButtonToKeyboardKey.Add(Buttons.Start, Keys.Enter);
            xboxButtonToKeyboardKey.Add(Buttons.DPadLeft, Keys.Left);
            xboxButtonToKeyboardKey.Add(Buttons.DPadRight, Keys.Right);
            xboxButtonToKeyboardKey.Add(Buttons.DPadUp, Keys.Up);
            xboxButtonToKeyboardKey.Add(Buttons.DPadDown, Keys.Down);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickLeft, Keys.Left);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickRight, Keys.Right);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickUp, Keys.Up);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickDown, Keys.Down);
            xboxButtonToKeyboardKey.Add(Buttons.RightTrigger, Keys.LeftShift);
            xboxButtonToKeyboardKey.Add(Buttons.LeftTrigger, Keys.LeftAlt);
            xboxButtonToKeyboardKey.Add(Buttons.Back, Keys.C);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.A, Keys.Enter);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.Back, Keys.Escape);
            //xboxButtonToKeyboardKeySecondary.Add(Buttons.X, Keys.LeftControl);
            update();
        }

        private bool getButtonHit(Buttons target, KeyboardState keyCurrentState, KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            if (!getButtonDown(target, keyPreviousState, padPreviousState) &&
                getButtonDown(target, keyCurrentState, padState))
                return true;
            return false;
        }

        private bool getButtonDown(Buttons target, KeyboardState keyState, GamePadState padState)
        {
            if (padState.IsButtonDown(target) || keyState.IsKeyDown(xboxButtonToKeyboardKey[target]) ||
                (xboxButtonToKeyboardKeySecondary.ContainsKey(target) && keyState.IsKeyDown(xboxButtonToKeyboardKeySecondary[target])))
                return true;
            return false;
        }

        public void update()
        {
            if (useKeyboard)
            {
                keyPreviousState = keyState;
                keyState = Keyboard.GetState(index);
            }
            padPreviousState = padState;
            padState = GamePad.GetState(index);
        }

        public bool getButtonHit(Buttons target)
        {
            return getButtonHit(target, keyState, keyPreviousState, padState, padPreviousState);
        }

        public bool getButtonDown(Buttons target)
        {
            return getButtonDown(target, keyState, padState);
        }

        public PlayerIndex getPlayerIndex()
        {
            return index;
        }
#if WINDOWS
        public List<Keys> getKeysHit()
        {
            List<Keys> keys = new List<Keys>();
            if (useKeyboard)
            {
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                    if (keyPreviousState.IsKeyUp(key) && keyState.IsKeyDown(key))
                        keys.Add(key);
            }
            return keys;
        }

        public bool isKeyHit(Keys key)
        {
            foreach (Keys hitkey in getKeysHit())
                if (key == hitkey)
                    return true;
            return false;
        }
#endif
    }
}
