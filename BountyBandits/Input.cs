using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BountyBandits
{
    public class Input
    {
        private Dictionary<Buttons, Keys> xboxButtonToKeyboardKey = new Dictionary<Buttons, Keys>(), 
            xboxButtonToKeyboardKeySecondary = new Dictionary<Buttons, Keys>();
        KeyboardState keyPreviousState;
        GamePadState padState, padPreviousState;
        public Input()
        {
            xboxButtonToKeyboardKey.Add(Buttons.A, Keys.Space);
            xboxButtonToKeyboardKey.Add(Buttons.X, Keys.LeftControl);
            xboxButtonToKeyboardKey.Add(Buttons.Start, Keys.Escape);
            xboxButtonToKeyboardKey.Add(Buttons.DPadLeft, Keys.Left);
            xboxButtonToKeyboardKey.Add(Buttons.DPadRight, Keys.Right);
            xboxButtonToKeyboardKey.Add(Buttons.DPadUp, Keys.Up);
            xboxButtonToKeyboardKey.Add(Buttons.DPadDown, Keys.Down);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickLeft, Keys.A);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickRight, Keys.D);
            xboxButtonToKeyboardKey.Add(Buttons.RightShoulder, Keys.E);
            xboxButtonToKeyboardKey.Add(Buttons.Back, Keys.C);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.DPadUp, Keys.W);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.DPadDown, Keys.S);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.A, Keys.Enter);
        }

        private bool getButtonHit(Buttons target, KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            if (!getButtonDown(target, keyPreviousState, padState, padPreviousState) &&
                getButtonDown(target, Keyboard.GetState(), padState, padPreviousState))
                return true;
            return false;
        }

        private bool getButtonDown(Buttons target, KeyboardState keyState, GamePadState padState, GamePadState padPreviousState)
        {
            if (padState.IsButtonDown(target) || keyState.IsKeyDown(xboxButtonToKeyboardKey[target]) ||
                (xboxButtonToKeyboardKeySecondary.ContainsKey(target) && keyState.IsKeyDown(xboxButtonToKeyboardKeySecondary[target])))
                return true;
            return false;
        }

        public void setCurrentInput(KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            this.keyPreviousState = keyPreviousState;
            this.padState = padState;
            this.padPreviousState = padPreviousState;
        }

        public bool getButtonHit(Buttons target)
        {
            return getButtonHit(target, keyPreviousState, padState, padPreviousState);
        }

        public bool getButtonDown(Buttons target)
        {
            return getButtonDown(target, keyPreviousState, padState, padPreviousState);
        }
        
    }
}
