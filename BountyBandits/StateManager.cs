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
            bool skip = (current == GameState.Gameplay && newState == GameState.Cutscene) ||
                        (current == GameState.Cutscene && newState == GameState.Gameplay);
            current = newState;
            if(!skip)
                gameref.network.sendGameStateUpdate();
        }
        public GameState getState() { return current; }
    }
}
