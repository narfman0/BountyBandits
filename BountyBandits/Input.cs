﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BountyBandits
{
    public class Input
    {
        private Dictionary<Buttons, Keys> xboxButtonToKeyboardKey = new Dictionary<Buttons, Keys>(), 
            xboxButtonToKeyboardKeySecondary = new Dictionary<Buttons, Keys>();
        KeyboardState keyState, keyPreviousState;
        GamePadState padState, padPreviousState;
        private PlayerIndex index;
        public bool useKeyboard = false;
        public Input(PlayerIndex index)
        {
            this.index = index;
            xboxButtonToKeyboardKey.Add(Buttons.A, Keys.Space);
            xboxButtonToKeyboardKey.Add(Buttons.X, Keys.LeftControl);
            xboxButtonToKeyboardKey.Add(Buttons.Start, Keys.Escape);
            xboxButtonToKeyboardKey.Add(Buttons.DPadLeft, Keys.Left);
            xboxButtonToKeyboardKey.Add(Buttons.DPadRight, Keys.Right);
            xboxButtonToKeyboardKey.Add(Buttons.DPadUp, Keys.Up);
            xboxButtonToKeyboardKey.Add(Buttons.DPadDown, Keys.Down);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickLeft, Keys.A);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickRight, Keys.D);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickUp, Keys.W);
            xboxButtonToKeyboardKey.Add(Buttons.LeftThumbstickDown, Keys.S);
            xboxButtonToKeyboardKey.Add(Buttons.RightShoulder, Keys.E);
            xboxButtonToKeyboardKey.Add(Buttons.Back, Keys.C);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.DPadUp, Keys.W);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.DPadDown, Keys.S);
            xboxButtonToKeyboardKeySecondary.Add(Buttons.A, Keys.Enter);
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
    }
}
