using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Network
{
    enum TimedUpdate { State, PlayersUpdates }
    public enum MessageType
    {
        PlayerFullUpdateClient, PlayerFullUpdateServer, GameState, 
        PlayersUpdate, PlayersUpdateClient, IncrementLevelRequest, LevelIndexChange
    }
}
