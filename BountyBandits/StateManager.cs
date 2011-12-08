using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    class StateManager
    {
        GameState current = GameState.RootMenu;
        public bool setState(GameState newState){
            current = newState;
            return true;
        }
        public GameState getState() { return current; }
    }
}
