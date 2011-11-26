using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    class StateManager
    {
        Game.GameState current = Game.GameState.RootMenu;
        public bool setState(Game.GameState newState){
            current = newState;
            return true;
        }
        public Game.GameState getState() { return current; }
    }
}
