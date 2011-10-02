using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BountyBandits
{
    public class Input
    {
        private Dictionary<Buttons, Keys> xboxButtonToKeyboardKey = new Dictionary<Buttons,Keys>();
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
            xboxButtonToKeyboardKey.Add(Buttons.RightShoulder, Keys.P);
            xboxButtonToKeyboardKey.Add(Buttons.Back, Keys.C);
        }

        private bool getButtonHit(Buttons target, KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            if (getButtonDown(target, keyPreviousState, padState, padPreviousState) &&
                (padPreviousState.IsButtonUp(target) || keyPreviousState.IsKeyUp(xboxButtonToKeyboardKey[target])) )
                return true;
            return false;
        }

        private bool getButtonDown(Buttons target, KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            if (padState.IsButtonDown(target) || Keyboard.GetState().IsKeyDown(xboxButtonToKeyboardKey[target]))
                return true;
            return false;
        }

        public void setCurrentInput(KeyboardState keyPreviousState, GamePadState padState, GamePadState padPreviousState)
        {
            this.keyPreviousState = keyPreviousState;
            this.padState = padState;
            this.padPreviousState = padPreviousState;
        }

        public bool getButtonHitCurrent(Buttons target)
        {
            return getButtonHit(target, keyPreviousState, padState, padPreviousState);
        }

        public bool getButtonDownCurrent(Buttons target)
        {
            return getButtonDown(target, keyPreviousState, padState, padPreviousState);
        }
        
    }
}
