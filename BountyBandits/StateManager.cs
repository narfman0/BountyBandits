using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    class StateManager
    {
        const int TIME_BETWEEN_STATE_CHANGES = 1000;
        Game.GameState current = Game.GameState.RootMenu;
        int timeSinceLastStateChange = 0;
        public bool setState(Game.GameState newState){
            if (Environment.TickCount - timeSinceLastStateChange > TIME_BETWEEN_STATE_CHANGES)
            {
                timeSinceLastStateChange = Environment.TickCount;
                current = newState;
                return true;
            }
            else
                return false;
        }
        public Game.GameState getState() { return current; }
    }
}
