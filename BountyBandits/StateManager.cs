using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    public class StateManager
    {
        private Game gameref;
        private GameState current = GameState.RootMenu;
        public StateManager(Game gameref)
        {
            this.gameref = gameref;
        }
        public void setState(GameState newState){
            current = newState;
            gameref.network.sendGameStateUpdate();
        }
        public GameState getState() { return current; }
    }
}
