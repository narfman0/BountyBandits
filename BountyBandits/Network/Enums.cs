using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Network
{
    enum TimedUpdate { PlayersUpdate, ObjectsUpdate }
    public enum MessageType
    {
        PlayerFullUpdateClient, PlayerFullUpdateServer, GameState, ObjectsFullUpdate, ObjectsUpdate,
        PlayersUpdate, PlayersUpdateClient, IncrementLevelRequest, LevelIndexChange
    }
}
